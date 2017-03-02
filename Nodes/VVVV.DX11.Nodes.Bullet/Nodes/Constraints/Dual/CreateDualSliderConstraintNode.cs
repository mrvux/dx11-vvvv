using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using VVVV.Nodes.Bullet;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Nodes.Constraints.Dual
{
    [PluginInfo(Name = "Slider", Author = "vux", Category = "Bullet", Version = "Constraint.Dual", AutoEvaluate = true)]
    public unsafe class CreateDualSliderConstraintNode : AbstractDualConstraintNode<SliderConstraint>
    {
        [Input("Matrix 1", Order = 10)]
        protected ISpread<SlimDX.Matrix> FMatrix1;

        [Input("Matrix 2", Order = 10)]
        protected ISpread<SlimDX.Matrix> FMatrix2;

        [Input("Linear Reference", Order = 11)]
        protected ISpread<bool> FLinearRef;

        protected override SliderConstraint CreateConstraint(RigidBody body1, RigidBody body2, int slice)
        {
            SlimDX.Matrix m1= this.FMatrix1[slice];
            SlimDX.Matrix m2 = this.FMatrix2[slice];

            SliderConstraint cst = new SliderConstraint(body1 ,body2, *(BulletSharp.Matrix*)&m1, *(BulletSharp.Matrix*)&m2, FLinearRef[slice])
            {
                LowerLinLimit = -15.0f,
                UpperLinLimit = -5.0f,
                
                //LowerLinearLimit = -10.0f,
                //UpperLinearLimit = -10.0f,
                LowerAngularLimit = -(float)Math.PI / 3.0f,
                UpperAngularLimit = (float)Math.PI / 3.0f,
            };

            return cst;
        }
    }
}
