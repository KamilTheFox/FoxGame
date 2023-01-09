using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

    public class RandomFright : RandomFustRun
    {
        private Transform Frightening;
        private List<Vector3> OldPosition = new();
        public RandomFright(Transform frightening)
        {
            Frightening = frightening;
        }
        private float GetDistanseFrigh(Vector3 vector)
        {
            return Vector3.Distance(Frightening.position, vector);
        }
        protected override bool СonditionRandomPoint(Vector3 vector)
        {
            bool flag = false;
            if (OldPosition.Count > 0 && GetDistanseFrigh(vector) > GetDistanseFrigh(OldPosition[OldPosition.Count - 1]))
                flag = true;
            OldPosition.Add(vector);
            return flag;
        }
    
}
