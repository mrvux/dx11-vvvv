using System;
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
using FeralTic.DX11.Resources;
using FeralTic.DX11;
using FeralTic.DX11.Queries;
using VVVV.DX11.Lib;
using VVVV.DX11.Internals.Helpers;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Renderer", Category = "DX11",Version="Texture1dArray", Author = "vux")]
    public class DX11Texture1dArrayRendererNode : IDX11RendererHost, IPluginEvaluate, IDisposable, IDX11Queryable
    {
        protected IPluginHost FHost;

        IDiffSpread<EnumEntry> FInFormat;

        [Input("Layer", Order = 1)]
        protected Pin<DX11Resource<DX11Layer>> FInLayer;

        [Input("Width", DefaultValue = 256, Order = 5)]
        protected IDiffSpread<int> FInSize;

        [Input("Element Count", DefaultValue = 1, Order = 4)]
        protected IDiffSpread<int> FInElementCount;

        [Input("Enabled", DefaultValue = 1, Order = 9)]
        protected ISpread<bool> FInEnabled;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        [Output("Texture Out", Order = 2, IsSingle = true)]
        protected ISpread<DX11Resource<DX11WriteableTexture1dArray>> FOutTexture;

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;

        protected SampleDescription sd = new SampleDescription(1, 0);

        protected List<DX11RenderContext> updateddevices = new List<DX11RenderContext>();
        protected List<DX11RenderContext> rendereddevices = new List<DX11RenderContext>();
        private int spmax;

        private DX11RenderSettings settings = new DX11RenderSettings();

        #region Constructor
        [ImportingConstructor()]
        public DX11Texture1dArrayRendererNode(IPluginHost FHost, IIOFactory iofactory)
        {
            string ename = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.UnorderedAccessView);

            InputAttribute tattr = new InputAttribute("Target Format");
            tattr.EnumName = ename;
            tattr.DefaultEnumEntry = "R8G8B8A8_UNorm";

            this.FInFormat = iofactory.CreateDiffSpread<EnumEntry>(tattr);
        }
        #endregion

        public void Evaluate(int SpreadMax)
        {
            this.spmax = SpreadMax;
            this.rendereddevices.Clear();
            this.updateddevices.Clear();

            if (this.FOutTexture[0] == null)
            {
                this.FOutTexture[0] = new DX11Resource<DX11WriteableTexture1dArray>();
            }

            if (this.FInFormat.IsChanged
                || this.FInSize.IsChanged
                || this.FInElementCount.IsChanged)
            {
                this.FOutTexture[0].Dispose();
            }
        }


        public void Update(DX11RenderContext context)
        {
            Device device = context.Device;

            if (this.spmax == 0) { return; }

            if (this.updateddevices.Contains(context)) { return; }

            if (!this.FOutTexture[0].Contains(context))
            {
                this.FOutTexture[0][context] = new DX11WriteableTexture1dArray(context, this.FInSize[0], this.FInElementCount[0], DeviceFormatHelper.GetFormat(this.FInFormat[0].Name));
            }

            this.updateddevices.Add(context);
        }

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
                if (this.BeginQuery != null)
                {
                    this.BeginQuery(context);
                }

                DX11WriteableTexture1dArray target = this.FOutTexture[0][context];

                if (this.FInLayer.PluginIO.IsConnected)
                {

                    int size = this.FInSize[0];

                    settings.ViewportIndex = 0;
                    settings.ViewportCount = 1;


                    settings.View = Matrix.Identity;
                    settings.Projection = Matrix.Identity;
                    settings.ViewProjection = Matrix.Identity;
                    settings.RenderWidth = size;
                    settings.RenderHeight = this.FInElementCount[0];
                    settings.BackBuffer = target;
                    settings.CustomSemantics.Clear();
                    settings.ResourceSemantics.Clear();

                    for (int j = 0; j < this.FInLayer.SliceCount; j++)
                    {
                        try
                        {
                            this.FInLayer[j][context].Render(context, settings);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                }

                if (this.EndQuery != null)
                {
                    this.EndQuery(context);
                }

                this.rendereddevices.Add(context);
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            this.FOutTexture.SafeDisposeAll(context);
        }

        public bool IsEnabled
        {
            get { return this.FInEnabled[0]; }
        }

        public void Dispose()
        {
            if (this.FOutTexture[0] != null) { this.FOutTexture[0].Dispose(); }
        }
    }
}
