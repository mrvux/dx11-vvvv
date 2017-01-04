#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
//using SlimDX.Direct3D9;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
//using VVVV.Utils.SlimDX;

using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing.Imaging;
using System.Diagnostics;
using FeralTic.DX11.Resources;
using VVVV.DX11;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Security.Permissions;
using FeralTic.DX11;

#endregion usings

namespace VVVV.Nodes.Recorder
{
    #region PluginInfo
    [PluginInfo(Name = "Recorder",
                Category = "DX11.Texture2D",
                Version = "",
                Help = "Captures texture to disk in background thread",
                Tags = "",
                AutoEvaluate = true)]
    #endregion PluginInfo
    public class RecordNodeDX11 : IPluginEvaluate, IDX11ResourceDataRetriever, IDisposable
    {
        class Instance : IDisposable
        {
            /* Task
            class Task : IDX11ScheduledTask
            {
                public DX11RenderContext Context
                {
                    get { throw new NotImplementedException(); }
                }

                public void Dispose()
                {
                    throw new NotImplementedException();
                }

                public bool IsDirty
                {
                    get { throw new NotImplementedException(); }
                }

                public void MarkForAbort()
                {
                    throw new NotImplementedException();
                }

                public void Process()
                {
                    throw new NotImplementedException();
                }

                public eDX11SheduleTaskStatus Status
                {
                    get
                    {

                        throw new NotImplementedException();
                    }
                }

                public event TaskStatusChangedDelegate StatusChanged;
            }
            */

            class Saver : IDisposable
            {
                //[DllImport("msvcrt.dll", SetLastError = false)]
                //static extern IntPtr memcpy(IntPtr dest, IntPtr src, int count);

                public enum State
                {
                    Available,
                    Saving
                }

                Thread FThread;
                string FFilename;
                SlimDX.Direct3D11.ImageFileFormat FFormat;

                FeralTic.DX11.DX11RenderContext FContext;
                DX11Texture2D FTexture;

                


                public State CurrentState { get; private set; }

                public bool Available
                {
                    get
                    {
                        return CurrentState == State.Available;
                    }
                }

                public Saver()
                {
                    CurrentState = State.Available;
                }

                public void Save(DX11Texture2D texture, FeralTic.DX11.DX11RenderContext context, string filename, SlimDX.Direct3D11.ImageFileFormat format)
                {
                    if (texture == null)
                    {
                        throw (new Exception("No texture"));
                    }
                    CurrentState = State.Saving;

                    if (FTexture == null || FTexture.Description.Width != texture.Width || FTexture.Description.Height != texture.Height || context != this.FContext)
                        {
                            if (FTexture != null)
                        {
                            FTexture.Dispose();
                        }

                        FContext = context;
                        FTexture = texture;
                    }


                    FContext = context;
                    FTexture = texture;


                    FFilename = filename;
                    FFormat = format;

                    //FThread = new Thread(ThreadedFunction);
                    //FThread.Name = "Recorder";
                    //FThread.Start();

                    //await Task.Run(() =>
                    //   ThreadedFunction()
                    //);


                    ThreadedFunction();
                }



                void ThreadedFunction()
                {

                    //SlimDX.Direct3D11.Device threadedDevice = null;
                    //DeviceContext threadedContext = null;
                    //Texture2D threadedTexture = null;



                    try
                    {
                        //SlimDX.Direct3D11.Device threadedDevice = new SlimDX.Direct3D11.Device(FContext.Adapter/*, DeviceCreationFlags.Debug*/);
                        SlimDX.Direct3D11.Device threadedDevice = new SlimDX.Direct3D11.Device(DriverType.Hardware, DeviceCreationFlags.Debug);

                        DeviceContext  threadedContext = new DeviceContext(threadedDevice);

                        var desc = new Texture2DDescription()
                        {
                            Width = FTexture.Width,
                            Height = FTexture.Height,
                            Format = FTexture.Format,
                            MipLevels = 1,
                            Usage = ResourceUsage.Staging,
                            BindFlags = BindFlags.None,
                            //OptionFlags = ResourceOptionFlags.None,
                            OptionFlags = ResourceOptionFlags.Shared,
                            CpuAccessFlags = CpuAccessFlags.Read,
                            SampleDescription = new SampleDescription(1, 0),
                            ArraySize = 1
                        };

                        //Texture2D threadedTexture = new Texture2D(threadedDevice, desc);
                        
                        //threadedContext.CopyResource(FTexture.Resource, threadedTexture);

                        //Surface s = threadedTexture.AsSurface();
                        //s.Map(SlimDX.DXGI.MapFlags.Read);


                        SlimDX.DXGI.Resource r = new SlimDX.DXGI.Resource(FTexture.Resource);

                        Texture2D tex2 = threadedDevice.OpenSharedResource<Texture2D>(r.SharedHandle);


                        /*
                        bool supportConcurrentRessources;
                        bool supportCommandLists;
                        threadedDevice.CheckThreadingSupport(out supportConcurrentRessources, out supportCommandLists);
                        */



                        var folder = Path.GetDirectoryName(FFilename);
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }
                        
                        //gain rights to write to file
                        (new FileIOPermission(FileIOPermissionAccess.Write, FFilename)).Demand();


                        // lock texture ??
                        //var db = threadedContext.MapSubresource(threadedTexture, 0, 0, MapMode.Read, SlimDX.Direct3D11.MapFlags.None);
                        //var canRead = db.Data.CanRead;


                        // which one is best? is there even adifference?
                        //Result r = Texture2D.SaveTextureToFile(threadedContext, threadedTexture, FFormat, FFilename);

                        //TextureLoader.SaveToFile(threadedContext, threadedTexture, FFilename, eImageFormat.Png);
                        SlimDX.Direct3D11.Resource.SaveTextureToFile(threadedContext, tex2, FFormat, FFilename);
                        //Result r = SlimDX.Direct3D11.Texture2D.ToFile(threadedContext, threadedTexture, FFormat, FFilename);

                        /*
                        MemoryStream ms = new MemoryStream();
                        Texture2D.ToStream(threadedContext.CurrentDeviceContext, FStaging.Resource, ImageFileFormat.Png, ms);
                        ms.Seek(0, SeekOrigin.Begin);

                        Bitmap bm = new Bitmap(ms, PixelFormat.Format64bppArgb);
                        bm.Save(FFilename, System.Drawing.Imaging.ImageFormat.Png);
                        */

                        // unlock texture
                        //threadedContext.UnmapSubresource(threadedTexture, 0);


                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.Print(e.Message);
                    }
                    finally
                    {
                        //if (threadedContext != null)
                        //{
                        //    threadedContext.Dispose();

                        //}

                        //if (threadedTexture != null)
                        //{
                        //    threadedTexture.Dispose();
                        //    threadedTexture = null;
                        //}
                        CurrentState = State.Available;
                    }
                }

                public void Dispose()
                {
                    if (FThread != null)
                    {
                        while (CurrentState != State.Available) 
                        {
                            Thread.Sleep(1);
                        }
                        FThread.Join();
                    }
                    //if (threadedTexture != null)
                    //{
                    //    threadedTexture.Dispose();
                    //    threadedTexture = null;
                    //}
                }
            }

         
            List<Saver> FSavers = new List<Saver>();

            public Instance()
            {
            }

            public  void WriteImage(DX11Resource<DX11Texture2D> resource, FeralTic.DX11.DX11RenderContext context, string filename, SlimDX.Direct3D11.ImageFileFormat format)
            {
                var saver = GetAvailableSaver();
                saver.Save(resource[context], context, filename, format);
            }

            Saver GetAvailableSaver()
            {
                foreach (var saver in FSavers)
                {
                    if (saver.Available)
                    {
                        return saver;
                    }
                }

                // creates a new saver everytime (for now)
                var newSaver = new Saver();
                FSavers.Add(newSaver);
                return newSaver;
            }

            public void CleanCompleteSavers(int minimumSavers)
            {
                HashSet<Saver> toRemove = new HashSet<Saver>();

                foreach (var saver in FSavers)
                {
                    if (FSavers.Count <= minimumSavers)
                    {
                        return;
                    }
                    if (saver.Available)
                    {
                        saver.Dispose();
                        toRemove.Add(saver);
                    }
                }

                FSavers.RemoveAll(saver => toRemove.Contains(saver));
            }

            public void Dispose()
            {
            }
        }
        
        #region fields & pins
        #pragma warning disable 0649
        [Input("Input")]
        Pin<DX11Resource<DX11Texture2D>> FInTexture;

        [Input("Minimum Savers", DefaultValue = 0)]
        ISpread<int> FInMinimumSavers;

        [Input("Format")]
        protected ISpread<SlimDX.Direct3D11.ImageFileFormat> FInFormat;

        [Input("Filename", StringType = StringType.Filename)]
        ISpread<string> FInFilename;

        [Input("Write")]
        ISpread<bool> FInWrite;

        [Output("Status")]
        ISpread<string> FOutStatus;

        [Import()]
        protected IPluginHost FHost;

        [Import]
        public ILogger FLogger;

        [Import]
        IHDEHost FHDEHost;

        Spread<Instance> FInstances = new Spread<Instance>();

        #pragma warning restore 0649
        #endregion fields & pins

        // import host and hand it to base constructor
        [ImportingConstructor()]
        public RecordNodeDX11(IPluginHost host)
        {
        }

        public void Evaluate(int SpreadMax)
        {
            try
            {
                if (FInTexture.PluginIO.IsConnected)
                {
                    if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                    if (this.AssignedContext == null) { return; }

                    var device = this.AssignedContext.Device;
                    var context = this.AssignedContext;

                    FInstances.ResizeAndDispose(SpreadMax);
                }
            }
            catch (Exception e)
            {
                FLogger.Log(e);
            }

            FOutStatus.SliceCount = SpreadMax;
            for (int i = 0; i < SpreadMax; i++)
            {
                if (FInWrite[i])
                {
                    try
                    {
                        FInstances[i].WriteImage(FInTexture[i], this.AssignedContext, FInFilename[i], FInFormat[i]);
                        FOutStatus[i] = "OK";
                    }
                    catch (Exception e)
                    {
                        FOutStatus[i] = e.Message;
                    }

                }
            }
        }

        public FeralTic.DX11.DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        public void Dispose()
        {
            FInstances.ResizeAndDispose(0);
        }
    }

    
}
