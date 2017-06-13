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
    [PluginInfo(Name = "Hinge", Author = "vux", Category = "Bullet", Version = "Constraint.Single", AutoEvaluate = true)]
    public class CreateHingeConstraintSingleNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("World")]
        protected ISpread<IConstraintContainer> contraintContainer;

        [Input("Body")]
        protected ISpread<RigidBody> FBody;

        [Input("Pivot")]
        protected ISpread<Vector3D> FPivot1;

        [Input("Axis", DefaultValues = new double[] { 0, 1, 0 })]
        protected ISpread<Vector3D> FAxis;

        [Input("Limit Low")]
        protected ISpread<float> FLimitLow;

        [Input("Limit High")]
        protected ISpread<float> FLimitHigh;

        [Input("Do Create", IsBang =true, Order = 15000)]
        protected ISpread<bool> FCreate;

        [Output("Constraints")]
        protected ISpread<HingeConstraint> constraintsOutput;

        private ConstraintPersister<HingeConstraint> persister;

        public void OnImportsSatisfied()
        {
            this.persister = new ConstraintPersister<HingeConstraint>(
                new ConstraintListener<HingeConstraint>(), this.constraintsOutput);
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
                        RigidBody body = FBody[i];
                        if (body != null)
                        {
                            HingeConstraint cst = new HingeConstraint(body, this.FPivot1[i].ToBulletVector(), this.FAxis[i].ToBulletVector());
                            cst.SetLimit(this.FLimitLow[i] * (float)Math.PI * 2.0f, this.FLimitHigh[i] * (float)Math.PI * 2.0f);
                            cst.DebugDrawSize = 5.0f;
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
