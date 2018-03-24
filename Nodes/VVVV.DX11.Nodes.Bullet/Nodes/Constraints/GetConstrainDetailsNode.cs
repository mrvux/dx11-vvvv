using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using BulletSharp;
using VVVV.Internals.Bullet;
using VVVV.Utils.VMath;
using VVVV.Bullet.Core;

namespace VVVV.Nodes.Bullet
{
	[PluginInfo(Name = "GetConstraintDetails", Category = "Bullet", Author = "vux")]
	public class GetConstraintDetailsNode : IPluginEvaluate
	{
		[Input("Contraint")]
        protected Pin<TypedConstraint> FInput;

        [Output("Body 1")]
        protected ISpread<RigidBody> Body1;

        [Output("Body 2")]
        protected ISpread<RigidBody> Body2;

        [Output("Body 2 Valid")]
        protected ISpread<bool> Body2Valid;

        [Output("Type")]
        protected ISpread<TypedConstraintType> FType;

		[Output("Id")]
        protected ISpread<int> FId;

		[Output("LifeTime")]
        protected ISpread<double> FLifeTime;

		[Output("Custom")]
        protected ISpread<string> FCustom;

		public void Evaluate(int SpreadMax)
		{
            if (this.FInput.IsConnected)
            {
                this.FType.SliceCount = SpreadMax;
                this.FLifeTime.SliceCount = SpreadMax;
                this.FId.SliceCount = SpreadMax;
                this.FCustom.SliceCount = SpreadMax;
                this.Body2.SliceCount = SpreadMax;
                this.Body1.SliceCount = SpreadMax;
                this.Body2Valid.SliceCount = SpreadMax;

                for (int i = 0; i < SpreadMax; i++)
                {
                    TypedConstraint cst = FInput[i];
                    ConstraintCustomData sc = (ConstraintCustomData)cst.UserObject;
                    FType[i] = cst.ConstraintType;
                    FId[i] = sc.Id;
                    FLifeTime[i] = sc.LifeTime;
                    FCustom[i] = sc.Custom;
                    Body1[i] = cst.RigidBodyA;
                    Body2[i] = cst.RigidBodyB;
                    Body2Valid[i] = cst.RigidBodyB != null;
                }
            }
            else
            {
                this.FType.SliceCount = 0;
                this.FLifeTime.SliceCount = 0;
                this.FId.SliceCount = 0;
                this.FCustom.SliceCount = 0;
                this.Body2.SliceCount = 0;
                this.Body1.SliceCount = 0;
                this.Body2Valid.SliceCount = 0;
            }

		}
	}
}
