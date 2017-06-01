using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;

using BulletSharp;
using VVVV.DataTypes.Bullet;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name="ConvexHull",Category="Bullet",Author="vux")]
	public class BulletConvexHullShapeNode : AbstractBulletRigidDynamicShapeNode
    {
		[Input("Vertices")]
        protected IDiffSpread<ISpread<Vector3D>> FVertices;

		public override void Evaluate(int SpreadMax)
		{
			int spmax = ArrayMax.Max(FVertices.SliceCount, this.BasePinsSpreadMax);

			if (this.FVertices.IsChanged || this.BasePinsChanged)
			{
				this.FShapes.SliceCount = spmax;

				for (int i = 0; i < spmax; i++)
				{
					//Vector3D size = this.FSize[i];
					Vector3[] vertices = new Vector3[this.FVertices[i].SliceCount];

					for (int j = 0; j < this.FVertices[i].SliceCount; j++)
					{
						vertices[j] = this.FVertices[i][j].ToBulletVector();
					}

					ConvexHullShapeDefinition chull = new ConvexHullShapeDefinition(vertices);
					chull.Mass = this.FMass[i];
					this.SetBaseParams(chull, i);

					this.FShapes[i] = chull;
				}
			}
		}
	}
}

