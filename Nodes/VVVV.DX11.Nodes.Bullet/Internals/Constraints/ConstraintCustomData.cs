using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Bullet.Core;

namespace VVVV.Internals.Bullet
{
	public class ConstraintCustomData : ObjectCustomData
	{
        public ConstraintCustomData(int id) : base(id) { }

		public bool IsSingle { get; set; }
	}
}
