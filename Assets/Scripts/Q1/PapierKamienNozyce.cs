using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class PapierKamienNozyce : MonoBehaviour
{
    private Animator animator;
    public Animator computerAnimator; // Przypisz drugi Animator w inspektorze
    
    // UI elementy
    public TextMeshProUGUI resultText; // Tekst wyniku (WYGRAŁEŚ/PRZEGRAŁEŚ/REMIS)
    public TextMeshProUGUI choicesText; // Tekst wyborów (Gracz: Papier, Komputer: Kamień)
    public TextMeshProUGUI instructionsText; // Tekst instrukcji (K - Kamień, N - Nożyce, P - Papier)
    public TextMeshProUGUI playerScoreText; // Tekst punktów gracza
    
    // Stany animacji: 0-idle, 1-kamień, 2-nożyce, 3-papier
    private const int IDLE = 0;
    private const int KAMIEN = 1;
    private const int NOZYCE = 2;
    private const int PAPIER = 3;
    
    private int playerChoice = IDLE;
    private int computerChoice = IDLE;
    private bool isGameRunning = false;
    private bool waitingForResult = false;
    
    private int playerScore = 2;
    private int computerScore = 0;
    
    private const int WIN_SCORE = 3; // Gra do 3 punktów

    void Start()
    {
        animator = GetComponent<Animator>();
        
        // Inicjalizacja UI
        if (instructionsText != null)
            instructionsText.text = "K - Kamień | N - Nożyce | P - Papier";
        
        UpdateScoreUI();
        StartNewRound();
    }

    void Update()
    {
        if (!waitingForResult)
        {
            // Naciśnięcia klawiszami (Input System package)
            if (Keyboard.current.kKey.wasPressedThisFrame) // Kamień
                PlayerChoose(KAMIEN);
            else if (Keyboard.current.nKey.wasPressedThisFrame) // Nożyce
                PlayerChoose(NOZYCE);
            else if (Keyboard.current.pKey.wasPressedThisFrame) // Papier
                PlayerChoose(PAPIER);
        }
    }

    public void PlayerChoose(int choice)
    {
        if (waitingForResult) return;

        playerChoice = choice;
        StartCoroutine(PlayRound());
    }

    private IEnumerator PlayRound()
    {
        waitingForResult = true;

        // Animacja gracza
        animator.SetInteger("Stan", playerChoice);
        
        // Losowy wybór komputera
        computerChoice = Random.Range(1, 4);
        
        // Animacja komputera
        if (computerAnimator != null)
            computerAnimator.SetInteger("Stan", computerChoice);

        // Czekanie na animację (dostosuj czas do długości animacji)
        yield return new WaitForSeconds(1f);

        // Czekanie 3 sekundy przed pokazaniem wyniku (razem z animacją = 4 sekundy)
        yield return new WaitForSeconds(3f);

        // Sprawdzenie wyniku
        DetermineWinner();
        
        // Wyświetl wynik przez dłużej (3 sekundy)
        yield return new WaitForSeconds(0.7f);

        // Powrót do idle
        animator.SetInteger("Stan", IDLE);
        if (computerAnimator != null)
            computerAnimator.SetInteger("Stan", IDLE);
        yield return new WaitForSeconds(0.5f);

        // Przygotowanie do następnej rundy
        waitingForResult = false;
        
        // Sprawdź czy ktoś wygrał mecz
        if (playerScore == WIN_SCORE)
        {
            // Gracz wygrał - ustaw bool w GameStateManager i zmień scenę na "Game"
            Debug.Log("GRACZ WYGRAŁ MECZ!");
            if (GameStateManager.Instance != null)
            {
                GameStateManager.Instance.SetPlayerWonPapierKamienNozyce(true);
                Debug.Log("[PapierKamienNozyce] ✅ Zapisano playerWonPapierKamienNozyce = true do GSM");
            }
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene("Game");
        }
        else if (computerScore == WIN_SCORE)
        {
            // Komputer wygrał - resetuj mecz
            Debug.Log("KOMPUTER WYGRAŁ MECZ! Reset...");
            playerScore = 0;
            computerScore = 0;
            UpdateScoreUI();
            yield return new WaitForSeconds(2f);
            StartNewRound();
        }
        else
        {
            // Kontynuuj grę
            StartNewRound();
        }
    }

    private void DetermineWinner()
    {
        string result = "";
        
        // Wyświetl wybory
        if (choicesText != null)
            choicesText.text = $"Gracz: {GetChoiceName(playerChoice)} | Komputer: {GetChoiceName(computerChoice)}";

        if (playerChoice == computerChoice)
        {
            result = "REMIS!";
            Debug.Log(result);
        }
        else if (IsPlayerWinning(playerChoice, computerChoice))
        {
            result = "WYGRAŁEŚ!";
            playerScore++;
            Debug.Log(result);
        }
        else
        {
            result = "PRZEGRAŁEŚ!";
            computerScore++;
            Debug.Log(result);
        }

        // Wyświetl wynik
        if (resultText != null)
            resultText.text = result;
        
        UpdateScoreUI();
        Debug.Log($"Gracz: {GetChoiceName(playerChoice)}, Komputer: {GetChoiceName(computerChoice)}");
    }

    private bool IsPlayerWinning(int player, int computer)
    {
        // Kamień (1) pokonuje Nożyce (2)
        if (player == KAMIEN && computer == NOZYCE) return true;
        // Nożyce (2) pokonują Papier (3)
        if (player == NOZYCE && computer == PAPIER) return true;
        // Papier (3) pokonuje Kamień (1)
        if (player == PAPIER && computer == KAMIEN) return true;

        return false;
    }

    private string GetChoiceName(int choice)
    {
        return choice switch
        {
            KAMIEN => "Kamień",
            NOZYCE => "Nożyce",
            PAPIER => "Papier",
            _ => "Idle"
        };
    }

    private void StartNewRound()
    {
        isGameRunning = true;
        playerChoice = IDLE;
        computerChoice = IDLE;
        animator.SetInteger("Stan", IDLE);
        if (computerAnimator != null)
            computerAnimator.SetInteger("Stan", IDLE);
        
        // Wyczyść UI
        if (resultText != null)
            resultText.text = "";
        if (choicesText != null)
            choicesText.text = "";
    }
    
    private void UpdateScoreUI()
    {
        if (playerScoreText != null)
            playerScoreText.text = $"(Komputer) {computerScore}:{playerScore} (Gracz)";
    }
}
