using SpeechToTextService;

namespace SpeechToText.Testbed
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // load apikey from a file called api.key
            Console.WriteLine("Speech to Text text. Press [ENTER] and then speak until the countdown completes.");
            Console.ReadKey();
            var key = File.ReadAllText("api.key"); //set your api key to 'copy if newer' or just drop it in the output dir
            await AudioToText.Test(key);
            

        }
    }
}