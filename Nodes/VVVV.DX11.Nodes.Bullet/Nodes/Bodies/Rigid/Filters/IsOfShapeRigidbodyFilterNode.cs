using BulletSharp;
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
    [PluginInfo(Name = "IsOfShape", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find a body of a specific shape type")]
    public class BulletIsOfShapeFilterNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Shape Type", IsSingle = true)]
        protected ISpread<BroadphaseNativeType> id;

        [Output("Output", IsSingle = true)]
        protected ISpread<SingleShapeFilter> output;


        public void OnImportsSatisfied()
        {
            this.output[0] = new SingleShapeFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            this.output[0].ShapeType = id[0];
        }


    }
}
