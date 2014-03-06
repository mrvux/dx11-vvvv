using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VVVV.DX11
{
    /// <summary>
    /// Interface to provide when we want to receive date on a per device resource
    /// </summary>
    public interface IDX11ResourceDataSink
    {
        void Assign(IDX11ResourceDataProvider original);
    }
}
