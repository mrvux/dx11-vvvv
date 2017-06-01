using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Bullet.Utils;

using BulletSharp;
using VVVV.DataTypes.Bullet;
using VVVV.Bullet.DataTypes;
using VVVV.Internals.Bullet;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "CreateStaticBody", Category = "Bullet", Version = "Rigid.Persist", Author = "vux", Help = "Creates a rigid static body,persists body in the list", AutoEvaluate = true)]
    public class BulletCreatePeristedStaticRigidBodyNode : IPluginEvaluate
    {
        [Input("World", IsSingle = true)]
        protected Pin<IRigidBodyContainer> worldInput;

        [Input("Shapes")]
        protected Pin<RigidShapeDefinitionBase> shapesInput;

        [Input("Initial Pose")]
        protected Pin<RigidBodyPose> initialPoseInput;

        [Input("Initial Properties")]
        protected Pin<RigidBodyProperties> initialProperties;

        [Input("Do Create", IsBang = true)]
        protected ISpread<bool> doCreate;

        [Output("Bodies")]
        protected ISpread<RigidBody> bodiesOutput;

        [Output("Id")]
        protected ISpread<int> idOutput;

        private RigidBodyListListener persistedList = new RigidBodyListListener();

        public void Evaluate(int SpreadMax)
        {
            IRigidBodyContainer world = this.worldInput[0];

            if (world != null)
            {
                this.persistedList.UpdateWorld(world);

                if (this.shapesInput.IsConnected)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (doCreate[i])
                        {
                            RigidBodyPose pose = this.initialPoseInput.IsConnected ? this.initialPoseInput[i] : RigidBodyPose.Default;
                            RigidBodyProperties properties = this.initialProperties.IsConnected ? this.initialProperties[i] : RigidBodyProperties.Default;

                            ShapeCustomData shapeData = new ShapeCustomData();
                            shapeData.ShapeDef = this.shapesInput[i];

                            CollisionShape collisionShape = shapeData.ShapeDef.GetShape(shapeData);
                            Vector3 localInertia = Vector3.Zero;

                            Tuple<RigidBody, int> bodyCreateResult = world.CreateRigidBody(collisionShape, ref pose, ref properties, ref localInertia, 0.0f);
                            bodyCreateResult.Item1.CollisionFlags = CollisionFlags.StaticObject;

                            this.persistedList.Append(bodyCreateResult.Item1, bodyCreateResult.Item2);
                        }
                    }
                }

                this.bodiesOutput.SliceCount = this.persistedList.Bodies.Count;
                this.idOutput.SliceCount = this.persistedList.Ids.Count;

                List<RigidBody> bodies = this.persistedList.Bodies;
                List<int> ids = this.persistedList.Ids;

                for (int i = 0; i < bodies.Count; i++)
                {
                    this.bodiesOutput[i] = bodies[i];
                    this.idOutput[i] = ids[i];
                }
            }
            else
            {
                this.bodiesOutput.SliceCount = 0;
                this.idOutput.SliceCount = 0;
            }
        }
    }
}

