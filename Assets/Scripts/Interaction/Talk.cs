using UnityEngine;

public class Talk : MonoBehaviour, IInteractable
{
    [Header("Ink Dialog")]
    public InkDialogController dialogController;
    public TextAsset inkJSON;     // opcjonalnie: własny plik dialogu dla NPC

    [Header("NPC ID")]
    public int idNPC;

    public string GetDescription()
    {
        if (dialogController != null && dialogController.IsDialogActive)
            return "";

        return "Talk";
    }

    public bool CanInteract()
    {
        // jeśli dialog aktywny → NIE pokazuj "E Talk"
        if (dialogController != null && dialogController.IsDialogActive)
            return false;

        return true;
    }

    public void Interact()
    {
        // -- zapisywanie pozycji gracza (jak wcześniej) --
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Transform camT = player.GetComponentInChildren<Camera>()?.transform;
            PlayerPositionManager.Instance.SavePlayerTransform(player.transform, camT);

            NPCPatrol patrol = GetComponent<NPCPatrol>();
            if (patrol != null)
                patrol.SaveState();

            Debug.Log("[NPCInteraction] Zapisano pozycję i rotację gracza oraz NPC.");
        }

        Debug.Log("[Talk] Interakcja – uruchamiam dialog INK.");

        // ---------- NOWOŚĆ: start dialogu INK ----------
        if (dialogController != null)
        {
            // 🔥 Używamy nowej metody SetupNPC do bezpiecznej zmiany ID i czyszczenia
            string npcId = $"npc_{idNPC}";
            dialogController.SetupNPC(npcId, dialogController.clearOnGameStart);

            if (inkJSON != null)
                dialogController.StartDialog(inkJSON);
            else
                dialogController.StartDialog();
        }
        else
        {
            Debug.LogWarning("[Talk] Brak przypisanego InkDialogController!");
        }
    }
}
