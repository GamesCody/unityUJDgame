using UnityEngine;



public class StatefulObject : MonoBehaviour

{

    [SerializeField] private string uniqueID;

    public string ID => uniqueID; // manager odczytuje ID

}