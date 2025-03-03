using System.Collections;
using UnityEngine;

namespace Assets.Resources.Тест
{
    [RequireComponent(typeof(MeshRenderer), typeof(Collider))]
    public class PaintedWall : MonoBehaviour
    {
        [SerializeField] private int widht = 1080, height = 1080;

        public Texture2D texture;

        private Material material;

        public void Awake()
        {
            material = GetComponent<Renderer>().sharedMaterial;

            texture = new Texture2D(widht, height);

            ResetTexture();

            for (int i = 0; i < texture.width; i++)
            {
                for (int p = 0; p < texture.height; p++)
                    texture.SetPixel(i, p, Color.red);
            }
            texture.Apply();
        }

        public void ResetTexture()
        {
            material.SetTexture("_MaskMap", texture);
        }
    }
}
