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
    [PluginInfo(Name = "SphereContainment", Category = "Bullet", Version = "Rigid.Filter", Author = "vux", Help = "Filter interface to find a body within, intersecting or outisde of a sphere")]
    public class BulletSphereContainmentNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("Center")]
        protected ISpread<SlimDX.Vector3> center;

        [Input("Radius", DefaultValue =0.5)]
        protected ISpread<float> radius;

        [Input("Body Comparison", DefaultEnumEntry = "Center")]
        protected ISpread<RigidBodyCheckType> comparisonType;

        [Input("Containments", DefaultEnumEntry ="Contains")]
        protected ISpread<ContainmentType> containements;

        [Output("Output", IsSingle = true)]
        protected ISpread<SphereContainmentFilter> output;

        public void OnImportsSatisfied()
        {
            this.output[0] = new SphereContainmentFilter();
        }

        public void Evaluate(int SpreadMax)
        {
            this.output[0].Bounds.Center = center[0];
            this.output[0].Bounds.Radius = radius[0];
            this.output[0].CheckType = comparisonType[0];
            this.output[0].Containments.Clear();
            for (int i = 0; i < this.containements.SliceCount; i++)
            {
                this.output[0].Containments.Add(this.containements[i]);
            }
        }


    }
}
