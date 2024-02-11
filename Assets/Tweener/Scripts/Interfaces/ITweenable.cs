
namespace Tweener
{
    internal interface ITweenable
    {
        bool isFixedUpdate { get; set; }
        float Timer { get; }
        bool IsUsed();
        void OnChange();
        void OnComplection();
    }
}
