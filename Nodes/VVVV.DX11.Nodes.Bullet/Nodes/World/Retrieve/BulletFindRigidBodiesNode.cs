using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SlimDX;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using BulletSharp;
using BulletSharp.SoftBody;
using VVVV.DataTypes.Bullet;
using VVVV.Bullet.DataTypes;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "FindRigidBodies", Category = "Bullet", Version ="", Author = "vux")]
    public class BulletFindRigidBodiesWorldNode : IPluginEvaluate
    {
        [Input("World", IsSingle = true)]
        protected ISpread<IRigidBodyCollection> FWorld;

        [Input("Filter", IsSingle = true)]
        protected ISpread<IRigidBodyFilter> FFilter;

        [Output("Rigid Bodies")]
        protected ISpread<RigidBody> FRigidBodies;

        private List<RigidBody> bodies = new List<RigidBody>(512);

        public void Evaluate(int SpreadMax)
        {
            if (this.FWorld[0] != null)
            {
                List<RigidBody> allBodies = this.FWorld[0].RigidBodies;
                List<RigidBody> filteredBodyList;

                IRigidBodyFilter filter = this.FFilter[0];

                if (this.FFilter[0] != null)
                {
                    this.bodies.Clear();

                    for (int i = 0; i < allBodies.Count; i++)
                    {
                        if (filter.Filter(allBodies[i]))
                        {
                            this.bodies.Add(allBodies[i]);
                        }
                    }
                    filteredBodyList = this.bodies;
                }
                else
                {
                    filteredBodyList = allBodies;
                }

                this.FRigidBodies.SliceCount = filteredBodyList.Count;

                var outputBuffer = this.FRigidBodies.Stream.Buffer;
                for (int i = 0; i < filteredBodyList.Count; i++)
                {
                    outputBuffer[i] = filteredBodyList[i];
                }
                this.FRigidBodies.Flush(true);
            }
            else
            {
                this.FRigidBodies.SliceCount = 0;
            }
        }
    }
}
