﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Device = SlimDX.Direct3D11.Device;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Hosting.Pins;
using VVVV.Hosting.Pins.Config;

using FeralTic.DX11;
using FeralTic.DX11.Resources;

using VVVV.DX11;
using VVVV.DX11.Internals.Helpers;

namespace VVVV.DX11.Lib.Rendering
{
    public enum eDepthBufferMode { None, Standard, ReadOnly }

    public class DepthBufferManager : IDisposable
    {
        private IPluginHost host;
        private IIOFactory factory;

        private IDiffSpread<eDepthBufferMode> pinmode;

        private eDepthBufferMode currentmode;

        private IIOContainer<Pin<DX11Resource<DX11DepthStencil>>> depthinputpin;

        private IIOContainer<Pin<DX11Resource<DX11DepthStencil>>> depthoutputpin;

        private IIOContainer<IDiffSpread<EnumEntry>> depthformatpin;

        public bool NeedReset { get; set; }

        public bool FormatChanged { get; set; }

        public eDepthBufferMode Mode { get { return this.currentmode; } }
         
        public DepthBufferManager(IPluginHost host, IIOFactory factory)
        {
            this.host = host;
            this.factory = factory;

            ConfigAttribute cattr = new ConfigAttribute("Depth Buffer Mode");
            cattr.IsSingle = true;
            pinmode = this.factory.CreateDiffSpread<eDepthBufferMode>(cattr);

            pinmode.Changed += Pinmode_Changed;

            this.currentmode = eDepthBufferMode.None;
        }

        private void Pinmode_Changed(IDiffSpread<eDepthBufferMode> spread)
        {
            if (this.currentmode != spread[0])
            {

                if (this.currentmode == eDepthBufferMode.Standard)
                {
                    if (this.depthoutputpin != null)
                    {
                        //Destroy depth stencil
                        if (this.depthoutputpin.IOObject[0] != null)
                        {
                            this.depthoutputpin.IOObject[0].Dispose();
                        }

                        this.depthoutputpin.Dispose();
                        this.depthformatpin.Dispose();

                        this.depthformatpin = null;
                        this.depthoutputpin = null;
                    }
                }

                if (this.currentmode == eDepthBufferMode.ReadOnly)
                {
                    if (this.depthinputpin != null)
                    {
                        this.depthinputpin.Dispose();
                        this.depthinputpin = null;
                    }
                }

                this.currentmode = spread[0];
                if (this.currentmode == eDepthBufferMode.Standard)
                {
                    OutputAttribute oattr = new OutputAttribute("Depth Buffer");
                    oattr.IsSingle = true;

                    this.depthoutputpin = this.factory.CreateIOContainer<Pin<DX11Resource<DX11DepthStencil>>>(oattr);
                    this.depthoutputpin.IOObject[0] = new DX11Resource<DX11DepthStencil>();

                    ConfigAttribute dfAttr = new ConfigAttribute("Depth Buffer Format");
                    dfAttr.EnumName = DX11EnumFormatHelper.NullDeviceFormats.GetEnumName(FormatSupport.DepthStencil);
                    dfAttr.DefaultEnumEntry = DX11EnumFormatHelper.NullDeviceFormats.GetAllowedFormats(FormatSupport.DepthStencil)[0];
                    dfAttr.IsSingle = true;

                    this.depthformatpin = this.factory.CreateIOContainer<IDiffSpread<EnumEntry>>(dfAttr);
                    this.depthformatpin.IOObject[0] = new EnumEntry(dfAttr.EnumName, 1);

                    this.depthformatpin.IOObject.Changed += depthformatpin_Changed;
                }

                if (this.currentmode == eDepthBufferMode.ReadOnly)
                {
                    InputAttribute oattr = new InputAttribute("Depth Buffer");
                    oattr.IsSingle = true;

                    this.depthinputpin = this.factory.CreateIOContainer<Pin<DX11Resource<DX11DepthStencil>>>(oattr);
                }

                this.NeedReset = true;
            }
        }

        private void depthformatpin_Changed(IDiffSpread<EnumEntry> spread)
        {
            this.NeedReset = true;
            this.FormatChanged = true;
        }

        public void Update(DX11RenderContext context, int w, int h,SampleDescription sd)
        {
            if (this.currentmode == eDepthBufferMode.Standard)
            {
                DX11DepthStencil ds;
                if (this.NeedReset || !this.depthoutputpin.IOObject[0].Data.ContainsKey(context))
                {
                    if (this.depthoutputpin.IOObject[0] != null)
                    {
                        this.depthoutputpin.IOObject[0].Dispose(context);
                    }

                    if (sd.Count > 1)
                    {
                        if (!context.IsAtLeast101)
                        {
                            host.Log(TLogType.Warning, "Device Feature Level Needs at least 10.1 to create Multisampled Depth Buffer, rolling back to 1");
                            sd.Count = 1;
                        }
                    }

                    ds = new DX11DepthStencil(context, w, h, sd, DeviceFormatHelper.GetFormat(this.depthformatpin.IOObject[0].Name));
                    #if DEBUG
                    ds.Resource.DebugName = "DepthStencil";
                    #endif
                    this.depthoutputpin.IOObject[0][context] = ds;
                } 
            }
        }

        public void Destroy(DX11RenderContext context)
        {
            //We only own the depth in standard mode
            if (this.currentmode == eDepthBufferMode.Standard)
            {
                if (this.depthoutputpin.IOObject[0] != null)
                {
                    this.depthoutputpin.IOObject[0].Dispose(context);
                }
            }
        }

        public bool HasDSV
        {
            get { return this.currentmode != eDepthBufferMode.None; }
        }

        public DX11DepthStencil GetDepthStencil(DX11RenderContext context)
        {
            if (this.currentmode == eDepthBufferMode.ReadOnly)
            {
                if (this.depthinputpin.IOObject.PluginIO.IsConnected)
                {
                    return this.depthinputpin.IOObject[0][context];
                }
                else
                {
                    return null;
                }
            }
            if (this.currentmode == eDepthBufferMode.Standard) { return this.depthoutputpin.IOObject[0][context]; }
            return null;           
        }

        public DepthStencilView GetDSV(DX11RenderContext context)
        {

            if (this.currentmode == eDepthBufferMode.ReadOnly)
            {
                if (this.depthinputpin.IOObject.PluginIO.IsConnected)
                {
                    return this.depthinputpin.IOObject[0][context].ReadOnlyDSV;
                }
                else
                {
                    return null;
                }
            }
            if (this.currentmode == eDepthBufferMode.Standard) { return this.depthoutputpin.IOObject[0][context].DSV; }
            return null;
        }
        

        public void Dispose()
        {
            if (this.currentmode == eDepthBufferMode.Standard)
            {
                if (this.depthoutputpin.IOObject[0] != null)
                {
                    this.depthoutputpin.IOObject[0].Dispose();
                }
            }
        }

        public void Clear(DX11RenderContext context)
        {
            if (this.currentmode == eDepthBufferMode.Standard)
            {
                context.CurrentDeviceContext.ClearDepthStencilView(this.depthoutputpin.IOObject[0][context].DSV, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
            }
        }
    }
}
