using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPositionManager : MonoBehaviour
{
    public static PlayerPositionManager Instance;

    public Vector3 savedPosition;
    public Quaternion savedRotation;
    public Quaternion savedCameraLocalRotation;
    public bool hasSavedTransform = false;

    [System.Serializable]
    public class DoorState
    {
        public bool isOpen;
        public Quaternion rotation;
    }

    private System.Collections.Generic.Dictionary<string, DoorState> doorStates = new System.Collections.Generic.Dictionary<string, DoorState>();

    [System.Serializable]
    public class NPCState
    {
        public Vector3 position;
        public Quaternion rotation;
    }

    private System.Collections.Generic.Dictionary<string, NPCState> npcStates = new System.Collections.Generic.Dictionary<string, NPCState>();

    [System.Serializable]
    public class ObjectState
    {
        public Vector3 position;
        public Quaternion rotation;
        public bool isActive;
    }

    private System.Collections.Generic.Dictionary<string, ObjectState> objectStates = new System.Collections.Generic.Dictionary<string, ObjectState>();

    // 🔥 LISTA OBIEKTÓW DO AUTOMATYCZNEGO ŚLEDZENIA
    [System.Serializable]
    public class TrackedObject
    {
        public string objectID;      // Unikatowy identyfikator
        public string gameObjectName; // 🔥 NAZWA obiektu zamiast referencji
    }

    [SerializeField] private System.Collections.Generic.List<TrackedObject> trackedObjects = new System.Collections.Generic.List<TrackedObject>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    public void SavePlayerTransform(Transform playerTransform, Transform cameraTransform = null)
    {
        savedPosition = playerTransform.position;
        savedRotation = playerTransform.rotation;
        savedCameraLocalRotation = cameraTransform != null ? cameraTransform.localRotation : Quaternion.identity;
        hasSavedTransform = true;

        Debug.Log($"[PlayerPositionManager] Zapisano: pos={savedPosition}, rot={savedRotation.eulerAngles}, camRot={savedCameraLocalRotation.eulerAngles}");
    }

    /// <summary>
    /// Szybka metoda do zapisu pozycji gracza - szuka gracza po tagu i zapisuje jego pozycję
    /// </summary>
    public void SavePlayerPositionNow()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Transform playerTransform = player.transform;
            Transform cameraTransform = playerTransform.Find("Camera") ?? 
                                       playerTransform.Find("FirstPersonCamera") ?? 
                                       playerTransform.Find("Head") ?? 
                                       playerTransform.Find("CameraHolder");
            
            SavePlayerTransform(playerTransform, cameraTransform);
            Debug.Log($"[PlayerPositionManager] Zapisano aktualną pozycję gracza");
        }
        else
        {
            Debug.LogWarning("[PlayerPositionManager] Nie znaleziono gracza z tagiem 'Player'");
        }
    }

    public void SaveDoorState(string doorID, bool isOpen, Transform doorTransform)
    {
        doorStates[doorID] = new DoorState { isOpen = isOpen, rotation = doorTransform.rotation };
        Debug.Log($"[PlayerPositionManager] Zapisano drzwi {doorID}: open={isOpen}, rot={doorTransform.rotation.eulerAngles}");
    }

    public bool RestoreDoorState(string doorID, IDoorState door)
    {
        if (doorStates.ContainsKey(doorID))
        {
            var state = doorStates[doorID];
            if (door != null)
            {
                door.SetDoorState(state.isOpen, state.rotation);
                Debug.Log($"[PlayerPositionManager] Przywrócono drzwi {doorID}: open={state.isOpen}, rot={state.rotation.eulerAngles}");
                return true;
            }
            else
            {
                Debug.LogWarning($"[PlayerPositionManager] Nie można przywrócić drzwi {doorID}: przekazany obiekt jest null.");
            }
        }

        return false;
    }

    public void SaveNPCTransform(string npcID, Transform npcTransform)
    {
        npcStates[npcID] = new NPCState { position = npcTransform.position, rotation = npcTransform.rotation };
        Debug.Log($"[PlayerPositionManager] Zapisano NPC {npcID}: pos={npcTransform.position}, rot={npcTransform.rotation.eulerAngles}");
    }

    public void RestoreNPCTransform(string npcID, Transform npcTransform)
    {
        if (npcStates.ContainsKey(npcID))
        {
            var state = npcStates[npcID];
            npcTransform.position = state.position;
            npcTransform.rotation = state.rotation;
            Debug.Log($"[PlayerPositionManager] Przywrócono NPC {npcID}: pos={state.position}, rot={state.rotation.eulerAngles}");
        }
    }

    // 🔥 ZAPISZ STAN OBIEKTU
    public void SaveObjectState(string objectID, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning($"[PlayerPositionManager] Próba zapisu obiektu {objectID}, ale obiekt jest null!");
            return;
        }

        objectStates[objectID] = new ObjectState 
        { 
            position = obj.transform.position, 
            rotation = obj.transform.rotation,
            isActive = obj.activeSelf
        };
        Debug.Log($"[PlayerPositionManager] Zapisano obiekt {objectID}: pos={obj.transform.position}, rot={obj.transform.rotation.eulerAngles}, active={obj.activeSelf}");
    }

    // 🔥 PRZYWRÓĆ STAN OBIEKTU
    public void RestoreObjectState(string objectID, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogWarning($"[PlayerPositionManager] Próba przywrócenia obiektu {objectID}, ale obiekt jest null!");
            return;
        }

        if (objectStates.ContainsKey(objectID))
        {
            var state = objectStates[objectID];
            obj.transform.position = state.position;
            obj.transform.rotation = state.rotation;
            obj.SetActive(state.isActive);
            Debug.Log($"[PlayerPositionManager] Przywrócono obiekt {objectID}: pos={state.position}, rot={state.rotation.eulerAngles}, active={state.isActive}");
        }
        else
        {
            Debug.LogWarning($"[PlayerPositionManager] Nie znaleziono zapisu dla obiektu {objectID}");
        }
    }

    // 🔥 AUTOMATYCZNE ZAPISYWANIE OBIEKTÓW Z LISTY
    public void SaveAllTrackedObjects()
    {
        if (trackedObjects == null || trackedObjects.Count == 0)
        {
            Debug.LogWarning("[PlayerPositionManager] Brak obiektów do śledzenia na liście!");
            return;
        }

        foreach (var tracked in trackedObjects)
        {
            // 🔥 Szukaj obiektu po nazwie w scenie
            GameObject obj = GameObject.Find(tracked.gameObjectName);
            if (obj != null)
            {
                SaveObjectState(tracked.objectID, obj);
            }
            else
            {
                Debug.LogWarning($"[PlayerPositionManager] Nie znaleziono obiektu o nazwie: {tracked.gameObjectName}");
            }
        }
        Debug.Log($"[PlayerPositionManager] Zapisano {trackedObjects.Count} obiektów ze śledzenia");
    }

    // 🔥 AUTOMATYCZNE PRZYWRACANIE OBIEKTÓW Z LISTY
    public void RestoreAllTrackedObjects()
    {
        if (trackedObjects == null || trackedObjects.Count == 0)
        {
            Debug.LogWarning("[PlayerPositionManager] Brak obiektów do przywrócenia na liście!");
            return;
        }

        foreach (var tracked in trackedObjects)
        {
            // 🔥 Szukaj obiektu po nazwie w scenie
            GameObject obj = GameObject.Find(tracked.gameObjectName);
            if (obj != null)
            {
                RestoreObjectState(tracked.objectID, obj);
            }
            else
            {
                Debug.LogWarning($"[PlayerPositionManager] Nie znaleziono obiektu do przywrócenia: {tracked.gameObjectName}");
            }
        }
        Debug.Log($"[PlayerPositionManager] Przywrócono {trackedObjects.Count} obiektów ze śledzenia");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PlayerPositionManager] Załadowano scenę: {scene.name}");
        
        // Czekaj jedną klatkę aby obiekty się załadowały
        StartCoroutine(RestorePlayerTransformDelayed());
    }

    private void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[PlayerPositionManager] Rozładowuję scenę: {scene.name}");
        
        // Zapisz pozycję gracza przed zmianą sceny
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Transform playerTransform = player.transform;
            Transform cameraTransform = playerTransform.Find("Camera") ?? 
                                       playerTransform.Find("FirstPersonCamera") ?? 
                                       playerTransform.Find("Head") ?? 
                                       playerTransform.Find("CameraHolder");
            
            SavePlayerTransform(playerTransform, cameraTransform);
            Debug.Log($"[PlayerPositionManager] Zapisano pozycję gracza przed zmianą sceny");
        }
        else
        {
            Debug.LogWarning("[PlayerPositionManager] Nie znaleziono gracza z tagiem 'Player'");
        }
    }

    private System.Collections.IEnumerator RestorePlayerTransformDelayed()
    {
        // Czekaj jedną klatkę aby scena w pełni się załadowała
        yield return null;
        
        // Jeśli mamy zapisaną pozycję - przywróć ją
        if (hasSavedTransform)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                Transform playerTransform = player.transform;
                playerTransform.position = savedPosition;
                playerTransform.rotation = savedRotation;
                
                // Przywróć rotację kamery jeśli znajdujemy kamerę dziecka
                if (savedCameraLocalRotation != Quaternion.identity)
                {
                    Transform cameraTransform = playerTransform.Find("Camera") ?? 
                                               playerTransform.Find("FirstPersonCamera") ?? 
                                               playerTransform.Find("Head") ?? 
                                               playerTransform.Find("CameraHolder");
                    
                    if (cameraTransform != null)
                    {
                        cameraTransform.localRotation = savedCameraLocalRotation;
                    }
                }
                
                Debug.Log($"[PlayerPositionManager] Przywrócono pozycję gracza: {savedPosition}, rotacja: {savedRotation.eulerAngles}");
            }
            else
            {
                Debug.LogWarning("[PlayerPositionManager] Nie znaleziono gracza do przywrócenia");
            }
        }
    }

    private System.Collections.IEnumerator RestoreTrackedObjectsDelayed()
    {
        // 🔥 Czekaj 1 klatkę aby obiekty się załadowały
        yield return null;
        
        // 🔥 Przywróć wszystkie obiekty z listy
        RestoreAllTrackedObjects();
    }
}
