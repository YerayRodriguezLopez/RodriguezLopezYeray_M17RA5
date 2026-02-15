using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game State")]
    [SerializeField] private bool gameStarted = false;
    [SerializeField] private bool gameOver = false;
    [SerializeField] private bool gameWon = false;

    [Header("Win Condition")]
    [SerializeField] private int requiredCollectables = 1;
    private int collectedItems = 0;

    [Header("References")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;

    private PlayerController player;

    private void Awake()
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
    }

    private void Start()
    {
        InitializeGame();
    }

    private void InitializeGame()
    {
        // Encontrar o crear el jugador
        player = FindObjectOfType<PlayerController>();

        if (player == null && playerPrefab != null && playerSpawnPoint != null)
        {
            GameObject playerGO = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);
            player = playerGO.GetComponent<PlayerController>();
        }

        gameStarted = true;
        gameOver = false;
        gameWon = false;
        collectedItems = 0;
    }

    public void CollectItem()
    {
        collectedItems++;

        Debug.Log($"Items recogidos: {collectedItems}/{requiredCollectables}");

        if (collectedItems >= requiredCollectables)
        {
            // Habilitar la puerta o activar el objetivo
            EnableVictoryCondition();
        }
    }

    private void EnableVictoryCondition()
    {
        Debug.Log("¡Todos los objetos recogidos! Busca la puerta.");
        UIManager.Instance?.ShowMessage("¡Objetos completados! Encuentra la puerta.", 5f);

        // Activar la puerta
        Door victoryDoor = FindObjectOfType<Door>();
        if (victoryDoor != null)
        {
            // La puerta ya está activa, solo notificar
        }
    }

    public void TriggerVictory()
    {
        if (gameWon || gameOver) return;

        gameWon = true;
        Debug.Log("¡Victoria!");

        UIManager.Instance?.ShowMessage("¡Victoria! Completaste el juego.", 5f);

        // Aquí puedes mostrar un menú de victoria
        Invoke(nameof(ShowVictoryScreen), 3f);
    }

    private void ShowVictoryScreen()
    {
        // Implementar pantalla de victoria
        UIManager.Instance?.ShowGameOver(); // Temporal, usar un panel de victoria
    }

    public void GameOver()
    {
        if (gameOver) return;

        gameOver = true;
        Debug.Log("Game Over");

        UIManager.Instance?.ShowGameOver();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        InitializeGame();
    }

    public PlayerController GetPlayer()
    {
        return player;
    }

    public bool IsGameActive()
    {
        return gameStarted && !gameOver && !gameWon;
    }
}