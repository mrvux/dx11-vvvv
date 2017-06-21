using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;


using FeralTic.DX11;
using FeralTic.DX11.Queries;

using VVVV.DX11.Lib.Devices;
using VVVV.DX11.Lib.RenderGraph;
using SlimDX.Direct3D11;
using FeralTic.Utils;


namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Info", Category = "DX11",Version="", Author = "vux",Tags= "debug", AutoEvaluate=true)]
    public class DX11RenderContextNode : IPluginEvaluate, IDX11Queryable
    {
        [Input("Refresh",IsBang=true)]
        protected ISpread<bool> FInRefresh;

        [Input("Clear Unlocked",IsBang=true)]
        protected ISpread<bool> FInClearU;

        [Input("Clear Cache", IsBang = true)]
        protected ISpread<bool> FInClear;

        [Output("Adapter Name")]
        protected ISpread<string> FOutAdapter;

        [Output("Feature Level")]
        protected ISpread<string> FOutFeatureLevel;

        [Output("Render Target Count")]
        protected ISpread<int> FOutRTCount;

        [Output("Buffer Count")]
        protected ISpread<int> FOutBufferCount;

        [Output("RenderTarget Stack Count")]
        protected ISpread<int> FOutRTStack;

        [Output("RenderState Stack Count")]
        protected ISpread<int> FOutRSStack;

        [Output("Last Frame Pins")]
        protected ISpread<int> FOutLastFrame;

        [Output("This Frame Pins")]
        protected ISpread<int> FOutThisFrame;

        [Output("Graph Nodes Count")]
        protected ISpread<int> FOutNodeCount;

        [Output("Processed Nodes Count")]
        protected ISpread<int> FOutProcessedCount;

        [Output("Pending Pins Count",IsSingle =true)]
        protected ISpread<int> FOutPPCount;

        [Output("Pending Links Count",IsSingle=true)]
        protected ISpread<int> FOutPLCount;

        [Output("Buffer Support")]
        protected ISpread<bool> FOUCS;

        [Output("Creation Flags")]
        protected ISpread<DeviceCreationFlags> FOutFlags;

        [Output("Query", Order = 200, IsSingle = true)]
        protected ISpread<IDX11Queryable> FOutQueryable;

        bool first = true;

        #region IPluginEvaluate Members
        public void Evaluate(int SpreadMax)
        {
            if (this.FOutQueryable[0] == null) { this.FOutQueryable[0] = this; }

            if (first)
            {
                DX11GlobalDevice.OnBeginRender += new EventHandler(DX11GlobalDevice_OnBeginRender);
                DX11GlobalDevice.OnEndRender += new EventHandler(DX11GlobalDevice_OnEndRender);
            }

            if (this.FInClear[0])
            {          
                foreach (DX11RenderContext ctx in DX11GlobalDevice.DeviceManager.RenderContexts)
                {
                    ctx.ResourcePool.Dispose();
                }
            }

            if (this.FInClearU[0])
            {
                foreach (DX11RenderContext ctx in DX11GlobalDevice.DeviceManager.RenderContexts)
                {
                    ctx.ResourcePool.ClearUnlocked();
                }
            }

            if (this.FInRefresh[0] || first)
            {
                List<DX11RenderContext> ctxlist = DX11GlobalDevice.DeviceManager.RenderContexts;

                this.FOutBufferCount.SliceCount = ctxlist.Count;
                this.FOutRTCount.SliceCount = ctxlist.Count;
                this.FOutRTStack.SliceCount = ctxlist.Count;
                this.FOutLastFrame.SliceCount = ctxlist.Count;
                this.FOutThisFrame.SliceCount = ctxlist.Count;
                this.FOutProcessedCount.SliceCount = ctxlist.Count;
                this.FOutFeatureLevel.SliceCount = ctxlist.Count;
                this.FOUCS.SliceCount = ctxlist.Count;
                this.FOutAdapter.SliceCount = ctxlist.Count;

                List<DeviceCreationFlags> flags = new List<DeviceCreationFlags>();

                int i = 0;
                foreach (DX11RenderContext ctx in ctxlist)
                {
                    DX11DeviceRenderer renderer = DX11GlobalDevice.RenderManager.RenderGraphs[ctx];

                    try
                    {
                        this.FOutAdapter[i] = ctx.Adapter.Description.Description;
                    }
                    catch
                    {
                        this.FOutAdapter[i] = "Unknown";
                    }

                    this.FOutBufferCount[i] = ctx.ResourcePool.BufferCount;
                    this.FOutRTCount[i] = ctx.ResourcePool.RenderTargetCount;
                    this.FOutRTStack[i] = ctx.RenderTargetStack.StackCount;
                    this.FOutRSStack[i] = ctx.RenderStateStack.Count;
                    this.FOutLastFrame[i] = renderer.LastPinsCount;
                    this.FOutThisFrame[i] = renderer.ThisFramePins;
                    this.FOutNodeCount[i] = renderer.Graph.Nodes.Count;
                    this.FOutProcessedCount[i] = renderer.ProcessedNodes;

                    int featureLevel = (int)ctx.FeatureLevel;
                    if (featureLevel == MagicNumberUtils.FeatureLevel11_1)
                    {
                        this.FOutFeatureLevel[i] = "Level_11_1";
                    }
                    else
                    {
                        this.FOutFeatureLevel[i] = ctx.FeatureLevel.ToString();
                    }
                    this.FOUCS[i] = ctx.ComputeShaderSupport;
 
                    if (ctx.Device.CreationFlags.HasFlag(DeviceCreationFlags.BgraSupport)) { flags.Add(DeviceCreationFlags.BgraSupport);}
                    if (ctx.Device.CreationFlags.HasFlag(DeviceCreationFlags.Debug)) { flags.Add(DeviceCreationFlags.Debug);}
                    if (ctx.Device.CreationFlags.HasFlag(DeviceCreationFlags.PreventThreadingOptimizations)) { flags.Add(DeviceCreationFlags.PreventThreadingOptimizations);}
                    if (ctx.Device.CreationFlags.HasFlag(DeviceCreationFlags.SingleThreaded)) { flags.Add(DeviceCreationFlags.SingleThreaded);}

                    i++;
                }

                if (flags.Count > 0)
                {
                    this.FOutFlags.AssignFrom(flags);
                }
                else
                {
                    this.FOutFlags.SliceCount = 1;
                    this.FOutFlags[0] = DeviceCreationFlags.None;
                }

                this.FOutPLCount[0] = DX11GlobalDevice.PendingLinksCount;
                this.FOutPPCount[0] = DX11GlobalDevice.PendingPinsCount;
            }

            first = false;
        }

        void DX11GlobalDevice_OnEndRender(object sender, EventArgs e)
        {
            if (this.EndQuery != null)
            {
                this.EndQuery(DX11GlobalDevice.DeviceManager.RenderContexts[0]);
            }  

        }

        void DX11GlobalDevice_OnBeginRender(object sender, EventArgs e)
        {
            if (this.BeginQuery != null)
            {
                this.BeginQuery(DX11GlobalDevice.DeviceManager.RenderContexts[0]);
            }         
        }
        #endregion

        public event DX11QueryableDelegate BeginQuery;

        public event DX11QueryableDelegate EndQuery;
    }
}
