using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Bullet.Core.Filters;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Nodes.Bodies.Rigid.Filters
{
    [PluginInfo(Name = "IsOfShape", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find a body of a specific shape type", AutoEvaluate = true)]
    public class BulletIsOfShapeFilterNode : IPluginEvaluate
    {
        [Input("Shape Type", IsSingle = true)]
        protected ISpread<BroadphaseNativeType> id;

        [Output("Output", IsSingle = true)]
        protected ISpread<SingleShapeFilter> output;

        public BulletIsOfShapeFilterNode()
        {
            this.output[0] = new SingleShapeFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            this.output[0].ShapeType = id[0];
        }
    }
}
