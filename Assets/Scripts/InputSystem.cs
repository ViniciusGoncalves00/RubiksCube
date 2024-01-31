using UnityEngine;

public class InputSystem : MonoBehaviour
{
    public static bool PressedSwitch;

    public static bool R01;
    public static bool R02;
    public static bool R03;
    public static bool R04;
    public static bool R05;
    public static bool R06;

    public static bool DefaultRotation;
    
    void Update()
    {
        PressedSwitch = Input.GetKey(KeyCode.LeftShift);
        
        R01 = Input.GetKeyDown(KeyCode.X);
        R02 = Input.GetKeyDown(KeyCode.S);
        R03 = Input.GetKeyDown(KeyCode.C);
        R04 = Input.GetKeyDown(KeyCode.D);
        R05 = Input.GetKeyDown(KeyCode.Z);
        R06 = Input.GetKeyDown(KeyCode.A);
        
        DefaultRotation = Input.GetKeyDown(KeyCode.P);
    }
}
