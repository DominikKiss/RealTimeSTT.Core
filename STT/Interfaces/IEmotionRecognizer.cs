namespace RealTimeSTTTest.STT.Interfaces;
    public interface IEmotionRecognizer
    {
        Task<IDictionary<string, double>> RecognizeAsync(Stream wavStream);
    }