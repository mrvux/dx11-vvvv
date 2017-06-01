using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Internals.Bullet;
using VVVV.Utils.VMath;
using VVVV.Bullet.Utils;
using VVVV.Bullet.Core;
using VVVV.Bullet.Core.Vehicle;

namespace VVVV.Bullet.Nodes.Bodies.Rigid
{
    [PluginInfo(Name = "CreateVehicle", Category = "Bullet", Author = "vux", Help = "Creates a vehicle", AutoEvaluate = true)]
    public class BulletCreateVehicleNode : IPluginEvaluate
    {
        protected int rightIndex = 0;
        protected int upIndex = 1;
        protected int forwardIndex = 2;

        [Input("World", IsSingle = true)]
        protected Pin<IRigidBulletWorld> worldInput;

        [Input("Chassis Shape")]
        protected Pin<DynamicShapeDefinitionBase> chassisShape;

        [Input("Initial Pose")]
        protected Pin<RigidBodyPose> initialPoseInput;

        [Input("Initial Properties")]
        protected Pin<RigidBodyProperties> initialProperties;

        [Input("Wheel Construction Properties")]
        protected ISpread<ISpread<WheelConstructionProperties>> wheelConstruction;

        [Input("Wheel Properties")]
        protected ISpread<WheelProperties> wheelInfoSettings;

        [Input("Do Create", IsBang = true)]
        protected ISpread<bool> doCreate;

        [Output("Vehicle")]
        protected ISpread<RaycastVehicle> vehicleOutput;

        [Output("Chassis")]
        protected ISpread<RigidBody> chassisOutput;

        private RigidBodyListListener persistedList = new RigidBodyListListener();

        public void Evaluate(int SpreadMax)
        {
            IRigidBulletWorld inputWorld = this.worldInput[0];

            SpreadMax = 1;

            if (inputWorld != null)
            {
                this.persistedList.UpdateWorld(inputWorld);

                if (this.chassisShape.IsConnected)
                {
                    for (int i = 0; i < SpreadMax; i++)
                    {
                        if (this.doCreate[i])
                        {
                            RigidBodyPose initialPose = this.initialPoseInput.IsConnected ? this.initialPoseInput[i] : RigidBodyPose.Default;
                            RigidBodyProperties properties = this.initialProperties.IsConnected ? this.initialProperties[i] : RigidBodyProperties.Default;

                            ShapeCustomData shapeData = new ShapeCustomData();
                            DynamicShapeDefinitionBase chassisShapeDefinition = this.chassisShape[i];

                            CollisionShape chassisShape = chassisShapeDefinition.GetShape(shapeData);
                            shapeData.ShapeDef = chassisShapeDefinition;

                            RaycastVehicle vehicle;
                            CompoundShape compoundShape = new CompoundShape();

                            Matrix localTrans = Matrix.Translation(Vector3.UnitY);
                            compoundShape.AddChildShape(localTrans, chassisShape);

                            //Build mass for dynamic object
                            Vector3 localInertia = Vector3.Zero;
                            if (chassisShapeDefinition.Mass > 0.0f)
                            {
                                compoundShape.CalculateLocalInertia(chassisShapeDefinition.Mass, out localInertia);
                            }

                            Tuple<RigidBody, int> createBodyResult = inputWorld.CreateRigidBody(chassisShape, ref initialPose, ref properties, ref localInertia, chassisShapeDefinition.Mass);
                            RigidBody carChassis = createBodyResult.Item1;

                            RaycastVehicle.VehicleTuning tuning = new RaycastVehicle.VehicleTuning();
                            VehicleRaycaster vehicleRayCaster = new DefaultVehicleRaycaster(inputWorld.World);
                            vehicle = new RaycastVehicle(tuning, carChassis, vehicleRayCaster);
                            vehicle.SetCoordinateSystem(rightIndex, upIndex, forwardIndex);

                            carChassis.ActivationState = ActivationState.DisableDeactivation;
                            inputWorld.World.AddAction(vehicle);

                            int wheelCount = this.wheelConstruction.SliceCount;

                            //Add wheels
                            for (int j = 0; j < this.wheelConstruction[i].SliceCount; j++)
                            {
                                WheelConstructionProperties wcs = this.wheelConstruction[i][j];
                                Vector3 connectionPointCS0 = wcs.localPosition.ToBulletVector();
                                WheelInfo wheel = vehicle.AddWheel(connectionPointCS0, wcs.wheelDirection.ToBulletVector(), wcs.wheelAxis.ToBulletVector(), wcs.SuspensionRestLength, wcs.WheelRadius, tuning, wcs.isFrontWheel);
                            }

                            //Set Wheel Properties
                            WheelProperties wis = this.wheelInfoSettings[i] != null ? this.wheelInfoSettings[i] : new WheelProperties();
                            for (int j = 0; j < vehicle.NumWheels; j++)
                            {
                                WheelInfo wheel = vehicle.GetWheelInfo(j);
                                wheel.SuspensionStiffness = wis.SuspensionStiffness;
                                wheel.WheelsDampingRelaxation = wis.WheelsDampingRelaxation;
                                wheel.WheelsDampingCompression = wis.WheelsDampingCompression;
                                wheel.FrictionSlip = wis.FrictionSlip;
                                wheel.RollInfluence = wis.RollInfluence;
                            }

                            BodyCustomData bd = (BodyCustomData)carChassis.UserObject;
                            bd.Vehicle = vehicle;

                            this.persistedList.Append(createBodyResult.Item1, createBodyResult.Item2);
                        }
                    }


                    List<RigidBody> bodies = this.persistedList.Bodies;
                    this.vehicleOutput.SliceCount = bodies.Count;
                    this.chassisOutput.SliceCount = bodies.Count;
                    for (int i = 0; i < bodies.Count; i++)
                    {
                        BodyCustomData bd = (BodyCustomData)bodies[i].UserObject;
                        this.vehicleOutput[i] = bd.Vehicle;
                        this.chassisOutput[i] = bodies[i];
                    }
                }
            }
            else
            {
                this.vehicleOutput.SliceCount = 0;
                this.chassisOutput.SliceCount = 0;
            }
        }
    }
}
