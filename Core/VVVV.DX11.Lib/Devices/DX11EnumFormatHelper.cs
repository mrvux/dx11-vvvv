using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.Direct3D11;
using VVVV.DX11.Internals.Helpers;

namespace FeralTic.DX11
{
    /// <summary>
    /// Simple Null device (Used for shader parsing mainly)
    /// </summary>
    public static class DX11EnumFormatHelper 
    {
        private static DeviceFormatHelper FNullDeviceFormats;
        public static DeviceFormatHelper NullDeviceFormats
        {
            get
            {
                CreateNullDeviceFormat();
                return FNullDeviceFormats;
            }
        }

        public static void CreateNullDeviceFormat()
        {
            if (FNullDeviceFormats == null)
            {
                FNullDeviceFormats = new DeviceFormatHelper(NullRenderDevice.Device);
            }
        }
    }
}
