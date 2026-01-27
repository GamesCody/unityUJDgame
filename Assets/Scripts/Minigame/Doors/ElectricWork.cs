using UnityEngine;

// ElectricWork: checks if there's a connection to an ElectricBox with matching id.
// When the matching ElectricBox is active (isActive == true) it logs the connection.
public class ElectricWork : MonoBehaviour
{
    public ElectricBox elecBox;
    public int idElec;



    void Start()
    {
        // If no ElectricBox assigned in inspector, try to find one with matching id
        if (elecBox == null)
        {
            var boxes = FindObjectsOfType<ElectricBox>();
            foreach (var b in boxes)
            {
                if (b != null && b.idElecBox == idElec)
                {
                    elecBox = b;
                    break;
                }
            }
        }
    }

    // Returns true when the target ElectricBox exists and is active
    public bool Polonczenie()
    {
        return elecBox != null && elecBox.idElecBox == idElec && elecBox.isActive;
    }

    void Update()
    {
        bool nowConnected = Polonczenie();
        if (nowConnected)
        {
            Debug.Log("polaczenie nawiazanie");
            Destroy(gameObject);
        }

    }
}
