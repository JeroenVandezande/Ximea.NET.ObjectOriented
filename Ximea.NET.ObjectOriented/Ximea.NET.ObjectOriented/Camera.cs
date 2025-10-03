using CameraInterface;
using UnitsNet;

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
    public (int Min, int Max) WidthRange { get; }
    public (int Min, int Max) HeightRange { get; }
    public (int Min, int Max) XOffsetRange { get; }
    public (int Min, int Max) YOffsetRange { get; }
    public Duration Exposure { get; set; }
    public ImageFormat CameraOutputFormat { get; set; }
    public event EventHandler<ImageData>? ImageReceived;
}