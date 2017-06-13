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
    [PluginInfo(Name = "Point2Point", Author = "vux", Category = "Bullet", Version = "Constraint.Dual", AutoEvaluate = true)]
    public class CreateDualP2PConstraintNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("World", Order = 1)]
        protected ISpread<IConstraintContainer> contraintContainer;

        [Input("Body Pair", Order = 5)]
        protected ISpread<RigidBodyPair> FPairs;

        [Input("Pivot 1", Order = 10)]
        protected ISpread<Vector3D> FPivot1;

        [Input("Pivot 2", Order = 11)]
        protected ISpread<Vector3D> FPivot2;

        [Input("Damping", Order = 12)]
        protected ISpread<float> FDamping;

        [Input("Impulse Clamp", Order = 13)]
        protected ISpread<float> FImpulseClamp;

        [Input("Tau", Order = 14)]
        protected ISpread<float> FTau;

        [Input("Do Create", IsBang = true, Order = 1400)]
        protected ISpread<bool> FCreate;

        [Output("Constraints")]
        protected ISpread<Point2PointConstraint> constraintsOutput;

        private ConstraintPersister<Point2PointConstraint> persister;

        public void OnImportsSatisfied()
        {
            this.persister = new ConstraintPersister<Point2PointConstraint>(
                new ConstraintListener<Point2PointConstraint>(), this.constraintsOutput);
        }

        public void Evaluate(int SpreadMax)
        {
            IConstraintContainer inputWorld = this.contraintContainer[0];

            if (inputWorld != null)
            {
                this.persister.UpdateWorld(inputWorld);

                for (int i = 0; i < SpreadMax; i++)
                {
                    if (FCreate[i])
                    {
                        RigidBodyPair pair = FPairs[i];
                        if (pair.body1 != null && pair.body2 != null)
                        {
                            Point2PointConstraint cst = new Point2PointConstraint(pair.body1, pair.body2,
                                this.FPivot1[i].ToBulletVector(), this.FPivot2[i].ToBulletVector());
                            cst.Setting.Damping = this.FDamping[i];
                            cst.Setting.ImpulseClamp = this.FImpulseClamp[i];
                            cst.Setting.Tau = this.FTau[i];

                            this.persister.Append(cst);
                        }
                    }
                }

                this.persister.Flush();
            }
            else
            {
                this.constraintsOutput.SliceCount = 0;
            }
        }
    }
}