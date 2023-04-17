using System.Text.Json;
using System.Net.Http.Headers;

namespace SpeechToTextService
{
    public class AudioToText
    {
        private readonly string _apiBaseUrl = "https://api.openai.com/v1/audio/transcriptions";
        //private readonly string _apiBaseUrl = "https://api.openai.com/v1/whisper/asr";

        private readonly string _apiKey;
        private readonly string _languageCode;
        private int sampleRate; // The sample rate of your audio (e.g. 16000)

        public AudioToText(string apiKey, string languageCode = "en-US", int sampleRate = 16000)
        {
            _apiKey = apiKey;
            _languageCode = languageCode;
            this.sampleRate = sampleRate;
        }        

        public static async Task Test(string key)
        {
            // Create a new AudioRecorder object with "test.wav" as the output file name
            AudioRecorder recorder = new AudioRecorder("test.wav");

            // Start recording
            Console.WriteLine("Recording...");
            recorder.StartRecording();

            Thread.Sleep(10000);

            // Stop recording
            Console.WriteLine("Recording stopped.");
            recorder.StopRecording();

            // Create a new AudioToText object with your OpenAI API key, language and sample rate as parameters
            AudioToText audioToText = new AudioToText(key, "en-US");

            // Get the text from the audio file
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
                Console.WriteLine($"Received: {response.StatusCode}");
                string jsonResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine("JSON Response: " + jsonResponse);

                using JsonDocument document = JsonDocument.Parse(jsonResponse);
                JsonElement root = document.RootElement;
                if (root.TryGetProperty("text", out JsonElement transcriptionElement))
                {
                    return transcriptionElement.GetString();
                }
            }
            else
            {
                Console.WriteLine("Error: " + response.StatusCode);
            }

            return null;
        }


        public async Task<string> GetTextFromAudio(string audioFileName)
        {
            // Create a new HttpClient object with your API key as an authorization header
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            // Create a new MultipartFormDataContent object to hold your audio file and parameters as form data
            MultipartFormDataContent content = new MultipartFormDataContent();

            // Add your audio file as a StreamContent object with "audio" as the name and "audio/wav" as the content type
            StreamContent audioContent = new StreamContent(File.OpenRead(audioFileName));
            audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
            content.Add(audioContent, "audio");

            // Add your language as a StringContent object with "language" as the name
            StringContent languageContent = new StringContent(_languageCode);
            content.Add(languageContent, "language");

            // Add your sample rate as a StringContent object with "sample_rate" as the name
            StringContent sampleRateContent = new StringContent(sampleRate.ToString());
            content.Add(sampleRateContent, "sample_rate");

            // Send a POST request to the OpenAI Whisper
            string response = await (await client.PostAsync("https://api.openai.com/v1/whisper", content)).Content.ReadAsStringAsync();            

            // Parse the response as a JSON object and get the "text" field            
            JsonDocument json = JsonDocument.Parse(response);
            string text = json.RootElement.GetProperty("text").GetString();

            // Return the text
            return text;
        }
    }
}