using System.Collections.Generic;

namespace Dash.Types
{
    public static class ModelBone
    {
        private static readonly Dictionary<Type, string> boneName = new Dictionary<Type, string>();

        static ModelBone()
        {
            boneName[Type.Bip001] = "Bip001";
            boneName[Type.Bip001_Spine1] = "Bip001 Spine";
            boneName[Type.Bip001_R_Hand] = "Bip001 R Hand";
            boneName[Type.Bip001_L_Hand] = "Bip001 L Hand";
            boneName[Type.Bip001_R_Toe0] = "Bip001 R Toe0";
            boneName[Type.Bip001_L_Toe0] = "Bip001 L Toe0";
            boneName[Type.Bip001_Head] = "Bip001 Head";
            boneName[Type.Bone_Weapon] = "Bone_Weapon";
            boneName[Type.Bone_R_Target01] = "Bone_R_Target01";
            boneName[Type.Bone_L_Target01] = "Bone_L_Target01";
            boneName[Type.Target_Dummy01] = "Target_Dummy01";
            boneName[Type.Target_Dummy02] = "Target_Dummy02";
            boneName[Type.Target_Dummy03] = "Target_Dummy03";
        }

        public enum Type
        {
            Undefined = 0,
            Bip001,
            Bip001_Spine1,
            Bip001_R_Hand,
            Bip001_L_Hand,
            Bip001_R_Toe0,
            Bip001_L_Toe0,
            Bip001_Head,
            Bone_Weapon,
            Bone_R_Target01,
            Bone_L_Target01,
            Target_Dummy01,
            Target_Dummy02,
            Target_Dummy03,
        }

        public static string GetBoneName(Type type)
        {
            if (boneName.ContainsKey(type) == true)
            {
                return boneName[type];
            }
            else
            {
                Common.Log.Logger.Instance.Error($"Bone name doesn't exist {type}");
                return string.Empty;
            }
        }
    }
}
