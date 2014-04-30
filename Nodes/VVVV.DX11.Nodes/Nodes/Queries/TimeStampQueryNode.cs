using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using FeralTic.DX11.Queries;
using FeralTic.DX11;

namespace VVVV.DX11.Nodes
{
    [PluginInfo(Name = "TimeStamp", Category = "DX11.Query", Version = "", Author = "vux", Tags = "debug")]
    public class TimeStampQueryNode : AbstractQueryNode<DX11TimeStampQuery>
    {
        [Output("Time", IsSingle = true)]
        protected ISpread<float> FOutTime;

        protected override DX11TimeStampQuery CreateQueryObject(DX11RenderContext context)
        {
            return new DX11TimeStampQuery(context);
        }

        protected override void OnEvaluate()
        {
            if (this.queryobject != null)
            {
                this.FOutTime[0] = this.queryobject.Elapsed;
            }
        }
    }




}
