using NAudio.Wave;
using System;
using System.Threading.Tasks;

public class AudioCapture
{
    public int SamplingRate { get; set; } = 44100;
    public int FrameSizeMs { get; set; } = 10; // 10ms
    public short[] Buffer { get; private set; }
    public event EventHandler<short[]> OnFrameCaptured;

    private WaveInEvent waveIn;
    private int bufferSize;

    public AudioCapture()
    {
        WaveInCapabilities deviceInfo = WaveInEvent.GetCapabilities(0);
        Console.WriteLine($"Device: {deviceInfo.ProductName}, Channels: {deviceInfo.Channels}");

        bufferSize = 256;
        Buffer = new short[bufferSize];

        waveIn = new WaveInEvent
        {
            WaveFormat = new WaveFormat(SamplingRate, 1), // 1 channel (mono)
            BufferMilliseconds = FrameSizeMs,
            DeviceNumber = 0 // Default audio input device
        };

        waveIn.DataAvailable += WaveIn_DataAvailable;
        waveIn.RecordingStopped += WaveIn_RecordingStopped;
    }

    private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
    {
        // Convert byte array to short array
        int bytesRecorded = e.BytesRecorded;
        int shortsRecorded = bytesRecorded / 2;

        if (shortsRecorded > bufferSize)
        {
            shortsRecorded = bufferSize; // Trim if necessary
        }

        Buffer = new short[shortsRecorded];
        Buffer.Initialize();

        for (int i = 0; i < shortsRecorded; i++)
        {
            Buffer[i] = (short)(e.Buffer[i * 2] | (e.Buffer[i * 2 + 1] << 8));
        }

        OnFrameCaptured?.Invoke(this, Buffer);
    }

    private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            Console.WriteLine("Recording stopped due to error: " + e.Exception.Message);
        }
    }

    public Task StartCapture()
    {
        return Task.Run(() =>
        {
            try
            {
                waveIn.StartRecording();
                Console.WriteLine("Audio capture started.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error starting audio capture: " + ex.Message);
            }
        });
    }

    public Task StopCapture()
    {
        return Task.Run(() =>
        {
            try
            {
                waveIn.StopRecording();
                Console.WriteLine("Audio capture stopped.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error stopping audio capture: " + ex.Message);
            }
        });
    }
}