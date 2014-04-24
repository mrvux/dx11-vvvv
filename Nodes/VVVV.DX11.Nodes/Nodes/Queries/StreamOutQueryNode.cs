using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Queries;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{

    [PluginInfo(Name = "StreamOut", Category = "DX11.Query", Version = "", Author = "vux", Tags = "debug")]
    public class StreamOutQueryNode : AbstractQueryNode<DX11StreamOutQuery>
    {
        [Output("Primitives Written", IsSingle = true)]
        protected ISpread<int> FOutPCount;

        [Output("Storage Needed", IsSingle = true)]
        protected ISpread<int> FOutSN;

        protected override DX11StreamOutQuery CreateQueryObject(DX11RenderContext context)
        {
            return new DX11StreamOutQuery(context);
        }

        protected override void OnEvaluate()
        {
            if (this.queryobject != null)
            {
                this.FOutPCount[0] = (int)this.queryobject.Statistics.PrimitivesWritten;
                this.FOutSN[0] = (int)this.queryobject.Statistics.StorageNeeded;
            }
        }
    }

}
