using xiApi.NET;

namespace Ximea.NET.ObjectOriented;

/// <summary>
/// Represents information about a XIMEA camera device.
/// </summary>
/// <remarks>
/// This class encapsulates details about connected XIMEA cameras, such as their serial number,
/// model name, device type, and connection paths. It is useful for identifying and managing
/// multiple cameras when they are connected to the system.
/// </remarks>
public class XimeaCameraInfo
{
    /// <summary>
    /// Gets or sets the unique identifier for the XIMEA camera device.
    /// </summary>
    /// <remarks>
    /// The ID property represents the index or identifier assigned to the camera device within the
    /// system. It is used to open and manage a specific camera instance, particularly in scenarios
    /// where multiple cameras are connected. This ID is primarily intended for internal use within
    /// the library to interact with the appropriate camera hardware.
    /// </remarks>
    public int ID { get; set; }

    /// <summary>
    /// Gets or sets the serial number of the XIMEA camera device.
    /// </summary>
    /// <remarks>
    /// The SerialNumber property represents the unique hardware identifier assigned to the camera
    /// by the manufacturer. It is useful for distinguishing between multiple connected cameras and
    /// ensuring that operations are performed on the intended device.
    /// </remarks>
    public string SerialNumber { get; set; }

    /// <summary>
    /// Gets or sets the model name of the XIMEA camera device.
    /// </summary>
    /// <remarks>
    /// The ModelName property represents the specific model designation of the XIMEA camera hardware.
    /// It provides a way to identify the type of camera being used, which can be helpful for
    /// distinguishing between different models in a multi-camera setup or when accessing
    /// model-specific features and capabilities.
    /// </remarks>
    public string ModelName { get; set; }

    /// <summary>
    /// Gets or sets the instance path of the XIMEA camera device.
    /// </summary>
    /// <remarks>
    /// The InstancePath property provides the system's unique path information for the camera device
    /// as identified by the operating system. This property is useful for advanced scenarios where
    /// direct access or specific identification of the device path is needed, such as debugging
    /// or troubleshooting connection issues with the camera hardware.
    /// </remarks>
    public string InstancePath { get; set; }

    /// <summary>
    /// Gets or sets the location path of the XIMEA camera device.
    /// </summary>
    /// <remarks>
    /// The LocationPath property specifies the physical or logical connection path
    /// of the camera device. This path provides details about how the camera is connected
    /// within the system, such as through a USB or PCIe interface. It is particularly
    /// helpful for low-level device management and troubleshooting connection issues.
    /// This property is typically populated by querying the camera hardware during
    /// device enumeration.
    /// </remarks>
    public string LocationPath { get; set; }

    /// <summary>
    /// Gets or sets the type of the XIMEA camera device.
    /// </summary>
    /// <remarks>
    /// The DeviceType property provides information about the specific type or category
    /// of the connected XIMEA camera device. This can be useful for distinguishing
    /// between different camera models or hardware variations within a system that
    /// incorporates multiple devices. It is typically retrieved through the camera
    /// enumeration process and reflects the type as defined by the XIMEA SDK.
    /// </remarks>
    public string DeviceType { get; set; }

    /// <summary>
    /// Gets or sets the user-defined identifier for the XIMEA camera device.
    /// </summary>
    /// <remarks>
    /// The UserID property allows the user to assign a custom identifier to the camera device,
    /// making it easier to reference and manage specific cameras in scenarios involving multiple
    /// connected devices. This value is typically defined by the user and provides a more
    /// descriptive or human-readable way of identifying the camera compared to system-generated IDs.
    /// </remarks>
    public string UserID { get; set; }

    /// <summary>
    /// Retrieves a collection of all connected XIMEA cameras along with their details.
    /// </summary>
    /// <returns>
    /// A read-only span containing information about each connected XIMEA camera,
    /// including details such as serial number, model name, instance path, location path,
    /// device type, and user ID.
    /// </returns>
    public static ReadOnlySpan<XimeaCameraInfo> GetAllCameras()
    {
        var camEnum = new xiCamEnum();
        var numberOfCameras = camEnum.ReEnumerate();
        var result = new XimeaCameraInfo[numberOfCameras];
        for (int i = 0; i < numberOfCameras; i++)
        {
            result[i] = new XimeaCameraInfo
            {
                ID = i,
                SerialNumber = camEnum.GetSerialNumById(i),
                ModelName = camEnum.GetDevNameById(i),
                InstancePath = camEnum.GetInstancePathById(i),
                LocationPath = camEnum.GetLocationPathById(i),
                DeviceType = camEnum.GetDeviceTypeById(i),
                UserID = camEnum.GetDeviceUserIdById(i)
            };
        }
        return result;
    }
}