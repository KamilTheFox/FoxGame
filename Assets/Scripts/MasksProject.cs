using UnityEngine;
    public static class MasksProject
    {
        public static LayerMask Default => LayerMask.GetMask(new string[] { "Default" });

        public static LayerMask Water => LayerMask.GetMask(new string[] { "Water" });
        public static LayerMask RigidObject => LayerMask.GetMask(new string[] { "Default" , "Terrain", "Entity" });

        public static LayerMask Terrain => LayerMask.GetMask(new string[] { "Terrain", "Default" });

        public static LayerMask RigidEntity => LayerMask.GetMask(new string[] { "Entity", "IntangibleEntity" });

        public static LayerMask Entity => LayerMask.NameToLayer("Entity");

        public static LayerMask IntangibleEntity => LayerMask.NameToLayer("IntangibleEntity");
        public static LayerMask Player => LayerMask.GetMask(new string[] { "Player" });
        public static LayerMask SkinPlayer => LayerMask.NameToLayer("SkinPlayer");
        public static LayerMask EntityPlayer => LayerMask.GetMask(new string[] { "Entity", "IntangibleEntity", "Player" , "SkinPlayer" });
}
