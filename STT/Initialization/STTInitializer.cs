using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;

namespace RealTimeSTT.STT.Initialization;

public class STTInitializer
{
    public static async Task<WhisperProcessor?> InitWhisperAsync(RuntimeOptions options)
    {
        // Modell letöltése / ellenőrzése
        if (!File.Exists(options.ModelFileName))
        {
            Console.WriteLine($"Model fájl '{options.ModelFileName}' nem található. Modellt töltök le: {options.ggmltype}...");
            using var modelStream = await WhisperGgmlDownloader.Default.GetGgmlModelAsync(options.ggmltype);
            using var fileWriter = File.OpenWrite(options.ModelFileName);
            await modelStream.CopyToAsync(fileWriter);
            Console.WriteLine("Modell letöltése sikeres.");
        }
        else
        {
            Console.WriteLine($"Modell fájl megtalálva: {options.ModelFileName}");
        }

        // CoreML kódoló letöltése / ellenőrzése
        if (!Directory.Exists(options.CoreMlModelcName))
        {
            Console.WriteLine($"CoreML kódoló '{options.CoreMlModelcName}' nem található. Letöltés...");
            await WhisperGgmlDownloader.Default.GetEncoderCoreMLModelAsync(options.ggmltype)
                .ExtractToPath(options.CoreMlModelcName);
            Console.WriteLine("CoreML kódoló letöltése és kicsomagolása sikeres.");
        }
        else
        {
            Console.WriteLine($"CoreML kódoló mappa megtalálva: {options.CoreMlModelcName}");
        }

        // Logger hozzáadása a natív backend kiválasztásához
        using var nativeLogger = LogProvider.AddLogger((level, message) =>
        {
            Console.WriteLine($"[Whisper Native] [{level}] {message}");
        });

        Console.WriteLine("WhisperProcessor inicializálása...");
        var factory = WhisperFactory.FromPath(options.ModelFileName);
        var builder = factory.CreateBuilder()
                             .WithLanguage("en");
        var processor = builder.Build();
        Console.WriteLine("Whisper processor inicializálva.");
        return processor;
    }
}