using CameraInterface;
using UnitsNet;
using xiApi.NET;

namespace Ximea.NET.ObjectOriented;

public class Camera: ICamera
{
    public void FireManualTrigger()
    {
       
    }

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
        set
        {
            _camera.SetParam(PRM.EXPOSURE, (int)value.Microseconds);
        } 
    }

    public Level Gain
    {
        get
        {
            _camera.GetParam(PRM.GAIN, out int gainAsdb);
            return Level.FromDecibels(gainAsdb);
        }
        set
        {
            _camera.SetParam(PRM.GAIN, (float)value.Decibels);
        }
    }
    
    public ImageFormat CameraOutputFormat { get; set; }
    public event EventHandler<ImageData>? ImageReceived;
    
    private readonly xiCam _camera = new();
}