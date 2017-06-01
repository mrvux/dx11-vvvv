using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using System.IO;
using System.Runtime.InteropServices;
using VVVV.Bullet.Core;

namespace VVVV.DataTypes.Bullet
{
	public class HeightFieldShapeDefinition : RigidShapeDefinitionBase
	{
		private int w, l;
		private float[] h;
		private float minh, maxh;
		private MemoryStream ms;
		

		public HeightFieldShapeDefinition(int w, int l, float[] h, float minh, float maxh)
		{
			this.w = w;
			this.l = l;
			this.h = h;
			this.minh = minh;
			this.maxh = maxh;
			this.ms = null;
		}

		protected override CollisionShape CreateShape()
		{
			if (this.ms == null)
			{
				byte[] terr = new byte[this.w * this.l * 4];
				ms = new MemoryStream(terr);
				BinaryWriter writer = new BinaryWriter(ms);
				for (int i = 0; i < this.w * this.l; i++)
				{
					writer.Write(this.h[i]);
				}
				writer.Flush();
			}
			ms.Position = 0;
			HeightfieldTerrainShape hs = new HeightfieldTerrainShape(w, l, ms, 0.0f, minh, maxh, 1, PhyScalarType.PhyFloat, false);
			hs.SetUseDiamondSubdivision(true);
			//hs.LocalScaling = new Vector3(this.sx, 1.0f, this.sz);
			return hs;
			
		}
	}
}
