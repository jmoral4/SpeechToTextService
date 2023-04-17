using System.Text.Json.Serialization;
using NAudio.Wave;
using System;
using System.IO;
using NAudio.Wave.Compression;
using System.Text;
using System.Diagnostics;

namespace SpeechToTextService
{
    public class AudioRecorder
    {
        private WaveInEvent _waveIn;
        private WaveFileWriter _waveFileWriter;
        private string _tempFilePath;
        private int sampleRate; // The sample rate of your audio (e.g. 16000)

        public AudioRecorder(string outputFileName, int sampleRate = 16000)
        {
            this._tempFilePath = outputFileName;
            this.sampleRate = sampleRate;
        }

        public void StartRecording()
        {
            _waveIn = new WaveInEvent { WaveFormat = new WaveFormat(sampleRate, 1) };            
            _waveIn.DataAvailable += OnDataAvailable;
            //_waveIn.RecordingStopped += OnRecordingStopped;
            _tempFilePath = "test.wav";// Path.GetTempFileName(); 
            _waveFileWriter = new WaveFileWriter(_tempFilePath, _waveIn.WaveFormat);
            _waveIn.StartRecording();
        }

        private void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            _waveFileWriter.Write(e.Buffer, 0, e.BytesRecorded);
           // _waveFileWriter.Flush();
        }
        private void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            // Dispose the WaveIn and WaveFileWriter objects
            _waveIn.Dispose();
            _waveFileWriter.Dispose();

            // If there is an exception, display it
            if (e.Exception != null)
            {
                Debug.WriteLine(e.Exception.Message);
            }
        }

        public void StopRecording()
        {
            if (_waveIn != null)
            {
                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
            }

            if (_waveFileWriter != null)
            {
                _waveFileWriter.Dispose();
                _waveFileWriter = null;
            }

            //string text = TranscribeAudioFile(_tempFilePath);
            Debug.WriteLine("Recording Stopped.");
        }
    }
}