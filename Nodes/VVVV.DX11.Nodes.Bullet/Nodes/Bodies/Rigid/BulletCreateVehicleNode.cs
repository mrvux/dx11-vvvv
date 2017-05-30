using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Bullet;
using BulletSharp;
using VVVV.DataTypes.Bullet;
using VVVV.Internals.Bullet;
using VVVV.Utils.VMath;
using VVVV.Bullet.Utils;
using VVVV.Bullet.DataTypes.Vehicle;
using VVVV.Bullet.Core;

namespace VVVV.Bullet.Nodes.Bodies.Rigid
{
    [PluginInfo(Name = "CreateVehicle", Category = "Bullet", Author = "vux",
		Help = "Creates a vehicle", AutoEvaluate = true)]
    public class BulletCreateVehicleNode : AbstractRigidBodyCreator
    {
        //private WheelInfoSettings wheelInfoSettings = new WheelInfoSettings();
        //private WheelConstructionSettings constructionSettings = new WheelConstructionSettings();

        int rightIndex = 0;
        int upIndex = 1;
        int forwardIndex = 2;

        [Input("Construction Settings")]
        protected ISpread<ISpread<WheelConstructionSettings>> wheelConstruction;

        [Input("Wheel Info")]
        protected ISpread<WheelInfoSettings> wheelInfoSettings;

        [Output("Vehicle")]
        protected ISpread<RaycastVehicle> FOutVehicle;

        private BulletRigidSoftWorld lastworld;

        public override void Evaluate(int sm)
        {
            if (this.lastworld != this.FWorld[0])
            {
                this.FOutVehicle.SliceCount = 0;
                this.lastworld = this.FWorld[0];
            }

            int spMax = 1;

            FOutVehicle.SliceCount = spMax;

            for (int i = 0; i < spMax; i++)
            {
                if (this.CanCreate(i))
                {

                    RaycastVehicle vehicle;
                    
                    AbstractRigidShapeDefinition shapedef = this.FShapes[i];
                    ShapeCustomData sc = new ShapeCustomData();
                    sc.ShapeDef = shapedef;


                    CompoundShape compound = new CompoundShape();

                    CollisionShape chassisShape = shapedef.GetShape(sc);
                    Matrix localTrans = Matrix.Translation(Vector3.UnitY);
                    compound.AddChildShape(localTrans, chassisShape);

                    float mass = shapedef.Mass;

                    bool isDynamic = (mass != 0.0f);

                    Vector3 localInertia = Vector3.Zero;
                    if (isDynamic)
                        chassisShape.CalculateLocalInertia(mass, out localInertia);

                    Vector3D pos = this.FPosition[i];
                    Vector4D rot = this.FRotation[i];

                    DefaultMotionState ms = BulletUtils.CreateMotionState(pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);


                    RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, ms, compound, localInertia);
                    RigidBody carChassis = new RigidBody(rbInfo);

                    BodyCustomData bd = new BodyCustomData(this.FWorld[0].GetNewBodyId());

                    carChassis.UserObject = bd;
                    bd.Custom = this.FCustom[i];

                    this.FWorld[0].Register(carChassis);


                    RaycastVehicle.VehicleTuning tuning = new RaycastVehicle.VehicleTuning();
                    VehicleRaycaster vehicleRayCaster = new DefaultVehicleRaycaster(this.FWorld[0].World);
                    vehicle = new RaycastVehicle(tuning, carChassis, vehicleRayCaster);

                    carChassis.ActivationState = ActivationState.DisableDeactivation;
                    this.FWorld[0].World.AddAction(vehicle);

                    // choose coordinate system
                    vehicle.SetCoordinateSystem(rightIndex, upIndex, forwardIndex);

                    int wheelCount = this.wheelConstruction.SliceCount;

                    for (int j = 0; j < this.wheelConstruction[i].SliceCount; j++)
                    {
                        WheelConstructionSettings wcs = this.wheelConstruction[i][j];
                        Vector3 connectionPointCS0 = wcs.localPosition.ToBulletVector();
                        WheelInfo wheel = vehicle.AddWheel(connectionPointCS0, wcs.wheelDirection.ToBulletVector(), wcs.wheelAxis.ToBulletVector(), wcs.SuspensionRestLength, wcs.WheelRadius, tuning, wcs.isFrontWheel);
                    }

                    WheelInfoSettings wis = this.wheelInfoSettings[i] != null ? this.wheelInfoSettings[i] : new DataTypes.Vehicle.WheelInfoSettings();
                    for (int j = 0; j < vehicle.NumWheels; j++)
                    {
                        WheelInfo wheel = vehicle.GetWheelInfo(j);
                        wheel.SuspensionStiffness = wis.SuspensionStiffness;
                        wheel.WheelsDampingRelaxation = wis.WheelsDampingRelaxation;
                        wheel.WheelsDampingCompression = wis.WheelsDampingCompression;
                        wheel.FrictionSlip = wis.FrictionSlip;
                        wheel.RollInfluence = wis.RollInfluence;
                    }

                    
                    FOutVehicle[i] = vehicle;
                }
            }
        }
    }
}
