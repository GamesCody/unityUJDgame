using UnityEngine;

[RequireComponent(typeof(Collider))]
public class QuestTrigger : MonoBehaviour
{
    [Header("Ink")]
    public string inkVariableName = "hasNotebook";
    public bool variableValue = true; // wartość zmiennej w Ink
    public bool triggerOnce = true;

    [Header("Dialog Controller")]
    public InkDialogController dialogController;
    [Header("Dialog Identity")]
    public string dialogId = ""; // optional: if set, will be applied to dialogController before setting variable

    private bool triggered = false;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null)
            col.isTrigger = true;
    }

    void Start()
    {
        if (dialogController == null)
            dialogController = FindObjectOfType<InkDialogController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (dialogController == null)
            dialogController = FindObjectOfType<InkDialogController>();

        // tylko ustaw zmienną, nie uruchamiaj dialogu
        if (dialogController != null)
        {
            if (!string.IsNullOrEmpty(dialogId))
                dialogController.dialogId = dialogId;

            dialogController.SetInkVariable(inkVariableName, variableValue);
        }

        if (triggerOnce)
            this.enabled = false;
    }
}
