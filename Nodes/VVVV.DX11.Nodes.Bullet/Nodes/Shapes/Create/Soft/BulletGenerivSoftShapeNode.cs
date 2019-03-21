using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using VVVV.Utils.VMath;
using VVVV.DataTypes.Bullet;

using BulletSharp;


namespace VVVV.Nodes.Bullet
{
	
	[PluginInfo(Name="SoftShape",Category="Bullet", Version ="Advanced", Author="vux", 
        Help ="Creates a soft body shape, manually specifying positions and link")]
	public class BulletSoftShapeNode : AbstractSoftShapeNode
	{
		[Input("Position")]
        protected IDiffSpread<SlimDX.Vector3> FPosition;

        [Input("Mass", DefaultValue =1.0)]
        protected IDiffSpread<float> FMass;

        [Input("Indices")]
        protected IDiffSpread<int> FIndices;

		protected override bool SubPinsChanged
		{
			get
			{
                return this.FPosition.IsChanged
                || this.FMass.IsChanged
                || this.FIndices.IsChanged;
			}
		}

		protected override AbstractSoftShapeDefinition GetShapeDefinition(int slice)
		{
            Vector3Array p = new Vector3Array(FPosition.SliceCount);
            ScalarArray m = new ScalarArray(FMass.SliceCount);

            for (int i = 0; i < FPosition.SliceCount; i++)
            {
                p[i] = FPosition[i].ToBulletVector();
            }

            for (int i = 0; i < FPosition.SliceCount; i++)
            {
                m[i] = FMass[i];
            }

            int[] indices = FIndices.ToArray();

            return new GenericSoftShapeDefinition(p, indices, m);

        }

		protected override int SubPinSpreadMax
		{
			get {  return 1; }

		}
	}
}
