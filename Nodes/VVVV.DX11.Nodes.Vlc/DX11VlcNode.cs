using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;
using System.IO;
using System.Diagnostics;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Resources;
using FeralTic.DX11;

using SlimDX;

using VVVV.DX11;
using VVVV.DX11.Vlc.Player;



namespace VVVV.Nodes.VideoPlayer
{
    [PluginInfo(Name = "FileStream", Category = "DX11.Texture", Version = "Vlc", AutoEvaluate = true, Author="vux",Credits="ft")]
    public class VLCNodeSPR : IPluginEvaluate, IDisposable, IDX11ResourceHost
    {
        #region Fields
        private IPluginHost FHost;

        private IValueIn FPinInPlay;
        private IValueIn FPinInLoop;
        private IValueIn FPinInSeekPos;
        private IValueIn FPinInStart;
        private IValueIn FPinInDoSeek;
        private IValueIn FPinInSpeed;
        private IValueIn FPinInVolume;

        private IStringIn FPinInPath;

        private IValueOut FPinOutFPS;
        private IValueOut FPinOutPosition;
        private IValueOut FPinOutDuration;
        private IValueOut FPinOutValid;

        private IValueOut FPinOutCopyTime;

        #endregion

        [Output("Size", Order = 99)]
        protected ISpread<int> FSizeOut;

        [Output("Texture Out", Order = 0)]
        protected Pin<DX11Resource<DX11DynamicTexture2D>> FTextureOut;

        private List<VlcPlayer> players = new List<VlcPlayer>();

        #region Set Plugin Host
        [ImportingConstructor()]
        public VLCNodeSPR(IPluginHost Host)
        {
            //assign host
            this.FHost = Host;

            this.FHost.CreateValueInput("Play", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInPlay);
            this.FPinInPlay.SetSubType(0, 1, 1, 0, false, true, false);

            this.FHost.CreateValueInput("Loop", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInLoop);
            this.FPinInLoop.SetSubType(0, 1, 1, 0, false, true, false);

            this.FHost.CreateValueInput("Reset Start", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInStart);
            this.FPinInStart.SetSubType(0, 1, 1, 0, true, false, false);

            this.FHost.CreateValueInput("Seek Time", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInSeekPos);
            this.FPinInSeekPos.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueInput("Do Seek", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInDoSeek);
            this.FPinInDoSeek.SetSubType(0, 1, 1, 0, true, false, false);

            this.FHost.CreateValueInput("Speed", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInSpeed);
            this.FPinInSpeed.SetSubType(double.MinValue, double.MaxValue, 0.01, 1, false, false, false);

            this.FHost.CreateValueInput("Volume", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInVolume);
            this.FPinInVolume.SetSubType(0, 1, 0.01, 0.5, false, false, false);

            this.FHost.CreateStringInput("Filename", TSliceMode.Dynamic, TPinVisibility.True, out this.FPinInPath);
            this.FPinInPath.SetSubType("", true);

            this.FHost.CreateValueOutput("Frame Rate", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutFPS);
            this.FPinOutFPS.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Position", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutPosition);
            this.FPinOutPosition.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Duration", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutDuration);
            this.FPinOutDuration.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

            this.FHost.CreateValueOutput("Is Valid", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FPinOutValid);
            this.FPinOutValid.SetSubType(0, 1, 1, 0, false, true, false);

            this.FHost.CreateValueOutput("Copy time", 1, null, TSliceMode.Single, TPinVisibility.True, out this.FPinOutCopyTime);
            this.FPinOutCopyTime.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);

        }
        #endregion

        //private bool first = true;
        //VlcPlayer vlc = null;

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                string path;
                this.FPinInPath.GetString(i, out path);

                //Check for update here, if path has not changed leave the file on
                bool needreset = false;
                bool add = false;
                if (this.players.Count < SpreadMax)
                {
                    //New player, need to create
                    needreset = true;
                    add = true;
                }
                else
                {
                    if (this.players[i] != null)
                    {
                        if (path != this.players[i].FileName)
                        {
                            this.players[i].Stop();
                            this.players[i].Dispose();
                            //this.players[i].DestroyDevice();
                            needreset = true;
                        }
                    }
                    else
                    {
                        needreset = true;
                    }
                }

                if (needreset)
                {
                    if (File.Exists(path))
                    {
                        if (add)
                        {
                            this.players.Add(new VlcPlayer());
                        }
                        else
                        {
                            this.players[i] = new VlcPlayer();
                        }

                        this.players[i].SetFileName(path);

                        /*if (this.players[i].IsValid)
                        {
                            this.players[i].Start();
                        }*/
                    }
                    else
                    {
                        if (add)
                        {
                            this.players.Add(null);
                        }
                        else
                        {
                            //Just set a null here
                            this.players[i] = null;
                        }

                    }
                }

            }

            if (this.players.Count > SpreadMax)
            {
                for (int i = SpreadMax; i < this.players.Count; i++)
                {
                    this.players[i].Stop();
                    this.players[i].Dispose();
                }
                this.players.RemoveRange(SpreadMax, this.players.Count - SpreadMax);
            }

            this.FTextureOut.SliceCount = SpreadMax;

            //Now we can set all play/loop/seek stuff
            for (int i = 0; i < SpreadMax; i++)
            {

                if (this.players[i] != null)
                {
                    if (this.players[i].IsValid)
                    {
                        double dbldoseek;
                        this.FPinInDoSeek.GetValue(i, out dbldoseek);

                        if (dbldoseek > 0.5)
                        {
                            double seekpos;
                            this.FPinInSeekPos.GetValue(i, out seekpos);

                            //Seek in milliseconds
                            this.players[i].SetPosition(Convert.ToSingle(seekpos));
                        }

                        double dblstart;
                        this.FPinInStart.GetValue(0, out dblstart);
                        if (dblstart > 0.5)
                        {
                            this.players[i].SetPosition(0.0f);
                        }

                        double dblplay, dblloop, dblspeed,dblvolume;
                        this.FPinInPlay.GetValue(i, out dblplay);
                        this.FPinInLoop.GetValue(i, out dblloop);
                        this.FPinInSpeed.GetValue(i, out dblspeed);
                        this.FPinInVolume.GetValue(i, out dblvolume);

                        this.players[i].Play = dblplay > 0.5;

                        this.players[i].Loop = dblloop > 0.5;
                        this.players[i].SetSpeed((float)dblspeed);
                        this.players[i].SetVolume((float)dblvolume);
                    }
                }

                if (this.FTextureOut[i] == null) { this.FTextureOut[i] = new DX11Resource<DX11DynamicTexture2D>(); }
            }
        
            //Process all outputs
            this.FPinOutPosition.SliceCount = this.players.Count;
            this.FPinOutFPS.SliceCount = this.players.Count;
            this.FPinOutDuration.SliceCount = this.players.Count;
            this.FPinOutValid.SliceCount = this.players.Count;
            this.FSizeOut.SliceCount = this.players.Count * 2;

            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.players[i] != null)
                {
                    if (this.players[i].IsValid)
                    {
                        VlcPlayer v = this.players[i];
                        v.GetStatus();
                        this.FPinOutFPS.SetValue(i, v.Fps);
                        this.FPinOutDuration.SetValue(i, v.Duration);
                        this.FPinOutPosition.SetValue(i, v.Position);
                        this.FPinOutValid.SetValue(i, Convert.ToDouble(v.IsValid));
                        this.FSizeOut[i * 2] = v.Width;
                        this.FSizeOut[i * 2 + 1] = v.Height;
                    }
                    else
                    {
                        this.FPinOutFPS.SetValue(i, -1);
                        this.FPinOutDuration.SetValue(i, -1);
                        this.FPinOutPosition.SetValue(i, -1);
                        this.FPinOutValid.SetValue(i, -1);
                        this.FSizeOut[i * 2] = -1;
                        this.FSizeOut[i * 2 + 1] = -1;
                    }
                }
                else
                {
                    this.FPinOutFPS.SetValue(i, 0);
                    this.FPinOutDuration.SetValue(i, 0);
                    this.FPinOutPosition.SetValue(i, 0);
                    this.FPinOutValid.SetValue(i, 0);
                    this.FSizeOut[i * 2] = -1;
                    this.FSizeOut[i * 2 + 1] = -1;
                }

            }
        }
        #endregion

        #region Dispose
        public void Dispose()
        {        
            try
            {
                foreach (VlcPlayer tp in this.players)
                {
                    if (tp != null)
                    {
                        tp.Stop();
                        tp.Dispose();
                    }
                }
            }
            catch
            {

            }

            this.FTextureOut.SafeDisposeAll();
        }
        #endregion

        public void Update(DX11RenderContext context)
        {
            Stopwatch w = Stopwatch.StartNew();

            int cnt = 0;
            foreach (VlcPlayer vlc in this.players)
            {

                if (vlc != null)
                {
                    if (vlc.IsValid && vlc.Width > 0 && vlc.Height > 0)
                    {
                        DX11DynamicTexture2D t;
                        if (!this.FTextureOut[cnt].Contains(context))
                        {
                            t = new DX11DynamicTexture2D(context, vlc.Width, vlc.Height, SlimDX.DXGI.Format.B8G8R8A8_UNorm);
                            #if DEBUG
                            t.Resource.DebugName = "Vlc";
                            #endif
                            this.FTextureOut[cnt][context] = t;
                        }
                        else
                        {
                            t = this.FTextureOut[cnt][context];
                            if (t.Width != vlc.Width || t.Height != vlc.Height)
                            {
                                this.FTextureOut[cnt].Dispose(context);
                                DX11DynamicTexture2D t2 = new DX11DynamicTexture2D(context, vlc.Width, vlc.Height, SlimDX.DXGI.Format.B8G8R8A8_UNorm);
                                #if DEBUG
                                t2.Resource.DebugName = "Vlc";
                                #endif
                                this.FTextureOut[cnt][context] = t2;
                                t = t2;
                            }
                        }

                        if (vlc.Play)
                        {
                            IntPtr ptr = vlc.frontBuffer;

                            if (ptr != IntPtr.Zero)
                            {
                                if (vlc.Width * 4 == t.GetRowPitch())
                                {
                                    t.WriteData(ptr, vlc.Width * vlc.Height * 4);
                                }
                                else
                                {
                                    t.WriteDataPitch(ptr, vlc.Width * vlc.Height * 4);
                                }
                            }


                        }
                    }
                    else
                    {
                        this.FTextureOut[cnt].Dispose(context);
                    }
                }
                cnt++;
            }

            w.Stop();
            this.FPinOutCopyTime.SetValue(0, w.ElapsedMilliseconds);
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FTextureOut.SafeDisposeAll(context);
        }
    }
}
