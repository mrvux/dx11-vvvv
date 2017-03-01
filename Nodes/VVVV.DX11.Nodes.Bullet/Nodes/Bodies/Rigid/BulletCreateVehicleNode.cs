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

namespace VVVV.Bullet.Nodes.Bodies.Rigid
{
    [PluginInfo(Name = "CreateVehicle", Category = "Bullet", Author = "vux",
		Help = "Creates a vehicle", AutoEvaluate = true)]
    public class BulletCreateVehicleNode : AbstractRigidBodyCreator
    {
        private WheelInfoSettings wheelInfoSettings = new WheelInfoSettings();
        private WheelConstructionSettings constructionSettings = new WheelConstructionSettings();

        int rightIndex = 0;
        int upIndex = 1;
        int forwardIndex = 2;
        float CUBE_HALF_EXTENTS = 1;
        /*Vector3 wheelDirectionCS0 = new Vector3(0, -1, 0);
        Vector3 wheelAxleCS = new Vector3(-1, 0, 0);*/


        [Output("Vehicle")]
        protected ISpread<RaycastVehicle> FOutVehicle;

        private BulletRigidSoftWorld lastworld;

        public override void Evaluate(int SpreadMax)
        {
            if (this.lastworld != this.FWorld[0])
            {
                this.FOutVehicle.SliceCount = 0;
                this.lastworld = this.FWorld[0];
            }

            for (int i = 0; i < SpreadMax; i++)
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

                    BodyCustomData bd = new BodyCustomData();

                    carChassis.UserObject = bd;
                    bd.Id = this.FWorld[0].GetNewBodyId();
                    bd.Custom = this.FCustom[i];

                    this.FWorld[0].Register(carChassis);


                    RaycastVehicle.VehicleTuning tuning = new RaycastVehicle.VehicleTuning();
                    VehicleRaycaster vehicleRayCaster = new DefaultVehicleRaycaster(this.FWorld[0].World);
                    vehicle = new RaycastVehicle(tuning, carChassis, vehicleRayCaster);

                    carChassis.ActivationState = ActivationState.DisableDeactivation;
                    this.FWorld[0].World.AddAction(vehicle);

                    bool isFrontWheel = true;


                    // choose coordinate system
                    vehicle.SetCoordinateSystem(rightIndex, upIndex, forwardIndex);

                    Vector3 connectionPointCS0 = new Vector3(CUBE_HALF_EXTENTS - (0.3f * constructionSettings.WheelWidth), constructionSettings.ConnectionHeight, 2 * CUBE_HALF_EXTENTS - constructionSettings.WheelRadius);
                    WheelInfo a = vehicle.AddWheel(connectionPointCS0, constructionSettings.wheelDirection.ToBulletVector() , constructionSettings.wheelAxis.ToBulletVector(), constructionSettings.SuspensionRestLength, constructionSettings.WheelRadius, tuning, isFrontWheel);

                    connectionPointCS0 = new Vector3(-CUBE_HALF_EXTENTS + (0.3f * constructionSettings.WheelWidth), constructionSettings.ConnectionHeight, 2 * CUBE_HALF_EXTENTS - constructionSettings.WheelRadius);
                    vehicle.AddWheel(connectionPointCS0, constructionSettings.wheelDirection.ToBulletVector(), constructionSettings.wheelAxis.ToBulletVector(), constructionSettings.SuspensionRestLength, constructionSettings.WheelRadius, tuning, isFrontWheel);

                    isFrontWheel = false;
                    connectionPointCS0 = new Vector3(-CUBE_HALF_EXTENTS + (0.3f * constructionSettings.WheelWidth), constructionSettings.ConnectionHeight, -2 * CUBE_HALF_EXTENTS + constructionSettings.WheelRadius);
                    vehicle.AddWheel(connectionPointCS0, constructionSettings.wheelDirection.ToBulletVector(), constructionSettings.wheelAxis.ToBulletVector(), constructionSettings.SuspensionRestLength, constructionSettings.WheelRadius, tuning, isFrontWheel);

                    connectionPointCS0 = new Vector3(CUBE_HALF_EXTENTS - (0.3f * constructionSettings.WheelWidth), constructionSettings.ConnectionHeight, -2 * CUBE_HALF_EXTENTS + constructionSettings.WheelRadius);
                    vehicle.AddWheel(connectionPointCS0, constructionSettings.wheelDirection.ToBulletVector(), constructionSettings.wheelAxis.ToBulletVector(), constructionSettings.SuspensionRestLength, constructionSettings.WheelRadius, tuning, isFrontWheel);


                    for (i = 0; i < vehicle.NumWheels; i++)
                    {
                        WheelInfo wheel = vehicle.GetWheelInfo(i);
                        wheel.SuspensionStiffness = wheelInfoSettings.SuspensionStiffness;
                        wheel.WheelsDampingRelaxation = wheelInfoSettings.WheelsDampingRelaxation;
                        wheel.WheelsDampingCompression = wheelInfoSettings.WheelsDampingCompression;
                        wheel.FrictionSlip = wheelInfoSettings.FrictionSlip;
                        wheel.RollInfluence = wheelInfoSettings.RollInfluence;
                    }

                    FOutVehicle.SliceCount = 1;
                    FOutVehicle[0] = vehicle;
                }
            }
        }
    }
}
