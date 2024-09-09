using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

namespace AIInput
{
    public sealed class AILinkPool : MonoBehaviour
    {
        private static Dictionary<bool, Stack<NavMeshLink>> meshLinks = new()
        {
            [true] = new Stack<NavMeshLink>(),
            [false] = new Stack<NavMeshLink>()
        };

        private static AILinkPool instance;
        public static AILinkPool Instance
        {
            get
            {
                if(instance == null)
                {
                    instance = new GameObject("AILinkPool",typeof(AILinkPool)).GetComponent<AILinkPool>();
                }
                return instance;
            }
        }
        public static NavMeshLink GetFreeLink()
        {
            NavMeshLink link;
            if (meshLinks[true].Count == 0)
            {
                link = Instance.gameObject.AddComponent<NavMeshLink>();
            }
            else
            {
                link = meshLinks[true].Pop();
                link.enabled = true;
            }
            meshLinks[false].Push(link);
            return link;
        }
        public static void PushLink(NavMeshLink link)
        {
            link.enabled = false;
            meshLinks[true].Push(link);
        }
        private void OnDisable()
        {
            instance = null;
            meshLinks[true].Clear();
            meshLinks[false].Clear();
        }
    }
}
