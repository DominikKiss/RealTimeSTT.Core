// Processors/RNNoiseProcessor.cs
using System;
using System.IO;
using System.Threading.Tasks;
using RNNoise.NET;
using RealTimeSTTTest.STT.Interfaces;

namespace RealTimeSTTTest.STT.Processors;

public class RNNoiseProcessor : IAudioProcessor
{
    private readonly Denoiser _denoiser;

    public RNNoiseProcessor()
    {
        _denoiser = new Denoiser();
    }

    public Task<MemoryStream> ProcessAsync(MemoryStream input)
    {
        input.Position = 0;
        var raw = input.ToArray();
        int sampleCount = raw.Length / sizeof(short);

        // Convert 16-bit PCM to float [-1,1]
        var floats = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(raw, i * 2);
            floats[i] = sample / 32768f;
        }

        // Compute RMS before denoise
        double rmsBefore = ComputeRms(floats);

        // Denoise the audio in place
        _denoiser.Denoise(floats.AsSpan());

        // Compute RMS after denoise
        double rmsAfter = ComputeRms(floats);
        // Calculate improvement in dB
        double improvementDb = (rmsAfter > 0) ? 20 * Math.Log10(rmsBefore / rmsAfter) : 0;
        Console.WriteLine($"[NoiseReduction] RMS before: {rmsBefore:F2}, after: {rmsAfter:F2}, improvement: {improvementDb:F1} dB");

        // Convert floats back to 16-bit PCM
        var output = new byte[sampleCount * sizeof(short)];
        for (int i = 0; i < sampleCount; i++)
        {
            short outSample = (short)(Math.Clamp(floats[i], -1f, 1f) * 32767);
            var bytes = BitConverter.GetBytes(outSample);
            output[i * 2] = bytes[0];
            output[i * 2 + 1] = bytes[1];
        }

        return Task.FromResult(new MemoryStream(output, writable: false));
    }

    private static double ComputeRms(float[] samples)
    {
        double sumSq = 0;
        foreach (var s in samples)
            sumSq += s * s;
        return Math.Sqrt(sumSq / samples.Length);
    }
}



