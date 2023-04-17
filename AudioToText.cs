using System.Text.Json;
using System.Net.Http.Headers;
using System.Diagnostics;
using System.Text;

namespace SpeechToTextService
{
    public class AudioToText
    {
        private readonly string _apiBaseUrl = "https://api.openai.com/v1/audio/transcriptions";
        //private readonly string _apiBaseUrl = "https://api.openai.com/v1/whisper/asr";

        private readonly string _apiKey;
        private readonly string _languageCode;
        private int _sampleRate; // The sample rate of your audio (e.g. 16000)

        public AudioToText(string apiKey, string languageCode = "en-US", int sampleRate = 16000)
        {
            _apiKey = apiKey;
            _languageCode = languageCode;
            this._sampleRate = sampleRate;
        }

        public static async Task PushToTalkTest(string key)
        {
            // Create a new AudioRecorder object with "test.wav" as the output file name
            AudioRecorder recorder = new AudioRecorder("test.wav");

            // Choose a key for Push-to-Talk, for example, the Spacebar
            ConsoleKey pushToTalkKey = ConsoleKey.Spacebar;

            Console.WriteLine($"Press and hold the {pushToTalkKey} key to start recording. Release the key to stop recording.");

            bool isRecording = false;

            while (true)
            {
                if (Keyboard.IsKeyDown(pushToTalkKey) && !isRecording)
                {
                    Console.WriteLine("Recording...");
                    recorder.StartRecording();
                    isRecording = true;
                }
                else if (!Keyboard.IsKeyDown(pushToTalkKey) && isRecording)
                {
                    Console.WriteLine("Recording stopped.");
                    recorder.StopRecording();
                    isRecording = false;

                    // Create a new AudioToText object with your OpenAI API key, language, and sample rate as parameters
                    AudioToText audioToText = new AudioToText(key, "en-US");

                    // Get the text from the audio file
                    Console.WriteLine("Getting text from audio...");
                    string text = await audioToText.TranscribeAudioFileAsync("test.wav");

                    // Display the text
                    Console.WriteLine("Text: " + text);

                    Console.WriteLine($"Press and hold the {pushToTalkKey} key to record again. Press 'Q' to quit.");
                }
                else if (Keyboard.IsKeyDown(ConsoleKey.Q))
                {
                    break; // Quit the application
                }

                await Task.Delay(50); // Polling interval
            }
        }


        public static async Task Test(string key, int recordingLengthInMilliseconds = 10000)
        {
            // Create a new AudioRecorder object with "test.wav" as the output file name
            AudioRecorder recorder = new AudioRecorder("test.wav");

            Console.WriteLine("Recording...");
            recorder.StartRecording();

            // Countdown while recording is in progress
            for (int i = recordingLengthInMilliseconds / 1000; i > 0; i--)
            {
                Console.WriteLine($"Time remaining: {i}");
                Thread.Sleep(1000);
            }            

            Console.WriteLine("Recording stopped.");
            recorder.StopRecording();

            // Create a new AudioToText object with your OpenAI API key, language and sample rate as parameters
            AudioToText audioToText = new AudioToText(key, "en-US");

            Console.WriteLine("Getting text from audio...");
            string text = await audioToText.TranscribeAudioFileAsync("test.wav");

            // Display the text
            Console.WriteLine("Text: " + text);
        }

        private async Task<string> TranscribeAudioFileAsync(string filePath)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            var content = new MultipartFormDataContent();

            var audioContent = new ByteArrayContent(await File.ReadAllBytesAsync(filePath));
            audioContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            content.Add(audioContent, "file", Path.GetFileName(filePath));

            content.Add(new StringContent("whisper-1"), "model");

            var response = await httpClient.PostAsync("https://api.openai.com/v1/audio/transcriptions", content);

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"Received: {response.StatusCode}");
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("JSON Response: " + jsonResponse);

                using JsonDocument document = JsonDocument.Parse(jsonResponse);
                JsonElement root = document.RootElement;
                if (root.TryGetProperty("text", out JsonElement transcriptionElement))
                {
                    return transcriptionElement.GetString();
                }
            }
            else
            {
                Debug.WriteLine("Error: " + response.StatusCode);
            }

            return null;
        }

    }
}