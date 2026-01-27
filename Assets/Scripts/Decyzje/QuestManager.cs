using UnityEngine;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    public InkDialogController dialog;

    [Header("Quest variables required")]
    public List<string> requiredQuests;

    private HashSet<string> completed = new HashSet<string>();

    public event System.Action OnAllQuestsCompleted;

    void Awake()
    {
        if (dialog == null)
            dialog = FindObjectOfType<InkDialogController>();
    }

    void OnEnable()
    {
        //dialog.OnInkVariableChanged += OnInkVarChanged;
    }

    void OnDisable()
    {
       // dialog.OnInkVariableChanged -= OnInkVarChanged;
    }

    void OnInkVarChanged(string name, object value)
    {
        if (!(value is bool b)) return;
        if (!requiredQuests.Contains(name)) return;

        if (b)
            completed.Add(name);
        else
            completed.Remove(name);

        if (completed.Count == requiredQuests.Count)
        {
            Debug.Log("[QuestManager] All quests completed");
            OnAllQuestsCompleted?.Invoke();
        }
    }
}
