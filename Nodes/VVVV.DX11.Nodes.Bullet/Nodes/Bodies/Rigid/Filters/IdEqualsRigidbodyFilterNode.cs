using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Bullet.Core.Filters;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Nodes.Bodies.Rigid.Filters
{
    [PluginInfo(Name = "IsIdEqualTo", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find a specific body index", AutoEvaluate = true)]
    public class BulletIdEqualdBulletFilterNode : IPluginEvaluate
    {
        [Input("Id", IsSingle = true, DefaultValue = 0.0)]
        protected ISpread<int> id;

        [Output("Output", IsSingle = true)]
        protected ISpread<EqualsIdRigidBodyFilter> output;

        public BulletIdEqualdBulletFilterNode()
        {
            this.output[0] = new EqualsIdRigidBodyFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            this.output[0].Id = id[0];
        }
    }

    [PluginInfo(Name = "IsIdContained", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find a specific body index", AutoEvaluate = true)]
    public class BulletIdContainsBulletFilterNode : IPluginEvaluate
    {
        [Input("Id", DefaultValue = 0.0)]
        protected ISpread<int> id;

        [Output("Output", IsSingle = true)]
        protected ISpread<ContainsIdRigidBodyFilter> output;

        public BulletIdContainsBulletFilterNode()
        {
            this.output[0] = new ContainsIdRigidBodyFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            this.output[0].IdList = id;
        }
    }
}
