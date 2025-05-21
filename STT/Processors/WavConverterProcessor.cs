using RealTimeSTTTest.STT.Interfaces;

namespace RealTimeSTTTest.STT.Processors;

public class WavConverterProcessor : IAudioProcessor
{
    public Task<MemoryStream> ProcessAsync(MemoryStream input)
    {
        // Assumes raw PCM 16kHz, 16-bit mono
        byte[] raw = input.ToArray();
        var wav = new MemoryStream();
        using (var writer = new BinaryWriter(wav, System.Text.Encoding.ASCII, true))
        {
            // RIFF header
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + raw.Length);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVE"));
            // fmt subchunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)1);
            writer.Write(16000);
            writer.Write(16000 * 2);
            writer.Write((short)2);
            writer.Write((short)16);
            // data subchunk
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data"));
            writer.Write(raw.Length);
            writer.Write(raw);
        }
        wav.Position = 0;
        return Task.FromResult(wav);
    }
}
