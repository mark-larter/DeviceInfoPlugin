using Plugin.DeviceInfo.Abstractions;
using System;
using Windows.System.Profile;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.Foundation.Metadata;
using Windows.ApplicationModel.Resources.Core;
using Windows.UI.ViewManagement;

namespace Plugin.DeviceInfo
{
    /// <summary>
    /// Implementation for DeviceInfo
    /// </summary>
    public class DeviceInfoImplementation : IDeviceInfo
    {

        EasClientDeviceInformation deviceInfo;
        public DeviceInfoImplementation()
        {
            deviceInfo = new EasClientDeviceInformation();
        }
        /// <inheritdoc/>
        public string GenerateAppId(bool usingPhoneId = false, string prefix = null, string suffix = null)
        {
            var appId = "";

            if (!string.IsNullOrEmpty(prefix))
                appId += prefix;

            appId += Guid.NewGuid().ToString();

            if (usingPhoneId)
                appId += Id;

            if (!string.IsNullOrEmpty(suffix))
                appId += suffix;

            return appId;
        }
        /// <inheritdoc/>
        public string Id
        {
            get
            {
                if (ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
                {
                    var token = HardwareIdentification.GetPackageSpecificToken(null);
                    var hardwareId = token.Id;
                    var dataReader = Windows.Storage.Streams.DataReader.FromBuffer(hardwareId);

                    var bytes = new byte[hardwareId.Length];
                    dataReader.ReadBytes(bytes);

                    return Convert.ToBase64String(bytes);
                }
                else
                {
                    return "unsupported";
                }
            }
        }
        /// <inheritdoc/>
        public string Model
        {
            get { return deviceInfo.SystemProductName; }
        }
        /// <inheritdoc/>
        public string Version
        {
            get
            {
                var sv = AnalyticsInfo.VersionInfo.DeviceFamilyVersion;
                try
                {
                    
                    var v = ulong.Parse(sv);
                    var v1 = (v & 0xFFFF000000000000L) >> 48;
                    var v2 = (v & 0x0000FFFF00000000L) >> 32;
                    var v3 = (v & 0x00000000FFFF0000L) >> 16;
                    var v4 = v & 0x000000000000FFFFL;
                    return  $"{v1}.{v2}.{v3}.{v4}";
                }
                catch { }

                return sv;
            }
        }
        /// <inheritdoc/>
        public Abstractions.Platform Platform
        {
            get
            {
                switch (AnalyticsInfo.VersionInfo.DeviceFamily)
                {
                    case "Windows.Mobile":
                        return Abstractions.Platform.WindowsPhone;
                    case "Windows.Desktop":
                        return UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse
                            ? Abstractions.Platform.Windows
                            : Abstractions.Platform.WindowsTablet;
                    case "Windows.IoT":
                        return Abstractions.Platform.IoT;
                    case "Windows.Xbox":
                        return Abstractions.Platform.Xbox;
                    case "Windows.Team":
                        return Abstractions.Platform.SurfaceHub;
                    default:
                        return Abstractions.Platform.Windows;
                }
            }
        }

        /// <inheritdoc/>
        public Version VersionNumber
        {
            get
            {
                try
                {
                    return new Version(Version);
                }
                catch
                {
                    return new Version(10, 0);
                }
            }
        }

        /// <inheritdoc/>
        public Idiom Idiom
        {
            get
            {
                switch (Platform)
                {
                    case Abstractions.Platform.Windows:
                        return Idiom.Desktop;
                    case Abstractions.Platform.WindowsPhone:
                        return Idiom.Phone;
                    case Abstractions.Platform.WindowsTablet:
                        return Idiom.Tablet;
                    default:
                        return Idiom.Unknown;

                }
            }
        }

        /// <inheritdoc/>
        public bool IsSimulated
        {
            get { throw new NotImplementedException(); }
        }
    }
}
