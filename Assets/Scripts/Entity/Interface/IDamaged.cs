
internal interface IDamaged : IDiesing
{
    float Health { get; }

    void TakeHit(IStriker striker);
}
