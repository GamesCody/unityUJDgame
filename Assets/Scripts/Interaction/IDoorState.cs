using UnityEngine;

public interface IDoorState
{
    void SetDoorState(bool open, Quaternion rotation);
}
