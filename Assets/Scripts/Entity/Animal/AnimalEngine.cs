using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GroupMenu;


public abstract class AnimalEngine : EntityEngine, IDiesing, IInteractive
{
    #region Description
    public static List<AnimalEngine> AnimalList
    {
        get
        {
            List<AnimalEngine> list = new();
            Entities[TypeEntity.Animal].ForEach(e => list.Add(e as AnimalEngine));
            return list;
        }
    }
    public override TypeEntity typeEntity => TypeEntity.Animal;

    public IRegdoll Regdool { get; private set; }

    [HideInInspector] public TypeAnimal TypeAnimal;

    /// <returns>Тип перечисления уникальных анимаций</returns>
    #endregion

    private AI AI;

    public AI GetAI => AI;

    private Animator Animator => AI?.Animator;

    public string NameAI => AI == null ? "No AI" : AI.GetType().Name;

    public string Behavior => AI == null ? "No Behavior" : AI.NameBehavior;

    protected override void OnStart()
    {
        SetAI(FactoryEntity.AnimalsCreating.GetAI(TypeAnimal));

        Regdool = new Regdoll(Animator, this);
    }
    public void SetAI(AI _AI)
    {
        if (IsDie)
        {
            Menu.Error(LText.ErrorSetAI.GetTextUI().ToString());
            return;
        }
        AI = _AI;
        AI.Start(this);
    }
    public static AnimalEngine AddAnimal(TypeAnimal animal, Vector3 position, Quaternion quaternion)
    {
        return AddEntity<AnimalEngine>(animal, position, quaternion);
    }
    public void Interaction()
    {
        None.SetInfoEntity(false);
        DebugAnimation.Animator = Animator;
        Menu.ActivateMenu<DebugAnimation>();
    }
    public override void Delete(float time = 0)
    {
        AI = null;
        base.Delete(time);
    }
    private void Update()
    {
        AI?.Update();
    }
    public Action<Collision, GameObject> BehaviorFromCollision => AI?.BehaviorFromCollision;

    public bool IsDie { get; private set; }

    public void Death()
    {
        if (IsDie) return;
        Regdool.Activate();
        AI.StopMove();
        AI.OnEnableMove();
        AI = null;
        IsDie = true;
        Delete(120F);
    }

}

