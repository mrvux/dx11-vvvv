using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.DXGI;
using SlimDX.Direct3D11;

using Device = SlimDX.Direct3D11.Device;

namespace VVVV.DX11.Internals.Helpers
{
	public static class FormatHelper
	{
		private static Dictionary<Format, int> formatsizes;
        

		#region Format Sizes
		/// <summary>
		/// List of known format size per element
		/// </summary>
		public static Dictionary<Format, int> FormatSizes
		{
			get
			{
				if (formatsizes == null)
				{
					BuildFormatSizes();
				}
				return formatsizes;
			}
		}
		#endregion

		#region Is Supported
		/// <summary>
		/// Checks if a format is supported for a specific usage
		/// </summary>
		/// <param name="dev">Device to check</param>
		/// <param name="usage">Desired format usage</param>
		/// <param name="format">Desired format</param>
		/// <returns>true if format supported, false otherwise</returns>
		public static bool IsSupported(Device dev, FormatSupport usage, Format format)
		{
			FormatSupport support = dev.CheckFormatSupport(format);
			return (support | usage) == support;
		}
		#endregion

		#region Supported Formats
		/// <summary>
		/// Lists supported DXGI formats for a given usage
		/// </summary>
		/// <param name="dev">Device to check format support for</param>
		/// <param name="usage">Requested Usage</param>
		/// <returns>List of Supported formats</returns>
		public static List<string> SupportedFormats(Device dev, FormatSupport usage)
		{
			List<string> result = new List<string>();
			foreach (string s in Enum.GetNames(typeof(Format)))
			{
				if (IsSupported(dev, usage, (Format)Enum.Parse(typeof(Format),s)))
				{
					result.Add(s);
				}
			}
			return result;
		}
		#endregion




		#region Build Format Sizes
		private static void BuildFormatSizes()
		{
			formatsizes = new Dictionary<Format, int>();

			foreach (string s in Enum.GetNames(typeof(Format)))
			{
				Format fmt = (Format)Enum.Parse(typeof(Format),s);
				if (s.StartsWith("A8_")) { formatsizes.Add(fmt, 1); }
				if (s.StartsWith("B8G8R8A8_")) { formatsizes.Add(fmt, 4); }
				if (s.StartsWith("B8G8R8X8_")) { formatsizes.Add(fmt, 4); }
                if (s.StartsWith("R16_")) { formatsizes.Add(fmt, 2); }
				if (s.StartsWith("R16G16_")) { formatsizes.Add(fmt, 4); }
				if (s.StartsWith("R16G16B16A16_")) { formatsizes.Add(fmt, 8); }
				if (s.StartsWith("R32_")) { formatsizes.Add(fmt, 4); }
				if (s.StartsWith("R32G32_")) { formatsizes.Add(fmt, 8); }
				if (s.StartsWith("R32G32B32_")) { formatsizes.Add(fmt, 12); }
				if (s.StartsWith("R32G32B32A32_")) { formatsizes.Add(fmt, 16); }
				if (s.StartsWith("R8_")) { formatsizes.Add(fmt, 1); }
				if (s.StartsWith("R8G8_")) { formatsizes.Add(fmt, 2); }
				if (s.StartsWith("R8G8B8_")) { formatsizes.Add(fmt, 3); }
				if (s.StartsWith("R8G8B8A8_")) { formatsizes.Add(fmt, 4); }
			}
		}
		#endregion
	}
}
