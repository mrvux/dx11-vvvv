using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using VVVV.DataTypes.Bullet;

namespace VVVV.Bullet.Internals
{
    public class BulletRogidBodyListListener
    {
        private BulletRigidSoftWorld currentWorld;
        private List<RigidBody> currentBodyList = new List<RigidBody>();
        private List<int> currentIdList = new List<int>();

        public List<RigidBody> Bodies
        {
            get { return this.currentBodyList; }
        }

        public List<int> Ids
        {
            get { return this.currentIdList; }
        }

        public void UpdateWorld(BulletRigidSoftWorld inputWorld)
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
            }

            if (currentWorld != null)
            {
                currentWorld.WorldHasReset += OnWorldReset;
                currentWorld.RigidBodyDeleted += OnRigidBodyDeleted;
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
