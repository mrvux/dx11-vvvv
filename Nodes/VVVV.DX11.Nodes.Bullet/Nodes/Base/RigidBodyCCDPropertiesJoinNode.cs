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

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "CCDProperties", Category = "Bullet", Version ="Join",
        Help = "Continous collision detection features for a rigid body", Author = "vux")]
    public unsafe class BulletRigidBodyCCDPropertiesJoinNode : IPluginEvaluate
    {
        [Input("Ccd Motion Threshold", DefaultValue =0.0)]
        protected ISpread<float> ccthreashold;

        [Input("Swept Sphere Radius", DefaultValue =0.2)]
        protected ISpread<float> SweptSphereRadius;


        [Output("Output")]
        protected ISpread<RigidBodyCCDProperties> output;

        public void Evaluate(int SpreadMax)
        {
            this.output.SliceCount = SpreadMax;
            
            fixed (RigidBodyCCDProperties* posePtr = &this.output.Stream.Buffer[0])
            {
                for (int i = 0; i < SpreadMax; i++)
                {
                    posePtr[i].CcdMotionThreshold = ccthreashold[i];
                    posePtr[i].CcdSweptSphereRadius = SweptSphereRadius[i];
                }
            }
            this.output.Flush(true);
        }
    }
}
