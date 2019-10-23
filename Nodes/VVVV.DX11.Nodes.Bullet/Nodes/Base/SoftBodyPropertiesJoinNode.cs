using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Utils.VMath;
using VVVV.Internals.Bullet;
using System.ComponentModel.Composition;
using VVVV.Core.Logging;
using VVVV.Bullet.DataTypes;
using VVVV.Bullet.Core;
using BulletSharp.SoftBody;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "SoftBodyProperties", Category = "Bullet", Version ="Join",
        Help = "Builds bullet rigid body properties", Author = "vux")]
    public unsafe class BulletSoftBodyPropertiesJoinNode : IPluginEvaluate
    {
        [Input("Aero Model", DefaultEnumEntry = "VPoint")]
        protected IDiffSpread<AeroModel> FPinInAeroModel;

        [Input("Is Volume Mass", DefaultValue = 0.0)]
        protected IDiffSpread<bool> FPinInIsVolumeMass;

        [Input("Mass", DefaultValue = 1.0)]
        protected IDiffSpread<float> FPinInMass;

        [Input("Damping Coefficient", DefaultValue = 0.0)]
        protected IDiffSpread<float> FPinInDampingCoefficient;

        [Input("Drag Coefficient", DefaultValue = 0.0)]
        protected IDiffSpread<float> FPinInDG;

        [Input("Dynamic Friction Coefficient", DefaultValue = 0.0)]
        protected IDiffSpread<float> FPinInDF;

        [Input("Pressure Coefficient", DefaultValue = 0.0)]
        protected IDiffSpread<float> FPinInPR;

        [Input("Volume Conservation Coefficient", DefaultValue = 0.0)]
        protected IDiffSpread<float> FPinInVC;

        [Input("Lift Coefficient", DefaultValue = 1.0)]
        protected IDiffSpread<float> FPinInLF;

        [Input("Rigid Contact Hardness", DefaultValue = 1.0)]
        protected IDiffSpread<float> FPinInCHR;

        [Input("Soft Contact Hardness", DefaultValue = 1.0)]
        protected IDiffSpread<float> FPinInSHR;

        [Input("Anchor Hardness", DefaultValue = 0.4)]
        protected IDiffSpread<float> FPinInAHR;

        [Output("Output")]
        protected ISpread<SoftBodyProperties> output;

        public void Evaluate(int SpreadMax)
        {
            this.output.SliceCount = SpreadMax;
            
            fixed (SoftBodyProperties* posePtr = &this.output.Stream.Buffer[0])
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    posePtr[i].AeroModel = FPinInAeroModel[i];
                    posePtr[i].AnchorHardness = FPinInAHR[i];
                    posePtr[i].DampingCoefficient = this.FPinInDampingCoefficient[i];
                    posePtr[i].DragCoefficient = FPinInDG[i];
                    posePtr[i].DynamicFrictionCoefficient = FPinInDF[i];
                    posePtr[i].IsVolumeMass = FPinInIsVolumeMass[i];
                    posePtr[i].LiftCoefficient = this.FPinInLF[i];
                    posePtr[i].Mass = FPinInMass[i];
                    posePtr[i].PressureCoefficient = FPinInPR[i];
                    posePtr[i].RigidContactHardness = FPinInCHR[i];
                    posePtr[i].SoftContactHardness = this.FPinInSHR[i];
                    posePtr[i].VolumeConservationCoefficient = FPinInVC[i];
                }
            }
            this.output.Flush(true);
        }
    }
}
