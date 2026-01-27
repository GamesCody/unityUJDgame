using UnityEngine;
using UnityEngine.SceneManagement;
using Ink.Runtime;
using System.Reflection;
using System.Collections.Generic;

/// <summary>
/// BossFight script monitors quest completion variables from different NPCs
/// and triggers a special dialog + scene change when the condition is met.
/// Attach this to an object or use it as a manager for coordinating boss fight transitions.
/// </summary>
public class BossFight : MonoBehaviour

{
    public InkDialogController dialog;
    private bool hasTriggered = false;
    private bool hasSetProfessorQuest = false;
    private bool hasTriggeredBossFight = false;

    [System.Serializable]
    private class InkVariableEntry { public string name; public string type; public string value; }
    [System.Serializable]
    private class InkVariableCollection { public List<InkVariableEntry> variables = new List<InkVariableEntry>(); }

    void Start()
    {
        if (dialog == null)
            dialog = FindObjectOfType<InkDialogController>();
    }
    
    void Update()
    {
        // 🔥 UWAGA: Logika ładowania sceny boss fight'u przeniesiona do InkVariableTransfer.cs
        // InkVariableTransfer.cs prawidłowo obsługuje:
        // 1. Transfer zmiennych między NPC
        // 2. Ładowanie BattleScene
        // 3. Odblokowywanie kontroli gracza przed zmianą sceny
        // Wszystkie te rzeczy teraz obsługuje InkVariableTransfer, więc BossFight może być deaktywny
        
        // Kod poniżej jest pozostawiony dla referencji, ale został wyłączony:
        /*
        if (dialog == null) return;

        // 🔥 LOGIKA PRZEKAZANIA QUESTA DO PROFESORA
        if (!hasSetProfessorQuest)
        {
            bool koniecQuestMacka = InkVariableTransfer.Q1; // Używaj Q1 zamiast ReadVariableBool
            
            if (koniecQuestMacka)
            {
                InkVariableTransfer.TransferVariableBool(
                    QuestVariables.MACIEK_NPC_ID, 
                    QuestVariables.MACIEK_QUEST_COMPLETE,
                    QuestVariables.PROFESOR_NPC_ID, 
                    QuestVariables.PROFESOR_EXAM_UNLOCKED);
                Debug.Log("[BossFight] ✅ Transfer: koniecQuestMacka → poprawkaZGrafow");
                hasSetProfessorQuest = true;
            }
        }

        // 🔥 LOGIKA ZMIANY SCENY
        if (!hasTriggeredBossFight)
        {
            bool walkaBoss1 = InkVariableTransfer.Q3; // Używaj Q3 zamiast ReadVariableBool
            
            if (walkaBoss1)
            {
                Debug.Log("[BossFight] walkaBoss1 = true! Zmiana sceny na boss fight...");
                hasTriggeredBossFight = true;
                SceneManager.LoadScene(QuestVariables.SCENE_BOSS_FIGHT);
            }
        }
        */
    }

    private void SaveToGlobalPlayerPrefs(string varName, bool value)
    {
        string globalKey = "InkVariables_global_v1";
        string varJson = "{\"variables\":[{\"name\":\"" + varName + "\",\"type\":\"bool\",\"value\":\"" + (value ? "true" : "false") + "\"}]}";
        PlayerPrefs.SetString(globalKey, varJson);
        PlayerPrefs.Save();
    }

    private bool ReadBoolFromPrefs(string varName)
    {
        // Check global key
        string globalKey = "InkVariables_global_v1";
        if (PlayerPrefs.HasKey(globalKey))
        {
            var json = PlayerPrefs.GetString(globalKey);
            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    var coll = JsonUtility.FromJson<InkVariableCollection>(json);
                    if (coll != null && coll.variables != null)
                    {
                        foreach (var e in coll.variables)
                            if (e.name == varName) return ParseBoolString(e.value);
                    }
                }
                catch {}
            }
        }

        // Check NPC specific keys (npc_1..npc_50)
        for (int i = 1; i <= 50; i++)
        {
            string key = $"InkVariables_npc_{i}_v1";
            if (!PlayerPrefs.HasKey(key)) continue;
            var json = PlayerPrefs.GetString(key);
            if (string.IsNullOrEmpty(json)) continue;
            try
            {
                var coll = JsonUtility.FromJson<InkVariableCollection>(json);
                if (coll != null && coll.variables != null)
                {
                    foreach (var e in coll.variables)
                        if (e.name == varName) return ParseBoolString(e.value);
                }
            }
            catch {}
        }

        // Check current dialog-specific key
        if (dialog != null)
        {
            string ownKey = $"InkVariables_{dialog.dialogId}_v1";
            if (PlayerPrefs.HasKey(ownKey))
            {
                var json = PlayerPrefs.GetString(ownKey);
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var coll = JsonUtility.FromJson<InkVariableCollection>(json);
                        if (coll != null && coll.variables != null)
                        {
                            foreach (var e in coll.variables)
                                if (e.name == varName) return ParseBoolString(e.value);
                        }
                    }
                    catch {}
                }
            }
        }

        return false;
    }

    private bool ParseBoolString(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        s = s.ToLower();
        return s == "true" || s == "1";
    }

    // 🔥 Metoda pomocnicza do pobierania Story obiektu
    private Story GetStoryFromDialog(InkDialogController dialogController)
    {
        var storyField = typeof(InkDialogController).GetField("story", BindingFlags.NonPublic | BindingFlags.Instance);
        if (storyField != null)
            return storyField.GetValue(dialogController) as Story;
        return null;
    }

    // 🔥 Generyczna metoda do pobierania zmiennych z Story
    private T GetVar<T>(Story story, string name)
    {
        try
        {
            if (story.variablesState.GlobalVariableExistsWithName(name))
                return (T)story.variablesState[name];
        }
        catch { }
        return default;
    }

    // 🔥 Metoda do aktualizacji zmiennej Profesora w PlayerPrefs
    private void UpdateProfessorVariable(string profesorId, string varName, string value)
    {
        Debug.Log($"[BossFight] Ustawianie {varName} = {value} dla profesora ({profesorId})...");
        
        string profesorKey = $"InkVariables_{profesorId}_v1";
        string profesorJson = PlayerPrefs.GetString(profesorKey, "");
        InkVariableCollection profesorColl = null;
        
        if (!string.IsNullOrEmpty(profesorJson))
        {
            try
            {
                profesorColl = JsonUtility.FromJson<InkVariableCollection>(profesorJson);
            }
            catch { }
        }
        
        if (profesorColl == null)
            profesorColl = new InkVariableCollection();
        
        // Usuń starą wartość
        profesorColl.variables.RemoveAll(v => v.name == varName);
        
        // Dodaj nową
        profesorColl.variables.Add(new InkVariableEntry { name = varName, type = "bool", value = value });
        
        // Zapisz
        string json = JsonUtility.ToJson(profesorColl);
        PlayerPrefs.SetString(profesorKey, json);
        PlayerPrefs.Save();
        
        Debug.Log($"[BossFight] ✅ {varName}={value} zapisana do PlayerPrefs dla profesora ({profesorId})");
    }
}
