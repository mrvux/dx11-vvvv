using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using VVVV.DX11;

namespace VVVV.Bullet.DataTypes.DebugView
{
    public class BulletDebugView : IDebugDraw
    {
        private List<string> warnings = new List<string>();

        private List<TextObject> texts = new List<TextObject>();

        public DebugDrawModes DebugMode
        {
            get;set;
        }

        public void Begin()
        {
            this.warnings.Clear();
            this.texts = new List<TextObject>();
        }

        public List<TextObject> TextObjects
        {
            get { return this.texts; }
        }

        public void Draw3dText(ref Vector3 location, string textString)
        {
            this.texts.Add(new TextObject()
            {
                Color = new SlimDX.Color4(1, 1, 1, 1),
                Matrix = SlimDX.Matrix.Translation(location.X, location.Y, location.Z),
                Text = textString
            });
        }

        public void DrawAabb(ref Vector3 from, ref Vector3 to, System.Drawing.Color color)
        {

        }

        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, System.Drawing.Color color, bool drawSect)
        {

        }

        public void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle, float maxAngle, System.Drawing.Color color, bool drawSect, float stepDegrees)
        {

        }

        public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, System.Drawing.Color color)
        {

        }

        public void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, ref Matrix trans, System.Drawing.Color color)
        {

        }

        public void DrawCapsule(float radius, float halfHeight, int upAxis, ref Matrix transform, System.Drawing.Color color)
        {
 
        }

        public void DrawCone(float radius, float height, int upAxis, ref Matrix transform, System.Drawing.Color color)
        {

        }

        public void DrawContactPoint(ref Vector3 pointOnB, ref Vector3 normalOnB, float distance, int lifeTime, System.Drawing.Color color)
        {

        }

        public void DrawCylinder(float radius, float halfHeight, int upAxis, ref Matrix transform, System.Drawing.Color color)
        {

        }

        public void DrawLine(ref Vector3 from, ref Vector3 to, System.Drawing.Color color)
        {

        }

        public void DrawLine(ref Vector3 from, ref Vector3 to, System.Drawing.Color fromColor, System.Drawing.Color toColor)
        {

        }

        public void DrawPlane(ref Vector3 planeNormal, float planeConst, ref Matrix transform, System.Drawing.Color color)
        {

        }

        public void DrawSphere(ref Vector3 p, float radius, System.Drawing.Color color)
        {

        }

        public void DrawSphere(float radius, ref Matrix transform, System.Drawing.Color color)
        {

        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, System.Drawing.Color color)
        {

        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, System.Drawing.Color color, float stepDegrees)
        {
 
        }

        public void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh, float minPs, float maxPs, System.Drawing.Color color, float stepDegrees, bool drawSphere)
        {

        }

        public void DrawTransform(ref Matrix transform, float orthoLen)
        {

        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, System.Drawing.Color color, float __unnamed004)
        {

        }

        public void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 __unnamed003, ref Vector3 __unnamed004, ref Vector3 __unnamed005, System.Drawing.Color color, float alpha)
        {

        }

        public void ReportErrorWarning(string warningString)
        {
            this.warnings.Add(warningString);
        }
    }
}
