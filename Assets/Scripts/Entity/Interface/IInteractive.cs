using UnityEngine;


public interface IInteractive
{
    public void Interaction();

    public EntityEngine GetEngine { get; }

    public Transform Transform { get;}

    public Rigidbody Rigidbody => null;

}
