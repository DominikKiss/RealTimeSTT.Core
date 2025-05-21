
using System.Globalization;
using RealTimeSTT.STT.Initialization;
using RealTimeSTTTest.STT.Processors;
using RealTimeSTTTest.STT.Input;
using RealTimeSTTTest.STT.Pipeline;

class Program
{
    static async Task Main(string[] args)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

        var options = new RuntimeOptions
        {
            ggmltype = Whisper.net.Ggml.GgmlType.LargeV3,
            ModelFileName = "ggml-largev3.bin",
            CoreMlModelcName = "ggml-largev3-encoder.mlmodelc"
        };

        var whisper = await STTInitializer.InitWhisperAsync(options);
        if (whisper == null)
        {
            Console.WriteLine("Whisper inicializálása sikertelen.");
            return;
        }

        var pipeline = new AudioProcessingPipeline();
        pipeline.AddProcessor(new NoiseReductionProcessor());
        pipeline.AddProcessor(new WavConverterProcessor());
        pipeline.AddProcessor(new WhisperRecognizer(whisper));

        var micInput = new MicrophoneInput(pipeline);
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;        // ne szakítsa meg rögtön a futást
                micInput.Stop();        // leállítjuk a PortAudio streamet
            };

            Console.WriteLine("Nyomj Ctrl+C-t a leállításhoz...");
            // Főszál blokkolását a CancelKeyPress-ig
            await Task.Delay(Timeout.Infinite, cts.Token);
    }
}

