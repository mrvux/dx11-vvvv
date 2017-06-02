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
    /// <typeparamref name="TConstraint">Constraint type</typeparamref>
    /// </summary>
    public class ConstraintListener<TConstraint> where TConstraint : TypedConstraint
    {
        private IConstraintContainer currentWorld;
        private List<TConstraint> constraintList = new List<TConstraint>();

        /// <summary>
        /// List of active bodies
        /// </summary>
        public List<TConstraint> Constraints
        {
            get { return this.constraintList; }
        }

        public void UpdateWorld(IConstraintContainer inputWorld)
        {
            if (currentWorld != inputWorld)
            {
                if (currentWorld != null)
                {
                    currentWorld.WorldHasReset -= OnWorldReset;
                    currentWorld.ConstraintDeleted -= OnConstraintDeleted;
                }

                this.constraintList.Clear();
                this.currentWorld = inputWorld;
            }

            if (currentWorld != null)
            {
                currentWorld.WorldHasReset += OnWorldReset;
                currentWorld.ConstraintDeleted += OnConstraintDeleted;
            }
        }

        public void Append(TConstraint constraint)
        {
            this.constraintList.Add(constraint);
        }

        private void OnConstraintDeleted(TypedConstraint constraint, int id)
        {
            if (constraint is TConstraint)
            {
                TConstraint tc = (TConstraint)constraint;
                this.constraintList.Remove(tc);
            }
        }

        private void OnWorldReset()
        {
            this.constraintList.Clear();
        }
    }
}
