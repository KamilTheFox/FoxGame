using FactoryLesson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GroupMenu;

    public abstract class AnimalEngine : EntityEngine, IAlive
{
    [SerializeField] private TypeAnimation StartAnumation = TypeAnimation.Idle;
    public IRegdoll Regdool { get; private set; }
   
    private Animator Animator { get; set; }
    /// <returns>Тип перечисления уникальных анимаций</returns>
    protected abstract Type Started();

    private List<string> NamesAnimation = new List<string>();

    public void Start()
    {
        NamesAnimation.AddRange(Enum.GetNames(typeof(TypeAnimation)));
        Animator = GetComponent<Animator>();
        Regdool = new Regdoll(Animator , this);
        Animator.Play(StartAnumation.ToString());
        Type typeNewAnimation = Started();
        if (typeNewAnimation != null)
        NamesAnimation.AddRange(Enum.GetNames(typeNewAnimation));
    }
    protected void SetAnimation(Enum _enum)
    {
        Animator.Play(_enum.ToString());
    }
    public static AnimalEngine AddAnimal(TypeAnimal animal, Vector3 position, Quaternion quaternion)
    {
        return AddEntity<AnimalEngine>(animal, position, quaternion);
    }
    public virtual void Interactive()
    {
        DebugAnimation.Animations = NamesAnimation;
        DebugAnimation.Animator = Animator;
        Menu.ActivateMenu(new DebugAnimation());
    }
    public abstract Action<Collision> BehaviorFromCollision { get; }

    public bool IsDead { get; private set; }

    public void Dead()
    {
        Regdool.Activate();
        IsDead = true;
        Delete(120F);
    }

}
public enum TypeAnimation
{
    None,
    Idle,
    Sits,
    Run,
    Walk
}
public enum TypeAnimal
{
    Fox,
    Fox_White,
    Fox_Red
}
