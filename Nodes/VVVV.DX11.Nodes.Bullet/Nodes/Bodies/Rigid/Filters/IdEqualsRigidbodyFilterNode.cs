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
    [PluginInfo(Name = "IsIdInList", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find in a list of indices")]
    public class IsIdContainedRigidBodyFilterNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Id", DefaultValue = 0.0)]
        protected ISpread<int> id;

        [Output("Output", IsSingle = true)]
        protected ISpread<ContainsIdRigidBodyFilter> output;

        public void OnImportsSatisfied()
        {
            this.output[0] = new ContainsIdRigidBodyFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            this.output[0].IdList = id;
        }
    }

    [PluginInfo(Name = "IsIdEqual", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find a specific body index")]
    public class IsIdEqualRigidBodyFilterNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Id", DefaultValue = 0.0)]
        protected ISpread<int> id;

        [Output("Output", IsSingle = true)]
        protected ISpread<EqualsIdRigidBodyFilter> output;

        public void OnImportsSatisfied()
        {
            this.output[0] = new EqualsIdRigidBodyFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            if (SpreadMax >0)
            {
                this.output[0].Id = id[0];
            }
            else
            {
                this.output[0].Id = null;
            }
        }
    }
}
