using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using SlimDX;
using SlimDX.Direct3D11;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using SlimDX.DXGI;
using VVVV.DX11.Internals;
using FeralTic.DX11.Utils;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name="GeometryBuffer",Category="DX11.Geometry",Version="Join",Author="vux")]
    public unsafe class GeometryBufferJoinNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        IPluginHost FHost;

        IValueFastIn FInput;

        [Input("Vertices Count", Order=1, AutoValidate=false,DefaultValue=1)]
        protected ISpread<int> FInVerticesCount;

        [Input("Topology", Order = 2,AutoValidate=false)]
        protected ISpread<PrimitiveTopology> FInTopology;

        [Input("Input Layout", Order = 3, AutoValidate=false)]
        protected Pin<InputElement> FInLayout;

        [Input("Apply", Order = 4, IsBang = true, DefaultValue = 1)]
        protected ISpread<bool> FInApply;

        [Output("Geometry Out", Order = 5, IsSingle = true)]
        protected Pin<DX11Resource<DX11VertexGeometry>> FOutput;

        private bool FInvalidate;
        private DataStream FStream;
        private int vertexsize;
        private InputElement[] inputlayout;
        private bool FFirst = true;

        [ImportingConstructor()]
        public GeometryBufferJoinNode(IPluginHost host)
        {
            this.FHost = host;

            this.FHost.CreateValueFastInput("Vertices", 1, null, TSliceMode.Dynamic, TPinVisibility.True, out this.FInput);
            this.FInput.SetSubType(double.MinValue, double.MaxValue, 0.01, 0, false, false, false);
            this.FInput.Order = 0;
            this.FInput.AutoValidate = false;
               
        }

        public void Evaluate(int SpreadMax)
        {
            this.FInvalidate = false;
            if (this.FOutput[0] == null)
            {
                this.FOutput[0] = new DX11Resource<DX11VertexGeometry>();
            }

            if (this.FInApply[0] || this.FFirst)
            {
                this.FFirst = false;

                this.FInput.Validate();
                this.FInLayout.Sync();
                this.FInTopology.Sync();
                this.FInVerticesCount.Sync();

                this.FInvalidate = true;
                DX11Resource<DX11VertexGeometry> instance = this.FOutput[0];
                this.FOutput[0] = instance;

                this.inputlayout = new InputElement[this.FInLayout.SliceCount];
                this.vertexsize = 0;
				for (int i = 0; i < this.FInLayout.SliceCount; i++)
				{

					if (this.FInLayout.IsConnected && this.FInLayout[i] != null)
					{
                        this.inputlayout[i] = this.FInLayout[i];
					}
					else
					{
                        //Set deault, can do better here
                        this.inputlayout[i] = new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0);
					}
                    this.vertexsize += FormatHelper.Instance.GetSize(this.inputlayout[i].Format);
				}
				InputLayoutFactory.AutoIndex(this.inputlayout);


                if (this.FStream != null) { this.FStream.Dispose(); }
                this.FStream = new DataStream(this.FInVerticesCount[0] * this.vertexsize, true, true);

                double* ptr;
                int ptrcnt;
                this.FInput.GetValuePointer(out ptrcnt, out ptr);

                for (int i = 0; i < this.FInVerticesCount[0] * (this.vertexsize / 4); i++)
                {
                    this.FStream.Write((float)ptr[i % ptrcnt]);
                }

                DX11Resource<DX11VertexGeometry> v = this.FOutput[0];
                this.FOutput[0] = v;

                
            }
        }

        #region IDX11Resource Members
        public void Update(DX11RenderContext context)
        {
            if (this.FInvalidate)
            {
                if (this.FOutput[0].Contains(context)) { this.FOutput[0].Dispose(context); }

                DX11VertexGeometry geom = new DX11VertexGeometry(context);
                geom.InputLayout = this.inputlayout;
                geom.VertexSize = this.vertexsize;

				try
				{
                    this.FStream.Position = 0;
					var vertices = new SlimDX.Direct3D11.Buffer(context.Device, this.FStream, new BufferDescription()
					{
						BindFlags = BindFlags.VertexBuffer,
						CpuAccessFlags = CpuAccessFlags.None,
						OptionFlags = ResourceOptionFlags.None,
						SizeInBytes = (int)this.FStream.Length,
						Usage = ResourceUsage.Default
					});

                    geom.VerticesCount = this.FInVerticesCount[0];
                    geom.VertexBuffer = vertices;
                    geom.Topology = this.FInTopology[0];
                    geom.HasBoundingBox = false;

                    /*if (geom.Topology == PrimitiveTopology.LineListWithAdjacency)
                    {
                        geom.VerticesCount /= 2;
                    }*/

                    this.FOutput[0][context] = geom;
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
				} 
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutput[0].Dispose(context);
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            if (this.FOutput[0] != null) { this.FOutput[0].Dispose(); }
        }
        #endregion
    }
}
