using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Bullet.Core;
using VVVV.Internals.Bullet;

namespace VVVV.Bullet.DataTypes.World
{
    public class ObjectLifetimeContainer<TType, TLifeTime> where TLifeTime : ObjectCustomData
    {
        private List<TType> objectList = new List<TType>();
        private Func<TType, TLifeTime> getDetailsFunc;

        private List<TType> deletionList = new List<TType>();
        private List<TLifeTime> idList = new List<TLifeTime>();

        public List<TType> ObjectList
        {
            get { return this.objectList; }
        }

        public ObjectLifetimeContainer(Func<TType, TLifeTime> getDetailsFunc)
        {
            if (getDetailsFunc == null)
                throw new ArgumentNullException("getDtailsFunc");

            this.getDetailsFunc = getDetailsFunc;
        }

        public void RegisterObject(TType obj)
        {
            this.objectList.Add(obj);
        }

        public void Clear()
        {
            this.objectList.Clear();
        }

        public void Process(double dt, Action<TType, TLifeTime> deleteAction)
        {
            this.deletionList.Clear();
            this.idList.Clear();

            for (int i = 0; i < this.objectList.Count; i++)
            {
                TType obj = this.objectList[i];
                TLifeTime lifeTime = getDetailsFunc(obj);

                if (!lifeTime.Created)
                {
                    lifeTime.LifeTime += dt;
                }
                lifeTime.Created = false;
                if (lifeTime.MarkedForDeletion)
                {
                    this.deletionList.Add(obj);
                    this.idList.Add(lifeTime);
                }
            }

            for (int i = 0; i < this.deletionList.Count; i++)
            {
                TType obj = this.deletionList[i];
                deleteAction(obj, idList[i]);
                this.objectList.Remove(obj);
            }
        }
    }
}
