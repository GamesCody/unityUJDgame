using UnityEngine;
using UnityEngine.InputSystem;

public class Push : MonoBehaviour
{
    private GameObject[] Obstackles;
    private GameObject[] ObjToPush;   

     void Start()
    {
    Obstackles = GameObject.FindGameObjectsWithTag("Wall");
    // 🔥 Szukamy zarówno Box jak i ElectricBox do popchnięcia
    GameObject[] boxes = GameObject.FindGameObjectsWithTag("Box");
    GameObject[] electricBoxes = GameObject.FindGameObjectsWithTag("ElectricBox");
    ObjToPush = new GameObject[boxes.Length + electricBoxes.Length];
    System.Array.Copy(boxes, ObjToPush, boxes.Length);
    System.Array.Copy(electricBoxes, 0, ObjToPush, boxes.Length, electricBoxes.Length);
    }

public bool Move(Vector2 direction)
    {
        if(ObjToBlocked(transform.position,direction))
        {
            return false;
        }
        else
        {
            transform.Translate(direction);
            return true;
        }
    }



        public bool ObjToBlocked(Vector3 position,Vector2 direction)
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
            return true;
            }

        }
        return false;
    }

}