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
    public static void MovementWASD(Rigidbody obj, float Speed, PlayerControll player)
    {
        Vector3 velosity = WASD(obj.transform, Speed);
        Vector3 newPosition = obj.position + new Vector3(velosity.x, 0, velosity.z);
        bool RayChack = RayCheck(obj.position, new Vector3(velosity.x, 0, velosity.z), player.RecommendedHeight);
            //if (obj.SweepTest(newPosition, out RaycastHit hit, Vector3.Distance(obj.position, newPosition)))
            //{
            //bool isStationaryEntity = false;
            //EntityEngine entity = hit.collider.gameObject.GetComponentInParent<EntityEngine>();
            //if (entity)
            //    isStationaryEntity = entity.Stationary;
            //   if (!isStationaryEntity && RayChack)
            //   {
            //     obj.MovePosition(newPosition);
            //   }
            //}
            if(RayChack)
                obj.MovePosition(newPosition);
       
    }
    public static void MovementWASDVelocity(Rigidbody obj, float Speed)
    {
        Vector3 velosity = WASD(obj.transform, Speed, out bool isMove, true);
        if (isMove)
            obj.velocity = new Vector3(velosity.x, obj.velocity.y, velosity.z);
        else 
            obj.velocity = Vector3.up * obj.velocity.y;
    }
    private static bool RayCheck(Vector3 oldV,Vector3 newV, float Height)
    {
        int Layer = LayerMask.GetMask(new string[] { "Terrain", "Default", "Entity" });
        EntityEngine Entity = null;
        bool collider = Physics.SphereCast(oldV + Height * Vector3.up, 0.35F * Height, newV, out RaycastHit hit, 0.26F, Layer);
        if(collider)
        Entity = hit.collider.gameObject.GetComponentInParent<EntityEngine>();
        if (Entity && !Entity.Stationary)
            return true;
        return !collider;
        //bool isTerrain = !Physics.Raycast(oldV, newV, out RaycastHit hit, 0.71F ,Layer ) && !Physics.Raycast(oldV + Vector3.up * 0.7F, newV, out hit, 0.71F, Layer);
        //if(!isTerrain && hit.collider.gameObject.TryGetComponent(out EntityEngine entity))
        //{
        //    return !entity.Stationary;
        //}
        //return isTerrain;
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
