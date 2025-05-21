namespace RealTimeSTTTest.STT.Interfaces
{
    public interface IAudioProcessor
    {
        Task<MemoryStream> ProcessAsync(MemoryStream input);
    }
}