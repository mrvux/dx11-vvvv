using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Bullet.Core;
using VVVV.Bullet.Core.Filters;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Nodes.Bodies.Rigid.Filters
{
    [PluginInfo(Name = "AND", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Aggregates two filters with a AND operator")]
    public class BulletAndFilterNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input 1", IsSingle = true)]
        protected ISpread<IRigidBodyFilter> input1;

        [Input("Input 2", IsSingle = true)]
        protected ISpread<IRigidBodyFilter> input2;

        [Output("Output", IsSingle = true)]
        protected ISpread<AndFilter> output;

        public void OnImportsSatisfied()
        {
            this.output[0] = new AndFilter();
        }

        public void Evaluate(int s)
        {
            this.output[0].First = input1[0];
            this.output[0].Second = input2[0];
        }
    }

    [PluginInfo(Name = "OR", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Aggregates two filters with a OR operator")]
    public class BulletOrFilterNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input 1", IsSingle = true)]
        protected ISpread<IRigidBodyFilter> input1;

        [Input("Input 2", IsSingle = true)]
        protected ISpread<IRigidBodyFilter> input2;

        [Output("Output", IsSingle = true)]
        protected ISpread<OrFilter> output;

        public void OnImportsSatisfied()
        {
            this.output[0] = new OrFilter();
        }

        public void Evaluate(int s)
        {
            this.output[0].First = input1[0];
            this.output[0].Second = input2[0];
        }
    }

    [PluginInfo(Name = "NOT", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Reverses result of a body filter")]
    public class BulletNotFilterNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Input", IsSingle = true)]
        protected ISpread<IRigidBodyFilter> input1;

        [Output("Output", IsSingle = true)]
        protected ISpread<NotFilter> output;

        public void OnImportsSatisfied()
        {
            this.output[0] = new NotFilter();
        }

        public void Evaluate(int s)
        {
            this.output[0].filter = input1[0];
        }
    }


}
