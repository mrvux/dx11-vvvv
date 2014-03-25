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
    public class DX11_TextureQRCodeNode : IPluginEvaluate, IDX11ResourceProvider, IDisposable
    {
        //little helper class used to store information for each
        //texture resource
       /* public class Info
        {
            public int Slice;
            public int PixelSize;
            public string Text;
            public ErrorCorrectionLevel ECLevel;
            public QuietZoneModules QZModules;
            public Bitmap QRCodeBMP;
        }*/

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

        private MemoryStream FMemoryStream;

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

            /*    FTextureOut.ResizeAndDispose(spreadMax, CreateTextureResource);
            for (int i = 0; i < spreadMax; i++)
            {
                var textureResource = FTextureOut[i];
                var info = textureResource.Metadata;
                //recreate textures if resolution was changed
                if (info.PixelSize != FPixelSize[i] || info.Text != FText[i] || info.ECLevel != FErrorCorrectionLevel[i] || info.QZModules != FQuietZoneModules[i])
                {
                    textureResource.Dispose();
                    textureResource = CreateTextureResource(i);
                    info = textureResource.Metadata;
                }

                //update textures if their colors changed
                if (FBackColor.IsChanged || FForeColor.IsChanged)
                {
                    ComputeQRCode(info, i);
                    textureResource.NeedsUpdate = true;
                }
                else
                    textureResource.NeedsUpdate = false;

                FTextureOut[i] = textureResource;
            }*/
        }

        Bitmap ComputeQRCode(int slice)
        {
            var qrEncoder = new QrEncoder(FErrorCorrectionLevel[slice]);
            var qrCode = new QrCode();
            if (qrEncoder.TryEncode(FText[slice], out qrCode))
            {
                using (var fore = new SolidBrush(FForeColor[slice].Color))
                using (var back = new SolidBrush(FBackColor[slice].Color))
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

        public void Update(IPluginIO pin, FeralTic.DX11.DX11RenderContext context)
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

        public void Destroy(IPluginIO pin, FeralTic.DX11.DX11RenderContext context, bool force)
        {
            for (int i = 0; i < this.FTextureOut.SliceCount; i++)
            {
                if (this.FTextureOut[i] != null)
                {
                    this.FTextureOut[i].Dispose(context);
                }
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < this.FTextureOut.SliceCount; i++)
            {
                if (this.FTextureOut[i] != null)
                {
                    this.FTextureOut[i].Dispose();
                }
            }
        }
    }
}
