using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;

    public abstract class AI
    {
    private NavMeshAgent Navigation;

    public bool IsStopped => Navigation.remainingDistance < 0.01F || Navigation.velocity == Vector3.zero;


    public AnimalEngine engine { get; private set; }

    public Animator Animator { get; private set; }

    private IBehavior Behavior { get; set; }

    public string NameBehavior => Behavior == null ? "No Behavior" : Behavior.Name;

    protected event Action ChangeStateBehaviour;
    protected abstract void OnStart();
    public void Start(AnimalEngine _engine)
    {
        engine = _engine;
        Animator = _engine.gameObject.GetComponent<Animator>();
        if (!_engine.gameObject.TryGetComponent(out Navigation))
        {
            Navigation = _engine.gameObject.AddComponent<NavMeshAgent>();
        }
        OnStart();
        if (GameObject.FindObjectOfType<NavMeshSurface>() != null && Behavior == null)
            SetBehavior(new Idle(UnityEngine.Random.Range(0,2) == 0));
    }

    protected abstract void OnUpdate(); 

    public void Update()
    {
        OnUpdate();
        Behavior?.Update();
    }
    public abstract Action<Collision, GameObject> BehaviorFromCollision { get; }

    public bool SetDestination(Vector3 position)
    {
        NavMeshPath navMeshPath = new NavMeshPath();
        Navigation.CalculatePath(position, navMeshPath);
        if (navMeshPath.status == NavMeshPathStatus.PathComplete)
        {
            Navigation.SetDestination(position);
            return true;
        }
        return false;
    }
    public void OnEnableMove()
    {
        Navigation.enabled = false;
    }
    public void StopMove()
    {
        Navigation.isStopped = true;
    }
    public void ContinueMove()
    {
        Navigation.isStopped = false;
    }
    public bool CheckStatePath(NavMeshPathStatus status)
    {
        return Navigation.pathStatus == status;
    }
    public void SetAnimation(TypeAnimationAnimal _enum)
    {
        if (_enum == TypeAnimationAnimal.Walk)
            _enum = TypeAnimationAnimal.None;
        Animator.Play(_enum.ToString());
        float speed;
        switch (_enum)
        { 
            case TypeAnimationAnimal.Run_Fast:
                speed = 3.5F * 2.4F;
                break;
            case TypeAnimationAnimal.Run:
                speed = 3.5F * 1.8F;
                break;
            default:
                speed = 3.5F;
                break;
        }
        Navigation.speed = speed;
    }

    public void SetBehavior(IBehavior behavioral)
    {
        Behavior?.Deactivate();
        Behavior = behavioral;
        behavioral.Activate(this);
        ChangeStateBehaviour?.Invoke();
    }
    public bool IsAnimationPlaying(string animationName)
    {
          return Animator.GetCurrentAnimatorStateInfo(0).IsName(animationName);
    }
}
