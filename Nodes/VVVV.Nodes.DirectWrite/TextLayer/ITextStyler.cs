using SlimDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VVVV.Nodes.DirectWrite
{
    public interface ITextStyler
    {
        void Apply(TextLayout layout);
    }
}
