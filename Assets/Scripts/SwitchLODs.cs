using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityMeshSimplifier;

namespace Assets.Scripts
{
    public class SwitchLODs : MonoBehaviour
    {
        private void Start()
        {
            LODGroup group = GetComponent<LODGroup>();
            Animator animator = GetComponentInChildren<Animator>();
            List<SkinnedMeshRenderer> renderers = animator.GetComponentsInChildren<SkinnedMeshRenderer>().ToList();
            foreach (LOD lod in group.GetLODs())
            {
                foreach(SkinnedMeshRenderer renderer in lod.renderers.Select(rend => rend as SkinnedMeshRenderer))
                {
                    SkinnedMeshRenderer skinned;
                    if ((skinned = renderers.Find(t => renderer.name.Contains(t.gameObject.name))) != null)
                    {
                        renderer.bones = skinned.bones;
                        renderer.rootBone = skinned.rootBone;
                        renderer.updateWhenOffscreen = true;
                        renderer.transform.SetParent(animator.transform);
                    }

                }
            }
            renderers.ForEach(renderer => renderer.gameObject.SetActive(false));
        }
    }
}
