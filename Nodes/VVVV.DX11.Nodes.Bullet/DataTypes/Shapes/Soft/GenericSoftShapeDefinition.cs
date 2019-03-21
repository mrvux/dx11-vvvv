using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp.SoftBody;
using BulletSharp;

namespace VVVV.DataTypes.Bullet
{
	public class GenericSoftShapeDefinition : AbstractSoftShapeDefinition
	{
		private Vector3Array vertices;
		private int[] indices;
        private ScalarArray mass;

		public GenericSoftShapeDefinition(Vector3Array vertices, int[] indices, ScalarArray mass)
		{
			this.vertices = vertices;
			this.indices = indices;
            this.mass = mass;
		}

		protected override SoftBody CreateSoftBody(SoftBodyWorldInfo si)
		{
            si.SparseSdf.Reset();

            SoftBody sb = new SoftBody(si, vertices, mass);

            int pairs = this.indices.Length / 2;
            for (int i = 0; i < pairs; i++)
            {
                sb.AppendLink(indices[i * 2], indices[i * 2 + 1]);
            }
            sb.RandomizeConstraints();
            this.SetConfig(sb);

            return sb;
        }

	}
}
