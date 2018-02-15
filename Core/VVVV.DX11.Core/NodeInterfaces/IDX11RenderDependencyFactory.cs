using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.DX11
{
    /// <summary>
    /// Allows to create a manual dependency in the dx11 graph.
    /// Usually update graphi is handled by node having resource pins, but there might be cases where we can't apply those and want a manual perform
    /// </summary>
    public interface IDX11RenderDependencyFactory
    {
        /// <summary>
        /// Creates a dependency in the dx11 graph
        /// </summary>
        /// <param name="inputPin">Input pin</param>
        /// <param name="outputPin">Output pin</param>
        void CreateDependency(IPin inputPin, IPin outputPin);

        /// <summary>
        /// Deletes dependency from the dx11 graph
        /// </summary>
        /// <param name="inputPin">Input pin</param>
        void DeleteDependency(IPin inputPin);
    }
}
