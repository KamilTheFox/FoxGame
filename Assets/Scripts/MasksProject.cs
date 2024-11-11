using UnityEngine;
    public static class MasksProject
    {
        public static LayerMask All => LayerMask.GetMask("Default", "NotClimbinding", "Entity", "Terrain", "IntangibleEntity", "Player", "SkinPlayer");
        public static LayerMask Default => LayerMask.GetMask("Default", "NotClimbinding");
        public static LayerMask Water => LayerMask.GetMask("Water");
        public static LayerMask RigidObject => LayerMask.GetMask("Default" , "Terrain", "Entity", "NotClimbinding");

        public static LayerMask Climbinding => LayerMask.GetMask("Default", "Terrain");

        public static LayerMask Terrain => LayerMask.GetMask("Terrain", "Default", "NotClimbinding");
    
        public static LayerMask RigidEntity => LayerMask.GetMask(new string[] { "Entity", "IntangibleEntity" });

        public static LayerMask Entity => LayerMask.NameToLayer("Entity");

        public static LayerMask IntangibleEntity => LayerMask.NameToLayer("IntangibleEntity");
        public static LayerMask Player => LayerMask.GetMask(new string[] { "Player" });
        public static LayerMask SkinPlayer => LayerMask.NameToLayer("SkinPlayer");
        public static LayerMask EntityPlayer => LayerMask.GetMask(new string[] { "Entity", "IntangibleEntity", "Player" , "SkinPlayer" });
}
