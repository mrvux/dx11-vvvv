using FeralTic.DX11.Resources;
using System;
using System.Collections.Generic;
using System.Text;

namespace VVVV.Internals.Bullet.EX9
{
	public class BulletMesh : IDisposable
	{
        public BulletMesh(IDX11Geometry geometry)
		{
            this.Geometry = geometry;
		}

        public IDX11Geometry Geometry
        {
            get;
            private set;
        }


		public void Dispose()
		{
			if (this.Geometry != null)
            {
                this.Geometry.Dispose();
            }
		}
	}
}
