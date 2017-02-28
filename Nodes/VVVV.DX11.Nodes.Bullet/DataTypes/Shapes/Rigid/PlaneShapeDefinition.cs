using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;

using SlimDX.Direct3D9;

using VVVV.Internals.Bullet.EX9;

namespace VVVV.DataTypes.Bullet
{
    public class PlaneShapeDefinition : AbstractRigidShapeDefinition
    {
        private Vector3 normal;
        private float w;

        public override int ShapeCount
        {
            get { return 1; }
        }

        public PlaneShapeDefinition(Vector3 normal, float w)
        {
            this.normal = normal;
            this.w = w;
        }


        protected override CollisionShape CreateShape()
        {
            CollisionShape shape = new StaticPlaneShape(this.normal, this.w);
            return shape;
        }

        protected override BulletMesh CreateMesh(Device device)
        {
            //Build the box mesh
            return null;// new BulletMesh(Mesh.CreateBox(device, this.w * 2.0f, this.h * 2.0f, this.d * 2.0f));
        }


    }
}
