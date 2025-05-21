using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using RealTimeSTTTest.STT.Interfaces;

namespace RealTimeSTTTest.STT.Recognition
{
    public class HumeEmotionRecognizer : IEmotionRecognizer
    {
        private readonly HttpClient _http;

        public HumeEmotionRecognizer(string apiKey)
        {
            _http = new HttpClient { BaseAddress = new Uri("https://api.hume.ai") };
            _http.DefaultRequestHeaders.Add("X-Hume-Api-Key", apiKey);
        }

        public async Task<IDictionary<string, double>> RecognizeAsync(Stream wavStream)
        {
            wavStream.Position = 0;
            using var content = new StreamContent(wavStream);
            content.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");

            var resp = await _http.PostAsync("/v0/audio/models?models=prosody", content);
            resp.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(await resp.Content.ReadAsStringAsync());
            var emotions = doc.RootElement
                .GetProperty("predictions")[0]
                .GetProperty("results")
                .GetProperty("predictions")[0]
                .GetProperty("emotions");

            return emotions.EnumerateArray()
                .ToDictionary(
                    e => e.GetProperty("name").GetString()!,
                    e => e.GetProperty("score").GetDouble());
        }
    }
}
