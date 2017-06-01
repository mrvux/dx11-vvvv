using BulletSharp;
using FeralTic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Bullet.Core;

namespace VVVV.DataTypes.Bullet
{
    public class CapsuleShapeDefinition : DynamicShapeDefinitionBase
    {
        private float radius;
        private float height;
        private Axis axis;

        public CapsuleShapeDefinition(float radius, float height, Axis axis)
        {
            this.radius = radius;
            this.height = height;
            this.axis = axis;
        }

        protected override CollisionShape CreateShape()
        {
            if (axis == Axis.X)
            {
                return new CapsuleShapeX(this.radius, this.height);
            }
            else if (axis == Axis.Y)
            {
                return new CapsuleShape(this.radius, this.height);
            }
            else
            {
                return new CapsuleShapeZ(this.radius, this.height);
            }
        }
    }
}
