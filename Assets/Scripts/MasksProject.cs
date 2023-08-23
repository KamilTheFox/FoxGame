using UnityEngine;
    public static class MasksProject
    {
        public static LayerMask RigidObject => LayerMask.GetMask(new string[] { "Terrain", "Entity",  "Default" });

        public static LayerMask RigidEntity => LayerMask.GetMask(new string[] { "Entity", "IntangibleEntity" });

        public static LayerMask Entity => LayerMask.NameToLayer("Entity");

        public static LayerMask IntangibleEntity => LayerMask.NameToLayer("IntangibleEntity");
        public static LayerMask Player => LayerMask.NameToLayer("Player");
        public static LayerMask SkinPlayer => LayerMask.NameToLayer("SkinPlayer");
        public static LayerMask EntityPlayer => LayerMask.GetMask(new string[] { "Entity", "IntangibleEntity", "Player" , "SkinPlayer" });
}
