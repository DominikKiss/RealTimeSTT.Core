namespace RealTimeSTTTest.STT.Interfaces
{
    public interface ISpeechRecognizer
    {
        Task<string> RecognizeAsync(Stream audio);
    }
}
