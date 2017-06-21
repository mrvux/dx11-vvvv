using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VColor;

using VVVV.DX11.Internals;
using VVVV.DX11;
using VVVV.DX11.Lib.Devices;
using VVVV.DX11.Lib.Rendering;

using SlimDX;
using SlimDX.DXGI;
using SlimDX.Direct3D11;


using Device = SlimDX.Direct3D11.Device;
using FeralTic.Resources;
using FeralTic.DX11.Queries;
using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11
{
    public abstract class AbstractDX11Renderer2DNode : IDX11RendererHost, IPluginEvaluate, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Clear", DefaultValue = 1, Order = 6)]
        protected ISpread<bool> FInClear;

        [Input("Clear Depth", DefaultValue = 1, Order = 6)]
        protected ISpread<bool> FInClearDepth;

        [Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 1 }, Order = 7)]
        protected ISpread<Color4> FInBgColor;

        [Input("AA Samples per Pixel", DefaultEnumEntry = "1", EnumName = "DX11_AASamples",Order=8)]
        protected IDiffSpread<EnumEntry> FInAASamplesPerPixel;

        [Input("Enabled", DefaultValue = 1, Order = 9)]
        protected ISpread<bool> FInEnabled;

        [Input("Enable Depth Buffer", Order = 9,DefaultValue=1)]
        protected IDiffSpread<bool> FInDepthBuffer;

        [Input("Clear Depth Value", Order = 9, DefaultValue = 1)]
        protected ISpread<float> FInClearDepthValue;

        [Input("View", Order = 10)]
        protected IDiffSpread<Matrix> FInView;

        [Input("Projection", Order = 11)]
        protected IDiffSpread<Matrix> FInProjection;

        [Input("Aspect Ratio", Order = 12, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<Matrix> FInAspect;

        [Input("Crop", Order = 13, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<Matrix> FInCrop;

        [Input("ViewPort", Order = 20)]
        protected Pin<Viewport> FInViewPort;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;


        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        protected int width;
        protected int height;
        protected SampleDescription sd = new SampleDescription(1, 0);

        protected Dictionary<DX11RenderContext, DX11GraphicsRenderer> renderers = new Dictionary<DX11RenderContext, DX11GraphicsRenderer>();
        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();
        protected DepthBufferManager depthmanager;

        protected abstract void OnEvaluate(int SpreadMax);
        protected abstract void OnUpdate(DX11RenderContext OnDevice);
        protected abstract void OnDestroy(DX11RenderContext OnDevice, bool force);

        protected abstract void BeforeRender(DX11GraphicsRenderer renderer, DX11RenderContext OnDevice);
        protected abstract void AfterRender(DX11GraphicsRenderer renderer, DX11RenderContext OnDevice);

        protected abstract void OnDispose();


        protected virtual IDX11RWResource GetMainTarget(DX11RenderContext device) { return null; }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }
            if (!this.depthmanager.FormatChanged) // do not clear reset if format changed
            {
                this.depthmanager.NeedReset = false;
            }
            else
            {
                this.depthmanager.FormatChanged = false; //Clear flag ok
            }

            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            this.OnEvaluate(SpreadMax);
        }

        public void Update(DX11RenderContext context)
        {
            Device device = context.Device;

            if (this.updateddevices.Contains(context)) { return; }

            if (!this.renderers.ContainsKey(context))
            {
                this.renderers.Add(context, new DX11GraphicsRenderer(context));
            }

            //Update what's needed
            this.OnUpdate(context);

            //Update depth manager
            this.depthmanager.Update(context, this.width, this.height, this.sd);

            this.updateddevices.Add(context);
        }

        protected virtual void DoClear(DX11RenderContext context)
        {
            if (this.FInClear[0])
            {
                this.renderers[context].Clear(this.FInBgColor[0]);
            }
        }


        #region Render
        public void Render(DX11RenderContext context)
        {
            Device device = context.Device;

            //Just in case
            if (!this.updateddevices.Contains(context))
            {
                this.Update(context);
            }

            if (this.rendereddevices.Contains(context)) { return; }

            if (this.FInEnabled[0])
            {

                DX11GraphicsRenderer renderer = this.renderers[context];

                if (this.BeginQuery != null)
                {
                    this.BeginQuery(context);
                }

                this.BeforeRender(renderer, context);

                Exception exception = null;

                try
                {
                    renderer.SetTargets();

                    if (this.FInClearDepth[0] && this.FInDepthBuffer[0])
                    {
                        this.depthmanager.Clear(context, this.FInClearDepthValue[0]);
                    }

                    this.DoClear(context);

                    if (this.FInLayer.IsConnected)
                    {

                        int rtmax = Math.Max(this.FInProjection.SliceCount, this.FInView.SliceCount);
                        rtmax = Math.Max(rtmax, this.FInViewPort.SliceCount);

                        DX11RenderSettings settings = new DX11RenderSettings();
                        settings.ViewportCount = rtmax;

                        bool viewportpop = this.FInViewPort.PluginIO.IsConnected;

                        float cw = (float)this.width;
                        float ch = (float)this.height;

                        for (int i = 0; i < rtmax; i++)
                        {
                            settings.ViewportIndex = i;
                            settings.ApplyTransforms(this.FInView[i], this.FInProjection[i], this.FInAspect[i], this.FInCrop[i]);


                            settings.RenderWidth = this.width;
                            settings.RenderHeight = this.height;
                            settings.BackBuffer = this.GetMainTarget(context);
                            settings.CustomSemantics.Clear();
                            settings.ResourceSemantics.Clear();

                            if (viewportpop)
                            {
                                context.RenderTargetStack.PushViewport(this.FInViewPort[i].Normalize(cw, ch));
                            }

                            try
                            {
                                this.FInLayer.RenderAll(context, settings);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }


                            if (viewportpop)
                            {
                                context.RenderTargetStack.Pop();
                            }
                        }
                    }

                    //Post render
                    this.AfterRender(renderer, context);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    if (this.EndQuery != null)
                    {
                        this.EndQuery(context);
                    }

                    renderer.CleanTargets();
                }

                this.rendereddevices.Add(context);

                if (exception != null)
                {
                    throw exception;
                }
            }

            
        }
        #endregion

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (this.renderers.ContainsKey(context))
            {
                this.renderers.Remove(context);
            }

            this.depthmanager.Destroy(context);

            this.OnDestroy(context, force);
        }

        public void Dispose()
        {
            this.depthmanager.Dispose();
            this.OnDispose();
        }
    }
}
