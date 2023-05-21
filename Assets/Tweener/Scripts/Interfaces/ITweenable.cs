
namespace Tweener
{
    internal interface ITweenable
    {
        float Timer { get; }
        bool IsUsed();
        void OnChange();
        void OnComplection();
    }
}
