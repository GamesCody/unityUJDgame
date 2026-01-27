using UnityEngine;

public class InstructionManager : MonoBehaviour
{

    public GameObject fullOverlay; 

    public void Open()
    {
        fullOverlay.SetActive(true); // Włącza tło + instrukcję + przycisk zamknij
    }

    public void Close()
    {
        fullOverlay.SetActive(false); // Ukrywa wszystko naraz
    }
}