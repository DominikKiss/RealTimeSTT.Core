using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using PortAudioSharp;
using RealTimeSTTTest.STT.Interfaces;
using RealTimeSTTTest.STT.Pipeline;

namespace RealTimeSTTTest.STT.Input
{
    public class MicrophoneInput
    {
        private readonly AudioProcessingPipeline _pipeline;
        private readonly SemaphoreSlim _semaphore = new(1,1);
        private MemoryStream _accumulator = new();
        private PortAudioSharp.Stream _stream;
        private const int SampleRate = 16000;
        private const int ChunkSeconds = 5;
        private readonly int ChunkBytes = SampleRate * sizeof(short) * ChunkSeconds;

        public MicrophoneInput(AudioProcessingPipeline pipeline)
        {
            _pipeline = pipeline;
            PortAudio.Initialize();
            Console.WriteLine($"PortAudio v{PortAudio.VersionInfo.versionText}");
            Console.WriteLine($"DeviceCount: {PortAudio.DeviceCount}");

            int dev = PortAudio.DefaultInputDevice;
            if (dev == PortAudio.NoDevice) throw new InvalidOperationException("Nincs alapértelmezett eszköz!");
            var info = PortAudio.GetDeviceInfo(dev);
            Console.WriteLine($"Using: [{dev}] {info.name}");

            var inParams = new StreamParameters
            {
                device = dev,
                channelCount = 1,
                sampleFormat = SampleFormat.Float32,
                suggestedLatency = info.defaultLowInputLatency,
                hostApiSpecificStreamInfo = IntPtr.Zero
            };

            _stream = new PortAudioSharp.Stream(
                inParams: inParams,
                outParams: null,
                sampleRate: SampleRate,
                framesPerBuffer: 0,
                streamFlags: StreamFlags.ClipOff,
                callback: AudioCallback,
                userData: IntPtr.Zero
            );

            _stream.Start();
            Console.WriteLine("Mikrofon stream elindítva.");
        }

        private StreamCallbackResult AudioCallback(IntPtr input, IntPtr output, uint frameCount,
            ref StreamCallbackTimeInfo timeInfo, StreamCallbackFlags statusFlags, IntPtr userData)
        {
            var samples = new float[frameCount];
            Marshal.Copy(input, samples, 0, (int)frameCount);
            var bytes = new byte[samples.Length * sizeof(short)];
            for (int i = 0; i < samples.Length; i++)
            {
                short s = (short)Math.Clamp(samples[i] * short.MaxValue, short.MinValue, short.MaxValue);
                var b = BitConverter.GetBytes(s);
                bytes[2*i] = b[0]; bytes[2*i+1] = b[1];
            }
            _accumulator.Write(bytes, 0, bytes.Length);

            if (_accumulator.Length >= ChunkBytes)
            {
                var data = _accumulator.ToArray();
                _accumulator.SetLength(0);
                var chunk = new MemoryStream(data, false);

                if (_semaphore.Wait(0))
                {
                    _ = Task.Run(async () =>
                    {
                        try { await _pipeline.RunAsync(chunk); }
                        finally { _semaphore.Release(); }
                    });
                }
            }
            return StreamCallbackResult.Continue;
        }

        public void Stop()
        {
            _stream?.Stop();
            _stream?.Dispose();
            PortAudio.Terminate();
        }
    }
}
