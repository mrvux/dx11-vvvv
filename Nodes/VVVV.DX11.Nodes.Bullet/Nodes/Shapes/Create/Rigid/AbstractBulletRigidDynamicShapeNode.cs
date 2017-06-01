using System;
using System.Collections.Generic;
using System.Text;

using BulletSharp;
using VVVV.Bullet.DataTypes;
using VVVV.DataTypes.Bullet;
using VVVV.Hosting.Pins.Input;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	public abstract class AbstractBulletRigidDynamicShapeNode : IPluginEvaluate
	{
		#region Pins
		[Input("Pose", CheckIfChanged =true)]
		protected Pin<RigidBodyPose> FPose;

		[Input("Scaling", DefaultValues = new double[] { 1.0, 1.0,1.0 })]
		protected IDiffSpread<Vector3D> FScaling;

		[Input("Mass", DefaultValue=1.0)]
		protected IDiffSpread<float> FMass;

		[Input("Custom")]
		protected IDiffSpread<string> FCustom;

		[Output("Shape")]
		protected ISpread<DynamicShapeDefinitionBase> FShapes;
		#endregion

		#region Evaluate

		public abstract void Evaluate(int SpreadMax);

		protected bool BasePinsChanged
		{
			get
			{
				return this.FCustom.IsChanged
					|| this.FMass.IsChanged
					|| this.FPose.IsChanged
					|| this.FScaling.IsChanged;
			}
		}

		protected int BasePinsSpreadMax
		{
			get
			{
				return SpreadUtils.SpreadMax(this.FCustom,
					this.FMass,
					this.FPose,
					this.FScaling);
			}
		}
		#endregion

		#region Set Local Transform
		protected void SetBaseParams(RigidShapeDefinitionBase sd, int sliceindex)
		{
            sd.Pose = FPose.IsConnected ? FPose[sliceindex] : RigidBodyPose.Default;
			sd.Scaling = this.FScaling[sliceindex].Abs().ToBulletVector();
			sd.CustomString = this.FCustom[sliceindex];
		}
		#endregion
	}
}
