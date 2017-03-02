using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="Slider",Author="vux",Category="Bullet",Version="Constraint.Single",AutoEvaluate=true)]
	public unsafe class CreateSliderConstraintNode : AbstractSingleConstraintNode<SliderConstraint>
	{
		[Input("Matrix", Order=10)]
        protected ISpread<SlimDX.Matrix> FMatrix;

		[Input("Linear Reference",Order=11)]
        protected ISpread<bool> FLinearRef;

		protected override SliderConstraint CreateConstraint(RigidBody body, int slice)
		{
            SlimDX.Matrix m = this.FMatrix[slice];

            SliderConstraint cst = new SliderConstraint(body, *(BulletSharp.Matrix*)&m, FLinearRef[slice])
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
