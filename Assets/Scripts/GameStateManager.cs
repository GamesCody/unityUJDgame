using System.Collections.Generic;

using UnityEngine;

using UnityEngine.SceneManagement;



[System.Serializable]

public struct ObjectState

{

    public string ID;

    public bool Active;

}



public class GameStateManager : MonoBehaviour

{

    public static GameStateManager Instance;

   

    [Header("Opcjonalne ustawienia")]

    [Tooltip("Jeśli puste, manager działa dla każdej sceny. Jeśli ustawione, działa tylko dla tej sceny.")]

    public string targetSceneName = "";



    [Header("Lista ID + stan")]

    public List<ObjectState> objectStates = new List<ObjectState>();

    // 🔥 STAN WŁAMANYCH DRZWI - zapamiętywanie które drzwi zostały już włamane
    private System.Collections.Generic.Dictionary<string, bool> breakableDoorStates = new System.Collections.Generic.Dictionary<string, bool>();
    
    // 🔥 HEALTHVALUE Z MINIGRY - przechowywanie wartości zdrowia z minigry
    private int playerHealth = 0;
    
    // 🎮 PAPIER KAMIEŃ NOŻYCE - informacja o wygranej gracza
    private bool playerWonPapierKamienNozyce = false;
    
    // 🔥 OBIEKTY ZALEŻNE OD QUESTÓW - będą zarządzane na podstawie postępu
    private GameObject[] questDependentObjectsToDeactivate;
    private GameObject[] questDependentObjectsToActivate;



    void Awake()

    {

        if (Instance == null)

        {

            Instance = this;

            DontDestroyOnLoad(gameObject);

            SceneManager.sceneLoaded += OnSceneLoaded;

            Debug.Log("[GSM] Awake - registered and will persist across scenes");

        }

        else

        {

            Destroy(gameObject);

        }

    }



    void OnDestroy()

    {

        SceneManager.sceneLoaded -= OnSceneLoaded;

    }



    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)

    {

        if (!string.IsNullOrEmpty(targetSceneName) && scene.name != targetSceneName)

        {

            Debug.Log($"[GSM] Scene '{scene.name}' ignored (target='{targetSceneName}').");

            return;

        }



        Debug.Log($"[GSM] Scene loaded: {scene.name} - starting RestoreStates");

        // odczekaj jedną klatkę, żeby scena była w pełni załadowana

        StartCoroutine(RestoreStates());

    }



    private System.Collections.IEnumerator RestoreStates()

    {

        Debug.Log("[GSM] RestoreStates coroutine started");

        yield return null; // czekamy jedną klatkę



        Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();



        // bierzemy rooty sceny

        GameObject[] roots = currentScene.GetRootGameObjects();



        // zamiast ręcznej rekurencji, korzystamy z GetComponentsInChildren(true)

        // aby znaleźć także nieaktywne obiekty

        foreach (var root in roots)

        {

            StatefulObject[] statefuls = root.GetComponentsInChildren<StatefulObject>(true);

            foreach (var so in statefuls)

            {

                int index = objectStates.FindIndex(s => s.ID == so.ID);

                if (index >= 0)

                {

                    so.gameObject.SetActive(objectStates[index].Active);

                    Debug.Log($"[GSM] {so.ID} -> {objectStates[index].Active}");

                }

                else

                {

                    objectStates.Add(new ObjectState { ID = so.ID, Active = so.gameObject.activeSelf });

                    Debug.Log($"[GSM] Added {so.ID} -> {so.gameObject.activeSelf}");

                }

            }

        }

    }



    private void ProcessHierarchy(Transform t)

    {

        // metoda zastąpiona — używamy GetComponentsInChildren(true) w RestoreStates

        return;

    }



    // do zmiany stanu w trakcie gry

    public void SetState(string id, bool active)

    {

        int index = objectStates.FindIndex(s => s.ID == id);

        if (index >= 0)

        {

            objectStates[index] = new ObjectState { ID = id, Active = active };

        }

        else

        {

            objectStates.Add(new ObjectState { ID = id, Active = active });

        }

    }
    
    /// <summary>
    /// Zapisuje stan GameObjectu do listy objectStates używając jego nazwy jako ID
    /// </summary>
    public void SaveObjectState(GameObject obj, bool activeState)
    {
        if (obj == null) return;
        
        string objectID = obj.name;
        SetState(objectID, activeState);
        Debug.Log($"[GSM] 💾 SaveObjectState: {objectID} = {activeState}");
    }

    // 🔥 ZAPISZ STAN WŁAMANYCH DRZWI

    public void SetBreakableDoorState(string doorID, bool hasBeenBroken)

    {

        breakableDoorStates[doorID] = hasBeenBroken;

        Debug.Log($"[GSM] Zapisano stan włamanych drzwi: {doorID} = {hasBeenBroken}");

    }

    // 🔥 ODCZYTAJ STAN WŁAMANYCH DRZWI

    public bool GetBreakableDoorState(string doorID)

    {

        if (breakableDoorStates.ContainsKey(doorID))

        {

            return breakableDoorStates[doorID];

        }

        return false; // Domyślnie drzwi nie są włamane

    }

    // 🔥 ZAPISZ HEALTHVALUE Z MINIGRY

    public void SetPlayerHealth(int health)

    {

        playerHealth = health;

        Debug.Log($"[GSM] Zapisano playerHealth: {health}");

    }

    // 🔥 ODCZYTAJ HEALTHVALUE Z MINIGRY

    public int GetPlayerHealth()

    {

        return playerHealth;

    }

    // 🎮 ZAPISZ STAN WYGRANEJ W PAPIER KAMIEŃ NOŻYCE

    public void SetPlayerWonPapierKamienNozyce(bool won)

    {

        playerWonPapierKamienNozyce = won;

        Debug.Log($"[GSM] Zapisano playerWonPapierKamienNozyce: {won}");

    }

    // 🎮 ODCZYTAJ STAN WYGRANEJ W PAPIER KAMIEŃ NOŻYCE

    public bool GetPlayerWonPapierKamienNozyce()

    {

        return playerWonPapierKamienNozyce;

    }

    // 🔥 USTAW OBIEKTY ZALEŻNE OD QUESTÓW

    /// <summary>

    /// Rejestruje obiekty które będą zarządzane na podstawie postępu questów

    /// </summary>

    public void SetQuestDependentObjects(GameObject[] toDeactivate, GameObject[] toActivate)

    {

        questDependentObjectsToDeactivate = toDeactivate;

        questDependentObjectsToActivate = toActivate;

        Debug.Log($"[GSM] Zarejestrowano obiekty zależne od questów: deactivate={toDeactivate?.Length ?? 0}, activate={toActivate?.Length ?? 0}");

        

        // Natychmiast zastosuj warunki

        ApplyQuestRequirements();

    }

    

    // 🔥 SPRAWDZENIE I ZASTOSOWANIE WARUNKÓW QUESTÓW

    /// <summary>

    /// Deaktywuje/aktywuje obiekty na podstawie postępu questów

    /// </summary>

    public void ApplyQuestRequirements()

    {

        // Pobierz InkDialogController

        InkDialogController dialogController = FindObjectOfType<InkDialogController>();

        if (dialogController == null)

        {

            Debug.LogWarning("[GSM] InkDialogController nie znaleziony!");

            return;

        }

        

        try

        {

            bool czyZnaPlan = dialogController.GetInkVariable<bool>("czy_zna_plan");

            int liczbaSwiec = dialogController.GetInkVariable<int>("liczba_swiec");

            

            Debug.Log($"[GSM] ApplyQuestRequirements: czy_zna_plan={czyZnaPlan}, liczba_swiec={liczbaSwiec}");

            

            bool questsCompleted = czyZnaPlan && liczbaSwiec >= 3;
            
            // objectsToDeactivate - WYŁĄCZONE
            if (questDependentObjectsToDeactivate != null)
            {
                foreach (var obj in questDependentObjectsToDeactivate)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"[GSM] ❌ SetActive(false): {obj.name}");
                    }
                }
            }
            
            // objectsToActivate - WŁĄCZONE
            if (questDependentObjectsToActivate != null)
            {
                foreach (var obj in questDependentObjectsToActivate)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"[GSM] ✅ SetActive(true): {obj.name}");
                    }
                }
            }

        }

        catch (System.Exception ex)

        {

            Debug.LogWarning($"[GSM] Błąd w ApplyQuestRequirements: {ex.Message}");

        }

    }

    

    // 🔥 ZASTOSOWANIE STANU WYGRANEJ W MINIGRE

    /// <summary>

    /// Deaktywuje obiekty z tabeli deactivate i aktywuje z tabeli activate

    /// </summary>

    public void ApplyWinState()

    {

        Debug.Log("═══════════════════════════════════════════════════════");

        Debug.Log("[GSM] 🔥 APPLY WIN STATE - Aktywuję/Dezaktywuję obiekty");

        Debug.Log("═══════════════════════════════════════════════════════");

        

        if (questDependentObjectsToDeactivate != null)

        {

            foreach (var obj in questDependentObjectsToDeactivate)

            {

                if (obj != null)

                {

                    obj.SetActive(false);

                    var statefulObj = obj.GetComponent<StatefulObject>();

                    if (statefulObj != null)

                        statefulObj.enabled = false;

                    Debug.Log($"[GSM] ❌ Deaktywowano (win): {obj.name}");

                }

            }

        }

        

        if (questDependentObjectsToActivate != null)

        {

            foreach (var obj in questDependentObjectsToActivate)

            {

                if (obj != null)

                {

                    obj.SetActive(true);

                    Debug.Log($"[GSM] ✅ AKTYWUJĘ (win): {obj.name}");

                }

            }

        }

        

        // Resetuj flagę

        playerWonPapierKamienNozyce = false;

        Debug.Log("[GSM] 🔄 Resetuję playerWonPapierKamienNozyce na false");

    }

}