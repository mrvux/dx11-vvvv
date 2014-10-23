using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace K2Tools
{
    /// <summary>
    /// Lambda based implementation of IBodySelector interface
    /// </summary>
    public class FuncBodySelector : IBodySelector
    {
        private readonly Func<IEnumerable<Body>, Body> selectFunc;

        public FuncBodySelector(Func<IEnumerable<Body>, Body> selectFunc)
        {
            if (selectFunc == null)
                throw new ArgumentNullException("selectFunc");
        }

        public Body Select(IEnumerable<Body> bodies)
        {
            return selectFunc(bodies);
        }
    }
}
