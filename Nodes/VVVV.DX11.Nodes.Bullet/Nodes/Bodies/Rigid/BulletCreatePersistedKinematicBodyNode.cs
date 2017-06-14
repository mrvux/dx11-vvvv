using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;

using BulletSharp;
using VVVV.DataTypes.Bullet;
using VVVV.Internals.Bullet;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "CreateKinematicBody", Category = "Bullet", Version = "Rigid.Persist", Author = "vux", Help = "Creates a rigid kinematic body, and preserves those in the output", AutoEvaluate = true)]
    public class BulletCreateKinematicRigidBodyPeristedNode : IPluginEvaluate
    {
        [Input("World", IsSingle = true)]
        protected Pin<IRigidBodyContainer> worldInput;

        [Input("Shapes")]
        protected Pin<DynamicShapeDefinitionBase> shapesInput;

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

        [Output("Created Bodies")]
        protected ISpread<RigidBody> createdBodiesOutput;


        private RigidBodyListListener persistedList = new RigidBodyListListener();
        private List<RigidBody> frameBodyOutput = new List<RigidBody>();

        public void Evaluate(int SpreadMax)
        {
            this.frameBodyOutput.Clear();

            IRigidBodyContainer inputWorld = this.worldInput[0];
            if (inputWorld != null)
            {
                this.persistedList.UpdateWorld(inputWorld);

                if (this.shapesInput.IsConnected)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (doCreate[i])
                        {
                            RigidBodyPose pose = this.initialPoseInput.IsConnected ? this.initialPoseInput[i] : RigidBodyPose.Default;
                            RigidBodyProperties properties = this.initialProperties.IsConnected ? this.initialProperties[i] : RigidBodyProperties.Default;
                            RigidBodyMotionProperties motionProperties = this.initialMotionProperties.IsConnected ? this.initialMotionProperties[i] : new RigidBodyMotionProperties();

                            ShapeCustomData shapeData = new ShapeCustomData();
                            DynamicShapeDefinitionBase shape = this.shapesInput[i];
                            shapeData.ShapeDef = shape;

                            CollisionShape collisionShape = shapeData.ShapeDef.GetShape(shapeData);

                            //Build mass for dynamic object
                            Vector3 localinertia = Vector3.Zero;
                            if (shape.Mass > 0.0f)
                            {
                                collisionShape.CalculateLocalInertia(shape.Mass, out localinertia);
                            }

                            Tuple<RigidBody, int> createBodyResult = inputWorld.CreateRigidBody(collisionShape, ref pose, ref properties, ref localinertia, shape.Mass);
                            createBodyResult.Item1.CollisionFlags |= CollisionFlags.KinematicObject;

                            createBodyResult.Item1.ApplyMotionProperties(ref motionProperties);

                            this.persistedList.Append(createBodyResult.Item1, createBodyResult.Item2);
                            frameBodyOutput.Add(createBodyResult.Item1);
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

                this.createdBodiesOutput.SliceCount = this.frameBodyOutput.Count;
                for (int i = 0; i < frameBodyOutput.Count; i++)
                {
                    this.createdBodiesOutput[i] = frameBodyOutput[i];
                }

            }
            else
            {
                this.bodiesOutput.SliceCount = 0;
                this.idOutput.SliceCount = 0;
                this.createdBodiesOutput.SliceCount = 0;
            }
        }

    }
}

