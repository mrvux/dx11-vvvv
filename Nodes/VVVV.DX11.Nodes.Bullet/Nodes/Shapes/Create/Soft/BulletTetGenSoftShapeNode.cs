using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

using BulletSharp;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "TetGen", Category = "Bullet",Version="SoftShape", Author = "vux")]
	public class BulletTetGenSoftShapeNode : AbstractSoftShapeNode
	{
		[Input("Elements")]
        protected IDiffSpread<string> FElements;

		[Input("Faces")]
        protected IDiffSpread<string> FFaces;

		[Input("Nodes")]
        protected IDiffSpread<string> FNodes;

		[Input("Face Links")]
        protected IDiffSpread<bool> FFaceLinks;

		[Input("Tetra Links")]
        protected IDiffSpread<bool> FTetraLinks;

		[Input("Faces from Tetras")]
        protected IDiffSpread<bool> FFacefromtetra;	

		protected override bool SubPinsChanged
		{
			get
			{
				return this.FElements.IsChanged ||
				this.FFacefromtetra.IsChanged ||
				this.FFaceLinks.IsChanged ||
				this.FFaces.IsChanged ||
				this.FNodes.IsChanged || this.FTetraLinks.IsChanged;
			}
		}

		protected override AbstractSoftShapeDefinition GetShapeDefinition(int slice)
		{
			return new TetGenDataSoftShapeDefinition(this.FElements[slice],
				this.FFaces[slice],this.FNodes[slice],this.FFaceLinks[slice],this.FTetraLinks[slice],this.FFacefromtetra[slice]);
		}

		protected override int SubPinSpreadMax
		{
			get
			{
				return ArrayMax.Max
					(
						this.FElements.SliceCount,
						this.FFacefromtetra.SliceCount,
						this.FFaceLinks.SliceCount,
						this.FFaces.SliceCount,
						this.FNodes.SliceCount,
						this.FTetraLinks.SliceCount
					);
			}

		}
	}
}

