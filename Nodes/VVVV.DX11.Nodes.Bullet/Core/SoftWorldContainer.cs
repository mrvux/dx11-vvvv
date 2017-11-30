using System;
using System.Collections.Generic;
using System.Text;
using BulletSharp;
using BulletSharp.SoftBody;
using VVVV.Internals.Bullet;
using VVVV.Bullet.Core;


namespace VVVV.Bullet.Core
{
	//Just to make it easier to manage than having million of stuff in
	//World node. Can also easily switch broadphases
	public class BulletSoftWorldContainer : IRigidBulletWorld, ISoftBodyCollection, IConstraintContainer
    {
		private DefaultCollisionConfiguration collisionConfiguration;
		private CollisionDispatcher dispatcher;
		private SequentialImpulseConstraintSolver solver;
		private BroadphaseInterface overlappingPairCache;
		private SoftRigidDynamicsWorld dynamicsWorld;
		private SoftBodyWorldInfo worldInfo;

		private int bodyindex;
		private int cstindex;

		protected bool enabled;
		protected float gx, gy, gz;
		protected float ts;
		protected int iter;

		public event RigidBodyDeletedDelegate RigidBodyDeleted;
		public event SoftBodyDeletedDelegate SoftBodyDeleted;
		public event ConstraintDeletedDelegate ConstraintDeleted;
		public event WorldResetDelegate WorldHasReset;

        private ObjectLifetimeContainer<RigidBody, BodyCustomData> bodyContainer;
        private ObjectLifetimeContainer<SoftBody, SoftBodyCustomData> softBodyContainer;
        private ObjectLifetimeContainer<TypedConstraint, ConstraintCustomData> constraintContainer;

        public BulletSoftWorldContainer()
        {
            this.bodyContainer = new ObjectLifetimeContainer<RigidBody, BodyCustomData>(b => (BodyCustomData)b.UserObject);
            this.softBodyContainer = new ObjectLifetimeContainer<SoftBody, SoftBodyCustomData>(s => (SoftBodyCustomData)s.UserObject);
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
            this.dynamicsWorld.DeleteAndDisposeBody(body);

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

		#region Soft Registry
		public void Register(SoftBody body)
		{
			this.dynamicsWorld.AddSoftBody(body);
			this.softBodyContainer.RegisterObject(body);
		}

        private void RemoveSoftBody(SoftBody body, SoftBodyCustomData cdata)
        {
            this.World.RemoveCollisionObject(body);
            body.Dispose();

            if (this.SoftBodyDeleted != null)
            {
                this.SoftBodyDeleted(body, cdata.Id);
            }
        }

		public List<SoftBody> SoftBodies
		{
			get { return this.softBodyContainer.ObjectList; }
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
            this.dynamicsWorld.DeleteAndDisposeConstraint(cst);

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
			dynamicsWorld = new SoftRigidDynamicsWorld(dispatcher, overlappingPairCache, solver, collisionConfiguration);
			worldInfo = new SoftBodyWorldInfo();
			worldInfo.Gravity = dynamicsWorld.Gravity;
			worldInfo.Broadphase = overlappingPairCache;
			worldInfo.Dispatcher = dispatcher;
			worldInfo.SparseSdf.Initialize();
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
            this.softBodyContainer.Process(dt, this.RemoveSoftBody);
            this.constraintContainer.Process(dt, this.RemoveConctraint);
        }
		#endregion

		#region Info
		public SoftBodyWorldInfo WorldInfo
		{
			get { return worldInfo; }
			set { worldInfo = value; }
		}

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
            this.dynamicsWorld.DeleteAndDisposeAllConstraints();

            this.dynamicsWorld.DeleteAndDisposeAllRigidBodies();

            dynamicsWorld.Dispose();
			solver.Dispose();
			overlappingPairCache.Dispose();
			dispatcher.Dispose();
			collisionConfiguration.Dispose();



            this.bodyContainer.Clear();
			this.softBodyContainer.Clear();
			this.constraintContainer.Clear();
			this.created = false;
		}
		#endregion

	}
}
