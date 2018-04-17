using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using FeralTic.Resources.Geometry;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using FeralTic.DX11;
using FeralTic.DX11.Resources;
using VVVV.Utils.VMath;

namespace VVVV.DX11.Nodes
{
    public class CopySubArray : IDX11Resource, IDisposable
    {
        private DX11RenderContext context;

        private DX11RenderTextureArray rtarr;
        public DX11RenderTextureArray Result { get { return rtarr; } }


        public CopySubArray(DX11RenderContext context)
        {
            this.context = context;
        }


        public void Reset(DX11Texture2D texture, int w, int h, int d, SlimDX.DXGI.Format format)
        {
            format = format == SlimDX.DXGI.Format.Unknown ? texture.Format : format;
            this.rtarr.Dispose();
            this.rtarr = new DX11RenderTextureArray(this.context, w, h, d, format, true, 1);
        }


        public void Apply(DX11Resource<DX11RenderTextureArray> textureArray, ISpread<int> slices)
        {
            int w = textureArray[context].Width;
            int h = textureArray[context].Height;
            int d = slices.SliceCount;
            Format f = textureArray[context].Format;

            Texture2DDescription descIn = textureArray[context].Resource.Description;

            // check if parameters match - if not, create a new rt array
            if (this.rtarr != null)
            {
                if (this.rtarr.ElemCnt != slices.SliceCount || 
                    this.rtarr.Width != w || 
                    this.rtarr.Height != h || 
                    this.rtarr.Format != f)
                {
                    this.rtarr.Dispose(); this.rtarr = null;
                }
            }

            if (this.rtarr == null)
            {
                this.rtarr = new DX11RenderTextureArray(this.context, w, h, d, f, true, 1);
            }

            // copy the ressources over
            for (int i = 0; i < slices.SliceCount; i++)
            {
                int slice = VMath.Zmod(slices[i], textureArray[context].ElemCnt);

                SlimDX.Direct3D11.Resource source = textureArray[context].Resource;

                int sourceSubres = SlimDX.Direct3D11.Texture2D.CalculateSubresourceIndex(0, slice, descIn.MipLevels);

                int destinationSubres = SlimDX.Direct3D11.Texture2D.CalculateSubresourceIndex(0, i, descIn.MipLevels);

                context.CurrentDeviceContext.CopySubresourceRegion(source, sourceSubres, this.rtarr.Resource, destinationSubres, 0, 0, 0);
            }
            
        }

       
        public void Dispose()
        {
            if (this.rtarr != null) { this.rtarr.Dispose(); }
        }
    }
}
