using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;
using VVVV.Internals.Bullet;

using BulletSharp.SoftBody;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "CreateSoftBody", Category = "Bullet", Author = "vux",
		Help = "Creates a soft body", AutoEvaluate = true)]
	public class BulletCreateSoftBodyNode : IPluginEvaluate
	{
		[Input("World", IsSingle = true)]
        protected Pin<ISoftBulletWorld> FWorld;

		[Input("Shapes")]
        protected Pin<AbstractSoftShapeDefinition> FShapes;

        [Input("Initial Pose")]
        protected Pin<RigidBodyPose> initialPoseInput;

        [Input("Scale", DefaultValues = new double[] { 1, 1, 1 })]
        protected ISpread<Vector3D> FScale;

		[Input("Friction")]
        protected ISpread<float> FFriction;

		[Input("Restitution")]
        protected ISpread<float> FRestitution;

		[Input("Custom")]
        protected ISpread<string> FCustom;

		[Input("Do Create", IsBang = true)]
        protected ISpread<bool> FDoCreate;

		[Output("Body")]
        protected ISpread<SoftBody> FOutBodies;

		public void Evaluate(int SpreadMax)
		{
			if (this.FWorld.PluginIO.IsConnected && this.FShapes.PluginIO.IsConnected)
			{
				List<SoftBody> bodies = new List<SoftBody>();
				for (int i = 0; i < SpreadMax; i++)
				{
					if (FDoCreate[i])
					{
						AbstractSoftShapeDefinition shapedef = this.FShapes[i];

						SoftBody body = shapedef.GetSoftBody(this.FWorld[0].WorldInfo);
                        RigidBodyPose pose = this.initialPoseInput.IsConnected ? this.initialPoseInput[i] : RigidBodyPose.Default;

                        body.Translate(pose.Position);
						body.Scale(this.FScale[i].ToBulletVector());
                        body.Rotate(pose.Orientation);
						body.Friction = this.FFriction[i];
						body.Restitution = this.FRestitution[i];

                        SoftBodyCustomData bd = new SoftBodyCustomData(this.FWorld[0].GetNewSoftBodyId());
						bd.Custom = this.FCustom[i];
						bd.HasUV = shapedef.HasUV;
						bd.UV = shapedef.GetUV(body);
						body.UserObject = bd;

				
						this.FWorld[0].Register(body);
						bodies.Add(body);
					}
				}

				this.FOutBodies.SliceCount = bodies.Count;
				for (int i = 0; i < bodies.Count; i++)
				{
					this.FOutBodies[i] = bodies[i];
				}
			}
			else
			{
				this.FOutBodies.SliceCount = 0;
			}

		}
	}
}
