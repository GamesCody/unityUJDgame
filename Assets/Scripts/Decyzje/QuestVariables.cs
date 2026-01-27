using UnityEngine;

/// <summary>
/// Centralne miejsce do definiowania wszystkich zmiennych quest'ów
/// Używaj tych stałych zamiast magic strings
/// </summary>
public static class QuestVariables
{
    // ===== NPC IDs =====
    public const string MACIEK_NPC_ID = "npc_123";
    public const string PROFESOR_NPC_ID = "npc_1234";

    // ===== MACIEK QUEST - VARIABLES =====
    public const string MACIEK_LAPTOP_RETURNED = "laptopReturned";
    public const string MACIEK_WAITING = "maciekWait";
    public const string MACIEK_QUEST_COMPLETE = "koniecQuestMacka";

    // ===== PROFESOR QUEST - VARIABLES =====
    public const string PROFESOR_EXAM_UNLOCKED = "poprawkaZGrafow";
    public const string PROFESOR_BOSS_FIGHT_TRIGGERED = "walkaBoss1";

    // ===== SCENE NAMES =====
    public const string SCENE_BOSS_FIGHT = "BattleScene";

    // ===== QUEST TAGS (dla ProcessTags w Ink) =====
    public const string TAG_MACIEK_QUEST_COMPLETE = "maciekQuestComplete";
    public const string TAG_PROFESOR_EXAM_STARTED = "profesorExamStarted";

    // ===== HELPER METHODS =====

    /// <summary>
    /// Zwraca listę wszystkich zmiennych quest'ów
    /// </summary>
    public static string[] GetAllQuestVariables()
    {
        return new string[]
        {
            MACIEK_QUEST_COMPLETE,
            PROFESOR_EXAM_UNLOCKED,
            PROFESOR_BOSS_FIGHT_TRIGGERED
        };
    }

    /// <summary>
    /// Zwraca opis zmiennej dla debug'owania
    /// </summary>
    public static string GetVariableDescription(string varName)
    {
        return varName switch
        {
            MACIEK_LAPTOP_RETURNED => "Laptop zwrócony do Maćka",
            MACIEK_WAITING => "Maciek czeka na pomoc",
            MACIEK_QUEST_COMPLETE => "Quest Maćka ukończony",
            PROFESOR_EXAM_UNLOCKED => "Egzamin u Profesora dostępny",
            PROFESOR_BOSS_FIGHT_TRIGGERED => "Boss fight aktywowany",
            _ => "Nieznana zmienna"
        };
    }
}
