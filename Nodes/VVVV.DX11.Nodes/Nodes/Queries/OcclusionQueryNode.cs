using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using FeralTic.DX11.Queries;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "Occlusion", Category = "DX11.Query", Version = "", Author = "vux",Tags="debug")]
    public class OcclusionQueryNode : AbstractQueryNode<DX11OcclusionQuery>
    {
        [Output("Pixels Drawn", IsSingle = true)]
        protected ISpread<int> FOuDrawn;

        protected override DX11OcclusionQuery CreateQueryObject(DX11RenderContext context)
        {
            return new DX11OcclusionQuery(context);
        }

        protected override void OnEvaluate()
        {
            if (this.queryobject != null)
            {
                this.FOuDrawn[0] = (int)this.queryobject.Statistics;
            }
        }
    }

}
