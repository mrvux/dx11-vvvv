using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.DX11;
using VVVV.PluginInterfaces.V1;
using VVVV.Hosting.Pins;
using VVVV.Utils.VMath;
using VVVV.DX11.Lib.Devices;
using SlimDX.Direct3D11;
using VVVV.DX11.Internals.Helpers;
using SlimDX.DXGI;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.DX11.Lib.Rendering
{
    public enum eRenderFormatMode { Inherit, InheritSize, Manual }

    public struct TexInfo
    {
        public int w;
        public int h;
        public Format format;
    }

    public class RenderTargetManager 
    {
        private IPluginHost host;
        private IIOFactory iofactory;

        private IIOContainer<IDiffSpread<eRenderFormatMode>> pinmode;

        //Texture which will provide format
        private IIOContainer<Pin<DX11Resource<DX11Texture2D>>> texinputpin;

        //Manual Format
        private IIOContainer<IDiffSpread<EnumEntry>> FInFormat;

        //Current Mode
        private eRenderFormatMode currentmode;

        //Manual Size
        [Input("Texture Size", Order = 8, AsInt =true, DefaultValues = new double[] { 400, 300 },CheckIfChanged=true)]
        public IIOContainer<IDiffSpread<Vector2D>> FInTextureSize;

        [Input("Scale", Order = 9, DefaultValues = new double[] { 1,1 }, CheckIfChanged = true)]
        public IIOContainer<IDiffSpread<Vector2D>> FInTextureScale;

        public RenderTargetManager(IPluginHost host, IIOFactory iofactory)
        {
            this.host = host;
            this.iofactory = iofactory;

            ConfigAttribute cattr = new ConfigAttribute("Texture Input Mode");
            cattr.IsSingle = true;
            cattr.DefaultEnumEntry = "Manual";

            pinmode = this.iofactory.CreateIOContainer<IDiffSpread<eRenderFormatMode>>(cattr);   
            pinmode.IOObject.Changed += Pinmode_Changed;

            this.currentmode = eRenderFormatMode.Manual;

            this.CreateFormat();
            this.CreateSize();
        }

        public TexInfo GetRenderTarget(DX11RenderContext context)
        {
            TexInfo ti = new TexInfo();

            if (this.currentmode == eRenderFormatMode.Inherit)
            {
                if (this.texinputpin.IOObject.IsConnected)
                {
                    DX11Texture2D t = this.texinputpin.IOObject[0][context];

                    if (t.Resource != null)
                    {
                        ti.w = t.Width;
                        ti.h = t.Height;
                        if (DX11EnumFormatHelper.NullDeviceFormats.GetAllowedFormats(FormatSupport.RenderTarget).Contains(t.Format.ToString()))
                        {
                            ti.format = t.Format;
                        }
                        else
                        {
                            ti.format = DeviceFormatHelper.GetFormat(this.FInFormat.IOObject[0]);
                        }
                        
                    }
                    else
                    {
                        ti.w = (int)this.FInTextureSize.IOObject[0].x;
                        ti.h = (int)this.FInTextureSize.IOObject[0].y;
                        ti.format = DeviceFormatHelper.GetFormat(this.FInFormat.IOObject[0]);
                    }
                }
                else
                {
                    ti.w = (int)this.FInTextureSize.IOObject[0].x;
                    ti.h = (int)this.FInTextureSize.IOObject[0].y;
                    ti.format = DeviceFormatHelper.GetFormat(this.FInFormat.IOObject[0]);
                }
            }

            if (this.currentmode == eRenderFormatMode.InheritSize)
            {
                if (this.texinputpin.IOObject.IsConnected)
                {
                    DX11Texture2D t = this.texinputpin.IOObject[0][context];

                    if (t.Resource != null)
                    {
                        ti.w = t.Width;
                        ti.h = t.Height;
                    }
                    else
                    {
                        ti.w = (int)this.FInTextureSize.IOObject[0].x;
                        ti.h = (int)this.FInTextureSize.IOObject[0].y;
                    }
                }
                else
                {
                    ti.w = (int)this.FInTextureSize.IOObject[0].x;
                    ti.h = (int)this.FInTextureSize.IOObject[0].y;
                }

                ti.format = DeviceFormatHelper.GetFormat(this.FInFormat.IOObject[0]);
            }

            if (this.currentmode == eRenderFormatMode.Manual)
            {
                ti.w = (int)this.FInTextureSize.IOObject[0].x;
                ti.h = (int)this.FInTextureSize.IOObject[0].y;
                ti.format = DeviceFormatHelper.GetFormat(this.FInFormat.IOObject[0]);
            }

            ti.w = Convert.ToInt32((double)ti.w * this.FInTextureScale.IOObject[0].x);
            ti.h = Convert.ToInt32((double)ti.h * this.FInTextureScale.IOObject[0].y);

            return ti;
        }

        private void CreateSize()
        {
            if (this.FInTextureSize == null)
            {
                InputAttribute a = new InputAttribute("Texture Size");
                a.Order = 8;
                a.DefaultValues = new double[] { 400, 300 };
                a.CheckIfChanged = true;
                a.AsInt = true;

                this.FInTextureSize = this.iofactory.CreateIOContainer<IDiffSpread<Vector2D>>(a);

                a.Name = "Texture Scale";
                a.DefaultValues = new double[] { 1, 1 };
                this.FInTextureScale = this.iofactory.CreateIOContainer<IDiffSpread<Vector2D>>(a);
            }
        }

        private void CreateFormat()
        {
            if (this.FInFormat == null)
            {
                string ename = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.RenderTarget);

                InputAttribute tattr = new InputAttribute("Target Format");
                tattr.EnumName = ename;
                tattr.DefaultEnumEntry = "R8G8B8A8_UNorm";
                tattr.DefaultString = "R8G8B8A8_UNorm";
                tattr.AllowDefault = true;
                tattr.Order = 0;
                tattr.CheckIfChanged = true;
                tattr.DefaultString = "R8G8B8A8_UNorm";

                this.FInFormat = this.iofactory.CreateIOContainer<IDiffSpread<EnumEntry>>(tattr);
            }
        }

        private void CreateTextureIn()
        {
            if (this.texinputpin == null)
            {
                InputAttribute tattr = new InputAttribute("Texture In");
                tattr.Order = -1;

                this.texinputpin = this.iofactory.CreateIOContainer<Pin<DX11Resource<DX11Texture2D>>>(tattr);
            }
        }

        private void DisposeTexIn()
        {
            if (this.texinputpin != null)
            {
                this.texinputpin.Dispose();
                this.texinputpin = null;
            }
        }

        private void Pinmode_Changed(IDiffSpread<eRenderFormatMode> spread)
        {
            if (this.currentmode == eRenderFormatMode.Manual)
            {
                this.DisposeTexIn();
            }

            this.currentmode = spread[0];

            if (this.currentmode == eRenderFormatMode.Inherit)
            {
                this.CreateTextureIn();
            }

            if (this.currentmode == eRenderFormatMode.InheritSize)
            {
                this.CreateTextureIn();
            }       
        }
    }
}
