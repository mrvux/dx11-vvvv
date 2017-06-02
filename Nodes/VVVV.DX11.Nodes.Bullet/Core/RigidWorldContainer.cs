using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using BulletSharp.SoftBody;
using VVVV.Internals.Bullet;
using VVVV.Bullet.Core;


namespace VVVV.Bullet.Core
{
	public class BulletRigidWorldContainer : IRigidBulletWorld, IConstraintContainer
    {
		private DefaultCollisionConfiguration collisionConfiguration;
		private CollisionDispatcher dispatcher;
		private SequentialImpulseConstraintSolver solver;
		private BroadphaseInterface overlappingPairCache;
		private DiscreteDynamicsWorld dynamicsWorld;

		private int bodyindex;
		private int cstindex;

		protected bool enabled;
		protected float gx, gy, gz;
		protected float ts;
		protected int iter;

		public event RigidBodyDeletedDelegate RigidBodyDeleted;
		public event ConstraintDeletedDelegate ConstraintDeleted;
		public event WorldResetDelegate WorldHasReset;

        private ObjectLifetimeContainer<RigidBody, BodyCustomData> bodyContainer;
        private ObjectLifetimeContainer<TypedConstraint, ConstraintCustomData> constraintContainer;

        public BulletRigidWorldContainer()
        {
            this.bodyContainer = new ObjectLifetimeContainer<RigidBody, BodyCustomData>(b => (BodyCustomData)b.UserObject);
            this.constraintContainer = new ObjectLifetimeContainer<TypedConstraint, ConstraintCustomData>(c => (ConstraintCustomData)c.UserObject);
        }

        public Dispatcher Dispatcher
        {
            get { return this.dispatcher; }
        }

        #region Rigid Registry
        public void Register(RigidBody body)
		{
			this.World.AddRigidBody(body);
            this.bodyContainer.RegisterObject(body);
		}

        private void RemoveRigidBody(RigidBody body, BodyCustomData cdata)
        {
            this.World.RemoveRigidBody(body);
            if (this.RigidBodyDeleted != null)
            {
                this.RigidBodyDeleted(body, cdata.Id);
            }
        }

		public List<RigidBody> RigidBodies
		{
			get { return this.bodyContainer.ObjectList; }
		}
		#endregion

		#region Contraints Registry
		public void Register(TypedConstraint cst, bool collideconnected)
		{
			this.World.AddConstraint(cst, !collideconnected);
            this.constraintContainer.RegisterObject(cst);
		}

		public void Register(TypedConstraint cst)
		{
			this.Register(cst, true);
		}

		public void RemoveConctraint(TypedConstraint cst, ConstraintCustomData cdata)
		{
            this.World.RemoveConstraint(cst);
            if (this.ConstraintDeleted != null)
            {
                this.ConstraintDeleted(cst, cdata.Id);
            }
		}

        public List<TypedConstraint> Constraints
		{
			get { return this.constraintContainer.ObjectList; }
		}
		#endregion

		#region Creation
		private bool created = false;
		public bool Created { get { return this.created; } }

		public void Create()
		{
			if (created)
			{
				this.Destroy();
			}

			this.bodyindex = 0;
			this.cstindex = 0;

			collisionConfiguration = new SoftBodyRigidBodyCollisionConfiguration();
			dispatcher = new CollisionDispatcher(collisionConfiguration);
			solver = new SequentialImpulseConstraintSolver();
			overlappingPairCache = new DbvtBroadphase();
			dynamicsWorld = new DiscreteDynamicsWorld(dispatcher, overlappingPairCache, solver, collisionConfiguration);
			this.created = true;

			if (this.WorldHasReset != null)
			{
				this.WorldHasReset();
			}
		}
		#endregion

		public int GetNewBodyId()
		{
			this.bodyindex++;
			return this.bodyindex;
		}

		public int GetNewConstraintId()
		{
			this.cstindex++;
			return this.cstindex;
		}

		#region Process Deletion
		internal void ProcessDelete(double dt)
		{
            this.bodyContainer.Process(dt, this.RemoveRigidBody);
            this.constraintContainer.Process(dt, this.RemoveConctraint);
        }
		#endregion

		#region Info
		public DynamicsWorld World
		{
			get { return this.dynamicsWorld; }
		}

		public int ObjectCount
		{
			get { return this.dynamicsWorld.NumCollisionObjects; }
		}
		#endregion

		#region Gravity/Enabled/Ans Step Stuff
		public void SetGravity(float x, float y, float z)
		{
			this.gx = x;
			this.gy = y;
			this.gz = z;
			this.dynamicsWorld.Gravity = new Vector3(this.gx, this.gy, this.gz);
		}

		public bool Enabled
		{
			set { this.enabled = value; }
			get { return this.enabled; }
		}

		public float TimeStep
		{
			set { this.ts = value; }
		}

		public int Iterations
		{
			set { this.iter = value; }
		}

		public void Step()
		{
			if (this.enabled)
			{
				this.dynamicsWorld.StepSimulation(this.ts, this.iter);
			}
		}
		#endregion

		#region Destroy
		public void Destroy()
		{
            dynamicsWorld.Dispose();
			solver.Dispose();
			overlappingPairCache.Dispose();
			dispatcher.Dispose();
			collisionConfiguration.Dispose();

            this.bodyContainer.Clear();
			this.constraintContainer.Clear();
			this.created = false;
		}
		#endregion

	}
}
