using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Container for objects lifetime
    /// </summary>
    /// <typeparam name="TType">Object type</typeparam>
    /// <typeparam name="TLifeTime">Lifetime type, must be of ObjectCustomData type</typeparam>
    public class ObjectLifetimeContainer<TType, TLifeTime> where TLifeTime : ObjectCustomData
    {
        private List<TType> objectList = new List<TType>();
        private Func<TType, TLifeTime> getDetailsFunc;

        private List<TType> deletionList = new List<TType>();
        private List<TLifeTime> idList = new List<TLifeTime>();

        /// <summary>
        /// Object List
        /// </summary>
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

                lifeTime.Step(dt);

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
