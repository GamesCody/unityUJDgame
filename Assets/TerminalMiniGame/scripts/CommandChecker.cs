using UnityEngine;
using TMPro;

public class CommandChecker : MonoBehaviour
{
    public string correctLinuxCommand; // Tutaj wpiszesz poprawną odpowiedź w edytorze
    private TMP_InputField inputField;
    private bool isCorrect = false; // 🔥 Flaga czy odpowiedź jest prawidłowa

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
    }

    public void Verify()
    {
        if (inputField.text == correctLinuxCommand)
        {
            inputField.textComponent.color = Color.green; // Zmienia tekst na zielony
            isCorrect = true; // 🔥 Ustaw flagę na true
            Debug.Log("Dobrze!");
        }
        else
        {
            inputField.textComponent.color = Color.red; // Zmienia tekst na czerwony
            isCorrect = false; // 🔥 Ustaw flagę na false
            Debug.Log("Źle!");
        }
    }

    // 🔥 Metoda sprawdzająca czy odpowiedź jest prawidłowa
    public bool IsCorrect()
    {
        return isCorrect;
    }
}