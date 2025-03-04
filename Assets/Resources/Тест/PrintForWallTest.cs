using System.Collections;
using UnityEngine;
using CameraScripts;

namespace Assets.Resources.Тест
{
    public class PrintForWallTest : MonoBehaviour, ICameraCastSubscriber
    {
        public enum MaskType
        {
            red,
            green,
            blue,
            black
        }

        public MaskType maskType = MaskType.blue;

        public int SizeBrush = 46;
        public void OnCameraCasting(RaycastHit hit)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                if (hit.collider.TryGetComponent(out PaintedWall wall))
                {
                    Color color = GetColor(maskType);
                    Vector2 textSize = new Vector2(wall.texture.width, wall.texture.height);

                    Vector2 positionUV = (hit.textureCoord * textSize);;

                    Debug.Log("Draw " + positionUV);

                    Debug.Log("textureCoord " + hit.textureCoord);

                    Debug.Log("texelSize " + textSize);

                    int radius = (SizeBrush / 2);

                    for (int x = 0; x < SizeBrush; x++)
                    {
                        for (int y = 0; y < SizeBrush; y++)
                        {
                            float xR = Mathf.Pow(x - radius, 2);
                            float yR = Mathf.Pow(y - radius, 2);
                            float r = Mathf.Pow(radius - 0.5F, 2);
                            if (xR + yR < r)
                            {
                                Color cur = wall.texture.GetPixel((int)positionUV.x + x - radius, (int)positionUV.y + y - radius);
                                wall.texture.SetPixel((int)positionUV.x + x - radius, (int)positionUV.y + y - radius, Color.Lerp(color,cur, 0.5F));
                            }
                        }
                    }

                    wall.texture.Apply();
                }
            }
        }

        public Color GetColor(MaskType mask)
        {
            switch (mask)
            {
                case MaskType.red
                    : return Color.red;
                    case MaskType.green
                    : return Color.green;
                    case MaskType.blue
                    : return Color.blue;
                    default:
                    return Color.black;
            }
        }

        void Start()
        {
            CameraControll.Instance.AddSubscribeToCast(this);
        }
    }
}
