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
    [PluginInfo(Name = "Renderer", Category = "DX11",Version="TextureArray", Author = "vux")]
    public class DX11TextureArrayRendererNode : IDX11RendererProvider, IPluginEvaluate, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        IDiffSpread<EnumEntry> FInFormat;

        [Input("Layer", Order = 1, IsSingle = true)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Size", DefaultValues  = new double[] { 512, 512 },AsInt=true, Order = 5)]
        protected IDiffSpread<Vector2> FInSize;

        [Input("Element Count", DefaultValue = 1, Order = 5)]
        protected IDiffSpread<int> FInElementCount;

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
        protected ISpread<DX11Resource<DX11RenderTextureArray>> FOutTexture;

        [Output("Texture Slices Out", Order = 3)]
        protected ISpread<DX11Resource<DX11Texture2D>> FOutSliceTextures;

        [Output("Depth Out", Order = 4, IsSingle = true)]
        protected ISpread<DX11Resource<DX11DepthTextureArray>> FOutDepthTexture;

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        protected SampleDescription sd = new SampleDescription(1, 0);

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();
        private int spmax;

        private DX11RenderSettings settings = new DX11RenderSettings();

        #region Constructor
        [ImportingConstructor()]
        public DX11TextureArrayRendererNode(IPluginHost FHost, IIOFactory iofactory)
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

            if (this.FOutTexture[0] == null) { this.FOutTexture[0] = new DX11Resource<DX11RenderTextureArray>(); }
            if (this.FOutDepthTexture[0] == null) { this.FOutDepthTexture[0] = new DX11Resource<DX11DepthTextureArray>(); }


            if (this.FInFormat.IsChanged
                || this.FInSize.IsChanged
                || this.FInElementCount.IsChanged)
            {
                this.FOutTexture[0].Dispose();
                this.FOutDepthTexture[0].Dispose();

                this.FOutSliceTextures.SliceCount = this.FInElementCount[0];

                for (int i = 0; i < this.FInElementCount[0]; i++)
                {
                    this.FOutSliceTextures[i] = new DX11Resource<DX11Texture2D>();
                }
            }
        
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            Device device = context.Device;

            if (this.spmax == 0) { return; }

            if (this.updateddevices.Contains(context)) { return; }

            if (!this.FOutTexture[0].Contains(context))
            {
                var result = new DX11RenderTextureArray(context, (int)this.FInSize[0].X, (int)this.FInSize[0].Y, this.FInElementCount[0], DeviceFormatHelper.GetFormat(this.FInFormat[0]), true);
                this.FOutTexture[0][context] = result;
                this.FOutDepthTexture[0][context] = new DX11DepthTextureArray(context, (int)this.FInSize[0].X, (int)this.FInSize[0].Y, this.FInElementCount[0], Format.R32_Float, true);
                for (int i = 0; i < this.FInElementCount[0]; i++)
                {
                    DX11Texture2D slice = DX11Texture2D.FromTextureAndSRV(context, result.Resource, result.SliceRTV[i].SRV);
                    this.FOutSliceTextures[i][context] = slice;
                }
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

                DX11RenderTextureArray target = this.FOutTexture[0][context];
                DX11DepthTextureArray depth = this.FOutDepthTexture[0][context];

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
                    for (int i = 0; i < target.ElemCnt; i++)
                    {
                        settings.ViewportIndex = i;
                        settings.ViewportCount = target.ElemCnt;
                        settings.View = this.FInView[i];

                        Matrix proj = this.FInProjection[i];
                        Matrix aspect = Matrix.Invert(this.FInAspect[i]);
                        Matrix crop = Matrix.Invert(this.FInCrop[i]);

                        settings.Projection = proj * aspect * crop;
                        settings.ViewProjection = settings.View * settings.Projection;
                        settings.RenderWidth = target.Width;
                        settings.RenderHeight = target.Height;
                        settings.RenderDepth = target.ElemCnt;
                        settings.BackBuffer = null;
                        settings.CustomSemantics.Clear();
                        settings.ResourceSemantics.Clear();

                        if (this.FInDepthBuffer[0])
                        {
                            context.RenderTargetStack.Push(depth.SliceDSV[i], false, target.SliceRTV[i]);
                        }
                        else
                        {
                            context.RenderTargetStack.Push(target.SliceRTV[i]);
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
            this.FOutTexture[0].Dispose(context);
            this.FOutDepthTexture[0].Dispose(context);
        }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Dispose()
        {
            if (this.FOutTexture[0] != null) { this.FOutTexture[0].Dispose(); }
            if (this.FOutDepthTexture[0] != null) { this.FOutDepthTexture[0].Dispose(); }
        }
    }
}
