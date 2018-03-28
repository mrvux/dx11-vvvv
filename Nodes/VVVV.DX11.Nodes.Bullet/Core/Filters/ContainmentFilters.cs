using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using SlimDX;

namespace VVVV.Bullet.Core.Filters
{
    public enum RigidBodyCheckType
    {
        Center = 0
    }

    public class SphereContainmentFilter : IRigidBodyFilter
    {
        public BoundingSphere Bounds;
        public RigidBodyCheckType CheckType;
        public List<ContainmentType> Containments = new List<ContainmentType>();

        public bool Filter(RigidBody body)
        {
            var center = new SlimDX.Vector3(body.MotionState.WorldTransform.M41,
                        body.MotionState.WorldTransform.M42, body.MotionState.WorldTransform.M43);

            var ct = BoundingSphere.Contains(Bounds, center);
            return this.Containments.Contains(ct);
        }
    }

    public class BoxContainmentFilter : IRigidBodyFilter
    {
        public BoundingBox Bounds;
        public RigidBodyCheckType CheckType;
        public List<ContainmentType> Containments = new List<ContainmentType>();

        public bool Filter(RigidBody body)
        {
            var center = new SlimDX.Vector3(body.MotionState.WorldTransform.M41,
                        body.MotionState.WorldTransform.M42, body.MotionState.WorldTransform.M43);

            var ct = BoundingBox.Contains(Bounds, center);
            return this.Containments.Contains(ct);
        }
    }
}
