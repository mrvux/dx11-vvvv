﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Hosting.Pins;

using FeralTic.DX11.Resources;
using VVVV.DX11.Lib.Rendering;
using FeralTic.DX11;
using VVVV.DX11.Internals.Helpers;
using FeralTic.DX11.Queries;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11",Version="CubeTexture", Author = "vux")]
    public class DX11CubeRendererNode : IDX11RendererProvider, IPluginEvaluate, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        IDiffSpread<EnumEntry> FInFormat;

        [Input("Layer", Order = 1, IsSingle = true)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Size", DefaultValue = 256, Order = 5)]
        protected IDiffSpread<int> FInSize;

        [Input("Clear", DefaultValue = 1, Order = 6)]
        protected ISpread<bool> FInClear;

        [Input("Clear Depth", DefaultValue = 1, Order = 6)]
        protected ISpread<bool> FInClearDepth;

        [Input("Background Color", DefaultColor = new double[] { 0, 0, 0, 1 }, Order = 7)]
        protected ISpread<Color4> FInBgColor;

        [Input("Enabled", DefaultValue = 1, Order = 9)]
        protected ISpread<bool> FInEnabled;

        [Input("Enable Depth Buffer", Order = 9, DefaultValue = 1)]
        protected IDiffSpread<bool> FInDepthBuffer;

        [Input("View", Order = 10)]
        protected IDiffSpread<Matrix> FInView;

        [Input("Projection", Order = 11)]
        protected IDiffSpread<Matrix> FInProjection;

        [Input("Aspect Ratio", Order = 12, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<Matrix> FInAspect;

        [Input("Crop", Order = 13, Visibility = PinVisibility.OnlyInspector)]
        protected IDiffSpread<Matrix> FInCrop;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        [Output("Texture Out", Order = 2, IsSingle = true)]
        protected ISpread<DX11Resource<DX11CubeRenderTarget>> FOutCubeTexture;

        [Output("Depth Out", Order = 2, IsSingle = true)]
        protected ISpread<DX11Resource<DX11CubeDepthStencil>> FOutCubeDepthTexture;

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        protected SampleDescription sd = new SampleDescription(1, 0);

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();
        private int spmax;

        private DX11RenderSettings settings = new DX11RenderSettings();

        #region Constructor
        [ImportingConstructor()]
        public DX11CubeRendererNode(IPluginHost FHost, IIOFactory iofactory)
        {
            string ename = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.RenderTarget);

            InputAttribute tattr = new InputAttribute("Target Format");
            tattr.EnumName = ename;
            tattr.DefaultEnumEntry = "R8G8B8A8_UNorm";

            this.FInFormat = iofactory.CreateDiffSpread<EnumEntry>(tattr);

            //this.depthmanager = new DepthBufferManager(FHost,iofactory);
        }
        #endregion

        public void Evaluate(int SpreadMax)
        {
            this.spmax = SpreadMax;
            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            if (this.FOutCubeTexture[0] == null) { this.FOutCubeTexture[0] = new DX11Resource<DX11CubeRenderTarget>(); }
            if (this.FOutCubeDepthTexture[0] == null) { this.FOutCubeDepthTexture[0] = new DX11Resource<DX11CubeDepthStencil>(); }

            if (this.FInFormat.IsChanged
                || this.FInSize.IsChanged)
            {
                this.FOutCubeTexture[0].Dispose();
                this.FOutCubeDepthTexture[0].Dispose();
            }
        }


        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            Device device = context.Device;

            if (this.spmax == 0) { return; }

            if (this.updateddevices.Contains(context)) { return; }

            if (!this.FOutCubeTexture[0].Contains(context))
            {
                this.FOutCubeTexture[0][context] = new DX11CubeRenderTarget(context, this.FInSize[0], this.sd, DeviceFormatHelper.GetFormat(this.FInFormat[0].Name), false, 1);
                this.FOutCubeDepthTexture[0][context] = new DX11CubeDepthStencil(context, this.FInSize[0], this.sd, Format.D32_Float);
            }

            this.updateddevices.Add(context);
        }

        public void Render(DX11RenderContext context)
        {
            Device device = context.Device;

            //Just in case
            if (!this.updateddevices.Contains(context))
            {
                this.Update(null, context);
            }

            if (this.rendereddevices.Contains(context)) { return; }

            if (this.FInEnabled[0])
            {
                if (this.BeginQuery != null)
                {
                    this.BeginQuery(context);
                }

                DX11CubeRenderTarget target = this.FOutCubeTexture[0][context];
                DX11CubeDepthStencil depth = this.FOutCubeDepthTexture[0][context];

                if (this.FInClear[0])
                {
                    context.CurrentDeviceContext.ClearRenderTargetView(target.RTV, this.FInBgColor[0]);

                    if (this.FInDepthBuffer[0])
                    {
                        context.CurrentDeviceContext.ClearDepthStencilView(depth.DSV, DepthStencilClearFlags.Depth, 1.0f, 0);
                    }
                }

                if (this.FInLayer.PluginIO.IsConnected)
                {

                    int size = this.FInSize[0];

                    for (int i = 0; i < 6; i++)
                    {
                        settings.ViewportIndex = i;
                        settings.ViewportCount = 6;
                        settings.View = this.FInView[i];

                        Matrix proj = this.FInProjection[i];
                        Matrix aspect = Matrix.Invert(this.FInAspect[i]);
                        Matrix crop = Matrix.Invert(this.FInCrop[i]);

                        settings.Projection = proj * aspect * crop;
                        settings.ViewProjection = settings.View * settings.Projection;
                        settings.RenderWidth = size;
                        settings.RenderHeight = size;
                        settings.BackBuffer = null;
                        settings.CustomSemantics.Clear();
                        settings.ResourceSemantics.Clear();

                        if (this.FInDepthBuffer[0])
                        {
                            context.RenderTargetStack.Push(depth.SliceDSV[i], false, target.SliceRTV[i]);
                        }
                        else
                        {
                            context.RenderTargetStack.Push(target);
                        }

                        

                        for (int j = 0; j < this.FInLayer.SliceCount; j++)
                        {
                            try
                            {
                                this.FInLayer[j][context].Render(this.FInLayer.PluginIO, context, settings);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                        }

                        context.RenderTargetStack.Pop();
                    }

                    
                }


                if (this.EndQuery != null)
                {
                    this.EndQuery(context);
                }

                this.rendereddevices.Add(context);
            }
        }

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            this.FOutCubeTexture[0].Dispose(context);
            this.FOutCubeDepthTexture[0].Dispose(context);
        }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Dispose()
        {
            if (this.FOutCubeTexture[0] != null) { this.FOutCubeTexture[0].Dispose(); }
            if (this.FOutCubeDepthTexture[0] != null) { this.FOutCubeDepthTexture[0].Dispose(); }
        }
    }
}
