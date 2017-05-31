using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Base object custom data, contains lifetime information and others
    /// </summary>
    public class ObjectCustomData
    {
        public readonly int Id;

        /// <summary>
        /// Constructor (not marked for deletion, created to true and 0 lifetime)
        /// </summary>
        /// <param name="id">Object id</param>
        public ObjectCustomData(int id)
        {
            this.Id = id;
            this.MarkedForDeletion = false;
            this.LifeTime = 0;
        }

        /// <summary>
        /// Steps timer, also sets created to false
        /// </summary>
        /// <param name="dt">Delta time</param>
        public void Step(double dt)
        {
            if (this.Created)
            {
                this.Created = false;
            }
            else
            {
                this.LifeTime += dt;
            }
        }

        /// <summary>
        /// Custom string
        /// </summary>
        public string Custom { get; set; }

        /// <summary>
        /// Tells if body is marked for deletion
        /// </summary>
        public bool MarkedForDeletion { get; set; }

        /// <summary>
        /// Tells if body has just been created
        /// </summary>
        public bool Created { get; private set; }

        /// <summary>
        /// Body lifetime
        /// </summary>
        public double LifeTime { get; private set; }
    }
}
