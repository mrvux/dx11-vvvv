#region usings
using System;
using System.ComponentModel.Composition;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

using SlimDX;
using SlimDX.Direct3D11;
using VVVV.Core.Logging;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.EX9;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using FeralTic.DX11.Resources;
using VVVV.DX11;


#endregion usings

namespace VVVV.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "QRCode",
                Category = "DX11.Texture",
                Author="vux",Credits="vvvv group",
                Help = "Encodes a string to be displayed as a QR code symbol on a texture. QR code is trademarked by Denso Wave, Inc.", Tags = "")]
    #endregion PluginInfo
    public class DX11_TextureQRCodeNode : IPluginEvaluate, IDX11ResourceHost, IDisposable
    {
        [Input("Text", DefaultString = "vvvv")]
        public IDiffSpread<string> FText;

        [Input("Pixel Size", DefaultValue = 5, MinValue = 1)]
        public IDiffSpread<int> FPixelSize;

        [Input("Back Color", DefaultColor = new double[] { 1, 1, 1, 1 })]
        public IDiffSpread<RGBAColor> FBackColor;

        [Input("Fore Color", DefaultColor = new double[] { 0, 0, 0, 1 })]
        public IDiffSpread<RGBAColor> FForeColor;

        [Input("Error Correction Level", DefaultEnumEntry = "H", Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<ErrorCorrectionLevel> FErrorCorrectionLevel;

        [Input("Quiet Zone Modules", DefaultEnumEntry = "Two", Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<QuietZoneModules> FQuietZoneModules;

        [Output("Texture Out")]
        public ISpread<DX11Resource<DX11Texture2D>> FTextureOut;
        /*public ISpread<TextureResource<Info>> FTextureOut;*/

        [Import()]
        public ILogger FLogger;

        //called when data for any output pin is requested
        public void Evaluate(int spreadMax)
        {
            if (this.FText.IsChanged || this.FPixelSize.IsChanged
                || this.FBackColor.IsChanged || this.FForeColor.IsChanged
                || this.FErrorCorrectionLevel.IsChanged || this.FQuietZoneModules.IsChanged)
                
            {
                for (int i = 0; i < this.FTextureOut.SliceCount; i++)
                {
                    if (this.FTextureOut[i] != null)
                    {
                        this.FTextureOut[i].Dispose();
                    }
                }
            }

            this.FTextureOut.SliceCount = spreadMax;
            for (int i = 0; i < this.FTextureOut.SliceCount; i++)
            {
                if (this.FTextureOut[i] == null)
                {
                    this.FTextureOut[i] = new DX11Resource<DX11Texture2D>();
                }
            }
        }

        Bitmap ComputeQRCode(int slice)
        {
            var qrEncoder = new QrEncoder(FErrorCorrectionLevel[slice]);
            var qrCode = new QrCode();
            if (qrEncoder.TryEncode(FText[slice], out qrCode))
            {
                var fc = FForeColor[slice];
                var bc = FBackColor[slice];
                fc = new RGBAColor(fc.B, fc.G, fc.R, fc.A);
                bc = new RGBAColor(bc.B, bc.G, bc.R, bc.A);
                using (var fore = new SolidBrush(fc.Color))
                using (var back = new SolidBrush(bc.Color))
                {
                    var renderer = new GraphicsRenderer(new FixedModuleSize(FPixelSize[slice], FQuietZoneModules[slice]), fore, back);
                    DrawingSize dSize = renderer.SizeCalculator.GetSize(qrCode.Matrix.Width);
                    var bmp = new Bitmap(dSize.CodeWidth, dSize.CodeWidth);
                    using (var g = Graphics.FromImage(bmp))
                        renderer.Draw(g, qrCode.Matrix);


                    return bmp;
                }
            }
            else
                return null;
        }

        public void Update(FeralTic.DX11.DX11RenderContext context)
        {
            for (int i = 0; i < this.FTextureOut.SliceCount; i++)
            {
                if (!this.FTextureOut[i].Contains(context))
                {
                    Bitmap bmp = ComputeQRCode(i);

                    DX11DynamicTexture2D tex = new DX11DynamicTexture2D(context, bmp.Width, bmp.Height, SlimDX.DXGI.Format.R8G8B8A8_UNorm);

                    int pitch = tex.GetRowPitch();

                    var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);

                    if (pitch != bmp.Width * 4)
                    {
                        tex.WriteDataPitch(data.Scan0, bmp.Width * bmp.Height * 4);
                    }
                    else
                    {
                        tex.WriteData(data.Scan0, bmp.Width * bmp.Height * 4);
                    }

                    this.FTextureOut[i][context] = tex;
                    
                }
            }
        }

        public void Destroy(FeralTic.DX11.DX11RenderContext context, bool force)
        {
            this.FTextureOut.SafeDisposeAll(context);
        }

        public void Dispose()
        {
            this.FTextureOut.SafeDisposeAll();
        }
    }
}
