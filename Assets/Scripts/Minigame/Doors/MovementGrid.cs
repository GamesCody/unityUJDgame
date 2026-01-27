using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class MovementGrid : MonoBehaviour
{
    private bool ReadyToMove ;
    private GameObject[] Obstackles;
     private GameObject[] ObjToPush;
    private InputAction moveAction;

     void Start()
    {
    Obstackles = GameObject.FindGameObjectsWithTag("Wall");
    // 🔥 Szukamy zarówno Box jak i ElectricBox do popchnięcia
    GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
    GameObject[] electricBoxes = GameObject.FindGameObjectsWithTag("ElectricBox");
    ObjToPush = new GameObject[boxes.Length + electricBoxes.Length];
    System.Array.Copy(boxes, ObjToPush, boxes.Length);
    System.Array.Copy(electricBoxes, 0, ObjToPush, boxes.Length, electricBoxes.Length);
    
    // Setup Input System with composite bindings for Vector2
    var inputMap = new InputActionMap("Gameplay");
    moveAction = inputMap.AddAction("Move", InputActionType.Value);
    
    // Add gamepad stick
    moveAction.AddBinding("<Gamepad>/leftStick");
    
    // Add WASD composite
    moveAction.AddCompositeBinding("2DVector")
        .With("Up", "<Keyboard>/w")
        .With("Down", "<Keyboard>/s")
        .With("Left", "<Keyboard>/a")
        .With("Right", "<Keyboard>/d");
    
    inputMap.Enable();
    }
    public bool Move(Vector2 direction)
    {
        if(Math.Abs(direction.x)<0.5)
        {
            direction.x=0;
        }
        else
        {
            direction.y=0;
        }
        direction.Normalize();
        if(Blocked(transform.position,direction))
        {
            return false;
        }
        else
        {
            transform.Translate(direction);
            return true;
        }

    }
 
    public bool Blocked(Vector3 position,Vector2 direction)
    {
        Vector2 newpos = new Vector2(position.x, position.y) + direction;
        foreach (var obj in Obstackles)
        {
            // 🔥 Sprawdzenie czy obiekt nie został zniszczony
            if (obj == null) continue;
            
            if (obj.transform.position.x == newpos.x && obj.transform.position.y == newpos.y)
            {
                return true;
            }

        }
        foreach (var obj in ObjToPush)
        {
            // 🔥 Sprawdzenie czy obiekt nie został zniszczony
            if (obj == null) continue;
            
            if (obj.transform.position.x == newpos.x && obj.transform.position.y == newpos.y)
            {
                Push objpush = obj.GetComponent<Push>();
                if(objpush && objpush.Move(direction))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

        }
        return false;
    }

void Update()
    {
      Vector2 moveinput = moveAction.ReadValue<Vector2>();
      moveinput.Normalize();
      if(moveinput.sqrMagnitude>0.5f)
      {
        if(ReadyToMove)
        {
            ReadyToMove=false;
            Move(moveinput);
        }
      }
      else
      {
        ReadyToMove=true;
    }
              
}
}