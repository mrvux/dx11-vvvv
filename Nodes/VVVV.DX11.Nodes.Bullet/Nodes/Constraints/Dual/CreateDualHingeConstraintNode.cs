using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.Bullet
{
	//[PluginInfo(Name="Hinge",Author="vux",Category="Bullet",Version="Constraint.Dual",AutoEvaluate=true)]
	public class CreateDualHingeConstraintNode : AbstractDualConstraintNode<HingeConstraint>
	{
		[Input("Pivot 1", Order = 10)]
        protected ISpread<Vector3D> FPivot1;

		[Input("Pivot 2", Order = 11)]
        protected ISpread<Vector3D> FPivot2;

		[Input("Damping", Order = 12)]
        protected ISpread<float> FDamping;

		[Input("Impulse Clamp", Order = 13)]
        protected ISpread<float> FImpulseClamp;

		[Input("Tau", Order = 14)]
        protected ISpread<float> FTau;

		protected override HingeConstraint CreateConstraint(RigidBody body1, RigidBody body2, int slice)
		{
			//HingeConstraint cst = new HingeConstraint(body1, body2,
			//	this.FPivot1[slice].ToBulletVector(), this.FPivot2[slice].ToBulletVector());
			//cst.se
			return null;	
		}
	}
}
