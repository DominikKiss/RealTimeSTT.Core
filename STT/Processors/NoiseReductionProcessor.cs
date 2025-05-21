using NAudio.Dsp;
using RealTimeSTTTest.STT.Interfaces;

namespace RealTimeSTTTest.STT.Processors;

public class NoiseReductionProcessor : IAudioProcessor
{
    private readonly BiQuadFilter _highPass;
    private readonly BiQuadFilter _lowpass;

    public NoiseReductionProcessor()
    {
        // 100 Hz high-pass filter, Q = 1
        _highPass = BiQuadFilter.HighPassFilter(16000, 100f, 1f);
        _lowpass = BiQuadFilter.LowPassFilter(16000, 3000f, 1f);
    }

    public Task<MemoryStream> ProcessAsync(MemoryStream input)
    {
        input.Position = 0;
        var raw = input.ToArray();
        int sampleCount = raw.Length / sizeof(short);
        var pcm = new short[sampleCount];
        for (int i = 0; i < sampleCount; i++)
            pcm[i] = BitConverter.ToInt16(raw, i * 2);

        double rmsBefore = ComputeRms(pcm);
        for (int i = 0; i < sampleCount; i++)
            pcm[i] = (short)_highPass.Transform(pcm[i]);
        double rmsAfter = ComputeRms(pcm);

        Console.WriteLine($"[NoiseReduction] RMS before: {rmsBefore:F2}, after: {rmsAfter:F2}");

        var outRaw = new byte[raw.Length];
        for (int i = 0; i < sampleCount; i++)
        {
            var bytes = BitConverter.GetBytes(pcm[i]);
            outRaw[i * 2] = bytes[0];
            outRaw[i * 2 + 1] = bytes[1];
        }

        return Task.FromResult(new MemoryStream(outRaw, writable: false));
    }

    private static double ComputeRms(short[] samples)
    {
        double sumSq = 0;
        foreach (var s in samples)
            sumSq += s * s;
        return Math.Sqrt(sumSq / samples.Length);
    }
}
