using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VMath;
using VVVV.Bullet.Core;
using System.ComponentModel.Composition;

namespace VVVV.Nodes.Bullet
{
    [PluginInfo(Name = "SetPoint2PointPivot", Author = "vux", Category = "Bullet", Version = "Constraint.Single", AutoEvaluate = true)]
    public class SetPoint2PointPivot : IPluginEvaluate
    {
        [Input("Constraint", Order = 11)]
        protected ISpread<Point2PointConstraint> FConstraint;

        [Input("Pivot", Order = 12)]
        protected ISpread<Vector3D> FPivot1;

        [Input("Apply", IsBang = true, Order = 15000)]
        protected ISpread<bool> FApply;

        public void Evaluate(int SpreadMax)
        {
            for (int i = 0; i < SpreadMax; i++)
            {
                if (this.FApply[i] && this.FConstraint[i] != null)
                {
                    Point2PointConstraint cst = this.FConstraint[i];
                    cst.PivotInA = this.FPivot1[i].ToBulletVector();
                }
            }
        }
    }
}
