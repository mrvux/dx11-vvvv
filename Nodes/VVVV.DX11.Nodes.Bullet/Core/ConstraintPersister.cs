using BulletSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Core
{
    /// <summary>
    /// Rigid body listener, process deleted items 
    /// <typeparamref name="TConstraint">Constraint type</typeparamref>
    /// </summary>
    public class ConstraintPersister<TConstraint> where TConstraint : TypedConstraint
    {
        private ConstraintListener<TConstraint> listener;

        private ISpread<TConstraint> persistedOutput;

        public ConstraintPersister(ConstraintListener<TConstraint> listener, ISpread<TConstraint> persistedOutput)
        {
            this.listener = listener;
            this.persistedOutput = persistedOutput;
        }

        public void Append(TConstraint constraint)
        {
            this.listener.Append(constraint);
        }

        public void UpdateWorld(IConstraintContainer container)
        {
            this.listener.UpdateWorld(container);
        }

        public void Flush()
        {
            List<TConstraint> constraints = this.listener.Constraints;
            this.persistedOutput.SliceCount = constraints.Count;

            for (int i = 0; i < constraints.Count; i++)
            {
                this.persistedOutput[i] = constraints[i];
            }
        }
    }
}
