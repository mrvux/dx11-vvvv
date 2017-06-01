using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Bullet.Core.Filters;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Nodes.Bodies.Rigid.Filters
{
    [PluginInfo(Name = "IsOlderThan", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interfact to check for minimum age")]
    public class BulletCreateDynamicRigidBodyNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Age", IsSingle = true, DefaultValue = 0.0)]
        protected ISpread<float> age;

        [Output("Output", IsSingle = true)]
        protected ISpread<MinimumAgeCollisionFilter> output;

        public void OnImportsSatisfied()
        {
            this.output[0] = new MinimumAgeCollisionFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            this.output[0].MinimumAge = age[0];
        }
    }
}
