using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using PlayerDescription;

namespace ScreenplayLevel
{
    
    public class EnemyBoss : MonoBehaviour
    {
        private Transform Transform { get; set; }

        [SerializeField] private float AngleInDegrees;

        [SerializeField] private float CooldownDrop;

        [SerializeField] private float CooldownDetonate;

        [SerializeField] private int DestroyBoxCount, RandomTNT = 4;

        [SerializeField] List<BoxEnemy> EnemyHealth = new ();

        [SerializeField] private Animator animator;
        public static EnemyBoss Instance { get; private set; }

        public byte countHealth => (byte)EnemyHealth.Count;

        private Vector3 fromTo, fromToXZ;

        [SerializeField] private Transform SpawnTransform;

        [SerializeField] private CharacterBody Player;

        private static UnityEvent deathEnemy = new();
        [Serializable]
        private struct DifficultyBoss
        {
            public float deltaCooldownDrop;
            public float deltaCooldownDetonate;
            public int DestroyBoxCountDelay;
            public int DestroyBoxCountRandomTNTDelay;
        }
        [SerializeField] private DifficultyBoss easy, normal, hard;
        [SerializeField] private DifficultyBoss current;
        public static event UnityAction DeathEnemy
        {
            add
            {
                deathEnemy.AddListener(value);
            }
            remove
            {
                deathEnemy.RemoveListener(value);
            }
            
        }
        private bool isShot = true;
        float Gravity => Physics.gravity.y;

        private ItemEngine[] TNTVariants;

        private void Awake()
        {
            Instance = this;
            Transform = transform;
            Player.OnDied += () => { isShot = false; };
            TNTVariants = new ItemEngine[]
            {
                ItemEngine.AddItem(TypeItem.TNT_3, SpawnTransform.position, Quaternion.identity, false),
                ItemEngine.AddItem(TypeItem.TNT, SpawnTransform.position, Quaternion.identity, false)
            };
            TNTVariants[0].gameObject.SetActive(false);
            TNTVariants[1].gameObject.SetActive(false);
        }
        private void Start()
        {
            switch(GameState.Difficulty)
            {
                case GameState.TypeDifficulty.Easy:
                    current = easy;
                    break;
                case GameState.TypeDifficulty.Hard:
                    current = hard;
                    break;
                default:
                    current = normal;
                    break;
            }
            StartCoroutine(BehavourShootTNT());
        }
        private IEnumerator BehavourShootTNT()
        {
            yield return new WaitForSeconds(1F);
            while (EnemyHealth.Count > 0 && isShot)
            {
                animator.transform.localEulerAngles = Vector3.zero;
                animator.transform.localPosition = Vector3.down;
                animator.SetTrigger("Throw");
                yield return new WaitForSeconds(CooldownDrop);
            }
            yield break;
        }
        public void Update()
        {
            Vector3 targetVector = new Vector3(Player.Transform.position.x, 2F, Player.Transform.position.z);
            fromTo = targetVector - SpawnTransform.position;
            fromToXZ = new Vector3(fromTo.x, 0f, fromTo.z);
            Transform.rotation = Quaternion.LookRotation(fromToXZ, Vector3.up);
            SpawnTransform.localEulerAngles = new Vector3(-AngleInDegrees, 0f, 0f);
        }
        public void Damage(BoxEnemy box)
        {
            if (DestroyBoxCount % current.DestroyBoxCountDelay == 0)
            {
                CooldownDrop *= 1F - current.deltaCooldownDrop;
                animator.speed += current.deltaCooldownDrop;
            }
            DestroyBoxCount++;
            if(DestroyBoxCount % current.DestroyBoxCountRandomTNTDelay == 0)
            {
                if (RandomTNT > 1)
                    RandomTNT--;
            }
            if (EnemyHealth.Contains(box))
            {
                CooldownDetonate *= 1F - current.deltaCooldownDetonate;
                EnemyHealth.Remove(box);
            }
            LevelFirstBoss.imageUIEnemy.SetText(EnemyHealth.Count.ToString());
            if (EnemyHealth.Count < 1)
                deathEnemy.Invoke();
        }
        private int t = 1;
        private void TntInteractDropLogic(Detonator tnt, Collision collision)
        {
            tnt.Rigidbody.velocity = Vector3.zero;
            tnt.Delete(CooldownDetonate);
        }
        public void Shot()
        {
            
            float x = fromToXZ.magnitude;
            float y = fromTo.y;

            float AngleInRadians = AngleInDegrees * Mathf.PI / 180;

            float v2 = (Gravity * x * x) / (2 * (y - Mathf.Tan(AngleInRadians) * x) * Mathf.Pow(Mathf.Cos(AngleInRadians), 2));
            float v = Mathf.Sqrt(Mathf.Abs(v2));

            IInteractive newBullet = GameObject.Instantiate<ItemEngine>(TNTVariants[(t % RandomTNT == 0 ? 0 : 1)]) as IInteractive;
            Detonator tnt = newBullet as Detonator;
            tnt.gameObject.SetActive(true);
            newBullet.Rigidbody.velocity = SpawnTransform.forward * v;
            newBullet.Rigidbody.AddTorque(UnityEngine.Random.rotation.eulerAngles,ForceMode.Force);
            newBullet.Interaction();
            tnt.OnCollision.AddListener(TntInteractDropLogic);
            foreach(Transform obj in tnt.gameObject.transform)
            {
                obj.gameObject.layer = LayerMask.NameToLayer("Default");
            }
            tnt.IsDetectionModeContinuousDynamic = true;
            tnt.CancelDelete();
            t++;
        }
    }
}
