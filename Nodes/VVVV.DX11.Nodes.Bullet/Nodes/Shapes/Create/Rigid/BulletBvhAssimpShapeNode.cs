using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.Nodes.Bullet;
using VVVV.Utils.VMath;
using BulletSharp;
using VVVV.Bullet.DataTypes.Shapes.Rigid;
using AssimpNet;

namespace VVVV.Bullet.Nodes.Shapes.Create.Rigid
{
    [PluginInfo(Name = "Bvh", Category = "Bullet", Version="Assimp", Author = "vux")]
    public class BulletBvhAssimpShapeNode : AbstractBulletRigidShapeNode
    {
        [Input("Assimp Mesh",CheckIfChanged=true)]
        protected Pin<AssimpMesh> FInMesh;

        public override void Evaluate(int SpreadMax)
        {
            if ((this.FInMesh.IsChanged || this.BasePinsChanged) && this.FInMesh.IsConnected)
            {
                int spmax = ArrayMax.Max(FInMesh.SliceCount, this.BasePinsSpreadMax);

                this.FShapes.SliceCount = spmax;

                for (int i = 0; i < spmax; i++)
                {
                    BvhShapeDefinition shadeDef = new BvhShapeDefinition(this.FInMesh[i]);
                    this.SetBaseParams(shadeDef, i);

                    this.FShapes[i] = shadeDef;
                }
            }
        }
    }
}
