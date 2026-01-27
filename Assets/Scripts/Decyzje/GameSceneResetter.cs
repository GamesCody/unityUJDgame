using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 🔥 GLOBALNY RESET STANÓW DIALOGU PO KAŻDEJ SCENIE
/// 
/// Ten skrypt resetuje flagi dialogu i odblokuje gracza na starcie każdej sceny.
/// Dodaj go jako pusty GameObject do każdej sceny gameplayowej (szczególnie tej po respawnie).
/// </summary>
public class GameSceneResetter : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("[GameSceneResetter] 🔥 Awake() - HARD RESET PlayerPrefs PRZED wszystkim!");
        
        // 🔥 CZYŚĆ PLAYERPREFS - WSZYSTKIE MOŻLIWE KLUCZE INK!
        // Usuń wszystkie możliwe klucze Ink zmiennych (pętla na wszelki wypadek)
        for (int i = 0; i < 100; i++)
        {
            PlayerPrefs.DeleteKey($"InkVariables_npc_{i}_v1");
            PlayerPrefs.DeleteKey($"InkVariables_{i}_v1");
        }
        
        // Usuń znane klucze NPC po nazwach
        PlayerPrefs.DeleteKey("InkVariables_Daniel_v1");
        PlayerPrefs.DeleteKey("InkVariables_Karol_v1");
        PlayerPrefs.DeleteKey("InkVariables_Szymek_v1");
        PlayerPrefs.DeleteKey("InkVariables_npc_Daniel_v1");
        PlayerPrefs.DeleteKey("InkVariables_global_v1");
        PlayerPrefs.DeleteKey("InkVariables_defaultDialog_v1");
        
        // Usuń liczniki świec
        PlayerPrefs.DeleteKey("candleCount");
        PlayerPrefs.DeleteKey("candlesResetOnScene");
        
        // Usuń inne zmienne stanu
        PlayerPrefs.DeleteKey("czy_zna_plan");
        PlayerPrefs.DeleteKey("liczba_swiec");
        
        PlayerPrefs.Save();
        Debug.Log("[GameSceneResetter] ✅ Wyczyszczono WSZYSTKIE zmienne z PlayerPrefs!");
        
        // 🔥 HARD RESET: Statyczna flaga dialogu musi być FALSE
        InkDialogController.IsAnyDialogActive = false;
        Debug.Log("[GameSceneResetter] ✅ InkDialogController.IsAnyDialogActive = false");

        // 🔥 ODBLOKUJ GRACZA: Włącz wszystkie komponenty sterowania
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Debug.Log("[GameSceneResetter] 🎮 Znaleziono gracza - resetuję komponenty sterowania");
            
            if (player.TryGetComponent<PlayerMovement>(out var movement))
            {
                movement.enabled = true;
                Debug.Log("[GameSceneResetter] ✅ PlayerMovement enabled");
            }
            
            if (player.TryGetComponent<CharacterController>(out var controller))
            {
                controller.enabled = true;
                Debug.Log("[GameSceneResetter] ✅ CharacterController enabled");
            }
            
            if (player.TryGetComponent<PlayerInteraction>(out var interaction))
            {
                interaction.enabled = true;
                Debug.Log("[GameSceneResetter] ✅ PlayerInteraction enabled");
            }
        }
        else
        {
            Debug.LogWarning("[GameSceneResetter] ⚠️ Gracz nie znaleziony w scenie!");
        }

        // 🔥 ODBLOKUJ MYSZ
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        Debug.Log("[GameSceneResetter] ✅ Kursor odblokowany");
        
        // Zmienne Ink zostały już zresetowane w Awake() - tu już tylko sprawdzenie
        Debug.Log("[GameSceneResetter] 🎉 Scena w pełni zresetowana!");
    }
}
