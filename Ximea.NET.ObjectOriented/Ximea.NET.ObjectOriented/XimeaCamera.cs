using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CameraInterface;
using UnitsNet;
using xiApi;
using xiApi.NET;

namespace Ximea.NET.ObjectOriented;

public class XimeaCamera : ICamera, IAsyncDisposable, IDisposable
{
    /// <summary>
    /// Represents a single XIMEA camera with a user-friendly and structured API for interaction.
    /// </summary>
    /// <remarks>
    /// This class provides methods and properties to configure and control a XIMEA camera device.
    /// It handles connection, configuration of settings such as resolution, exposure, and triggers,
    /// as well as image capture and retrieval.
    /// </remarks>
    /// <exception cref="XimeaCameraException">
    /// Thrown when errors occur during interaction with the camera or its API.
    /// </exception>
    public XimeaCamera(XimeaCameraInfo ximeaCameraInfo)
    {
        _camera.OpenDevice(ximeaCameraInfo.ID);
        XimeaCameraInfo = ximeaCameraInfo;
        InitializeDefaults();
        _loopTask = Task.Run(() => RunAsync(_cts.Token));
    }
    
    private void InitializeDefaults()
    {
        // Unsafe buffer policy for GetImage(byte[] ...)
        _camera.SetParam(PRM.BUFFER_POLICY, BUFF_POLICY.UNSAFE);

        // Reasonable defaults
        _camera.SetParam(PRM.EXPOSURE, 10_000); // 10 ms
        _camera.SetParam(PRM.IMAGE_DATA_FORMAT, IMG_FORMAT.MONO8);
        _camera.SetParam(PRM.TRG_SOURCE, TRG_SOURCE.SOFTWARE);
        _camera.StartAcquisition();
        _acquisitionIsRunning = true;
    }
    
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
        set => _camera.SetParam(PRM.EXPOSURE, (int)value.Microseconds);
    }

    public Level Gain
    {
        get
        {
            _camera.GetParam(PRM.GAIN, out int gainAsdb);
            return Level.FromDecibels(gainAsdb);
        }
        set => _camera.SetParam(PRM.GAIN, (float)value.Decibels);
    }

    /// <summary>Timeout for GetImage (default: 1 seconds).</summary>
    public Duration ImageTimeout { get; set; } = Duration.FromSeconds(1);

    /// <summary>
    /// Specifies the format of the output image data captured by the XIMEA camera.
    /// Defines how pixel data is structured, such as monochrome or RGB formats.
    /// </summary>
    public ImageFormat CameraOutputFormat {
        get => _cameraOutputFormat;
        set
        {
            _camera.SetParam(PRM.IMAGE_DATA_FORMAT, ApiMappings.ToXiImageFormat(value));
            _cameraOutputFormat = value;
        }
    }

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
    private bool _acquisitionIsRunning = false;
    private ImageFormat _cameraOutputFormat;

    private unsafe byte[] _getDataFromXiImg(XI_IMG img)
    {
        var buffer = new byte[img.bp_size];
        Marshal.Copy((IntPtr)img.bp, buffer, 0, buffer.Length);
        return buffer;
    }
    private void RunAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (!_acquisitionIsRunning)
            {
                Thread.Sleep(100);
                continue;
            }

            try
            {
                var img = _camera.GetXI_IMG((int)ImageTimeout.Milliseconds);
                if (img.bp_size == 0)
                {
                    continue;
                }
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
                var result = new ImageData(buffer, img.width, img.height, stride, CameraOutputFormat);
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
            catch (xiExc ex) when (ex.ErrorCode == 45) // Acquisition is stopped
            {
                _acquisitionIsRunning = false;
            }
            catch (xiExc ex) when (ex.ErrorCode == 10) // Timeout
            {
                //do nothing
            }
        }
    }
}