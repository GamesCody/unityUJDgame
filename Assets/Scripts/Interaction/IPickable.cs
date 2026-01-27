using UnityEngine;

public interface IPickable
{
    void Pick();
    string GetPickDescription();
    string GetItemName();
}
