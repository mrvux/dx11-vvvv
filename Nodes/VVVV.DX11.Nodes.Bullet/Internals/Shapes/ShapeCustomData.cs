using System;
using System.Collections.Generic;
using System.Text;
using VVVV.Bullet.Core;
using VVVV.DataTypes.Bullet;

namespace VVVV.Internals.Bullet
{
	public class ShapeCustomData
	{
		private int id;
		private RigidShapeDefinitionBase def;

		public int Id
		{
			get { return id; }
			set { id = value; }
		}

		public string CustomString { get; set; }

		//Original shape definition (To build mesh on request)
		public RigidShapeDefinitionBase ShapeDef
		{
			get { return this.def; }
			set { this.def = value; }
		}
	}
}
