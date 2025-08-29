using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    public static readonly string AxisVertical = "Vertical";
    public static readonly string AxisHorizontal = "Horizontal";
    public static readonly string AxisFire1 = "Fire1";

    public float MoveVertical { get; private set; }
    public float MoveHorizontal { get; private set; }
    public float Rotate { get; private set; }
    
    public bool Fire { get; private set; }

    private void Update()
    {
        MoveVertical = Input.GetAxis(AxisVertical);
        MoveHorizontal = Input.GetAxis(AxisHorizontal);
        Fire = Input.GetButton(AxisFire1);
    }
}
