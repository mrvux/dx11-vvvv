using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VVVV.DX11.Nodes;
using VVVV.PluginInterfaces.V2;
using System.Linq;
using System.IO;
using VVVV.DX11.Nodes.Nodes.Text;
using VVVV.DX11.Nodes.AssetImport;
using MSKinect.Nodes;

namespace VVVV.DX11.Integration.Tests
{
    public partial class HelpFilesTests
    {
        [TestMethod]
        public void VVVVDX11Kinect2NodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(VVVV.DX11.Nodes.MSKinect.KinectRayTextureNode));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath, "kinect2");

            foreach (Type t in assembly.GetExportedTypes())
            {
                if (typeof(IPluginEvaluate).IsAssignableFrom(t) && !t.IsAbstract)
                {
                    var attr = t.GetCustomAttributes<PluginInfoAttribute>().FirstOrDefault();
                    if (attr != null)
                    {
                        string nodeName = attr.Systemname;
                        string helpFile = nodeName + " help.v4p";

                        if (!File.Exists(Path.Combine(path, helpFile)))
                        {
                            sb.AppendLine(nodeName);
                            missingCount++;
                        }
                    }
                }
            }

            string msg = sb.ToString();
            Assert.AreEqual(missingCount, 0, "Missing help files: \r\n" + sb.ToString());
        }
    }
}
