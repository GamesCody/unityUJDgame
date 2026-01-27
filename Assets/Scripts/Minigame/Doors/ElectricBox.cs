using UnityEngine;

// Attach this to a GameObject with a Collider (2D or 3D) set as Trigger.
// When an object tagged "Player" enters the trigger, logs "wygrales".
public class ElectricBox : MonoBehaviour
{
    [SerializeField] private string elecTag = "ElectricBox";
    public int idElecBox;
    public bool isActive = false;

    void Reset()
    {
        // Try to mark existing collider as trigger so it's ready by default in the editor
        var col2D = GetComponent<Collider2D>();
        if (col2D != null)
            col2D.isTrigger = true;

    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other != null && other.CompareTag(elecTag))
        {
            isActive = true;
        }
    }

}
