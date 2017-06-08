using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Rigid body listener, process deleted items 
    /// </summary>
    public class RigidBodyListListener
    {
        private IRigidBodyContainer currentWorld;
        private List<RigidBody> currentBodyList = new List<RigidBody>();
        private List<int> currentIdList = new List<int>();

        /// <summary>
        /// List of active bodies
        /// </summary>
        public List<RigidBody> Bodies
        {
            get { return this.currentBodyList; }
        }

        /// <summary>
        /// List of active ids
        /// </summary>
        public List<int> Ids
        {
            get { return this.currentIdList; }
        }

        public void UpdateWorld(IRigidBodyContainer inputWorld)
        {
            if (currentWorld != inputWorld)
            {
                if (currentWorld != null)
                {
                    currentWorld.WorldHasReset -= OnWorldReset;
                    currentWorld.RigidBodyDeleted -= OnRigidBodyDeleted;
                }

                this.currentBodyList.Clear();
                this.currentIdList.Clear();

                this.currentWorld = inputWorld;
                if (currentWorld != null)
                {
                    currentWorld.WorldHasReset += OnWorldReset;
                    currentWorld.RigidBodyDeleted += OnRigidBodyDeleted;
                }
            }

        }

        public void Append(RigidBody rigidBody, int id)
        {
            this.currentBodyList.Add(rigidBody);
            this.currentIdList.Add(id);
        }

        private void OnRigidBodyDeleted(RigidBody rb, int id)
        {
            this.currentIdList.Remove(id);
            this.currentBodyList.Remove(rb);
        }

        private void OnWorldReset()
        {
            this.currentBodyList.Clear();
            this.currentIdList.Clear();
        }
    }
}
