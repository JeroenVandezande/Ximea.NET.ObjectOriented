using System.Runtime.InteropServices;
using CameraInterface;
using UnitsNet;
using xiApi;
using xiApi.NET;

namespace Ximea.NET.ObjectOriented;

public class XimeaCamera : ICamera, IAsyncDisposable, IDisposable
{
    public void FireManualTrigger()
    {
        _camera.SetParam(PRM.TRG_SOFTWARE, 0);
    }

    /// <summary>
    /// Contains relevant information about a specific XIMEA camera, including its ID, serial number, and model name.
    /// </summary>
    public XimeaCameraInfo XimeaCameraInfo { get; }

    public int Width { get; set; }
    public int Height { get; set; }
    public int XOffset { get; set; }
    public int YOffset { get; set; }

    public (int Min, int Max) WidthRange
    {
        get
        {
            _camera.GetParam(PRM.WIDTH_MIN, out int min);
            _camera.GetParam(PRM.WIDTH_MAX, out int max);
            return (min, max);
        }
    }

    public (int Min, int Max) HeightRange
    {
        get
        {
            _camera.GetParam(PRM.HEIGHT_MIN, out int min);
            _camera.GetParam(PRM.HEIGHT_MAX, out int max);
            return (min, max);
        }
    }

    public (int Min, int Max) XOffsetRange
    {
        get
        {
            _camera.GetParam(PRM.OFFSET_X_MIN, out int min);
            _camera.GetParam(PRM.OFFSET_X_MAX, out int max);
            return (min, max);
        }
    }

    public (int Min, int Max) YOffsetRange
    {
        get
        {
            _camera.GetParam(PRM.OFFSET_Y_MIN, out int min);
            _camera.GetParam(PRM.OFFSET_Y_MAX, out int max);
            return (min, max);
        }
    }

    public Duration Exposure
    {
        get
        {
            _camera.GetParam(PRM.EXPOSURE, out int exposure);
            return Duration.FromMicroseconds(exposure);
        }
        set { _camera.SetParam(PRM.EXPOSURE, (int)value.Microseconds); }
    }

    public Level Gain
    {
        get
        {
            _camera.GetParam(PRM.GAIN, out int gainAsdb);
            return Level.FromDecibels(gainAsdb);
        }
        set { _camera.SetParam(PRM.GAIN, (float)value.Decibels); }
    }

    /// <summary>Timeout for GetImage (default: 10 seconds).</summary>
    public Duration ImageTimeout { get; set; } = Duration.FromSeconds(10);

    public ImageFormat CameraOutputFormat { get; set; }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public event EventHandler<ImageData>? ImageReceived;

    private readonly xiCam _camera = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loopTask;

    private unsafe byte[] _getDataFromXiImg(XI_IMG img)
    {
        var buffer = new byte[img.bp_size];
        Marshal.Copy((IntPtr)img.bp, buffer, 0, buffer.Length);
        return buffer;
    }
    private void RunAsync(CancellationToken ct)
    {
        ImageData result;
        while (!ct.IsCancellationRequested)
        {
            try
            {
                var img = _camera.GetXI_IMG((int)ImageTimeout.Milliseconds);
                int bytesPerPixel = CameraOutputFormat switch
                {
                    ImageFormat.Mono8 => 1,
                    ImageFormat.Mono16 => 2,
                    ImageFormat.Rgb24 => 3,
                    ImageFormat.Rgb32 => 4,
                    _ => 1
                };
                int stride = Width * bytesPerPixel + img.padding_x;
                var buffer = _getDataFromXiImg(img);
                result = new ImageData(buffer, img.width, img.height, stride, CameraOutputFormat);
                ImageReceived?.Invoke(this, result);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (xiExc ex) when (ex.ErrorCode == 10) // Timeout
            {
                //do nothing
            }
        }
    }
}