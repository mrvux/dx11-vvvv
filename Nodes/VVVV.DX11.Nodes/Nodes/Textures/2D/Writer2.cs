using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using SlimDX.Direct3D11;

using VVVV.Core.Logging;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using System.IO;
using System.Threading.Tasks;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Writer", Category = "DX11.Texture", Version = "2d native", Author = "vux", AutoEvaluate = true)]
    public class WriterTextureNode2 : IPluginEvaluate, IDX11ResourceDataRetriever
    {
        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> FTextureIn;

        [Input("Filename", StringType = StringType.Filename, DefaultString = "render")]
        protected ISpread<string> FInPath;

        [Input("Format")]
        protected ISpread<eImageFormat> FInFormat;
        //protected ISpread<ImageFileFormat> FInFormat;

        [Input("Threaded", IsSingle = true, DefaultBoolean = true)]
        protected ISpread<bool> FThreaded;

        [Input("Create Folder", IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<bool> FCreateFolder;

        [Input("Write", IsBang = true)]
        protected ISpread<bool> FInSave;

        [Output("Valid")]
        protected ISpread<bool> FOutValid;

        [Import()]
        protected IPluginHost FHost;

        [Import()]
        protected ILogger FLogger;

        public DX11RenderContext AssignedContext
        {
            get;
            set;
        }

        public event DX11RenderRequestDelegate RenderRequest;

        //List<Task> tasks = new List<Task>();


        #region IPluginEvaluate Members

        public async void Evaluate(int SpreadMax)
        {
            this.FOutValid.SliceCount = SpreadMax;

            if (this.FTextureIn.PluginIO.IsConnected)
            {
                if (this.RenderRequest != null) { this.RenderRequest(this, this.FHost); }

                if (this.AssignedContext == null) { this.FOutValid.SliceCount = 0; return; }
                //Do NOT cache this, assignment done by the host


                
                /*
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (this.FTextureIn[i].Contains(this.AssignedContext) && this.FInSave[i])
                    {
                        tasks.Clear();

                        FLogger.Log(LogType.Debug, "Task {0} adding:", i);

                        Task t = Task.Run(() =>
                                TextureLoader.SaveToFile(this.AssignedContext,
                                this.FTextureIn[i][this.AssignedContext],
                                this.FInPath[i], this.FInFormat[i])
                            );
                        tasks.Add(t);
                        FLogger.Log(LogType.Debug, "Task {0} added:", i);
                    }
                }

                if (tasks.Count > 0)
                {
                    try
                    {
                        Task.WaitAll(tasks.ToArray());

                        foreach (Task t in tasks)
                            FLogger.Log(LogType.Debug, "Task {0} Status: {1}", t.Id, t.Status);
                    }
                    catch (Exception e)
                    {
                        FLogger.Log(LogType.Debug, "Exception: " + e);
                    }
                    
                }
                */




                
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (this.FTextureIn[i].Contains(this.AssignedContext) && this.FInSave[i])
                    {
                        //List<Task> tasks = new List<Task>();

                        if (this.FCreateFolder[0])
                        {
                            string path = Path.GetDirectoryName(this.FInPath[i]);
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                        }

                        //await Task.Run(() => DoSave(i));


                        


                        try
                        {
                            /*
                            DX11RenderContext threadContext = new DX11RenderContext(this.AssignedContext.Device);

                            Texture2D FBackSurface = new Texture2D(threadContext.Device, this.FTextureIn[i][this.AssignedContext].Description);

                            threadContext.CurrentDeviceContext.CopyResource(this.FTextureIn[i][this.AssignedContext].Resource, FBackSurface);
                            */
                            //DX11Texture2D FBackSurface = DX11Texture2D.FromTextureAndSRV(threadContext, this.FTextureIn[i][this.AssignedContext].Resource, this.FTextureIn[i][this.AssignedContext].SRV);


                            if (FThreaded[0])
                            {

                                // working half-way:
                                // await Task.Run(() => TextureLoader.SaveToFile(threadContext, FBackSurface, FInPath[i], FInFormat[i]) );

                                saver(this.AssignedContext, this.FTextureIn[i][this.AssignedContext], FInPath[i], FInFormat[i]);


                                    await Task.Run(() => saver(this.AssignedContext, this.FTextureIn[i][this.AssignedContext], FInPath[i], FInFormat[i]));

                                    //try
                                    //{
                                    //    await Task.Run(() => saver(threadContext, FBackSurface, FInPath[i], FInFormat[i]));

                                    //}
                                    //catch (Exception e)
                                    //{
                                    //    FLogger.Log(e);
                                    //}
                                    //finally
                                    //{
                                    //    if (threadContext.CurrentDeviceContext != null)
                                    //    {
                                    //        threadContext.CurrentDeviceContext.Dispose();
                                    //    }
                                    //}

                                
                                
                            }
                            else
                            {
                                //TextureLoader.SaveToFile(threadContext,
                                //FBackSurface,
                                //FInPath[i], FInFormat[i]);
                            }

                            // formerly:
                            // TextureLoader.SaveToFile(this.AssignedContext, this.FTextureIn[i][this.AssignedContext], this.FInPath[i], this.FInFormat[i]);



                            this.FOutValid[i] = true;
                        }
                        catch (Exception ex)
                        {
                            FLogger.Log(ex);
                            this.FOutValid[i] = false;
                        }
                    }
                    else
                    {
                        this.FOutValid[i] = false;
                    }
                }

            }
            else
            {
                this.FOutValid.SliceCount = 0;

            }
        }

        public void saver( DX11RenderContext assignedContext, DX11Texture2D textureIn, string path, eImageFormat format)
        {

            DX11RenderContext threadContext = new DX11RenderContext(assignedContext.Device);
            Texture2D FBackSurface = new Texture2D(threadContext.Device, textureIn.Description);
            threadContext.CurrentDeviceContext.CopyResource(textureIn.Resource, FBackSurface);

            bool ts;
            bool cs;
            threadContext.Device.CheckThreadingSupport(out ts, out cs);
            if (cs && ts)
            {
                TextureLoader.SaveToFile(threadContext, FBackSurface, path, format);
                if (threadContext.CurrentDeviceContext != null)
                {
                    threadContext.CurrentDeviceContext.Dispose();
                }

                //try
                //{
                //    TextureLoader.SaveToFile(threadContext, FBackSurface, path, format);
                //}
                //catch (Exception e)
                //{
                //    //FLogger.Log(e);
                //}
                //finally
                //{
                //    if (threadContext.CurrentDeviceContext != null)
                //    {
                //        threadContext.CurrentDeviceContext.Dispose();
                //    }
                //}
            }

        }

        #endregion
    }
}
