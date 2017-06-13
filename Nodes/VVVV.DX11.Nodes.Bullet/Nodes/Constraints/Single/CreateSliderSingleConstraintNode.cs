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
    [PluginInfo(Name = "Slider", Author = "vux", Category = "Bullet", Version = "Constraint.Single", AutoEvaluate = true)]
    public unsafe class CreateSliderConstraintSingleNode : IPluginEvaluate, IPartImportsSatisfiedNotification
    {
        [Input("World")]
        protected ISpread<IConstraintContainer> contraintContainer;

        [Input("Body")]
        protected ISpread<RigidBody> FBody;

        [Input("Frame")]
        protected ISpread<SlimDX.Matrix> FFrame;

        [Input("Linear Limit Min/Max", DefaultValues = new double[] { 0, 1 })]
        protected ISpread<SlimDX.Vector2> FLinearLimit;

        [Input("Angular Limit Min/Max", DefaultValues =new double[] { 0, 1 })]
        protected ISpread<SlimDX.Vector2> FAngularLimit;

        [Input("Do Create", IsBang =true, Order = 15000)]
        protected ISpread<bool> FCreate;

        [Output("Constraints")]
        protected ISpread<SliderConstraint> constraintsOutput;

        private ConstraintPersister<SliderConstraint> persister;

        public void OnImportsSatisfied()
        {
            this.persister = new ConstraintPersister<SliderConstraint>(
                new ConstraintListener<SliderConstraint>(), this.constraintsOutput);
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
                            SlimDX.Matrix m = this.FFrame[i];
                            SliderConstraint cst = new SliderConstraint(body, *(BulletSharp.Matrix*)&m, true);
                            cst.LowerLinLimit = this.FLinearLimit[i].X;
                            cst.UpperLinLimit = this.FLinearLimit[i].Y;
                            cst.LowerAngularLimit = this.FAngularLimit[i].X * (float)Math.PI * 2.0f;
                            cst.UpperAngularLimit = this.FAngularLimit[i].Y * (float)Math.PI * 2.0f;
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
