using System;
using System.Threading.Tasks;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Starting Voice Detection System...");

        VoiceDetectionSystem system = new VoiceDetectionSystem();

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Stopping Voice Detection System...");
            system.Stop();
            Task.Delay(1000).Wait(); // Give it time to stop
        };

        await system.Run();

        Console.WriteLine("Voice Detection System stopped.");
    }
}