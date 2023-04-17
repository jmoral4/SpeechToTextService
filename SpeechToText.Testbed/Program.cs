using SpeechToTextService;

namespace SpeechToText.Testbed
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // load apikey from a file called api.key
            Console.WriteLine("Speech to Text text. Speak for 10 seconds.");
            var key = File.ReadAllText("api.key");
            await AudioToText.Test(key);
            

        }
    }
}