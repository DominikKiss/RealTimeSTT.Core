using System.IO;
using RealTimeSTTTest.STT.Interfaces;
using Whisper.net;

namespace RealTimeSTTTest.STT.Processors
{
    public class WhisperRecognizer : IAudioProcessor
    {
        private readonly WhisperProcessor _proc;
        public WhisperRecognizer(WhisperProcessor p) => _proc = p;

        public async Task<MemoryStream> ProcessAsync(MemoryStream input)
        {
            input.Position = 0;
            string text = string.Empty;
            await foreach (var seg in _proc.ProcessAsync(input)) text += seg.Text + " ";
            Console.WriteLine("[Transcription] " + text.Trim());
            input.Position = 0;
            return input;
        }
    }
}