using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CameraScripts;
using PlayerDescription;
using UnityEngine;
using VulpesTool;
using static TowerСrane;

class TowerСrane : ItemEngine, IInteractive
{

    [HideInInspector]
    [SerializeField] private Transform tower;
    [Space]
    [HideInInspector]
    [SerializeField] private Transform[] ropesToCart;
    [HideInInspector]
    [SerializeField] private Transform cart;
    [HideInInspector]
    [SerializeField] private Vector3 startPositionCart;
    [HideInInspector]
    [SerializeField] private Vector3 maxPositionCart;

    
    [HideInInspector]
    [SerializeField] private Vector3 maxScaleRopes;
    [HideInInspector]
    [SerializeField] private Transform hook;
    [HideInInspector]
    [SerializeField] private Transform ropesHook;
    [HideInInspector]
    [SerializeField] private Vector3 startPositionHook;
    [HideInInspector]
    [SerializeField] private Vector3 minPositionHook;
    [HideInInspector]
    [SerializeField] private Vector3 minScaleHook;
    [HideInInspector]
    [SerializeField]
    private GameObject cell;
    [HideInInspector]
    [SerializeField]
    private GameObject towerCell;
    [HideInInspector]
    [SerializeField]
    private List<GameObject> initCells;

    [HideInInspector]
    [SerializeField]
    private Transform raiseObject;
    [HideInInspector]
    [SerializeField]
    private Transform raiseObjectParent;

    private float raiseObjectHeight = 0;

    private int oldMask = 0;

    [SerializeField]
    private Transform cameraPosition;

    [SerializeField]
    private Collider towerCollider;

    private IBeingLifted beingLifted;

    protected override void OnAwake()
    {
        base.OnAwake();
        if(raiseObject != null)
            beingLifted = raiseObject.gameObject.GetComponent<IBeingLifted>();
    }

    private void Reset()
    {
        tower.eulerAngles = new Vector3(270,0,0);
        hook.localPosition = startPositionHook;
        cart.localPosition = startPositionCart;
        UpdateRopesToCart();
        UpdateRopesToHook();
    }

    private void UpdateRopesToCart()
    {
        if (cart == null || ropesToCart == null) return;

        float currentOffset = cart.localPosition.y - startPositionCart.y;

        float maxOffset = maxPositionCart.y - startPositionCart.y;

        float percentY = currentOffset / maxOffset;

        Vector3 newScale = Vector3.LerpUnclamped(Vector3.one, maxScaleRopes, percentY);

        foreach (var rope in ropesToCart)
        {
            if (rope != null)
                rope.localScale = newScale;
        }
    }

    private void UpdateRopesToHook()
    {
        if (hook == null || ropesHook == null) return;

        float currentOffset = hook.localPosition.z - startPositionHook.z;
        float maxOffset = minPositionHook.z - startPositionHook.z;
        float percentZ = currentOffset / maxOffset;

        Vector3 newScale = Vector3.LerpUnclamped(Vector3.one, minScaleHook, percentZ);
        ropesHook.localScale = newScale;
    }

    public void TryRaise()
    {
        IBeingLifted being = null;
        Physics.OverlapSphere(hook.position, 0.5f).First(col => col.TryGetComponent(out being));
        if(being != null)
        {
            beingLifted?.Throw();
            being.Raise();
            raiseObjectParent = being.gameObject.transform.parent;
            raiseObject = being.gameObject.transform;
            beingLifted = being;
            raiseObject.SetParent(hook);

            oldMask = raiseObject.gameObject.layer;

            raiseObject.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            Renderer[] renderers = being.gameObject.GetComponentsInChildren<Renderer>();

            Bounds overallBounds = renderers[0].bounds;

            foreach (Renderer r in renderers)
            {
                overallBounds.Intersects(r.bounds);
            }
            raiseObjectHeight = overallBounds.size.y;
        }
    }

    public void Throw()
    {
        beingLifted?.Throw();
        if (raiseObject == null)
            return;
        raiseObject.SetParent(raiseObjectParent);
        raiseObject.gameObject.layer = oldMask;
        raiseObjectParent = null;
        raiseObject = null;
        raiseObjectHeight = 0;
    }

    public void AddTowerCell()
    {
        if (initCells.Count == 0)
        {
            GameObject newCell = Instantiate(cell, towerCell.transform.parent);
            initCells.Add(newCell);
        }
        else
        {
            GameObject lastCell = initCells[initCells.Count - 1];
            Vector3 newPosition = lastCell.transform.localPosition;

            newPosition.z += 2F;

            GameObject newCell = Instantiate(cell, towerCell.transform.parent);

            towerCell.transform.localPosition = newPosition;
            newCell.transform.localPosition = newPosition;

            initCells.Add(newCell);
        }
    }

    public void RemoveTowerCell()
    {
        if (initCells.Count <= 1)
        {
            Debug.LogWarning("Нельзя удалить основную ячейку!");
            return;
        }

        // Удаляем последнюю ячейку
        GameObject cellToRemove = initCells[initCells.Count - 1];
        initCells.RemoveAt(initCells.Count - 1);
#if UNITY_EDITOR
        DestroyImmediate(cellToRemove);
#else
        Destroy(cellToRemove);
#endif
        towerCell.transform.localPosition = towerCell.transform.localPosition - (Vector3.forward * 2);
    }

    public void MoveHook(float vector)
    {
        Vector3 newPos = hook.localPosition;
        newPos.z += vector;
        if(newPos.z > -1)
        {
            newPos.z = -1;
        }
        if (Physics.Raycast(hook.position, Vector3.up * vector, out RaycastHit hit, Mathf.Abs(vector) + raiseObjectHeight))
        {
            return;
        }
        hook.localPosition = newPos;
        UpdateRopesToHook();
    }

    public void MoveCart(float vector)
    {
        Vector3 newPos = cart.localPosition;
        newPos.y += vector;
        if (newPos.y < maxPositionCart.y)
        {
            newPos.y = maxPositionCart.y;
        }
        if(newPos.y > -1)
        {
            newPos.y = -1;
        }
        cart.localPosition = newPos;
        UpdateRopesToCart();
    }
    public void RotateTower(float angle)
    {
        tower.Rotate(Vector3.forward, angle);
    }

    public void Interaction()
    {
        CameraControll.instance.ChangeViewPerson(new CameraControllTower(CameraControll.instance, cameraPosition,this));
    }



    [CreateGUI(title: "Controls",color: ColorsGUI.SuccessGreen)]
    public void Controls()
    {
        GUILayout.BeginVertical("box");

        GUILayout.Label("Поворот башни");
        GUILayout.BeginHorizontal();
        if (GUILayout.RepeatButton("← Влево"))
        {
            RotateTower(-1f);
        }
        if (GUILayout.RepeatButton("Вправо →"))
        {
            RotateTower(1f);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label("Управление тележкой");
        GUILayout.BeginHorizontal();
        if (GUILayout.RepeatButton("← Назад"))
        {
            MoveCart(0.1f);
        }
        if (GUILayout.RepeatButton("Вперед →"))
        {
            MoveCart(-0.1F);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label("Управление крюком");
        GUILayout.BeginHorizontal();
        if (GUILayout.RepeatButton("↑ Поднять"))
        {
            MoveHook(0.1F);
        }
        if (GUILayout.RepeatButton("Опустить ↓"))
        {
            MoveHook(-0.1F);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        if (GUILayout.RepeatButton("Сбросить позиции"))
        {
            Reset();
        }
        GUILayout.Space(10);
        GUILayout.Label("Высота крана");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Добавить"))
        {
            AddTowerCell();
        }
        if (GUILayout.Button("Убрать"))
        {
            RemoveTowerCell();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("Предмет");
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Поднять"))
        {
            TryRaise();
        }
        if (GUILayout.Button("Бросить"))
        {
            Throw();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
        GUILayout.Label($"Тележка Y: {cart.localPosition.y:F2}");
        GUILayout.Label($"Крюк Z: {hook.localPosition.z:F2}");
        GUILayout.Label($"Поворот башни: {tower.eulerAngles.y:F2}°");

        GUILayout.EndVertical();
    }

    

    public interface IBeingLifted
    {
        public GameObject gameObject { get; }

        public void Throw();

        public void Raise();
    }

    private class CameraControllTower : IViewedCamera
    {
        private Transform _parent;
        private CameraControll _camera;

        TowerСrane towerСrane;

        public CameraControllTower(CameraControll camera, Transform Parent, TowerСrane сrane)
        {
            _parent = Parent;
            _camera = camera;
            towerСrane = сrane;
        }

        public void OnGUI()
        {
            towerСrane.Controls();
            if (GUILayout.Button("Выйти"))
            {
                CameraControll.instance.OnFirstPerson();
            }
        }

        public void Construct()
        {
            _camera.Transform.parent = _parent;
            _camera.Transform.localPosition = Vector3.zero;
            _camera.Transform.localEulerAngles = _parent.localEulerAngles;
            Cursor.lockState = CursorLockMode.None;
        }
        public Vector3 RotateBody()
        {
            return _camera.EulerHorizontal;
        }
        public Vector2 ViewAxisMaxVertical => new Vector2(-40, -20);

        public void ViewAxis(Transform camera, Vector3 euler)
        {
            _camera.Transform.localEulerAngles = -Vector3.left * euler.x;

            _camera.Transform.position = _parent.position;
        }

        public void Dispose()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
