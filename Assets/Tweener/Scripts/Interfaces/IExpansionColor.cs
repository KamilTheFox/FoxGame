namespace Tweener
{
    public interface IExpansionColor : IExpansionTween<IExpansionColor>
    {
        IExpansionColor TypeOfColorChange(TypeChangeColor type);
        IExpansionColor IgnoreAdd(IgnoreARGB ARGB);

        IExpansionColor TypeComponentChange(TypeComponentChangeColor typeComponent);
    }
}
