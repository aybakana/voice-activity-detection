using System.Collections.Concurrent;

public class VoiceDetectionSystem
{
    private AudioCapture _capture;
    private FFTProcessor _fft;
    private VoiceActivityDetector _vad;
    private BlockingCollection<short[]> _audioQueue = new BlockingCollection<short[]>();
    private CancellationTokenSource _cts = new CancellationTokenSource();

    public VoiceDetectionSystem()
    {
        _capture = new AudioCapture();
        _fft = new FFTProcessor { Points = 256 };
        _vad = new VoiceActivityDetector();

        _capture.OnFrameCaptured += (sender, frame) =>
        {
            if (!_audioQueue.IsAddingCompleted)
            {
                _audioQueue.Add(frame);
            }
        };
    }

    public async Task Run()
    {
        try
        {
            await _capture.StartCapture();

            // Process audio data
            await Task.Run(async () =>
            {
                foreach (var audioFrame in _audioQueue.GetConsumingEnumerable(_cts.Token))
                {
                    try
                    {
                        var spectrum = _fft.Process(audioFrame);
                        var features = _vad.ExtractFeatures(spectrum);
                        bool isVoiceDetected = _vad.IsVoiceDetected(features);
                    }
                    catch (OperationCanceledException)
                    {
                        // Task cancelled, exit loop
                        break;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error processing audio: " + ex.Message);
                    }
                }
            }, _cts.Token);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error running voice detection system: " + ex.Message);
        }
        finally
        {
            await _capture.StopCapture();
            _audioQueue.CompleteAdding();
        }
    }

    public void Stop()
    {
        _cts.Cancel();
    }
}