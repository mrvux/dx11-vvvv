using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using FeralTic.DX11;
using FeralTic.DX11.Geometry;
using FeralTic.DX11.Resources;
using FeralTic.DX11.StockEffects;
using FeralTic.DX11.Utils;
using SlimDX.Direct3D11;
using BulletSharp.SoftBody;

namespace VVVV.Bullet.DataTypes
{
    /// <summary>
    /// Per device data for bullet elements 
    /// </summary>
    public unsafe class BulletImmediateWorldDebugRenderer : IDebugDraw, System.IDisposable
    {
        private readonly DX11RenderContext context;

        private SolidColorTransformed solidColorShader;

        //Bulk of geometries
        private DX11IndexedGeometry box;
        private DX11IndexedGeometry sphere;
        private DX11IndexedGeometry cylinder;
        private InputLayout boxSphereColorLayout;

        //Lines and triangle strip
        private DX11VertexGeometry dynamicLine;
        private DX11VertexGeometry dynamicLineTriangle;
        private DX11VertexGeometry arcLine;
        private InputLayout lineLayout;

        private DX11RenderState defaultRenderState = new DX11RenderState();

        private DX11RenderState shapesRenderState;
        private DX11RenderState aabbRenderState;

        public BulletImmediateWorldDebugRenderer(DX11RenderContext context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            this.context = context;

            this.solidColorShader = new SolidColorTransformed(context);
            this.box = context.Primitives.Box(new FeralTic.DX11.Geometry.Box());
            this.sphere = context.Primitives.Sphere(new FeralTic.DX11.Geometry.Sphere()
            {
                Radius = 1.0f
            });
            this.cylinder = context.Primitives.Cylinder(new Cylinder()
            {
                Length = 2.0f,
                Radius1 = 1.0f,
                Radius2 = 1.0f,
            });

            this.box.ValidateLayout(this.solidColorShader.EffectPass, out this.boxSphereColorLayout);

            this.dynamicLine = new DX11VertexGeometry(context);
            this.dynamicLine.InputLayout = Pos3Vertex.Layout;
            this.dynamicLine.Topology = PrimitiveTopology.LineList;
            this.dynamicLine.VertexBuffer = BufferHelper.CreateDynamicVertexBuffer(context, 12 * 2);
            this.dynamicLine.VertexSize = Pos3Vertex.VertexSize;
            this.dynamicLine.VerticesCount = 2;

            this.dynamicLineTriangle = new DX11VertexGeometry(context);
            this.dynamicLineTriangle.InputLayout = Pos3Vertex.Layout;
            this.dynamicLineTriangle.Topology = PrimitiveTopology.LineStrip;
            this.dynamicLineTriangle.VertexBuffer = BufferHelper.CreateDynamicVertexBuffer(context, 12 * 3);
            this.dynamicLineTriangle.VertexSize = Pos3Vertex.VertexSize;
            this.dynamicLineTriangle.VerticesCount = 3;


            this.arcLine = new DX11VertexGeometry(context);
            this.arcLine.InputLayout = Pos3Vertex.Layout;
            this.arcLine.Topology = PrimitiveTopology.LineStrip;
            this.arcLine.VertexBuffer = BufferHelper.CreateDynamicVertexBuffer(context, 12 * 2048);
            this.arcLine.VertexSize = Pos3Vertex.VertexSize;
            this.arcLine.VerticesCount = 2048;

            this.dynamicLine.ValidateLayout(this.solidColorShader.EffectPass, out this.lineLayout);
        }

        public DebugDrawModes DebugMode
        {
            get;set;
        }

        public void Begin(SlimDX.Matrix vp, DX11RenderState shapes, DX11RenderState aabb)
        {
            this.shapesRenderState = shapes != null ? shapes : this.defaultRenderState;
            this.aabbRenderState = aabb != null ? aabb : this.shapesRenderState;

            this.solidColorShader.ApplyCamera(vp);
            context.RenderStateStack.Push(shapesRenderState);
            context.CleanShaderStages();
        }

        public void End()
        {
            context.RenderStateStack.Pop();
            context.CleanShaderStages();
        }


        public void Draw3dText(ref Vector3 location, string textString)
        {

        }

        public void DrawAabb(ref Vector3 from, ref Vector3 to, Color color)
        {
            context.RenderStateStack.Push(this.aabbRenderState);
            Vector3 scaling = to - from;
            Vector3 center = (to + from) * 0.5f;

            Matrix m = Matrix.Scaling(scaling) * Matrix.Translation(center);
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(new SlimDX.Color4(color.ToArgb()));

            this.solidColorShader.ApplyPass();
            this.box.Bind(this.boxSphereColorLayout);
            this.box.Draw();
            context.RenderStateStack.Pop();
        }

        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, Color color, bool drawSect)
        {
            DrawArc(ref center, ref normal, ref axis, radiusA, radiusB, minAngle, maxAngle, color, drawSect, 10.0f);
        }

        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, Color color, bool drawSect, float stepDegrees)
        {
            Vector3 vx = axis;
            Vector3 vy = Vector3.Cross(normal, axis);
            float step = stepDegrees * ((float)Math.PI / 180.0f);
            int nSteps = (int)((maxAngle - minAngle) / step);
            if (nSteps == 0)
                nSteps = 1;

            Vector3 next = center + radiusA * vx * (float)Math.Cos(minAngle) + radiusB * vy * (float)Math.Sin(minAngle);

            if (drawSect)
                DrawLine(ref center, ref next, color);

            var ds = context.CurrentDeviceContext.MapSubresource(this.dynamicLine.VertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None).Data;
            ds.Position = 0;
            ds.Write(next);
            for (int i = 1; i <= nSteps; i++)
            {
                float angle = minAngle + (maxAngle - minAngle) * (float)i / (float)nSteps;
                next = center + radiusA * vx * (float)Math.Cos(angle) + radiusB * vy * (float)Math.Sin(angle);
                ds.Write(next);
            }
            context.CurrentDeviceContext.UnmapSubresource(this.dynamicLine.VertexBuffer, 0);


            Matrix m = Matrix.Identity;
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(new SlimDX.Color4(color.ToArgb()));

            this.solidColorShader.ApplyPass();
            this.arcLine.Bind(this.lineLayout);
            this.context.CurrentDeviceContext.Draw(nSteps+1, 0);

            if (drawSect)
                DrawLine(ref center, ref next, color);
        }

        public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, Color color)
        {
            Vector3 scaling = bbMax - bbMin;
            Vector3 center = (bbMax + bbMin) * 0.5f;

            Matrix m = Matrix.Scaling(scaling) * Matrix.Translation(center);
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(new SlimDX.Color4(color.ToArgb()));

            this.solidColorShader.ApplyPass();
            this.box.Bind(this.boxSphereColorLayout);
            this.box.Draw();
        }

        public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, ref Matrix trans, Color color)
        {
            Vector3 scaling = bbMax - bbMin;
            Vector3 center = (bbMax + bbMin) * 0.5f;

            var c = new SlimDX.Color4(color.ToArgb());

            Matrix m = Matrix.Scaling(scaling) * Matrix.Translation(center) * trans;
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(c);

            this.solidColorShader.ApplyPass();
            this.box.Bind(this.boxSphereColorLayout);
            this.box.Draw();
        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, ref Matrix transform, Color color)
        {
            //Capsule is two spheres and a cylinder , can make it more efficient but for early debug it's ok

            /*Matrix m = Matrix.Scaling(radius, halfHeight, radius) * transform;
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(new SlimDX.Color4(color.ToArgb()));

            this.solidColorShader.ApplyPass();
            this.cylinder.Bind(this.boxSphereColorLayout);
            this.cylinder.Draw();*/
        }

        public void DrawCone(float radius, float height, int upAxis, ref Matrix transform, Color color)
        {

        }

        public void DrawContactPoint(ref Vector3 pointOnB, ref Vector3 normalOnB, float distance, int lifeTime, Color color)
        {
            Vector3 to = pointOnB + normalOnB;
            DrawLine(ref pointOnB, ref to, color);
        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, ref Matrix transform, Color color)
        {
            //Up axis 2 = y
            //if (upAxis == 2)
            //{
                //xz on radius, y on hh
                Matrix m = Matrix.Scaling(radius, halfHeight, radius) * transform;
                this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
                this.solidColorShader.ApplyColor(new SlimDX.Color4(color.ToArgb()));

                this.solidColorShader.ApplyPass();
                this.cylinder.Bind(this.boxSphereColorLayout);
                this.cylinder.Draw();
            //}
        }

        public void DrawLine(ref Vector3 from, ref Vector3 to, Color color)
        {
            var ds = context.CurrentDeviceContext.MapSubresource(this.dynamicLine.VertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None).Data;
            ds.Position = 0;
            ds.Write(from);
            ds.Write(to);
            context.CurrentDeviceContext.UnmapSubresource(this.dynamicLine.VertexBuffer, 0);

            Matrix m = Matrix.Identity;
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(new SlimDX.Color4(color.ToArgb()));

            this.solidColorShader.ApplyPass();
            this.dynamicLine.Bind(this.lineLayout);
            this.dynamicLine.Draw();
        }

        public void DrawLine(ref Vector3 from, ref Vector3 to, Color fromColor, Color toColor)
        {
            var ds = context.CurrentDeviceContext.MapSubresource(this.dynamicLine.VertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None).Data;
            ds.Position = 0;
            ds.Write(from);
            ds.Write(to);
            context.CurrentDeviceContext.UnmapSubresource(this.dynamicLine.VertexBuffer, 0);

            Matrix m = Matrix.Identity;
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(new SlimDX.Color4(fromColor.ToArgb()));

            this.solidColorShader.ApplyPass();
            this.dynamicLine.Bind(this.lineLayout);
            this.dynamicLine.Draw();
        }

        public void DrawPlane(ref Vector3 planeNormal, float planeConst, ref Matrix transform, Color color)
        {

        }

        public void DrawSphere(ref Vector3 p, float radius, Color color)
        {
            Matrix m = Matrix.Scaling(radius, radius, radius) * Matrix.Translation(p);
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(new SlimDX.Color4(color.ToArgb()));

            this.solidColorShader.ApplyPass();
            this.sphere.Bind(this.boxSphereColorLayout);
            this.sphere.Draw();
        }

        public void DrawSphere(float radius, ref Matrix transform, Color color)
        {
            Matrix m = Matrix.Scaling(radius, radius, radius) * transform;
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(new SlimDX.Color4(color.ToArgb()));

            this.solidColorShader.ApplyPass();
            this.sphere.Bind(this.boxSphereColorLayout);
            this.sphere.Draw();
        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Color color)
        {

        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Color color, float stepDegrees)
        {

        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, Color color, float stepDegrees, bool drawSphere)
        {

        }

        public void DrawTransform(ref Matrix transform, float orthoLen)
        {

        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, Color color, float __unnamed004)
        {
            var ds = context.CurrentDeviceContext.MapSubresource(this.dynamicLineTriangle.VertexBuffer, 0, MapMode.WriteDiscard, MapFlags.None).Data;
            ds.Position = 0;
            ds.Write(v0);
            ds.Write(v1);
            ds.Write(v2);
            context.CurrentDeviceContext.UnmapSubresource(this.dynamicLine.VertexBuffer, 0);

            Matrix m = Matrix.Identity;
            this.solidColorShader.ApplyWorld(*(SlimDX.Matrix*)&m);
            this.solidColorShader.ApplyColor(new SlimDX.Color4(color.ToArgb()));

            this.solidColorShader.ApplyPass();
            this.dynamicLineTriangle.Bind(this.lineLayout);
            this.dynamicLineTriangle.Draw();
        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 __unnamed003, ref Vector3 __unnamed004, ref Vector3 __unnamed005, Color color, float alpha)
        {
            DrawTriangle(ref v0, ref v1, ref v2, color, alpha);
        }

        public void ReportErrorWarning(string warningString)
        {

        }

        public void Dispose()
        {
            if (this.solidColorShader != null)
            {
                this.solidColorShader.Dispose();
                this.solidColorShader = null;
            }
            if (this.dynamicLine != null)
            {
                this.dynamicLine.Dispose();
                this.dynamicLine = null;
            }
            if (this.dynamicLineTriangle != null)
            {
                this.dynamicLineTriangle.Dispose();
                this.dynamicLineTriangle = null;
            }
            if (this.boxSphereColorLayout != null)
            {
                this.boxSphereColorLayout.Dispose();
                this.boxSphereColorLayout = null;
            }
            if (this.box != null)
            {
                this.box.Dispose();
                this.box = null;
            }
            if (this.sphere != null)
            {
                this.sphere.Dispose();
                this.sphere = null;
            }
            if (this.cylinder != null)
            {
                this.cylinder.Dispose();
                this.cylinder = null;
            }
        }
    }
}
