using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SzymekBossFight : MonoBehaviour
{
    [SerializeField] private InkDialogController dialogController;
    [SerializeField] private TextAsset szymekInkAsset;
    [SerializeField] private string gameSceneName = "Papier";

    [Header("Obiekty do aktywacji / dezaktywacji")]
    [SerializeField] private GameObject[] objectsToActivate;
    [SerializeField] private GameObject[] objectsToDeactivate;

    [HideInInspector] public bool ma_zapalki = false;

    private bool hasStartedDialog = false;
    private bool lastMaZapalki = false;
    private static bool sceneLoadedThisSession = false;

    void Start()
    {
        if (dialogController == null)
            dialogController = FindObjectOfType<InkDialogController>();

        if (dialogController != null)
            InkDialogController.OnDialogEnded += OnDialogFinished;

        // 🔍 Sprawdź czy gracz wygrał boss fight (powrót ze sceny Papier)
        CheckBossFightStatus();
    }

    void OnDestroy()
    {
        if (dialogController != null)
            InkDialogController.OnDialogEnded -= OnDialogFinished;
    }

    void Update()
    {
        if (!hasStartedDialog || dialogController == null)
            return;

        if (!dialogController.IsDialogActive)
        {
            try
            {
                ma_zapalki = dialogController.GetInkVariable<bool>("ma_zapalki");
            }
            catch
            {
                ma_zapalki = false;
            }

            hasStartedDialog = false;

            if (ma_zapalki && !sceneLoadedThisSession)
            {
                sceneLoadedThisSession = true;
                ApplyObjectVisibility(true);
                StartCoroutine(LoadGameSceneWithDelay());
            }
        }
    }

    public void StartDialogWithSzymek()
    {
        if (hasStartedDialog)
            return;

        if (dialogController == null)
            dialogController = FindObjectOfType<InkDialogController>();

        if (dialogController != null && szymekInkAsset != null)
        {
            dialogController.StartDialog(szymekInkAsset);
            hasStartedDialog = true;
        }
    }

    private void OnDialogFinished()
    {
        bool newMaZapalki;

        try
        {
            newMaZapalki = dialogController.GetInkVariable<bool>("ma_zapalki");
        }
        catch
        {
            return;
        }

        if (newMaZapalki && !lastMaZapalki && !sceneLoadedThisSession)
        {
            sceneLoadedThisSession = true;
            
            // 💾 ZAPISZ DO GSM ŻE WYGRALIŚMY - zanim zmienisz scenę
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetPlayerWonPapierKamienNozyce(true);
                Debug.Log("[SzymekBossFight] 💾 Zapisano do GSM: playerWonPapierKamienNozyce = true");
            }
            
            StartCoroutine(LoadGameSceneWithDelay());
        }

        lastMaZapalki = newMaZapalki;
    }
    
    /// <summary>
    /// Sprawdza czy gracz wygrał boss fight (po powrocie ze sceny Papier)
    /// </summary>
    private void CheckBossFightStatus()
    {
        if (GameStateManager.Instance != null && GameStateManager.Instance.GetPlayerWonPapierKamienNozyce())
        {
            Debug.Log("[SzymekBossFight] ✅ Gracz wygrał! Aktywuję obiekty...");
            ApplyObjectVisibility(true);
            
            // 🔄 Reset flagi aby się nie powtarzała
            GameStateManager.Instance.SetPlayerWonPapierKamienNozyce(false);
        }
        else
        {
            Debug.Log("[SzymekBossFight] ❌ Gracz nie wygrał. Stan normalny.");
            ApplyObjectVisibility(false);
        }
    }

    private void ApplyObjectVisibility(bool hide)
    {
        if (!hide)
        {
            // STAN NORMALNY - Szymek widoczny, Daniel/Karol uśpieni
            if (objectsToDeactivate != null)
            {
                foreach (var obj in objectsToDeactivate)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"[SzymekBossFight] ✅ {obj.name} SetActive(true)");
                    }
                }
            }
            
            if (objectsToActivate != null)
            {
                foreach (var obj in objectsToActivate)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"[SzymekBossFight] ❌ {obj.name} SetActive(false)");
                    }
                }
            }
        }
        else
        {
            // PO WYGRANIU - Szymek uśpiony, Daniel/Karol widoczni
            if (objectsToDeactivate != null)
            {
                foreach (var obj in objectsToDeactivate)
                {
                    if (obj != null)
                    {
                        obj.SetActive(false);
                        Debug.Log($"[SzymekBossFight] ❌ {obj.name} SetActive(false) [PO WYGRANIU]");
                    }
                }
            }
            
            if (objectsToActivate != null)
            {
                foreach (var obj in objectsToActivate)
                {
                    if (obj != null)
                    {
                        obj.SetActive(true);
                        Debug.Log($"[SzymekBossFight] ✅ {obj.name} SetActive(true) [PO WYGRANIU]");
                    }
                }
            }
        }
    }

    private IEnumerator LoadGameSceneWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene(gameSceneName);
    }

    public void ResetBossFight()
    {
        hasStartedDialog = false;
        ma_zapalki = false;
        lastMaZapalki = false;
        sceneLoadedThisSession = false;
        ApplyObjectVisibility(false);
    }
}
