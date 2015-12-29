using VVVV.SkeletonInterfaces;
using System.Collections.Generic;
using VVVV.Utils.VMath;
using AssimpNet;

namespace VVVV.DX11.Nodes.AssetImport
{
    public static class MatExt
    {
        public static Matrix4x4 ToMatrix4x4(this SlimDX.Matrix m)
        {
            return new Matrix4x4(m.M11, m.M12, m.M13, m.M14,
                m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34,
                m.M41, m.M42, m.M43, m.M44);
        }
    }

    public class AssimpBoneWrapper : IJoint
    {
        private string FName;
        private int FId;
        private IJoint FParent;
        private List<IJoint> FChildren;
        private List<Vector2D> FConstraints;

        private Matrix4x4 FBaseTransform;
        private Matrix4x4 FAnimationTransform;
        private Matrix4x4 FCachedCombinedTransform;
        private Vector3D FCachedTranslation;
        private Vector3D FCachedRotation;
        private Vector3D FCachedScale;
        private bool FDirty;

        public AssimpBoneWrapper(int id, string name)
        {
            FId = id;
            FName = name;
            FChildren = new List<IJoint>();

            FBaseTransform = VMath.IdentityMatrix;
            FAnimationTransform = VMath.IdentityMatrix;
            FConstraints = new List<Vector2D>();
            FConstraints.Add(new Vector2D(-1.0, 1.0));
            FConstraints.Add(new Vector2D(-1.0, 1.0));
            FConstraints.Add(new Vector2D(-1.0, 1.0));
            SetDirty();
        }

        public AssimpBoneWrapper(AssimpNode node)
            : this(-1, node.Name)
        {
            FBaseTransform = node.LocalTransform.ToMatrix4x4();// bone.GetTransformMatrix(0).ToMatrix4x4();
        }

        public string Name
        {
            set
            {
                FName = value;
            }
            get
            {
                return FName;
            }
        }

        public int Id
        {
            set
            {
                FId = value;
            }
            get
            {
                return FId;
            }
        }

        public Matrix4x4 BaseTransform
        {
            set
            {
                FBaseTransform = value;
                SetDirty();
            }
            get
            {
                return FBaseTransform;
            }
        }

        public Matrix4x4 AnimationTransform
        {
            set
            {
                FAnimationTransform = value;
                SetDirty();
            }
            get
            {
                return FAnimationTransform;
            }
        }

        public IJoint Parent
        {
            get
            {
                return FParent;
            }

            set
            {
                FParent = value;
                SetDirty();
            }
        }

        public List<IJoint> Children
        {
            get
            {
                return FChildren;
            }
        }

        public Vector3D Rotation
        {
            get
            {
                UpdateCachedValues();
                return FCachedRotation;
            }
        }

        public Vector3D Translation
        {
            get
            {
                UpdateCachedValues();
                return FCachedTranslation;
            }
        }

        public Vector3D Scale
        {
            get
            {
                UpdateCachedValues();
                return FCachedScale;
            }
        }

        public List<Vector2D> Constraints
        {
            get
            {
                return FConstraints;
            }
            set
            {
                FConstraints = value;
            }
        }

        public Matrix4x4 CombinedTransform
        {
            get
            {
                UpdateCachedValues();
                return FCachedCombinedTransform;
            }
        }

        public void CalculateCombinedTransforms()
        {
            UpdateCachedValues();
        }

        public void AddChild(IJoint joint)
        {
            joint.Parent = this;
            Children.Add(joint);
        }

        public void ClearAll()
        {
            Children.Clear();
        }

        public IJoint DeepCopy()
        {
            AssimpBoneWrapper copy = new AssimpBoneWrapper(Id, Name);
            copy.BaseTransform = new Matrix4x4(BaseTransform);
            copy.AnimationTransform = new Matrix4x4(AnimationTransform);

            foreach (IJoint child in Children)
                copy.AddChild(child.DeepCopy());

            for (int i = 0; i < 3; i++)
                copy.Constraints[i] = new Vector2D(Constraints[i]);

            return copy;
        }

        public bool IsDirty()
        {
            return FDirty;
        }

        public void SetDirty()
        {
            if (!IsDirty())
            {
                FDirty = true;
                foreach (IJoint joint in Children)
                {
                    ((AssimpBoneWrapper)joint).SetDirty();
                }
            }
        }

        private void UpdateCachedValues()
        {
            if (IsDirty())
            {
                AnimationTransform.Decompose(out FCachedScale, out FCachedRotation, out FCachedTranslation);
                if (Parent != null)
                    FCachedCombinedTransform = AnimationTransform * BaseTransform * Parent.CombinedTransform;
                else
                    FCachedCombinedTransform = AnimationTransform * BaseTransform;
                FDirty = false;
            }
        }
    }
}