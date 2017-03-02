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
using VVVV.Bullet.Internals;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "CreateDynamicBody", Category = "Bullet", Version = "Rigid.Persist", Author = "vux", Help = "Creates a rigid dynamic body, and preserves those in the output", AutoEvaluate = true)]
    public class BulletCreateDynamicRigidBodyPeristedNode : IPluginEvaluate
    {
        [Input("World", IsSingle = true)]
        protected Pin<BulletRigidSoftWorld> worldInput;

        [Input("Shapes")]
        protected Pin<AbstractRigidShapeDefinition> shapesInput;

        [Input("Initial Pose")]
        protected Pin<RigidBodyPose> initialPoseInput;

        [Input("Initial Motion Properties")]
        protected Pin<RigidBodyMotionProperties> initialMotionProperties;

        [Input("Initial Properties")]
        protected Pin<RigidBodyProperties> initialProperties;

        [Input("Do Create", IsBang = true)]
        protected ISpread<bool> doCreate;

        [Output("Bodies")]
        protected ISpread<RigidBody> bodiesOutput;

        [Output("Id")]
        protected ISpread<int> idOutput;


        private BulletRogidBodyListListener persistedList = new BulletRogidBodyListListener();

        public void Evaluate(int SpreadMax)
        {
            BulletRigidSoftWorld inputWorld = this.worldInput[0];
            this.persistedList.UpdateWorld(inputWorld);

            if (inputWorld != null && this.shapesInput.IsConnected)
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    if (doCreate[i])
                    {
                        RigidBodyPose pose = this.initialPoseInput.IsConnected ? this.initialPoseInput[i] : RigidBodyPose.Default;
                        RigidBodyProperties properties = this.initialProperties.IsConnected ? this.initialProperties[i] : RigidBodyProperties.Default;
                        RigidBodyMotionProperties motionProperties = this.initialMotionProperties.IsConnected ? this.initialMotionProperties[i] : new RigidBodyMotionProperties();

                        ShapeCustomData shapeData = new ShapeCustomData();
                        shapeData.ShapeDef = this.shapesInput[i];

                        CollisionShape collisionShape = shapeData.ShapeDef.GetShape(shapeData);

                        //Build mass for dynamic object
                        Vector3 localinertia = Vector3.Zero;
                        if (shapeData.ShapeDef.Mass > 0.0f)
                        {
                            collisionShape.CalculateLocalInertia(shapeData.ShapeDef.Mass, out localinertia);
                        }

                        Tuple<RigidBody, int> createBodyResult = inputWorld.CreateRigidBody(collisionShape, ref pose, ref properties, ref localinertia, shapeData.ShapeDef.Mass);

                        createBodyResult.Item1.ApplyMotionProperties(ref motionProperties);

                        this.persistedList.Append(createBodyResult.Item1, createBodyResult.Item2);
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
        }

    }
}

