using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp.SoftBody;
using VVVV.Utils.VColor;
using VVVV.DataTypes.Bullet;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	public abstract class AbstractSoftShapeNode : IPluginEvaluate
	{
		[Input("Soft Body Properties",DefaultEnumEntry="VPoint")]
        protected Pin<SoftBodyProperties> FPinInSoftProperties;

		[Input("Generate Bending Constraints", DefaultValue = 0.0)]
        protected IDiffSpread<bool> FPinInGenBend;

		[Input("Bending Constraints Distance", DefaultValue = 1.0)]
        protected IDiffSpread<int> FPinInBendDist;

		[Output("Shape")]
        protected ISpread<AbstractSoftShapeDefinition> FPinOutShapes;

		protected abstract bool SubPinsChanged { get; }
		protected abstract AbstractSoftShapeDefinition GetShapeDefinition(int slice);
		protected abstract int SubPinSpreadMax { get; }

		public void Evaluate(int SpreadMax)
		{
			if (this.FPinInSoftProperties.IsChanged
				|| this.FPinInGenBend.IsChanged
				|| this.FPinInBendDist.IsChanged
				|| this.FPinInBendDist.IsChanged
				|| this.FPinInGenBend.IsChanged
				|| this.SubPinsChanged)
			{
				this.FPinOutShapes.SliceCount =
					ArrayMax.Max(
					this.FPinInSoftProperties.SliceCount,
					this.FPinInBendDist.SliceCount,
					this.FPinInGenBend.SliceCount,
					this.SubPinSpreadMax
					);

				for (int i = 0; i < SpreadMax; i++)
				{
					AbstractSoftShapeDefinition shape = this.GetShapeDefinition(i);
			
					shape.GenerateBendingConstraints = this.FPinInGenBend[i];
					shape.BendingDistance = this.FPinInBendDist[i];
                    shape.Properties = this.FPinInSoftProperties.IsConnected ? this.FPinInSoftProperties[i] : SoftBodyProperties.Default;
                    this.FPinOutShapes[i] = shape;
				}
			}
			
		}
	}
}
