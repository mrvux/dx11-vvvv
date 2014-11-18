using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Device = SlimDX.Direct3D11.Device;
using SlimDX.DXGI;
using SlimDX.Direct3D11;
using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using VVVV.PluginInterfaces.V1;
using VVVV.DX11.Internals;
using System.Drawing;

using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using System.Diagnostics;
using VVVV.DX11.Lib.Devices;
using VVVV.DX11.Lib.Rendering;

using VVVV.Utils.IO;
using System.Windows.Forms;


using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2.Graph;
using FeralTic.DX11.Queries;
using FeralTic.DX11;
using FeralTic.DX11.Resources;
using VVVV.DX11.Nodes.Renderers.Graphics.Touch;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name="Renderer",Category="DX11",Author="vux,tonfilm",AutoEvaluate=true,
        InitialWindowHeight=300,InitialWindowWidth=400,InitialBoxWidth=400,InitialBoxHeight=300, InitialComponentMode=TComponentMode.InAWindow)]
    public partial class DX11RendererNode : IPluginEvaluate, IDisposable, IDX11RendererProvider, IDX11RenderWindow, IDX11Queryable, IUserInputWindow, IBackgroundColor
    {
        #region Touch Stuff
        private object m_touchlock = new object();
        private Dictionary<int, TouchData> touches = new Dictionary<int, TouchData>();

        private event EventHandler<WMTouchEventArgs> Touchdown;
        private event EventHandler<WMTouchEventArgs> Touchup;
        private event EventHandler<WMTouchEventArgs> TouchMove;

        private void OnTouchDownHandler(object sender, WMTouchEventArgs e)
        {
            lock (m_touchlock)
            {
                TouchData t = new TouchData();
                t.Id = e.Id;
                t.IsNew = true;
                t.Pos = new Vector2(e.LocationX, e.LocationY);
                this.touches.Add(e.Id, t);
            }
        }

        private void OnTouchUpHandler(object sender, WMTouchEventArgs e)
        {
            lock (m_touchlock)
            {
                this.touches.Remove(e.Id);
            }
        }

        private void OnTouchMoveHandler(object sender, WMTouchEventArgs e)
        {
            lock (m_touchlock)
            {
                TouchData t = this.touches[e.Id];
                t.Pos = new Vector2(e.LocationX, e.LocationY);
            }
        }


        protected override void WndProc(ref Message m) // Decode and handle WM_TOUCH message.
        {
            bool handled;
            switch (m.Msg)
            {
                case TouchConstants.WM_TOUCH:
                    handled = DecodeTouch(ref m);
                    break;
                default:
                    handled = false;
                    break;
            }
            base.WndProc(ref m);  // Call parent WndProc for default message processing.

            if (handled) // Acknowledge event if handled.
                m.Result = new System.IntPtr(1);
        }

        private bool DecodeTouch(ref Message m)
        {
            // More than one touchinput may be associated with a touch message,
            int inputCount = (m.WParam.ToInt32() & 0xffff); // Number of touch inputs, actual per-contact messages
            TOUCHINPUT[] inputs = new TOUCHINPUT[inputCount];

            if (!TouchConstants.GetTouchInputInfo(m.LParam, inputCount, inputs, Marshal.SizeOf(new TOUCHINPUT())))
            {
                return false;
            }

            bool handled = false;
            for (int i = 0; i < inputCount; i++)
            {
                TOUCHINPUT ti = inputs[i];

                EventHandler<WMTouchEventArgs> handler = null;
                if ((ti.dwFlags & TouchConstants.TOUCHEVENTF_DOWN) != 0)
                {
                    handler = Touchdown;
                }
                else if ((ti.dwFlags & TouchConstants.TOUCHEVENTF_UP) != 0)
                {
                    handler = Touchup;
                }
                else if ((ti.dwFlags & TouchConstants.TOUCHEVENTF_MOVE) != 0)
                {
                    handler = TouchMove;
                }

                // Convert message parameters into touch event arguments and handle the event.
                if (handler != null)
                {
                    WMTouchEventArgs te = new WMTouchEventArgs();

                    // TOUCHINFO point coordinates and contact size is in 1/100 of a pixel; convert it to pixels.
                    // Also convert screen to client coordinates.
                    te.ContactY = ti.cyContact / 100;
                    te.ContactX = ti.cxContact / 100;
                    te.Id = ti.dwID;
                    {
                        Point pt = PointToClient(new Point(ti.x / 100, ti.y / 100));
                        te.LocationX = pt.X;
                        te.LocationY = pt.Y;
                    }
                    te.Time = ti.dwTime;
                    te.Mask = ti.dwMask;
                    te.Flags = ti.dwFlags;

                    handler(this, te);

                    // Mark this event as handled.
                    handled = true;
                }
            }
            TouchConstants.CloseTouchInputHandle(m.LParam);

            return handled;
        }
        #endregion

        #region Input Pins
        IPluginHost FHost;

        protected IHDEHost hde;



        [Import()]
        protected IPluginHost2 host2;

        [Import()]
        protected ILogger logger;

        [Input("Layers", Order=1,IsSingle = true)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Clear",DefaultValue=1,Order = 2)]
        protected ISpread<bool> FInClear;

        [Input("Clear Depth", DefaultValue = 1, Order = 2)]
        protected ISpread<bool> FInClearDepth;

        [Input("Background Color",DefaultColor=new double[] { 0,0,0,1 },Order=3)]
        protected ISpread<RGBAColor> FInBgColor;

        [Input("VSync",Visibility=PinVisibility.OnlyInspector, IsSingle=true)]
        protected ISpread<bool> FInVsync;

        [Input("Buffer Count", Visibility = PinVisibility.OnlyInspector, DefaultValue=1, IsSingle=true)]
        protected ISpread<int> FInBufferCount;

        [Input("Do Not Wait", Visibility = PinVisibility.OnlyInspector, IsSingle=true)]
        protected ISpread<bool> FInDNW;

        [Input("Show Cursor", DefaultValue = 0, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<bool> FInShowCursor;

        [Input("Fullscreen", Order = 5)]
        protected IDiffSpread<bool> FInFullScreen;

        [Input("Enable Depth Buffer", Order = 6,DefaultValue=1)]
        protected IDiffSpread<bool> FInDepthBuffer;

        [Input("AA Samples per Pixel", DefaultEnumEntry="1",EnumName="DX11_AASamples")]
        protected IDiffSpread<EnumEntry> FInAASamplesPerPixel;

        /*[Input("AA Quality", Order = 8)]
        protected IDiffSpread<int> FInAAQuality;*/

        [Input("Enabled", DefaultValue = 1, Order = 9)]
        protected ISpread<bool> FInEnabled;

        [Input("View", Order = 10)]
        protected IDiffSpread<Matrix> FInView;

        [Input("Projection", Order = 11)]
        protected IDiffSpread<Matrix> FInProjection;

        [Input("Aspect Ratio", Order = 12,Visibility=PinVisibility.Hidden)]
        protected IDiffSpread<Matrix> FInAspect;

        [Input("Crop", Order = 13, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<Matrix> FInCrop;

        [Input("ViewPort", Order = 20)]
        protected Pin<Viewport> FInViewPort;

        string oldbbformat = "";
        #endregion

        #region Output Pins
        [Output("Mouse State",AllowFeedback=true)]
        protected ISpread<MouseState> FOutMouseState;

        [Output("Keyboard State", AllowFeedback = true)]
        protected ISpread<KeyboardState> FOutKState;

        [Output("Touch Supported",IsSingle=true)]
        protected ISpread<bool> FOutTouchSupport;

        [Output("Touch Data", AllowFeedback = true)]
        protected ISpread<TouchData> FOutTouchData;

        [Output("Actual BackBuffer Size", AllowFeedback = true)]
        protected ISpread<Vector2D> FOutBackBufferSize;

        [Output("Texture Out")]
        protected ISpread<DX11Resource<DX11SwapChain>> FOutBackBuffer;

        protected ISpread<DX11Resource<DX11SwapChain>> FOuFS;

        [Output("Present Time",IsSingle=true)]
        protected ISpread<double> FOutPresent;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        [Output("Control", Order = 201, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<Control> FOutCtrl;

        [Output("Node Ref", Order = 201, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        protected ISpread<INode> FOutRef;
        #endregion

        #region Fields
        public event DX11QueryableDelegate BeginQuery;
        public event DX11QueryableDelegate EndQuery;

        private Vector2D FMousePos;
        private Vector3D FMouseButtons;
        private List<Keys> FKeys = new List<Keys>();
        private int wheel = 0;

        private Dictionary<DX11RenderContext, DX11GraphicsRenderer> renderers = new Dictionary<DX11RenderContext, DX11GraphicsRenderer>();
        private List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        private List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();
        private DepthBufferManager depthmanager;

        private DX11RenderSettings settings = new DX11RenderSettings();

        private bool FInvalidateSwapChain;
        private bool FResized = false;
        private DX11RenderContext primary;
        #endregion

        #region Evaluate
        public void Evaluate(int SpreadMax)
        {
            this.FOutCtrl[0] = this;
            this.FOutRef[0] = (INode)this.FHost;

            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }
            if (this.FOutBackBuffer[0] == null)
            {
                this.FOutBackBuffer[0] = new DX11Resource<DX11SwapChain>();
                this.FOuFS = new Spread<DX11Resource<DX11SwapChain>>();
                this.FOuFS.SliceCount = 1;
                this.FOuFS[0] = new DX11Resource<DX11SwapChain>();
            }

            this.updateddevices.Clear();
            this.rendereddevices.Clear();
            this.FInvalidateSwapChain = false;

            if (!this.depthmanager.FormatChanged) // do not clear reset if format changed
            {
                this.depthmanager.NeedReset = false;
            } 
            else
            {
                this.depthmanager.FormatChanged = false; //Clear flag ok
            }
            
            if (FInAASamplesPerPixel.IsChanged || this.FInBufferCount.IsChanged)
            {
                this.depthmanager.NeedReset = true;
                this.FInvalidateSwapChain = true;
            }

            if (this.FInFullScreen.IsChanged)
            {
                string path;
                this.FHost.GetNodePath(false, out path);
                INode2 n2 = hde.GetNodeFromPath(path);

                if (n2.Window != null)
                {
                    if (n2.Window.IsVisible)
                    {
                        if (this.FInFullScreen[0])
                        {
                            hde.SetComponentMode(n2, ComponentMode.Fullscreen);
                        }
                        else
                        {
                            hde.SetComponentMode(n2, ComponentMode.InAWindow);
                        }
                    }
                }
            }

            /*if (this.FInFullScreen.IsChanged)
            {
                if (this.FInFullScreen[0])
                {
                    string path;
                    this.FHost.GetNodePath(false, out path);
                    INode2 n2 = hde.GetNodeFromPath(path);
                    hde.SetComponentMode(n2, ComponentMode.Fullscreen);
                }
                else
                {
                    string path;
                    this.FHost.GetNodePath(false, out path);
                    INode2 n2 = hde.GetNodeFromPath(path);
                    hde.SetComponentMode(n2, ComponentMode.InAWindow);
                }
            }*/

            this.FOutKState[0] = new KeyboardState(this.FKeys);
            this.FOutMouseState[0] = MouseState.Create(this.FMousePos.x, this.FMousePos.y, this.FMouseButtons.x > 0.5f, this.FMouseButtons.y > 0.5f, this.FMouseButtons.z> 0.5f, false, false, this.wheel);
            this.FOutBackBufferSize[0] = new Vector2D(this.Width, this.Height);
            
            this.FOutTouchSupport[0] = this.touchsupport;

            this.FOutTouchData.SliceCount = this.touches.Count;

            int tcnt = 0;
            float fw = (float)this.ClientSize.Width;
            float fh = (float)this.ClientSize.Height;
            lock (m_touchlock)
            {
                foreach (int key in touches.Keys)
                {
                    TouchData t = touches[key];

                    this.FOutTouchData[tcnt] = t.Clone(fw, fh);
                    t.IsNew = false;
                    tcnt++;
                }
            }
        }
        #endregion

        #region Dispose
        void IDisposable.Dispose()
        {
            if (this.FOutBackBuffer[0] != null) { this.FOutBackBuffer[0].Dispose(); }
        }
        #endregion

        #region Is Enabled
        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }
        #endregion

        #region Render
        public void Render(DX11RenderContext context)
        {
            Device device = context.Device;
            
            if (!this.updateddevices.Contains(context)) { this.Update(null, context); }

            if (this.rendereddevices.Contains(context)) { return; }

            Exception exception = null;

            if (this.FInEnabled[0])
            {

                if (this.BeginQuery != null)
                {
                    this.BeginQuery(context);
                }

                DX11SwapChain chain = this.FOutBackBuffer[0][context];
                DX11GraphicsRenderer renderer = this.renderers[context];

                renderer.EnableDepth = this.FInDepthBuffer[0];
                renderer.DepthStencil = this.depthmanager.GetDepthStencil(context);
                renderer.DepthMode = this.depthmanager.Mode;
                renderer.SetRenderTargets(chain);
                renderer.SetTargets();

                try
                {
                    if (this.FInClear[0])
                    {
                        //Remove Shader view if bound as is
                        context.CurrentDeviceContext.ClearRenderTargetView(chain.RTV, this.FInBgColor[0].Color);
                    }

                    if (this.FInClearDepth[0])
                    {
                        if (this.FInDepthBuffer[0])
                        {
                            this.depthmanager.Clear(context);
                        }
                    }

                    //Only call render if layer connected
                    if (this.FInLayer.PluginIO.IsConnected)
                    {
                        int rtmax = Math.Max(this.FInProjection.SliceCount, this.FInView.SliceCount);
                        rtmax = Math.Max(rtmax, this.FInViewPort.SliceCount);

                        settings.ViewportCount = rtmax;

                        bool viewportpop = this.FInViewPort.PluginIO.IsConnected;

                        for (int i = 0; i < rtmax; i++)
                        {
                            this.RenderSlice(context, settings, i, viewportpop);
                        }
                    }

                    if (this.EndQuery != null)
                    {
                        this.EndQuery(context);
                    }
                } 
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    renderer.CleanTargets();
                }
            }

            this.rendereddevices.Add(context);

            //Rethrow
            if (exception != null)
            {
                throw exception;
            }
        }
        #endregion

        private void RenderSlice(DX11RenderContext context,DX11RenderSettings settings, int i, bool viewportpop)
        {
            float cw = (float)this.ClientSize.Width;
            float ch = (float)this.ClientSize.Height;

            settings.ViewportIndex = i;
            settings.View = this.FInView[i];

            Matrix proj = this.FInProjection[i];
            Matrix aspect = Matrix.Invert(this.FInAspect[i]);
            Matrix crop = Matrix.Invert(this.FInCrop[i]);


            settings.Projection = proj * aspect * crop;
            settings.ViewProjection = settings.View * settings.Projection;
            settings.BackBuffer = this.FOutBackBuffer[0][context];
            settings.RenderWidth = this.FOutBackBuffer[0][context].Resource.Description.Width;
            settings.RenderHeight = this.FOutBackBuffer[0][context].Resource.Description.Height;
            settings.ResourceSemantics.Clear();
            settings.CustomSemantics.Clear();

            if (viewportpop)
            {
                context.RenderTargetStack.PushViewport(this.FInViewPort[i].Normalize(cw, ch));
            }


            //Call render on all layers
            for (int j = 0; j < this.FInLayer.SliceCount; j++)
            {
                this.FInLayer[j][context].Render(this.FInLayer.PluginIO, context, settings);
            }

            if (viewportpop)
            {
                context.RenderTargetStack.PopViewport();
            }
        }

        #region Update
        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            Device device = context.Device;

            if (this.updateddevices.Contains(context)) { return; }

            int samplecount = Convert.ToInt32(FInAASamplesPerPixel[0].Name);

            SampleDescription sd = new SampleDescription(samplecount, 0);

            if (this.FResized || this.FInvalidateSwapChain || this.FOutBackBuffer[0][context] == null)
            {
                this.FOutBackBuffer[0].Dispose(context);

                List<SampleDescription> sds = context.GetMultisampleFormatInfo(Format.R8G8B8A8_UNorm);
                int maxlevels = sds[sds.Count - 1].Count;

                if (sd.Count > maxlevels)
                {
                    logger.Log(LogType.Warning, "Multisample count too high for this format, reverted to: " + maxlevels);
                    sd.Count = maxlevels;
                }

                this.FOutBackBuffer[0][context] = new DX11SwapChain(context, this.Handle, Format.R8G8B8A8_UNorm, sd, 60,
                    this.FInBufferCount[0]);

                #if DEBUG
                this.FOutBackBuffer[0][context].Resource.DebugName = "BackBuffer";
                #endif
                this.depthmanager.NeedReset = true;
            }

            DX11SwapChain sc = this.FOutBackBuffer[0][context];

            if (this.FResized)
            {
                
                //if (!sc.IsFullScreen)
                //{
                   // sc.Resize();
               // }
               //this.FInvalidateSwapChain = true;
            }

            
            if (!this.renderers.ContainsKey(context)) { this.renderers.Add(context, new DX11GraphicsRenderer(this.FHost, context)); }

            this.depthmanager.Update(context, sc.Width, sc.Height, sd);

            this.updateddevices.Add(context);
        }
        #endregion

        #region Destroy
        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            //if (this.FDepthManager != null) { this.FDepthManager.Dispose(); }

            if (this.renderers.ContainsKey(context)) { this.renderers.Remove(context); }

            this.FOutBackBuffer[0].Dispose(context);
        }
        #endregion

        #region Render Window
        public void Present()
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                PresentFlags flags = this.FInDNW[0] ? (PresentFlags)8 : PresentFlags.None;
                if (this.FInVsync[0])
                {
                    this.FOutBackBuffer[0][this.RenderContext].Present(1, flags); 
                }
                else
                {
                    this.FOutBackBuffer[0][this.RenderContext].Present(0, flags); 
                }
            }
            catch
            {
                
            }

            sw.Stop();
            this.FOutPresent[0] = sw.Elapsed.TotalMilliseconds;

            this.FResized = false;
        }

        public DX11RenderContext RenderContext
        {
            get { return this.primary; }
            set
            {
                this.primary = value;
            }
        }

        public IntPtr WindowHandle
        {
            get 
            {
                return this.Handle;
            }
        }
        #endregion


        public bool IsVisible
        {
            get
            {
                INode node = (INode)this.FHost;

                if (node.Window != null)
                {
                    return node.Window.IsVisible();
                }
                else
                {
                    return false;
                }
            }
        }

        /*private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DX11RendererNode
            // 
            this.Name = "DX11RendererNode";
            this.ResumeLayout(false);

        }*/

        public IntPtr InputWindowHandle
        {
            get { return this.Handle; }
        }

        public RGBAColor BackgroundColor
        {
            get { return new RGBAColor(0, 0, 0, 1); }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // DX11RendererNode
            // 
            this.BackColor = System.Drawing.Color.Black;
            this.Name = "DX11RendererNode";
            this.ResumeLayout(false);

        }
    }
}