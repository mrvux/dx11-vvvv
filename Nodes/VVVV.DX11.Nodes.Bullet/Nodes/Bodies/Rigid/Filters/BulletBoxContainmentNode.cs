using BulletSharp;
using SlimDX;
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
    [PluginInfo(Name = "BoxContainment", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find a body within or outside a bounding box")]
    public class BulletBoxContainmentNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Bounds Min", DefaultValues = new double[] { -10, -10, -10 })]
        protected ISpread<SlimDX.Vector3> minimum;

        [Input("Bounds Max", DefaultValues = new double[] { 10, 10, 10 })]
        protected ISpread<SlimDX.Vector3> maximum;

        [Input("Body Comparison", DefaultEnumEntry = "Center")]
        protected ISpread<RigidBodyCheckType> comparisonType;

        [Input("Containments", DefaultEnumEntry ="Contains")]
        protected ISpread<ContainmentType> containements;

        [Output("Output", IsSingle = true)]
        protected ISpread<BoxContainmentFilter> output;

        public void OnImportsSatisfied()
        {
            this.output[0] = new BoxContainmentFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            this.output[0].Bounds.Minimum = minimum[0];
            this.output[0].Bounds.Maximum = maximum[0];
            this.output[0].CheckType = comparisonType[0];
            this.output[0].Containments.Clear();
            for (int i = 0; i < this.containements.SliceCount; i++)
            {
                this.output[0].Containments.Add(this.containements[i]);
            }
        }


    }
}
