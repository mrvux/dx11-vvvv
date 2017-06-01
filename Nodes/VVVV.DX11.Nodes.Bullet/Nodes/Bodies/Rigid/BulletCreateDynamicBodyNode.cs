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
    [PluginInfo(Name = "CreateDynamicBody", Category = "Bullet", Version = "Rigid", Author = "vux", Help = "Creates a rigid dynamic body", AutoEvaluate = true)]
    public class BulletCreateDynamicRigidBodyNode : IPluginEvaluate
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

        private List<RigidBody> frameBodyOutput = new List<RigidBody>();
        private List<int> frameIdOutput = new List<int>();

        public void Evaluate(int SpreadMax)
        {
            this.frameBodyOutput.Clear();
            this.frameIdOutput.Clear();

            if (SpreadMax == 0)
            {
                this.bodiesOutput.SliceCount = 0;
                this.idOutput.SliceCount = 0;
                return;
            }

            IRigidBodyContainer world = this.worldInput[0];

            if (world != null && this.shapesInput.IsConnected)
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

                        Tuple<RigidBody, int> createBodyResult = world.CreateRigidBody(collisionShape, ref pose, ref properties, ref localinertia, shape.Mass);

                        createBodyResult.Item1.ApplyMotionProperties(ref motionProperties);

                        this.frameBodyOutput.Add(createBodyResult.Item1);
                        this.frameIdOutput.Add(createBodyResult.Item2);
                    }
                }

                this.bodiesOutput.SliceCount = this.frameBodyOutput.Count;
                this.idOutput.SliceCount = this.frameIdOutput.Count;

                for (int i = 0; i < frameBodyOutput.Count; i++)
                {
                    this.bodiesOutput[i] = frameBodyOutput[i];
                    this.idOutput[i] = frameIdOutput[i];
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

