using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class SceneMenu : MonoBehaviour
{
    Vector3 StartPosition;
    private void Awake()
    {
        StartCoroutine(RandonGiveEntity());
    }
    static System.Random random = new System.Random();
    public void LoadLevel(int level)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(level);
    }
    public IEnumerator RandonGiveEntity()
    {
        yield return new WaitForSeconds(0.5F);
        RaycastHit hit;
        Ray ray = CameraControll.MainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        if (Physics.Raycast(ray, out hit))
        {
            StartPosition = hit.point + ray.direction.normalized * 0.01F;
        }
        StartPosition = StartPosition + Vector3.up * 5F;
        while (true)
        {
            yield return new WaitForSeconds(1F);
            Vector3 vector = new Vector3((float)(random.Next(-5, 6) * random.NextDouble()), 0, (float)(random.Next(-5, 6) * random.NextDouble()));
            Quaternion quaternion = Quaternion.Euler(new Vector3(0, random.Next(0, 360),0));
                int rnd = random.Next(1, ItemEngine.CountItemTypes);
                if (rnd == (int)TypeItem.TNT && random.Next(1, 5) != (int)TypeItem.TNT)
                {
                    rnd = random.Next(1, ItemEngine.CountItemTypes);
                }
                ItemEngine.AddItem((TypeItem)rnd, StartPosition + vector, quaternion, false);
            if (ItemEngine.GetItems.Length == 25)
            {
                yield return new WaitForSeconds(1F);
                ItemEngine.AddItem(TypeItem.TNT, StartPosition + vector, Quaternion.identity,false).Interaction();
                yield return new WaitForSeconds(7F);
                Vector3 forward = CameraControll.instance.Transform.forward * 13;
                CameraControll.instance.Transform.position -= forward;
                    yield return new WaitForSeconds(12F);
                CameraControll.instance.Transform.position += forward;
            }
        }
       // yield break;
    }
   
}
