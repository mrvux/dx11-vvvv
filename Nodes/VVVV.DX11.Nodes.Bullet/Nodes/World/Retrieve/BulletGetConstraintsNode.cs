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
    [PluginInfo(Name = "Constraints", Category = "Bullet", Version ="", Author = "vux")]
    public class BulletGetConstraintsNode : IPluginEvaluate
    {
        [Input("World", IsSingle = true)]
        protected ISpread<IConstraintCollection> FWorld;

        [Output("Constraints")]
        protected ISpread<TypedConstraint> FConstraints;

        public void Evaluate(int SpreadMax)
        {
            if (this.FWorld[0] != null)
            {
                var constraints = this.FWorld[0].Constraints;
                this.FConstraints.SliceCount = constraints.Count;

                var outputBuffer = this.FConstraints.Stream.Buffer;
                for (int i = 0; i < constraints.Count; i++)
                {
                    outputBuffer[i] = constraints[i];
                }
                this.FConstraints.Flush(true);
            }
            else
            {
                this.FConstraints.SliceCount = 0;
            }
        }
    }
}
