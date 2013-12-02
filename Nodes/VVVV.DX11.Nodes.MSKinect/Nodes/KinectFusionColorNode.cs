using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V1;
using FeralTic.DX11;
using Microsoft.Kinect.Toolkit.Fusion;
using Microsoft.Kinect;
using VVVV.MSKinect.Lib;
using FeralTic.DX11.Resources;
using System.Threading;
using System.Collections.ObjectModel;
using SlimDX;
using FeralTic.DX11.Geometry;
using FeralTic.DX11.Utils;
using System.Reflection;
using System.Runtime.InteropServices;
using SlimDX.Direct3D11;
using System.Threading.Tasks;

namespace VVVV.DX11.Nodes.Nodes
{
    #region FusionColorVertex
    [StructLayout(LayoutKind.Sequential)]
    public struct FusionColorVertex
    {
        public SlimDX.Vector3 Position;
        public SlimDX.Vector3 Normals;
        public int Color;

        private static InputElement[] layout;

        public static InputElement[] Layout
        {
            get
            {
                if (layout == null)
                {
                    layout = new InputElement[]
                    {
                        new InputElement("POSITION",0,SlimDX.DXGI.Format.R32G32B32_Float,0, 0),
                        new InputElement("NORMAL",0,SlimDX.DXGI.Format.R32G32B32_Float,12,0),
                        new InputElement("COLOR",0,SlimDX.DXGI.Format.R8G8B8A8_UNorm,24,0)
                    };
                }
                return layout;
            }
        }

        public static int VertexSize
        {
            get { return Marshal.SizeOf(typeof(FusionColorVertex)); }
        }
    }
    #endregion

    [PluginInfo(Name="FusionColor",Category="Kinect",Version="Microsoft",Author="vux")]
    public unsafe class KinectFusionColorNode : IPluginEvaluate,IDX11ResourceProvider, IPluginConnections
    {
        [Input("Kinect Runtime")]
        private Pin<KinectRuntime> FInRuntime;

        [Input("Voxel Per Meter", DefaultValue = 256)]
        private ISpread<int> FInVPM;

        [Input("Voxel X",DefaultValue=256)]
        private ISpread<int> FInVX;

        [Input("Voxel Y", DefaultValue = 256)]
        private ISpread<int> FInVY;

        [Input("Voxel Z", DefaultValue = 256)]
        private ISpread<int> FInVZ;

        [Input("View Color", DefaultValue = 1)]
        private ISpread<bool> FinViewColor;

        [Input("Integrate Color Step", DefaultValue = 1)]
        private ISpread<int> FInColorStep;

        [Input("Enabled", DefaultValue = 1)]
        private ISpread<bool> FInEnabled;

        [Input("Reset", IsBang = true)]
        private ISpread<bool> FInReset;

        [Input("Export Geom", IsBang = true)]
        private ISpread<bool> FInExport;

        [Input("Export Voxels", IsBang = true)]
        private ISpread<bool> FInVoxels;

        [Input("Geom Voxel Step", DefaultValue=1)]
        private ISpread<int> FInGeomVoxelStep;

        [Output("Texture", IsSingle = true)]
        protected Pin<DX11Resource<DX11DynamicTexture2D>> FTextureOutput;

        [Output("Point Cloud", IsSingle = true)]
        protected Pin<DX11Resource<IDX11ReadableStructureBuffer>> FPCOut;

        [Output("Geometry Out", IsSingle = true)]
        ISpread<DX11Resource<DX11IndexedGeometry>> FGeomOut;

        [Output("Voxel Buffer Out", IsSingle = true)]
        ISpread<DX11Resource<IDX11ReadableStructureBuffer>> FOutVoxels;

        [Output("WorldCamera")]
        protected ISpread<SlimDX.Matrix> FOutWorldCam;

        [Output("WorldVoxel")]
        protected ISpread<SlimDX.Matrix> FOutWorldVoxel;

        [Output("Success")]
        protected ISpread<bool> FOutSuccess;

       

        private int VoxelsPerMeter = 256;
        private int VoxelResolutionX = 256;
        private int VoxelResolutionY = 256;
        private int VoxelResolutionZ = 256;
        private const ReconstructionProcessor ProcessorType = ReconstructionProcessor.Amp;
        private float minDepthClip = FusionDepthProcessor.DefaultMinimumDepth;
        private float maxDepthClip = FusionDepthProcessor.DefaultMaximumDepth;

        private FusionFloatImageFrame depthFloatBuffer;
        private FusionPointCloudImageFrame pointCloudBuffer;
        private FusionColorImageFrame shadedSurfaceColorFrame;
        private Matrix4 worldToCameraTransform;
        private Matrix4 defaultWorldToVolumeTransform;
        private ColorReconstruction volume;
        private FusionColorImageFrame mappedColorFrame = new FusionColorImageFrame(640, 480);

        private bool FInvalidateConnect = false;
        protected KinectRuntime runtime;

        private int width = 640;
        private int height = 480;
        private bool FInvalidate = true;
        private object m_lock = new object();

        private int[] pic = new int[640 * 480];
        private float[] piccloud = new float[640 * 480 * 6];

        private byte[] colorImagePixels = new byte[640 * 480 * 4];
        private int[] colorPixels = new int[640 * 480];
        private ColorImagePoint[] colorCoordinates = new ColorImagePoint[640 * 480];
        private int[] mappedColorPixels = new int[640 * 480];
        private CoordinateMapper mapper;
        private DepthImagePixel[] depthImagePixels = new DepthImagePixel[640 * 480];

        private int frameindex;

        public void Evaluate(int SpreadMax)
        {
            this.VoxelResolutionX = this.FInVX[0];
            this.VoxelResolutionY = this.FInVY[0];
            this.VoxelResolutionZ = this.FInVZ[0];
            this.VoxelsPerMeter = this.FInVPM[0];

            if (this.FTextureOutput[0] == null) { this.FTextureOutput[0] = new DX11Resource<DX11DynamicTexture2D>(); }
            if (this.FPCOut[0] == null) { this.FPCOut[0] = new DX11Resource<IDX11ReadableStructureBuffer>(); }
            if (this.FGeomOut[0] == null) { this.FGeomOut[0] = new DX11Resource<DX11IndexedGeometry>(); }

            if (this.FOutVoxels[0] == null) { this.FOutVoxels[0] = new DX11Resource<IDX11ReadableStructureBuffer>(); }

            if (this.FInExport[0]) { this.FGeomOut[0].Dispose(); this.FGeomOut[0] = new DX11Resource<DX11IndexedGeometry>(); }

            if (this.FInvalidateConnect)
            {
                this.FInvalidateConnect = false;

                if (this.FInRuntime.PluginIO.IsConnected)
                {
                    this.runtime = this.FInRuntime[0];
                    this.runtime.AllFrameReady += this.runtime_DepthFrameReady;

                    var volParam = new ReconstructionParameters(VoxelsPerMeter, VoxelResolutionX, VoxelResolutionY, VoxelResolutionZ);
                    this.worldToCameraTransform = Matrix4.Identity;



                    this.volume = ColorReconstruction.FusionCreateReconstruction(volParam, ProcessorType, 0, this.worldToCameraTransform);

                    
                    //this.volume.
                    /*FusionPointCloudImageFrame pc;
                    pc.*/

                    this.defaultWorldToVolumeTransform = this.volume.GetCurrentWorldToVolumeTransform();

                    // Depth frames generated from the depth input
                    this.depthFloatBuffer = new FusionFloatImageFrame(width, height);

                    // Point cloud frames generated from the depth float input
                    this.pointCloudBuffer = new FusionPointCloudImageFrame(width, height);

                    // Create images to raycast the Reconstruction Volume
                    this.shadedSurfaceColorFrame = new FusionColorImageFrame(width, height);

                    this.ResetReconstruction();
                }
            }

            if (this.runtime != null)
            {
                bool needreset = this.FInReset[0];

                if (needreset) { this.ResetReconstruction(); }
            }

        }

    
        void runtime_DepthFrameReady(object sender, Microsoft.Kinect.AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (null != colorFrame)
                {
                    // Copy color pixels from the image to a buffer
                    colorFrame.CopyPixelDataTo(this.colorImagePixels);
                }
            }

            using (DepthImageFrame frame = e.OpenDepthImageFrame())
            {
                if (frame != null && !processing && this.FInEnabled[0])
                {
                   

                    // Copy the pixel data from the image to a temporary array
                    frame.CopyDepthImagePixelDataTo(this.depthImagePixels);

                    this.processing = true;
                    Thread thr = new Thread(new ThreadStart(this.Run));
                    thr.Priority = ThreadPriority.BelowNormal;
                    thr.Start();
                }
            }

            /*SlimDX.Direct3D11.DomainShader ds;
            SlimDX.D3DCompiler.ShaderReflection sr;
            sr = new SlimDX.D3DCompiler.ShaderReflection(null);
            sr.GetResourceBindingDescription(0).*/
        }

        private bool processing = false;

        private void Run()
        {
            this.ProcessDepthData(this.depthImagePixels);
        }

        #region GetMat
        private SlimDX.Matrix getmat(Matrix4 m)
        {
            SlimDX.Matrix res = new Matrix();
            res.M11 = m.M11;
            res.M12 = m.M12;
            res.M13 = m.M13;
            res.M14 = m.M14;

            res.M21 = m.M21;
            res.M22 = m.M22;
            res.M23 = m.M23;
            res.M24 = m.M24;

            res.M31 = m.M31;
            res.M32 = m.M32;
            res.M33 = m.M33;
            res.M34 = m.M34;

            res.M41 = m.M41;
            res.M42 = m.M42;
            res.M43 = m.M43;
            res.M44 = m.M44;
            return res;
        }
        #endregion

        private void ProcessDepthData(DepthImagePixel[] depthPixels)
        {
            try
            {
                // Convert the depth image frame to depth float image frame
                FusionDepthProcessor.DepthToDepthFloatFrame(
                    depthPixels,
                    this.width,
                    this.height,
                    this.depthFloatBuffer,
                    FusionDepthProcessor.DefaultMinimumDepth,
                    FusionDepthProcessor.DefaultMaximumDepth,
                    false);

                bool integrateColor = this.frameindex % this.FInColorStep[0] == 0;

                bool trackingSucceeded;
                if (integrateColor)
                {
                    this.MapColorToDepth();

                    trackingSucceeded = this.volume.ProcessFrame(
                        this.depthFloatBuffer,
                        this.mappedColorFrame,
                        FusionDepthProcessor.DefaultAlignIterationCount,
                        FusionDepthProcessor.DefaultIntegrationWeight,
                        FusionDepthProcessor.DefaultColorIntegrationOfAllAngles,
                        this.volume.GetCurrentWorldToCameraTransform());
                }
                else
                {
                    trackingSucceeded = this.volume.ProcessFrame(
                        this.depthFloatBuffer,
                        FusionDepthProcessor.DefaultAlignIterationCount,
                        FusionDepthProcessor.DefaultIntegrationWeight,
                        this.volume.GetCurrentWorldToCameraTransform());
                }


                if (!trackingSucceeded)
                {
                    this.FOutSuccess[0] = false;
                }
                else
                {
                    Matrix4 calculatedCameraPose = this.volume.GetCurrentWorldToCameraTransform();
                    Matrix4 sdfPose = this.volume.GetCurrentWorldToVolumeTransform();

                    this.FOutWorldCam[0] = this.getmat(calculatedCameraPose);
                    this.FOutWorldVoxel[0] = this.getmat(sdfPose);
   

                    // Set the camera pose and reset tracking errors
                    this.worldToCameraTransform = calculatedCameraPose;
                    this.FOutSuccess[0] = true;
                }

                // Calculate the point cloud
                /**/
                //this.volume.AlignDepthFloatToReconstruction

                if (this.FinViewColor[0])
                {
                    this.volume.CalculatePointCloud(this.pointCloudBuffer, this.shadedSurfaceColorFrame, this.worldToCameraTransform);
                }
                else
                {
                    this.volume.CalculatePointCloud(this.pointCloudBuffer, this.worldToCameraTransform);
                    FusionDepthProcessor.ShadePointCloud(
                    this.pointCloudBuffer,
                    this.worldToCameraTransform,
                    this.shadedSurfaceColorFrame,
                    null);
                }

                // Shade point cloud and render
                /**/

                lock (m_lock)
                {
                    this.shadedSurfaceColorFrame.CopyPixelDataTo(this.pic);

                    //this.LockFrameAndExecute((Action<IntPtr>)(src => Marshal.Copy(src, destinationPixelData, 0, this.PixelDataLength)));

                    var v = (Action<IntPtr>) (src => Marshal.Copy(src, this.piccloud, 0, 640*480*6));

                    Type t = this.pointCloudBuffer.GetType();
                    MethodInfo m =t.GetMethod("LockFrameAndExecute",BindingFlags.NonPublic | BindingFlags.Instance);
                    m.Invoke(this.pointCloudBuffer, new object[] { v });
                    //MethodInfo m = 

                    //this.pointCloudBuffer.CopyPixelDataTo(this.piccloud);


                    this.FInvalidate = true;
                }
            }
            catch (Exception ex)
            {
                Console.Write("Test");
            }

            this.processing = false;
            this.frameindex++;
        }

        #region Update
        public void Update(IPluginIO pin, DX11RenderContext context)
        {
            if (!this.FTextureOutput[0].Contains(context))
            {
                this.FTextureOutput[0][context] = new DX11DynamicTexture2D(context, this.width, this.height, SlimDX.DXGI.Format.B8G8R8A8_UNorm);
                this.FPCOut[0][context] = new DX11DynamicStructuredBuffer<float>(context, 640 * 480 * 6);

            }

            if (this.FInvalidate)
            {
                fixed (int* f = &this.pic[0])
                {
                    IntPtr ptr = new IntPtr(f);
                    this.FTextureOutput[0][context].WriteData(ptr, this.width * this.height * 4);
                }

                /*fixed (float* f = &this.piccloud[0])
                {*
                    IntPtr ptr = new IntPtr(f);*/

                    DX11DynamicStructuredBuffer<float> db = (DX11DynamicStructuredBuffer<float>)this.FPCOut[0][context];
                    db.WriteData(this.piccloud);
                //}

                this.FInvalidate = false;
            }

            if (this.FInVoxels[0])
            {
                if (this.FOutVoxels[0].Contains(context))
                {
                    this.FOutVoxels[0].Dispose(context);
                }

                short[] data = new short[this.VoxelResolutionX * this.VoxelResolutionY * this.VoxelResolutionZ];

                this.volume.ExportVolumeBlock(0, 0, 0, this.VoxelResolutionX, this.VoxelResolutionY, this.VoxelResolutionZ, 1, data);

                DX11DynamicStructuredBuffer<int> b = new DX11DynamicStructuredBuffer<int>(context, this.VoxelResolutionX * this.VoxelResolutionY * this.VoxelResolutionZ);

                int[] idata = new int[this.VoxelResolutionX * this.VoxelResolutionY * this.VoxelResolutionZ];

                for (int i = 0; i < this.VoxelResolutionX * this.VoxelResolutionY * this.VoxelResolutionZ; i++)
                {
                    idata[i] = data[i];
                }

                b.WriteData(idata);

                this.FOutVoxels[0][context] = b;
            }

            if (this.FInExport[0])
            {
                if (this.FGeomOut[0].Contains(context))
                {
                    this.FGeomOut[0].Dispose(context);
                }

                if (this.volume != null)
                {
                    ColorMesh m = this.volume.CalculateMesh(this.FInGeomVoxelStep[0]);
                    DX11IndexedGeometry geom = new DX11IndexedGeometry(context);

                    ReadOnlyCollection<int> inds = m.GetTriangleIndexes();

                    DataStream ds = new DataStream(inds.Count*4,true,true);
                    ds.WriteRange<int>(inds.ToArray());
                    ds.Position = 0;

                    DX11IndexBuffer ibo = new DX11IndexBuffer(context, ds, false, true);

                    ReadOnlyCollection<Microsoft.Kinect.Toolkit.Fusion.Vector3> pos = m.GetVertices();
                    ReadOnlyCollection<Microsoft.Kinect.Toolkit.Fusion.Vector3> norm = m.GetNormals();
                    ReadOnlyCollection<int> color = m.GetColors();

                    DataStream dsv = new DataStream(FusionColorVertex.VertexSize * pos.Count, true, true);

                    SlimDX.Vector3 bmin = new SlimDX.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                    SlimDX.Vector3 bmax = new SlimDX.Vector3(float.MinValue, float.MinValue, float.MinValue);

                    for (int i = 0; i < pos.Count; i++)
                    {
                        Microsoft.Kinect.Toolkit.Fusion.Vector3 p = pos[i];
                        Microsoft.Kinect.Toolkit.Fusion.Vector3 n = norm[i];

                        dsv.Write<Microsoft.Kinect.Toolkit.Fusion.Vector3>(p);
                        dsv.Write<Microsoft.Kinect.Toolkit.Fusion.Vector3>(n);
                        dsv.Write<int>(color[i]);

                        if (p.X < bmin.X) { bmin.X = p.X; }
                        if (p.Y < bmin.Y) { bmin.Y = p.Y; }
                        if (p.Z < bmin.Z) { bmin.Z = p.Z; }

                        if (p.X > bmax.X) { bmax.X = p.X; }
                        if (p.Y > bmax.Y) { bmax.Y = p.Y; }
                        if (p.Z > bmax.Z) { bmax.Z = p.Z; }
                    }

                    geom.IndexBuffer = ibo;
                    geom.HasBoundingBox = true;
                    geom.InputLayout = FusionColorVertex.Layout;
                    geom.Topology = SlimDX.Direct3D11.PrimitiveTopology.TriangleList;
                    geom.VertexSize = FusionColorVertex.VertexSize;
                    geom.VertexBuffer = BufferHelper.CreateVertexBuffer(context, dsv, false, true);
                    geom.VerticesCount = pos.Count;
                    geom.BoundingBox = new BoundingBox(bmin, bmax);


                    this.FGeomOut[0][context] = geom;

                    m.Dispose();
                }
            }
        }
        #endregion

        public void Destroy(IPluginIO pin, DX11RenderContext context, bool force)
        {
            this.FTextureOutput[0].Dispose(context);
        }

        public void ConnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        public void DisconnectPin(IPluginIO pin)
        {
            if (pin == this.FInRuntime.PluginIO)
            {
                this.FInvalidateConnect = true;
            }
        }

        /// <summary>
        /// Reset the reconstruction to initial value
        /// </summary>
        private void ResetReconstruction()
        {
            // Set the world-view transform to identity, so the world origin is the initial camera location.
            this.worldToCameraTransform = Matrix4.Identity;

            if (null != this.volume)
            {
                    this.volume.ResetReconstruction(this.worldToCameraTransform);
            }
        }

        #region Map Color to depth
        private unsafe void MapColorToDepth()
        {
            if (null == this.mapper)
            {
                // Create a coordinate mapper
                this.mapper = new CoordinateMapper(this.runtime.Runtime);
            }

            this.mapper.MapDepthFrameToColorFrame(DepthImageFormat.Resolution640x480Fps30, this.depthImagePixels, ColorImageFormat.RgbResolution640x480Fps30, this.colorCoordinates);

            int depthWidth = 640;
            int depthHeight = 480;
            int colorWidth = 640;
            int colorHeight = 480;
            // Here we make use of unsafe code to just copy the whole pixel as an int for performance reasons, as we do
            // not need access to the individual rgba components.
            fixed (byte* ptrColorPixels = this.colorImagePixels)
            {
                int* rawColorPixels = (int*)ptrColorPixels;


                // Horizontal flip the color image as the standard depth image is flipped internally in Kinect Fusion
                // to give a viewpoint as though from behind the Kinect looking forward by default.
                Parallel.For(
                    0,
                    depthHeight,
                    y =>
                    {
                        int destIndex = y * depthWidth;
                        int flippedDestIndex = destIndex + (depthWidth - 1); // horizontally mirrored

                        for (int x = 0; x < depthWidth; ++x, ++destIndex, --flippedDestIndex)
                        {
                            // calculate index into depth array
                            int colorInDepthX = colorCoordinates[destIndex].X;
                            int colorInDepthY = colorCoordinates[destIndex].Y;

                            // make sure the depth pixel maps to a valid point in color space
                            if (colorInDepthX >= 0 && colorInDepthX < colorWidth && colorInDepthY >= 0
                                && colorInDepthY < colorHeight && depthImagePixels[destIndex].Depth != 0)
                            {
                                // Calculate index into color array- this will perform a horizontal flip as well
                                int sourceColorIndex = colorInDepthX + (colorInDepthY * colorWidth);

                                // Copy color pixel
                                this.mappedColorPixels[flippedDestIndex] = rawColorPixels[sourceColorIndex];
                            }
                            else
                            {
                                this.mappedColorPixels[flippedDestIndex] = 0;
                            }
                        }
                    });
            }

            this.mappedColorFrame.CopyPixelDataFrom(this.mappedColorPixels);
        }
        #endregion
    }
}
