using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using VVVV.DataTypes.Bullet;
using AssimpNet;
using VVVV.Bullet.Core;

namespace VVVV.Bullet.DataTypes.Shapes.Rigid
{
    public unsafe class BvhShapeDefinition : RigidShapeDefinitionBase
    {
        private Vector3[] vertices;
        private int[] indices;

        public BvhShapeDefinition(Vector3[] vertices, int[] indices)
        {
            this.vertices = vertices;
            this.indices = indices;
        }

        public BvhShapeDefinition(AssimpMesh mesh)
        {
            this.vertices = new Vector3[mesh.VerticesCount];
            this.indices = mesh.Indices.ToArray();

            Vector3* v = (Vector3*)mesh.Positions();

            for (int i = 0; i < mesh.VerticesCount; i++)
            {
                this.vertices[i] = v[i];
            }
        }

        protected override CollisionShape CreateShape()
        {
            TriangleIndexVertexArray tiv = new TriangleIndexVertexArray(this.indices, this.vertices);
            BvhTriangleMeshShape bvh = new BvhTriangleMeshShape(tiv, true, true);
            return bvh;
        }
    }
}
