using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Bullet.Core.Filters;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Nodes.Bodies.Rigid.Filters
{
    [PluginInfo(Name = "OlderThan", Category = "Bullet", Version = "Rigid", Author = "vux", Help = "Filter interfact to check for minimum age", AutoEvaluate = true)]
    public class BulletCreateDynamicRigidBodyNode : IPluginEvaluate
    {
        [Input("Age", IsSingle = true, DefaultValue = 0.0)]
        protected ISpread<float> age;

        [Output("Output", IsSingle = true)]
        protected ISpread<MinimumAgeCollisionFilter> output;

        public BulletCreateDynamicRigidBodyNode()
        {
            this.output[0] = new MinimumAgeCollisionFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            this.output[0].MinimumAge = age[0];
        }
    }
}
