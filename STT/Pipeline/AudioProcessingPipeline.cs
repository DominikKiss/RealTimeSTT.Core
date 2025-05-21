using RealTimeSTTTest.STT.Interfaces;

namespace RealTimeSTTTest.STT.Pipeline
{
    public class AudioProcessingPipeline
    {
        private readonly List<IAudioProcessor> _processors = new();
        public void AddProcessor(IAudioProcessor processor) => _processors.Add(processor);

        public async Task<MemoryStream> RunAsync(MemoryStream input)
        {
            MemoryStream current = input;
            foreach (var proc in _processors)
            {
                current = await proc.ProcessAsync(current);
            }
            return current;
        }
    }
}