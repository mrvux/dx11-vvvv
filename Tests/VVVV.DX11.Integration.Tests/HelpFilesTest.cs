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
    [TestClass]
    public partial class HelpFilesTests
    {
        const string helpFilesRelativePath = "../../../../girlpower/nodes/plugins";

        [TestMethod]
        public void VVVVDX11NodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(DX11RendererNode));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath);

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

        [TestMethod]
        public void VVVVDX11DirectWriteNodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(TextLayoutNode));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath);

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

        [TestMethod]
        public void VVVVDX11AssimpNodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(AssimpSceneNode));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath);

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

        [TestMethod]
        public void VVVVDX11DShowNodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(VVVV.DX11.Nodes.DShow.VideoInNode));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath, "dshow");

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

        [TestMethod]
        public void VVVVDX11Kinect1NodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(KinectSmoothParamsNode));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath);

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

        [TestMethod]
        public void VVVVDX11QRCodeNodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(VVVV.Nodes.DX11_TextureQRCodeNode));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath);

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

        [TestMethod]
        public void VVVVDX11Text3dNodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(TextMeshNode));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath);

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

        [TestMethod]
        public void VVVVDX11VlcNodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(VVVV.Nodes.VideoPlayer.VLCNodeSPR));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath, "vlc");

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

        [TestMethod]
        public void VVVVDX11BulletNodesHelpPatchesTest()
        {
            Assembly assembly = Assembly.GetAssembly(typeof(VVVV.Nodes.Bullet.BulletCompoundSpreadNode));

            int missingCount = 0;
            StringBuilder sb = new StringBuilder();

            string path = System.IO.Path.GetDirectoryName(assembly.Location);
            path = Path.Combine(path, helpFilesRelativePath);

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
