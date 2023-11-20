using UnityEngine;

public static class MovementMode
{
    public static float MovementSpeed = 1F;
    public static void MovementWASD(CharacterController obj, float Speed)
    {
        Vector3 velosity = WASD(obj.transform,Speed); 
        obj.Move(new Vector3(velosity.x, -0.1F, velosity.z));
    }
    public static Vector3 WASD(Transform transform,float Speed, bool FixedUpdate = false)
    {
        float deltaTimeSpeed = Speed * MovementSpeed *  (FixedUpdate? Time.fixedDeltaTime : Time.deltaTime);
        Vector3 newPosition = transform.forward * deltaTimeSpeed;
        Vector3 velosity = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) velosity += newPosition;
        if (Input.GetKey(KeyCode.S)) velosity += -newPosition;
        newPosition = transform.right * deltaTimeSpeed;
        if (Input.GetKey(KeyCode.D)) velosity += newPosition;
        if (Input.GetKey(KeyCode.A)) velosity += -newPosition;

        return velosity;
    }
    public static Vector3 WASD(Transform transform, float Speed, out bool IsMove, bool FixedUpdate = false)
    {
        IsMove = false;
        float deltaTimeSpeed = Speed * MovementSpeed * (FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime);
        Vector3 newPosition = transform.forward * deltaTimeSpeed;
        Vector3 velosity = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) { velosity += newPosition; IsMove = true; }
        if (Input.GetKey(KeyCode.S)) { velosity += -newPosition; IsMove = true; }
        newPosition = transform.right * deltaTimeSpeed;
        if (Input.GetKey(KeyCode.D)) { velosity += newPosition; IsMove = true; }
        if (Input.GetKey(KeyCode.A)) { velosity += -newPosition; IsMove = true; }
        return velosity;
    }
    public static void MovementWASD(Transform obj, float Speed)
    {
        Vector3 velosity = WASD(obj, Speed);
        obj.localPosition = obj.localPosition + velosity;
    }
    public static void MovementFlySpaseLSift(Transform obj, float Speed)
    {
        float deltaTimeSpeed = Speed * Time.deltaTime;
        if (Input.GetKey(KeyCode.LeftShift)) obj.transform.position += Vector3.down * deltaTimeSpeed;
        if (Input.GetKey(KeyCode.Space)) obj.transform.position += Vector3.up * deltaTimeSpeed;
    }
    public static void MovementFlySpaseLSift(Rigidbody obj, float Speed,bool isGround = false)
    {
        float deltaTimeSpeed = Speed * MovementSpeed * Time.deltaTime;
        Vector3 velosity = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftShift)&& !isGround) velosity += Vector3.down * deltaTimeSpeed;
        if (Input.GetKey(KeyCode.Space)) velosity += Vector3.up * deltaTimeSpeed;
        obj.MovePosition(obj.position + new Vector3(0, velosity.y, 0));
    }
    public static void MovementFlySpaseLSift(CharacterController obj, float Speed, bool isGround = false)
    {
        float deltaTimeSpeed = Speed * MovementSpeed * Time.deltaTime;
        Vector3 velosity = Vector3.zero;
        if (Input.GetKey(KeyCode.LeftShift) && !isGround) velosity += Vector3.down * deltaTimeSpeed;
        if (Input.GetKey(KeyCode.Space)) velosity += Vector3.up * deltaTimeSpeed;
        obj.Move(obj.transform.position + new Vector3(0, velosity.y, 0));
    }
}
