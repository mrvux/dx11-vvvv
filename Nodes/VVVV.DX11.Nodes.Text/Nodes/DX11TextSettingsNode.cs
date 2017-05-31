using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeralTic.DX11;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;

namespace VVVV.DX11.Nodes.Text
{
    [PluginInfo(Name = "TextSettings", Author = "vux", Category = "DX11", Version = "Advanced")]
    public class DX11TextSettingsNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        private readonly SlimDX.DirectWrite.Factory dwFactory;

        [Input("Glyph Sheet Width", DefaultValue = 512, IsSingle = true, MinValue = 32)]
        public IDiffSpread<int> sheetSizeX;

		[Input("Glyph Sheet Height", DefaultValue = 512, IsSingle = true, MinValue = 32)]
        public IDiffSpread<int> sheetSizeY;

		[Input("Max Glyph Width", DefaultValue = 384, IsSingle = true, MinValue = 32)]
        public IDiffSpread<int> glyphWidth;

		[Input("Max Glyph Height", DefaultValue = 384, IsSingle = true, MinValue = 32)]
        public IDiffSpread<int> glyphHeight;

		[Input("Sheet Mip Levels", DefaultValue = 5, IsSingle = true, MinValue = 1)]
        public IDiffSpread<int> sheetMips;

		[Input("Anisotropic Filtering", DefaultValue = 1, IsSingle = true)]
        public IDiffSpread<bool> aniso;

		[Output("Output", IsSingle = true)]
        public ISpread<DX11Resource<TextFontRenderer>> FOutTextWrapper;

        [ImportingConstructor()]
        public DX11TextSettingsNode(SlimDX.DirectWrite.Factory dwFactory)
        {
            this.dwFactory = dwFactory;
        }

        public void Evaluate(int SpreadMax)
        {
            if (SpreadUtils.AnyChanged(this.sheetMips, this.sheetSizeX, this.sheetSizeY, this.glyphWidth, this.glyphHeight, this.aniso))
            {
                this.FOutTextWrapper.SafeDisposeAll();
            }

            if (this.FOutTextWrapper[0] == null)
            {
                this.FOutTextWrapper[0] = new DX11Resource<TextFontRenderer>();
            }
        }

        public void Update(DX11RenderContext context)
        {
            if (!this.FOutTextWrapper[0].Contains(context))
            {
                SharpFontWrapper.FontWrapperCreationParameters createParams = new SharpFontWrapper.FontWrapperCreationParameters()
                {
                    AnisotropicFiltering = aniso[0] ? 1 : 0,
                    GlyphSheetWidth = sheetSizeX[0],
                    GlyphSheetHeight = sheetSizeY[0],
                    DisableGeometryShader = 0,
                    MaxGlyphCountPerSheet = 0,
                    MaxGlyphHeight = glyphHeight[0],
                    MaxGlyphWidth = glyphWidth[0],
                    SheetMipLevels = sheetMips[0],
                    VertexBufferSize = 0,
                    DefaultFontParams = new SharpFontWrapper.DirectWriteFontParameters()
                    {
                        FontFamily = "Arial",
                        FontStretch = SharpDX.DirectWrite.FontStretch.Normal,
                        FontStyle = SharpDX.DirectWrite.FontStyle.Normal,
                        FontWeight = SharpDX.DirectWrite.FontWeight.Normal,
                    }
                };

                var wrapper = FontWrapperFactory.GetFactory().CreateFontWrapper(
                    new SharpDX.Direct3D11.Device(context.Device.ComPointer),
                    new SharpDX.DirectWrite.Factory(this.dwFactory.ComPointer), ref createParams);
                this.FOutTextWrapper[0][context] = new TextFontRenderer(wrapper);
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {

        }

        public void Dispose()
        {
            this.FOutTextWrapper.SafeDisposeAll();
        }
    }
}
