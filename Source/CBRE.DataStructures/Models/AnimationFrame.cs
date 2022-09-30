using CBRE.DataStructures.Geometric;
using System.Collections.Generic;
using System.Linq;

namespace CBRE.DataStructures.Models
{
    public class AnimationFrame
    {
        public List<BoneAnimationFrame> Bones { get; private set; }

        public AnimationFrame()
        {
            Bones = new List<BoneAnimationFrame>();
        }

        public List<MatrixF> GetBoneTransforms(bool transformBones, bool applyDefaults)
        {
            return Bones.Select(bone => GetAnimationTransform(bone.Bone, transformBones, applyDefaults)).ToList();
        }

        public MatrixF GetAnimationTransform(Bone b, bool transformBones, bool applyDefaults)
        {
            MatrixF m = transformBones ? MatrixF.Identity : GetDefaultBoneTransform(b).Inverse();
            while (b != null)
            {
                QuaternionF ang = Bones[b.BoneIndex].Angles;
                CoordinateF pos = Bones[b.BoneIndex].Position;
                if (applyDefaults)
                {
                    ang *= QuaternionF.EulerAngles(b.DefaultAngles);
                    pos += b.DefaultPosition;
                }
                m *= ang.GetMatrix().Translate(pos);
                //var test = Bones[b.BoneIndex].Angles * QuaternionF.EulerAngles(b.DefaultAngles);
                //m *= test.GetMatrix().Translate(Bones[b.BoneIndex].Position + b.DefaultPosition);
                //m *= Bones[b.BoneIndex].Angles.GetMatrix().Translate(Bones[b.BoneIndex].Position);
                b = b.Parent;
            }
            return m;
        }

        private static MatrixF GetDefaultBoneTransform(Bone b)
        {
            MatrixF m = MatrixF.Identity;
            while (b != null)
            {
                m *= QuaternionF.EulerAngles(b.DefaultAngles).GetMatrix().Translate(b.DefaultPosition);
                b = b.Parent;
            }
            return m;
        }
    }
}