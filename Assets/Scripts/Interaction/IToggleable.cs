using UnityEngine;

public interface IToggleable
{
    void Toggle();
    string GetToggleDescription();
    bool IsActive();
}
