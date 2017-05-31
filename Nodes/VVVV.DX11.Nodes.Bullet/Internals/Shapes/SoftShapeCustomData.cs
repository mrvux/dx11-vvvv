using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.DataTypes.Bullet;

namespace VVVV.Internals.Bullet
{
	public class SoftShapeCustomData
	{
		private int id;
		private AbstractSoftShapeDefinition descriptor;

        public SoftShapeCustomData(int id, AbstractSoftShapeDefinition descriptor)
        {
            this.id = id;
            this.descriptor = descriptor;
        }

        public int Id
		{
			get { return id; }
		}

		//Original shape definition (To build mesh on request)
		public AbstractSoftShapeDefinition Descriptor
		{
			get { return this.descriptor; }
		}
	}
}
