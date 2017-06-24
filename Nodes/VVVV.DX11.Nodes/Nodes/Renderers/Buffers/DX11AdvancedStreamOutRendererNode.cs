using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using SlimDX;

using SlimDX.Direct3D11;
using System.ComponentModel.Composition;

using FeralTic.DX11.Queries;
using FeralTic.DX11.Resources;
using FeralTic.DX11;
using FeralTic.DX11.Utils;
using FeralTic.DX11.Resources.Misc;
using VVVV.DX11.Internals.Helpers;
using VVVV.Utils.VMath;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11", Version = "StreamOut.Advanced", Author = "vux", AutoEvaluate = false,
        Help ="Advanced version of stream out renderer, allows to set up to 4 buffers")]
    public class DX11AdvancedStreamOutRendererNode : IPluginEvaluate, IDX11RendererHost, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Buffer Count", Order = 7, DefaultValue = 1, MinValue = 1, MaxValue = 4)]
        protected IDiffSpread<int> FInBufferCount;

        [Input("Vertex Size", Order = 8, DefaultValue = 12)]
        protected IDiffSpread<int> FInVSize;

        [Input("Element Count", Order = 8, DefaultValue = 512)]
        protected IDiffSpread<int> FInElemCount;

        [Input("Output Layout", Order = 10005)]
        protected IDiffSpread<InputElement> FInLayouts;

        [Input("Output Draw Mode", Order = 10005)]
        protected IDiffSpread<StreamOutputBufferWithRawSupport.OutputDrawMode> FinOutputDrawMode;

        [Input("Output Layout Element Count", Order = 10006, DefaultValue = -1)]
        protected IDiffSpread<int> FInLayoutsElementCount;

        [Input("Attach Index Buffer", Order = 10006, DefaultValue = 0, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<bool> FAttachIndexBuffer;

        [Input("Vertex Buffer Binding", Order = 10006, DefaultValue = 0, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<int> FInVertexBufferBinding;

        [Input("Index Buffer Binding", Order = 10006, DefaultValue = 0, Visibility = PinVisibility.Hidden)]
        protected IDiffSpread<int> FInIndexBufferBinding;

        [Input("Enabled", DefaultValue = 1, Order = 15)]
        protected ISpread<bool> FInEnabled;

        [Input("Keep In Memory", Order = 18, Visibility = PinVisibility.OnlyInspector, IsSingle=true)]
        protected ISpread<bool> FInKeepInMemory;

        [Output("Geometry Out")]
        protected ISpread<DX11Resource<IDX11Geometry>> FOutput;

        [Output("Geometry Slices Out")]
        protected ISpread<DX11Resource<IDX11Geometry>> FOutSlices;

        [Output("Buffer Out")]
        protected ISpread<DX11Resource<DX11RawBuffer>> FOutBuffer;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();

        public event DX11QueryableDelegate BeginQuery;
        public event DX11QueryableDelegate EndQuery;


        private DX11RenderSettings settings = new DX11RenderSettings();

        private StreamOutputBufferWithRawSupport[] outputBuffer = new StreamOutputBufferWithRawSupport[4];
        private int currentBufferCount = 0;
        private bool invalidate = false;


        [ImportingConstructor()]
        public DX11AdvancedStreamOutRendererNode(IPluginHost FHost)
        {

        }

        public void Evaluate(int SpreadMax)
        {
            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }

            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            invalidate = this.FInVSize.IsChanged || this.FInElemCount.IsChanged ||
                this.FInLayouts.IsChanged || this.FInLayoutsElementCount.IsChanged
                || this.FinOutputDrawMode.IsChanged || this.FInVertexBufferBinding.IsChanged || this.FInIndexBufferBinding.IsChanged
                || this.FAttachIndexBuffer.IsChanged;

            int bufferCount = SpreadMax == 0 ? 0 : this.FInBufferCount[0];
            if (bufferCount > 4)
                bufferCount = 4;

            if (bufferCount < 0)
                bufferCount = 0;

            if (bufferCount != this.currentBufferCount)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (this.outputBuffer[i] != null)
                    {
                        this.outputBuffer[i].Dispose();
                        this.outputBuffer[i] = null;
                    }
                }
                invalidate = true;
            }
            this.currentBufferCount = bufferCount;

            
            this.FOutBuffer.SliceCount = this.currentBufferCount;
            this.FOutSlices.SliceCount = this.currentBufferCount;

            bool attachIndex = this.FAttachIndexBuffer[0];
            if (attachIndex)
            {
                int mx = SpreadUtils.SpreadMax(this.FInVertexBufferBinding, this.FInIndexBufferBinding);
                this.FOutput.SliceCount = mx;
            }
            else
            {
                this.FOutput.SliceCount = this.currentBufferCount;
            }

            for (int i = 0; i < this.currentBufferCount; i++)
            {
                if (this.FOutBuffer[i] == null)
                {
                    this.FOutBuffer[i] = new DX11Resource<DX11RawBuffer>();
                    this.FOutSlices[i] = new DX11Resource<IDX11Geometry>();
                }
            }

            for (int i = 0; i < this.FOutput.SliceCount; i++)
            {
                if (this.FOutput[i] == null)
                {
                    this.FOutput[i] = new DX11Resource<IDX11Geometry>();
                }
            }


        }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Render(DX11RenderContext context)
        {
            if (this.currentBufferCount == 0)
                return;

            Device device = context.Device;
            DeviceContext ctx = context.CurrentDeviceContext;

            //Just in case
            if (!this.updateddevices.Contains(context))
            {
                this.Update(context);
            }

            if (!this.FInLayer.IsConnected) { return; }

            if (this.rendereddevices.Contains(context)) { return; }

            if (this.FInEnabled[0])
            {
                if (this.BeginQuery != null)
                {
                    this.BeginQuery(context);
                }


                context.CurrentDeviceContext.OutputMerger.SetTargets(new RenderTargetView[0]);

                StreamOutputBufferBinding[] binding = new StreamOutputBufferBinding[this.currentBufferCount];
                for (int i = 0; i < this.currentBufferCount; i++)
                {
                    binding[i].Buffer = this.outputBuffer[i].D3DBuffer;
                    binding[i].Offset = 0;
                }
                ctx.StreamOutput.SetTargets(binding);

                settings.ViewportIndex = 0;
                settings.ViewportCount = 1;
                settings.View = Matrix.Identity;
                settings.Projection = Matrix.Identity;
                settings.ViewProjection = Matrix.Identity;
                settings.RenderWidth = 1;
                settings.RenderHeight =1;
                settings.RenderDepth =1;
                settings.BackBuffer = null;

                this.FInLayer.RenderAll(context, settings);

                ctx.StreamOutput.SetTargets(null);

                if (this.EndQuery != null)
                {
                    this.EndQuery(context);
                }

            }
        }



        public void Update(DX11RenderContext context)
        {
            if (this.currentBufferCount == 0)
                return;

            if (this.updateddevices.Contains(context)) { return; }
            if (this.invalidate || this.outputBuffer[0] == null)
            {
                this.DisposeBuffers(context);

                bool all = this.FInLayoutsElementCount[0] == -1;
                int currentOffset = 0;

                bool attachIndex = this.FAttachIndexBuffer[0];

                if (all)
                {
                    for (int index = 0; index < this.currentBufferCount; index++)
                    {
                        this.outputBuffer[index] = new StreamOutputBufferWithRawSupport(context, DeviceFormatHelper.ComputeVertexSize(this.FInVSize[index], this.FInLayouts.ToArray()), this.FInElemCount[index], 
                            this.FinOutputDrawMode[index], attachIndex, this.FInLayouts.ToArray());
                        this.FOutSlices[index][context] = this.outputBuffer[index].VertexGeometry;
                        this.FOutBuffer[index][context] = this.outputBuffer[index].RawBuffer;
                        if (!attachIndex)
                        {
                            this.FOutput[index][context] = this.outputBuffer[index].VertexGeometry;
                        }

                    }
                }
                else
                {
                    for (int index = 0; index < this.currentBufferCount; index++)
                    {

                        int elemCount = this.FInLayoutsElementCount[index];
                        elemCount = elemCount < 0 ? this.FInLayouts.SliceCount : elemCount;

                        InputElement[] elems = new InputElement[elemCount];
                        for (int j = 0; j < elems.Length; j++)
                        {
                            elems[j] = this.FInLayouts[currentOffset++];
                        }

                        this.outputBuffer[index] = new StreamOutputBufferWithRawSupport(context, DeviceFormatHelper.ComputeVertexSize(this.FInVSize[index], elems),
                            this.FInElemCount[index], this.FinOutputDrawMode[index], attachIndex, elems);
                        this.FOutSlices[index][context] = this.outputBuffer[index].VertexGeometry;
                        this.FOutBuffer[index][context] = this.outputBuffer[index].RawBuffer;
                        if (!attachIndex)
                        {
                            this.FOutput[index][context] = this.outputBuffer[index].VertexGeometry;
                        }
                    }
                }

                if (attachIndex)
                {
                    //Construct output binding instead, as a tuple v/i from slice max
                    int mx = SpreadUtils.SpreadMax(this.FInVertexBufferBinding, this.FInIndexBufferBinding);
                    for (int i = 0; i < mx; i++)
                    {
                        var vbo = this.outputBuffer[VMath.Zmod(this.FInVertexBufferBinding[i], this.currentBufferCount)];
                        var ibo = this.outputBuffer[VMath.Zmod(this.FInIndexBufferBinding[i], this.currentBufferCount)];

                        DX11IndexedGeometry ig = new DX11IndexedGeometry(context);
                        ig.HasBoundingBox = false;
                        ig.IndexBuffer = ibo.IndexBuffer;
                        ig.InputLayout = vbo.VertexGeometry.InputLayout;
                        ig.Topology = PrimitiveTopology.TriangleList;
                        ig.VertexBuffer = vbo.D3DBuffer;
                        ig.VertexSize = vbo.VertexGeometry.VertexSize;
                        ig.VerticesCount = vbo.VertexGeometry.VerticesCount;

                        this.FOutput[i][context] = ig;
                    }

                        
                }

            }

            this.updateddevices.Add(context);
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (force || this.FInKeepInMemory[0] == false)
            {
                this.DisposeBuffers(context);
            }
        }

        #region Dispose Buffers
        private void DisposeBuffers(DX11RenderContext context)
        {
            for (int i = 0; i < 4; i++)
            {
                if (this.outputBuffer[i] != null)
                {
                    this.outputBuffer[i].Dispose();
                    this.outputBuffer[i] = null;
                }
            }
        }
        #endregion

        public void Dispose()
        {
            for (int i = 0; i < 4; i++)
            {
                if (this.outputBuffer[i] != null)
                {
                    this.outputBuffer[i].Dispose();
                    this.outputBuffer[i] = null;
                }
            }
        }
    }


}
