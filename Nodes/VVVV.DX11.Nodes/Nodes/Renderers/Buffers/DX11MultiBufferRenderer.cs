﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using SlimDX;
using SlimDX.Direct3D11;

using FeralTic.DX11.Queries;
using FeralTic.DX11.Resources;
using FeralTic.DX11;
using VVVV.DX11;
using VVVV.DX11.Lib.Rendering;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11.StructuredBuffer", Version = "Multiple", Author = "microdee", AutoEvaluate = false)]
    public class DX11MultiStructuredBufferRendererNode : IPluginEvaluate, IDX11RendererProvider, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Semantic", Order = 2, DefaultString = "UAV")]
        protected IDiffSpread<string> FSemantic;
        [Input("Bind SRV", Order = 3)]
        protected IDiffSpread<bool> FBindSRV;
        [Input("SRV Semantic", Order = 4, DefaultString = "SRV")]
        protected IDiffSpread<string> FSRVSemantic;

        [Input("Element Count", Order = 5, DefaultValue = 512)]
        protected IDiffSpread<int> FInSize;

        [Input("Stride", Order = 6, DefaultValue = 16)]
        protected IDiffSpread<int> FInStride;

        [Input("Buffer Mode", Order = 7, DefaultValue = 0)]
        protected IDiffSpread<eDX11BufferMode> FInMode;

        [Input("Reset Counter", Order = 8, IsBang = true)]
        protected IDiffSpread<bool> FInResetCounter;

        [Input("Reset Counter Value", Order = 9)]
        protected IDiffSpread<int> FInResetCounterValue;

        [Input("Enabled", DefaultValue = 1, Order = 10)]
        protected ISpread<bool> FInEnabled;

        [Input("View", Order = 11)]
        protected IDiffSpread<Matrix> FInView;

        [Input("Projection", Order = 12)]
        protected IDiffSpread<Matrix> FInProjection;

        [Output("Buffers")]
        protected ISpread<DX11Resource<IDX11RWStructureBuffer>> FOutBuffers;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        protected List<int> sizes = new List<int>();
        protected List<int> strides = new List<int>();
        protected List<string> semantics = new List<string>();
        protected List<IDX11RenderSemantic> rsemantics = new List<IDX11RenderSemantic>();
        // private List<DX11RawBufferFlags> flags = new List<DX11RawBufferFlags>();

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();

        private bool reset = false;


        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        private DX11RenderSettings settings = new DX11RenderSettings();

        [ImportingConstructor()]
        public DX11MultiStructuredBufferRendererNode(IPluginHost FHost)
        {

        }

        public void Evaluate(int SpreadMax)
        {
            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            FOutBuffers.SliceCount = FSemantic.SliceCount;

            reset = this.FInSize.IsChanged || FInMode.IsChanged || FInStride.IsChanged || this.FSemantic.IsChanged || FBindSRV.IsChanged || FSRVSemantic.IsChanged;

            for (int i = 0; i < FOutBuffers.SliceCount; i++)
            {
                if (this.FOutBuffers[i] == null)
                {
                    this.FOutBuffers[i] = new DX11Resource<IDX11RWStructureBuffer>();
                    reset = true;
                }
            }
            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }

            for (int i = 0; i < FOutBuffers.SliceCount; i++)
            {
                DX11Resource<IDX11RWStructureBuffer> res = this.FOutBuffers[i];
                this.FOutBuffers[i] = res;
            }

            if (reset)
            {
                sizes.Clear();
                strides.Clear();
                semantics.Clear();
                for (int i = 0; i < FSemantic.SliceCount; i++)
                {
                    sizes.Add(FInSize[i]);
                    strides.Add(FInStride[i]);
                    semantics.Add(FSemantic[i]);
                }
            }
        }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Render(DX11RenderContext context)
        {
            Device device = context.Device;
            DeviceContext ctx = context.CurrentDeviceContext;

            //Just in case
            if (!this.updateddevices.Contains(context))
            {
                this.Update(null, context);
            }

            if (!this.FInLayer.PluginIO.IsConnected) { return; }

            if (this.rendereddevices.Contains(context)) { return; }

            if (this.FInEnabled[0])
            {
                if (this.BeginQuery != null)
                {
                    this.BeginQuery(context);
                }

                context.CurrentDeviceContext.OutputMerger.SetTargets(new RenderTargetView[0]);

                settings.ViewportIndex = 0;
                settings.ViewportCount = 1;
                settings.View = this.FInView[0];
                settings.Projection = this.FInProjection[0];
                settings.ViewProjection = settings.View * settings.Projection;
                settings.BackBuffer = null;
                settings.CustomSemantics = rsemantics;

                for (int i = 0; i < FSemantic.SliceCount; i++)
                {
                    settings.RenderWidth = sizes[i];
                    settings.RenderHeight = sizes[i];
                    settings.RenderDepth = sizes[i];

                    if (FInResetCounter[i])
                    {
                        int[] resetval = { FInResetCounterValue[i] };
                        var uavarray = new UnorderedAccessView[1] { FOutBuffers[i][context].UAV };
                        context.CurrentDeviceContext.ComputeShader.SetUnorderedAccessViews(uavarray, 0, 1, resetval);
                    }
                }
                FInLayer[0][context].Render(FInLayer.PluginIO, context, settings);

                if (EndQuery != null) EndQuery.Invoke(context);
            }
        }

        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (this.updateddevices.Contains(context)) { return; }

            foreach (IDX11RenderSemantic semres in rsemantics)
            {
                semres.Dispose();
            }
            if (reset)
            {
                rsemantics.Clear();
                this.DisposeBuffers(context);

                for (int i = 0; i < FOutBuffers.SliceCount; i++)
                {
                    if (reset || !this.FOutBuffers[i].Contains(context))
                    {
                        var mode = FInMode[i];
                        DX11RWStructuredBuffer rb = new DX11RWStructuredBuffer(context.Device, this.sizes[i], strides[i], mode);
                        this.FOutBuffers[i][context] = rb;

                        RWStructuredBufferRenderSemantic uavbs = new RWStructuredBufferRenderSemantic(FSemantic[i], false);
                        uavbs.Data = this.FOutBuffers[i][context];
                        rsemantics.Add(uavbs);

                        if (FBindSRV[i])
                        {
                            StructuredBufferRenderSemantic srvbs = new StructuredBufferRenderSemantic(FSRVSemantic[i], false);
                            srvbs.Data = this.FOutBuffers[i][context];
                            rsemantics.Add(srvbs);
                        }
                    }
                }
            }

            this.updateddevices.Add(context);
        }

        public void Destroy(IPluginIO pin, DX11RenderContext OnDevice, bool force)
        {
            //this.DisposeBuffers(OnDevice.Device);
        }

        #region Dispose Buffers
        private void DisposeBuffers(DX11RenderContext context)
        {
            for (int i = 0; i < this.FOutBuffers.SliceCount; i++)
            {
                this.FOutBuffers[i].Dispose(context);
            }
        }
        #endregion

        public void Dispose()
        {
            for (int i = 0; i < this.FOutBuffers.SliceCount; i++)
            {
                if (this.FOutBuffers[i] != null) { this.FOutBuffers[i].Dispose(); }
            }
        }
    }
}
