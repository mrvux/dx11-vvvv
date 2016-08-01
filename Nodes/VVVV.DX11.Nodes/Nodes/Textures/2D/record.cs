#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D9;
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
                Texture2D FBackSurface;
                FeralTic.DX11.DX11RenderContext FContext;

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

                    if (FBackSurface == null || FBackSurface.Description.Width != texture.Width || FBackSurface.Description.Height != texture.Height || context != FContext)
                    {
                        if (FBackSurface != null)
                        {
                            FBackSurface.Dispose();
                        }
                        var description = new Texture2DDescription()
                        {
                            Width = texture.Width,
                            Height = texture.Height,
                            Format = texture.Format,
                            MipLevels = 1,
                            Usage = ResourceUsage.Staging,
                            BindFlags = BindFlags.None,
                            CpuAccessFlags = CpuAccessFlags.Read,
                            SampleDescription = new SampleDescription(1, 0),
                            ArraySize = 1
                        };
                        FBackSurface = new Texture2D(context.Device, description);
                        FContext = context;
                    }

                    context.CurrentDeviceContext.CopyResource(texture.Resource, FBackSurface);
                    FFilename = filename;
                    FFormat = format;


                    FThread = new Thread(ThreadedFunction);
                    FThread.Name = "Recorder";
                    FThread.Start();


                    //await System.Threading.Tasks.Task.Run( () => ThreadedFunction() );

                    //ThreadedFunction();
                }

                void ThreadedFunction()
                {
                    //DeviceContext threadContext = null;
                    DX11RenderContext threadContext = null;
                    try
                    {
                        //threadContext = new DeviceContext(FContext.Device);
                        threadContext = new DX11RenderContext(FContext.Device);
                        threadContext.Initialize();

                        var folder = Path.GetDirectoryName(FFilename);
                        if (!Directory.Exists(folder))
                        {
                            Directory.CreateDirectory(folder);
                        }

                        // gain rights to write to file
                        //(new FileIOPermission(FileIOPermissionAccess.Write, FFilename)).Demand();

                        //Texture2D.SaveTextureToFile(FContext.CurrentDeviceContext, FBackSurface, FFormat, FFilename);
                        //Texture2D.SaveTextureToFile(threadContext, FBackSurface, FFormat, FFilename);

                        // schreibt kein file:
                        TextureLoader.SaveToFile(threadContext, FBackSurface, FFilename, eImageFormat.Png);

                        // exception:
                        //TextureLoader.SaveToFile(FContext.CurrentDeviceContext, FBackSurface, FFilename, eImageFormat.Png);

                        

                        //TextureLoader.SaveToFile(FContext, FBackSurface, FFilename, eImageFormat.Png);
                        /*
                        await System.Threading.Tasks.Task.Run(() =>
                            //Texture2D.SaveTextureToFile(FContext.CurrentDeviceContext, FBackSurface, FFormat, FFilename)
                            TextureLoader.SaveToFile(FContext, FBackSurface, FFilename, eImageFormat.Png)
                        );
                        */
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.Print(e.Message);
                    }
                    finally
                    {
                        if (threadContext != null)
                        {
                            //threadContext.Dispose();
                        }
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
                    if (FBackSurface != null)
                    {
                        FBackSurface.Dispose();
                        FBackSurface = null;
                    }
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
                        //await System.Threading.Tasks.Task.Run(() =>
                        FInstances[i].WriteImage(FInTexture[i], this.AssignedContext, FInFilename[i], FInFormat[i]);
                        //);
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
