using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Device = SlimDX.Direct3D11.Device;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using FeralTic.DX11.Utils;


namespace VVVV.DX11.Internals.Helpers
{
    /// <summary>
    /// Caches format supports and create relevant enums
    /// </summary>
	public class DeviceFormatHelper
	{
		private Device device;
		private Dictionary<FormatSupport, List<string>> usageformats = new Dictionary<FormatSupport, List<string>>();

		public DeviceFormatHelper(Device device)
		{
			this.device = device;

            foreach (object o in Enum.GetValues(typeof(FormatSupport)))
            {
                FormatSupport usage = (FormatSupport)o;
                this.RegisterFormats(usage);

                string[] fmts = this.usageformats[usage].ToArray();
                //host.UpdateEnum(this.GetEnumName(usage), fmts[0], fmts);
                EnumManager.UpdateEnum(this.GetEnumName(usage), fmts[0], fmts);
            }
		}


        /// <summary>
        /// Register supported formats for specified usage
        /// </summary>
        /// <param name="usage">Usage to register</param>
		private void RegisterFormats(FormatSupport usage)
		{
			//Shouldn't happen but just in case
			if (!this.usageformats.ContainsKey(usage))
			{
				this.usageformats.Add(usage, FormatHelper.Instance.SupportedFormats(this.device, usage));
			}
		}

        /// <summary>
        /// Get the VVVV enum name for a specified usage
        /// </summary>
        /// <param name="usage">Format usage</param>
        /// <returns>VVVV Enum name</returns>
		public string GetEnumName(FormatSupport usage)
		{
            return usage.ToString() + "_Formats_DX11_"  +device.ComPointer.ToInt32().ToString();
		}

		public List<string> GetAllowedFormats(FormatSupport usage)
		{
			return this.usageformats[usage];
		}

        public static Format GetFormat(string fmt)
        {
            return (Format)Enum.Parse(typeof(Format), fmt);
        }

        public static int GetFormatStrideInBytes(SlimDX.DXGI.Format format, int width)
        {
            SharpDX.DXGI.Format sf = (SharpDX.DXGI.Format)format;
            return SharpDX.DXGI.FormatHelper.SizeOfInBytes(sf) * width;
        }

        public static int GetPixelSizeInBytes(SlimDX.DXGI.Format format)
        {
            SharpDX.DXGI.Format sf = (SharpDX.DXGI.Format)format;
            return SharpDX.DXGI.FormatHelper.SizeOfInBytes(sf);
        }

        public static int ComputeVertexSize(int manualSize, InputElement[] elements)
        {
            if (manualSize > 0)
                return manualSize;

            int size = 0;
            for (int i = 0; i < elements.Length; i++)
            {
                size += GetPixelSizeInBytes(elements[i].Format);
            }
            return size;

        }

    }
}
