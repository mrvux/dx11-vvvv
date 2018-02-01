using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;

using FeralTic.DX11;
using FeralTic.DX11.Queries;
using SlimDX;
using FeralTic.DX11.Resources;
using System.Reflection;
using SlimDX.Direct3D11;
using FeralTic.DX11.Geometry;

namespace VVVV.DX11.Nodes
{
    //[PluginInfo(Name = "ExtractChannel", Category = "DX11.Texture", Author = "vux")]
    public class ExtractChannelNode : IPluginEvaluate, IDX11LayerHost, IDX11Queryable
    {
        public enum Channel
        {
            Red,
            Green,
            Blue,
            Alpha       
        }

        private enum TexturePixelFormat
        {
            FloatOrUnorm,
            Int,
            Uint
        }

        [Input("Texture In")]
        protected Pin<DX11Resource<DX11Texture2D>> textureInput;

        [Input("Channel")]
        protected ISpread<Channel> channel;

        [Input("Single Channel Output")]
        protected ISpread<bool> singleChannelOut;

        [Input("Texture Out")]
        protected ISpread<DX11Resource<DX11Texture2D>> textureOutput;

        [Output("Message", Order = 5)]
        protected ISpread<string> message;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> queryable;

        private List<DX11ResourcePoolEntry<DX11RenderTarget2D>> framePool = new List<DX11ResourcePoolEntry<DX11RenderTarget2D>>();

        public void Evaluate(int SpreadMax)
        {
            
        }

        #region IDX11ResourceProvider Members
        public void Update(DX11RenderContext context)
        {
            if (this.BeginQuery != null)
            {
                this.BeginQuery(context);
            }

            if (this.EndQuery != null)
            {
                this.EndQuery(context);
            }

        }

        public void Destroy(DX11RenderContext context, bool force)
        {
 
        }
        #endregion

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;
    }
}
