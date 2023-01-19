using UnityEngine;
    public static class MasksProject
    {
        public static LayerMask RigidObject => LayerMask.GetMask(new string[] { "Terrain", "Entity", "Default" });

        public static LayerMask RigidEntity => 1 << LayerMask.NameToLayer("Entity");

        public static LayerMask Entity => LayerMask.NameToLayer("Entity");

        public static LayerMask Player => LayerMask.NameToLayer("Player");
    }
