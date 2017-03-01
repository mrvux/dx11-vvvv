using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using FeralTic.DX11;
using VVVV.Bullet.DataTypes;
using VVVV.Bullet.DataTypes.DebugView;
using VVVV.DataTypes.Bullet;
using VVVV.DX11;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Bullet.Nodes.World
{
    [PluginInfo(Name = "DebugDraw", Category = "DX11.Layer", Version = "Bullet", Author = "vux", Help = "Renders a full bullet debug view")]
    public class BulletWorldDebugLayerNode : IPluginEvaluate, IDX11ResourceHost, IPartImportsSatisfiedNotification, System.IDisposable
    {
        [Input("World", IsSingle = true)]
        protected Pin<BulletRigidSoftWorld> worldPin;

        [Input("Shape Render State", IsSingle = true)]
        protected ISpread<DX11RenderState> shapeRenderState;

        [Input("AABB Render State", IsSingle = true)]
        protected ISpread<DX11RenderState> aabbRenderState;

        [Input("Mode", DefaultEnumEntry = "MaxDebugDrawMode")]
        protected ISpread<DebugDrawModes> drawMode;

        [Input("Enabled", IsSingle = true)]
        protected ISpread<bool> enabled;

        [Output("Layer Out")]
        protected ISpread<DX11Resource<DX11Layer>> layer;

        [Output("Debug Text")]
        protected ISpread<TextObject> textObjects;

        private BulletRigidSoftWorld currentWorld;

        //Used to collect texts and warnings
        private BulletDebugView debugView = new BulletDebugView();

        //Immediate view renderer;
        private DX11ContextElement<BulletImmediateWorldDebugRenderer> immediateDebugView = new DX11ContextElement<BulletImmediateWorldDebugRenderer>();

        public void OnImportsSatisfied()
        {
            this.layer[0] = new DX11Resource<DX11Layer>();

        }

        public void Evaluate(int SpreadMax)
        {
            if (this.currentWorld != this.worldPin[0])
            {
                if (this.currentWorld != null)
                {
                    this.currentWorld.World.DebugDrawer = null;
                }
                this.currentWorld = this.worldPin[0];
            }

            if (this.currentWorld != null)
            {
                if (this.enabled[0])
                {
                    this.currentWorld.World.DebugDrawer = this.debugView;

                    DebugDrawModes mode = DebugDrawModes.None;
                    for (int i = 0; i < this.drawMode.SliceCount; i++)
                    {
                        mode |= this.drawMode[i];
                    }
                    this.debugView.DebugMode = mode;

                    this.debugView.Begin();
                    this.currentWorld.World.DebugDrawWorld();

                    this.textObjects.SliceCount = this.debugView.TextObjects.Count;
                    for (int i = 0; i < this.textObjects.SliceCount; i++)
                    {
                        this.textObjects[i] = this.debugView.TextObjects[i];
                    }
                }
                else
                {
                    this.currentWorld.World.DebugDrawer = null;

                    this.textObjects.SliceCount = 0;
                }
            }


        }

        public void Update(DX11RenderContext context)
        {
            if (!this.layer[0].Contains(context))
            {
                this.layer[0][context] = new DX11Layer();
                this.layer[0][context].Render = this.Render;
            }
            if (!this.immediateDebugView.Contains(context))
            {
                this.immediateDebugView[context] = new DataTypes.BulletImmediateWorldDebugRenderer(context);
            }
        }

        private void Render(DX11RenderContext context, DX11RenderSettings settings)
        {
            if (this.enabled[0] && this.currentWorld != null)
            {
                var drawer = this.immediateDebugView[context];
                drawer.DebugMode = this.debugView.DebugMode;

                this.currentWorld.World.DebugDrawer = drawer;

                drawer.Begin(settings.ViewProjection, this.shapeRenderState[0], this.aabbRenderState[0]);

                this.currentWorld.World.DebugDrawWorld();

                drawer.End();
            }
        }

        public void Destroy(DX11RenderContext context, bool force)
        {
            if (force)
            {
                this.layer.SafeDisposeAll(context);
                this.immediateDebugView.Dispose(context);
            }
        }

        public void Dispose()
        {
            this.layer.SafeDisposeAll();
            this.immediateDebugView.Dispose();
        }
    }
}
