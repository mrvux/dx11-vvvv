using FeralTic.DX11;
using SharpFontWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11.Nodes.Text
{
    [Startable(Name = "Font Factory", Lazy = false)]
    public class FontWrapperFactory : IStartable
    {
        private static Factory fontFactory;
        private static FontWrapper fontWrapper;

        public static Factory GetFactory()
        {
            if (fontFactory == null)
            {
                fontFactory = new Factory();
            }
            return fontFactory;
        }

        public static FontWrapper GetWrapper(DX11RenderContext context, SlimDX.DirectWrite.Factory dwFactory)
        {
            if (fontWrapper == null)
            {
                FontWrapperCreationParameters p = new FontWrapperCreationParameters()
                {
                    SheetMipLevels = 5,
                    AnisotropicFiltering = 1,
                    DefaultFontParams = new DirectWriteFontParameters()
                    {
                        FontFamily = "Arial",
                        FontStretch = SharpDX.DirectWrite.FontStretch.Normal,
                        FontStyle = SharpDX.DirectWrite.FontStyle.Normal,
                        FontWeight = SharpDX.DirectWrite.FontWeight.Normal,
                    },
                    DisableGeometryShader = 0,
                    GlyphSheetHeight = 0,
                    GlyphSheetWidth =0,
                    MaxGlyphCountPerSheet = 0,
                    MaxGlyphHeight = 0,
                    MaxGlyphWidth = 0,
                    VertexBufferSize = 0
                };

                var factory = GetFactory();
                fontWrapper = factory.CreateFontWrapper(new SharpDX.Direct3D11.Device(context.Device.ComPointer), new SharpDX.DirectWrite.Factory(dwFactory.ComPointer), ref p);
            }
            return fontWrapper;
        }

        public void Shutdown()
        {
            if (fontWrapper != null)
            {
                fontWrapper.Dispose();
                fontWrapper = null;
            }
            if (fontFactory != null)
            {
                fontFactory.Dispose();
                fontFactory = null;
            }
        }



        public void Start()
        {
            
        }
    }
}
