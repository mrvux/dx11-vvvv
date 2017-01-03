#region usings
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SlimDX.DXGI;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.AForge.Video;
using VVVV.AForge.Video.DirectShow;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.DX11.Nodes.DShow
{
    #region VideoInThread
    public class VideoInThread : IDisposable
	{
		private VideoCaptureDevice videoSource;
		
		private bool isRunning;
		
		private IntPtr buffer0 = IntPtr.Zero;
		private IntPtr buffer1 = IntPtr.Zero;
		
		public IntPtr frontBuffer { get { return buffer1; } }
		
		public event EventHandler OnStartCapture;
		public event EventHandler OnFrameReady;
		public event EventHandler OnStopped;
		
		[DllImport("msvcrt.dll", EntryPoint = "memcpy", CallingConvention = CallingConvention.Cdecl, SetLastError = false)]
		public unsafe static extern IntPtr memcpy(byte* dest, byte* src, int count);
		
		public VideoInThread(VideoCaptureDevice videoSource)
		{
			this.videoSource = videoSource;
		}
		
		public int GetWidth()
		{
			return videoSource.VideoResolution.FrameSize.Width;
		}
		
		public int GetHeight()
		{
			return videoSource.VideoResolution.FrameSize.Height;
		}
		
		public int GetFramerate()
		{
			return videoSource.Framerate;
		}

		public int GetSize()
		{
			return videoSource.VideoResolution.FrameSize.Width * videoSource.VideoResolution.FrameSize.Height * 4;
		}
		
		public bool IsRunning()
		{
			return isRunning;
		}
		
		public VideoCapabilities GetVideoCapabilities()
		{
			return videoSource.VideoResolution;
		}
		
		public void Start()
		{
			buffer0 = Marshal.AllocCoTaskMem(GetSize());
			buffer1 = Marshal.AllocCoTaskMem(GetSize());
			
			videoSource.VideoSourceError += VideoSourceError;
			videoSource.UpdateBuffer += UpdateBuffer;
			
			videoSource.Start();
			
			if (OnStartCapture != null)
			{
				OnStartCapture(this, new EventArgs());
			}
			
			isRunning = true;
		}
		
		private void UpdateBuffer(object sender, UpdateBufferEventArgs eventArgs)
		{
			IntPtr buffer = eventArgs.Buffer;
			int bufferLen = eventArgs.BufferLen;
			int width = eventArgs.Width;
			int height = eventArgs.Height;
			
			int stride = width * 4;
			unsafe
			{
				byte* dst = (byte*) buffer0.ToPointer( ) + stride * ( height - 1 ); ;
				byte* src = (byte*) buffer.ToPointer( );
				
				for ( int y = 0; y < height; y++ )
				{
					memcpy( dst, src, stride );
					dst -= stride;
					src += stride;
				}
			}
			
			// swap
			IntPtr temp = buffer0;
			buffer0 = buffer1;
			buffer1 = temp;
			
			if (OnFrameReady != null)
			{
				OnFrameReady(this, new EventArgs());
			}
		}
		
		private void VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
		{
			Dispose();
		}
		
		public void ShowProperties()
		{
			videoSource.DisplayPropertyPage(IntPtr.Zero);
		}
		
		public void Stop()
		{
			videoSource.VideoSourceError -= VideoSourceError;
			videoSource.UpdateBuffer -= UpdateBuffer;
			
			videoSource.SignalToStop();
			videoSource.WaitForStop();
			
			Dispose();
			
			if (OnStopped != null)
			{
				OnStopped(this, new EventArgs());
			}
		}
		
		public void Dispose()
		{
			Marshal.FreeCoTaskMem(buffer0);
			Marshal.FreeCoTaskMem(buffer1);
			
			isRunning = false;
		}
	}
	#endregion VideoInThread
	 
	#region PluginInfo
	[PluginInfo(Name = "VideoIn", AutoEvaluate = true, Category = "DX11", Version = "DShow",  Author = "vux,gumilastik", Help = "")]
	#endregion PluginInfo
	  
	public class VideoInNode : IPluginEvaluate, IDX11ResourceHost, IDisposable, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Device", EnumName = "VideoIn_Device", Order = 0)]
		public IDiffSpread<EnumEntry> FInDevice;
		
		public List<IIOContainer<Pin<EnumEntry>>> FInputs = new List<IIOContainer<Pin<EnumEntry>>>();
		
		[Input("Properties", IsBang = true, Order = 5)]
		public IDiffSpread<bool> FInProperties;
		
		[Input("Update", IsBang = true, Order = 6)]
		public IDiffSpread<bool> FInUpdate;
		
		[Input("Enable", Order = 7)]
		public IDiffSpread<bool> FInEnable;
		
		[Output("Texture Out", IsSingle = true)]
		protected Pin<DX11Resource<DX11DynamicTexture2D>> FTextureOutput;
		
		[Output("Width")]
		protected ISpread<int> FOutWidth;
		
		[Output("Height")]
		protected ISpread<int> FOutHeight;
		
		[Output("Framerate (Average)")]
		protected ISpread<int> FOutFramerate;
		
		[Output("IsUpdated")]
		public ISpread<bool> FOutIsNewFrame;
		
		[Output("IsValid")]
		public ISpread<bool> FOutIsValid;
		
		[Import()]
		public IIOFactory FIOFactory;
		
		[Import()]
        protected IPluginHost FHost;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		private string enumDevice;
		private string enumVideoFormat;
		private string enumResolution;
		private string enumFramerate;
		
		private bool invalidateTexture;
		private bool resetTexture;
        private bool isNewFrame;
		
		private VideoInThread videoin;
		private VideoCaptureDevice videoSource;
		
		#region pin management
		public void AddInputEnum(List<IIOContainer<Pin<EnumEntry>>> FInput, int pinOrder, string pinName, string enumName, string enumDefault, string[] enums)
		{
			EnumManager.UpdateEnum(enumName, enumDefault, enums);
			
			InputAttribute ioAttribute = new InputAttribute(enumName);
			ioAttribute.Name = pinName;
			ioAttribute.EnumName = enumName;
			ioAttribute.Order = pinOrder;
			FInput.Add(FIOFactory.CreateIOContainer<Pin<EnumEntry>>(ioAttribute));
		}
		
		public void OnImportsSatisfied()
		{
			string nodePath;
			FHost.GetNodePath(false, out nodePath);
			
			enumDevice = "VideoIn_Device";
			enumVideoFormat = string.Format("VideoIn_VideoFormat_{0}", nodePath);
			enumResolution = string.Format("VideoIn_Resolution_{0}", nodePath);
			enumFramerate = string.Format("VideoIn_Framerate_{0}", nodePath);
			
			EnumManager.UpdateEnum(enumDevice, "None", new string[] { "None" });
			
			AddInputEnum(FInputs, 1, "Video Format", enumVideoFormat, "Default", new string[] { "Default" });
			AddInputEnum(FInputs, 2, "Resolution", enumResolution, "Default", new string[] { "Default" });
			AddInputEnum(FInputs, 3, "Framerate", enumFramerate, "Default", new string[] { "Default" });
		}
		#endregion
		
		public bool IsEnabled
		{
			get { return true; }
		}
		
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (this.FTextureOutput[0] == null)
			{
				this.FTextureOutput[0] = new DX11Resource<DX11DynamicTexture2D>();
			}

            if (videoin != null)
			{
				FOutWidth.SliceCount = 1;
				FOutHeight.SliceCount = 1;
				FOutFramerate.SliceCount = 1;
				
				FOutWidth[0] = videoin.GetWidth();
				FOutHeight[0] = videoin.GetHeight();
				FOutFramerate[0] = videoin.GetFramerate();
			}
			else
			{
				FOutWidth.SliceCount = 0;
				FOutHeight.SliceCount = 0;
				FOutFramerate.SliceCount = 0;
			}
			
			if (FInUpdate.IsChanged && FInUpdate[0])
			{
				UpdateDevice();
			}
			
			if (FInDevice.IsChanged || FInputs[0].IOObject.IsChanged || FInputs[1].IOObject.IsChanged || FInputs[2].IOObject.IsChanged)
			{
				if(UpdateDevice())
				{
					if (FInEnable[0])
					{
						StopCapture();
						StartCapture();
					}
				}
			}
			
			if (FInEnable.IsChanged || (FInEnable[0] && videoin == null))
			{
				if (FInEnable[0])
				{
					StartCapture();
				}
				else
				{
					StopCapture();
				}
			}
			
			if (FInProperties.IsChanged && FInProperties[0])
			{
				if (videoin != null && videoin.IsRunning())
				{
					videoin.ShowProperties();
				}
			}
			
            FOutIsNewFrame[0] = isNewFrame;
            isNewFrame = false;
			FOutIsValid[0] = (videoin != null && videoin.IsRunning());
		}
		
		private bool UpdateDevice()
		{
			FilterInfoCollection videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
			
			// update devices
			List<string> deviceName = videoDevices.Cast<FilterInfo>().Select(x => x.Name).ToList();
			EnumManager.UpdateEnum(enumDevice, FInDevice[0].Name, deviceName.Count > 0 ? deviceName.ToArray() : new string[] { "None" });
			
			List<string> videoFormatName = new List<string>();
			List<string> resolutionName = new List<string>();
			List<string> framerateName = new List<string>();
			
			bool valid = false;
			
			FilterInfo filterInfo = videoDevices.Cast<FilterInfo>().ToList().Find(x => x.Name.Equals(FInDevice[0].Name));
			if (filterInfo != null)
			{
				videoSource = new VideoCaptureDevice(filterInfo.MonikerString);
				
				// update formats
				videoFormatName = videoSource.VideoCapabilities.Select(x=>x.MediaType).Distinct().ToList();
				//videoFormatName.Insert(0, "Default");
				
				List<VideoCapabilities> videoModeInfo = videoSource.VideoCapabilities.ToList().FindAll(y => y.MediaType.Equals(FInputs[0].IOObject[0].Name)).ToList();
				if (videoModeInfo.Count > 0)
				{
					// update resolutions
					List<KeyValuePair<Tuple<int, int>, string>> resolution = videoModeInfo.Select(x => new KeyValuePair<Tuple<int, int>, string>(new Tuple<int, int>(x.FrameSize.Width, x.FrameSize.Height), x.FrameSize.Width + "x" + x.FrameSize.Height)).ToList();
					resolutionName = resolution.Distinct().OrderBy(x => x.Key).Reverse().ToList().Select(x => x.Value).ToList();
					//resolutionName.Insert(0, "Default");
					
					List<int> framerate = new List<int>();
					
					videoModeInfo = videoModeInfo.FindAll(x => (x.FrameSize.Width + "x" + x.FrameSize.Height).Equals(FInputs[1].IOObject[0].Name));
					if (videoModeInfo.Count > 0)
					{
						int res = 0;
						int.TryParse(FInputs[2].IOObject[0].Name, out res);
						
						videoModeInfo.ForEach(delegate(VideoCapabilities vcap)
						{
							// forming fps
							int fps = vcap.MinimumFrameRate;
							int fpsMax = vcap.MaximumFrameRate;
							
							while (fps <= fpsMax) // step by 5 fps
							{
								if(!framerate.Contains(fps)) framerate.Add(fps);
								
								if (fps == fpsMax) break;
								
								if (fps % 5 != 0) fps = 5 * (int)Math.Ceiling(((double)fps) / 5.0); // starting from 5
								else fps += 5;
								
								if (fps > fpsMax) fps = fpsMax;
							}
							
							if (framerate.Contains(res))
							{
								videoSource.VideoResolution = vcap;
								videoSource.Framerate = res;
								
								valid  = true;
							}
						});
						
						framerate.Sort();
						framerate.Reverse();
						
						framerateName = framerate.ConvertAll(x => x.ToString()).ToList();
						//framerateName.Insert(0, "Default");
					}
				}
			}
			
			EnumManager.UpdateEnum(enumVideoFormat, FInputs[0].IOObject[0].Name, videoFormatName.Count > 0 ? videoFormatName.ToArray() : new string[] { "Default" });
			EnumManager.UpdateEnum(enumResolution, FInputs[1].IOObject[0].Name, resolutionName.Count > 0 ? resolutionName.ToArray() : new string[] { "Default" });
			EnumManager.UpdateEnum(enumFramerate, FInputs[2].IOObject[0].Name, framerateName.Count > 0 ? framerateName.ToArray() : new string[] { "Default" });
			
			return valid;
		}
		
		private void StartCapture()
		{
			if (videoSource != null && videoSource.VideoResolution != null && videoSource.Framerate != 0)
			{
				videoin = new VideoInThread(videoSource);
				videoin.OnStartCapture += videoin_OnStartCapture;
				videoin.OnFrameReady += videoin_OnFrameReady;
				videoin.OnStopped += videoin_OnStopped;
				videoin.Start();
			}
		}
		
		void videoin_OnStopped(object sender, EventArgs e)
		{
            //videoSource = null;
		}
		
		void videoin_OnStartCapture(object sender, EventArgs e)
		{
			resetTexture = true;
		}
		
		void videoin_OnFrameReady(object sender, EventArgs e)
		{
			invalidateTexture = true;
            isNewFrame = true;
		}
		
		private void StopCapture()
		{
			if (videoin != null)
			{
				videoin.Stop();
			}
		}
		
		public void Update(DX11RenderContext context)
		{
			if (videoin != null && videoin.IsRunning())
			{
				if (this.resetTexture || !this.FTextureOutput[0].Contains(context))
				{
					if (this.FTextureOutput[0].Contains(context))
					{
						this.FTextureOutput[0].Dispose(context);
					}
					
					this.FTextureOutput[0][context] = new DX11DynamicTexture2D(context, this.videoin.GetWidth(), this.videoin.GetHeight(), SlimDX.DXGI.Format.B8G8R8A8_UNorm);
					this.resetTexture = false;
				}
				else if (this.invalidateTexture)
				{
					if (this.videoin.GetWidth() * 4 == this.FTextureOutput[0][context].GetRowPitch())
					{
						this.FTextureOutput[0][context].WriteData(this.videoin.frontBuffer, this.videoin.GetSize());
					}
					else
					{
						this.FTextureOutput[0][context].WriteDataPitch(this.videoin.frontBuffer, this.videoin.GetSize());
					}
				}
			}
			
			FOutIsNewFrame[0] = invalidateTexture;
			invalidateTexture = false;
		}
		
		
		public void Destroy(DX11RenderContext context, bool force)
		{
            this.FTextureOutput.SafeDisposeAll(context);
		}
		
		public void Dispose()
		{
			StopCapture();

            this.FTextureOutput.SafeDisposeAll();
        }
	}
}
