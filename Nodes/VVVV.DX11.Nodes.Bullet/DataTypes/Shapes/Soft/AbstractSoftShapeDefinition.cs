using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using SlimDX.Direct3D9;
using BulletSharp.SoftBody;
using VVVV.Bullet.Core;

namespace VVVV.DataTypes.Bullet
{
	public abstract class AbstractSoftShapeDefinition
	{
		private bool genbend;
		private int benddist;

        public SoftBodyProperties Properties { get; set; }

        public bool GenerateBendingConstraints
		{
			get { return this.genbend; }
			set { this.genbend = value; }
		}

		public int BendingDistance
		{
			get { return this.benddist; }
			set { this.benddist = value; }
		}



		public SoftBody GetSoftBody(SoftBodyWorldInfo si)
		{
			SoftBody body = this.CreateSoftBody(si);
			return body;
		}


		protected abstract SoftBody CreateSoftBody(SoftBodyWorldInfo si);
		public virtual bool HasUV { get { return false; } }
		public virtual float[] GetUV(SoftBody sb) { return null; }

		protected void SetConfig(SoftBody sb)
		{
			sb.Cfg.AeroModel = this.Properties.AeroModel;
			sb.Cfg.DF = this.Properties.DynamicFrictionCoefficient;
			sb.Cfg.DP = this.Properties.DampingCoefficient;
			sb.Cfg.PR = this.Properties.PressureCoefficient;
			sb.Cfg.LF = this.Properties.LiftCoefficient;
			sb.Cfg.VC = this.Properties.VolumeConservationCoefficient;
			sb.Cfg.Collisions |= FCollisions.VFSS;

			sb.Cfg.Chr = this.Properties.RigidContactHardness;
			sb.Cfg.Shr = this.Properties.SoftContactHardness;


			sb.Cfg.DG = this.Properties.DragCoefficient;
			sb.Cfg.Ahr = this.Properties.AnchorHardness;
			if (this.Properties.IsVolumeMass)
			{
				sb.SetVolumeMass(this.Properties.Mass);
			}
			else
			{
				sb.SetTotalMass(this.Properties.Mass, false);
			}

			if (this.GenerateBendingConstraints)
			{
				sb.GenerateBendingConstraints(this.BendingDistance);
			}
		}
	}
}
