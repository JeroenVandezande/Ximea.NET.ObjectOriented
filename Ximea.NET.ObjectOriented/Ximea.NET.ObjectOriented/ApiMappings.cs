using CameraInterface;
using xiApi.NET;

namespace Ximea.NET.ObjectOriented;

internal static class ApiMappings
{
    public static int ToXiImageFormat(ImageFormat f) => f switch
    {
        ImageFormat.Mono8     => IMG_FORMAT.MONO8,
        ImageFormat.Mono16    => IMG_FORMAT.MONO16,
        ImageFormat.Rgb24     => IMG_FORMAT.RGB24,
        ImageFormat.Rgb32     => IMG_FORMAT.RGB32,
        _ => IMG_FORMAT.MONO8
    };

}