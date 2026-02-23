using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ─── Scene names ────────────────────────────────────────────────────────
    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene   = "MainMenu";
    [SerializeField] private string exteriorScene   = "Exterior";
    [SerializeField] private string interiorScene   = "Interior";

    // ─── State ──────────────────────────────────────────────────────────────
    private bool gameStarted;
    private bool doorOpened;

    public bool DoorOpened  => doorOpened;
    public bool GameStarted => gameStarted;

    // ─── Events ─────────────────────────────────────────────────────────────
    public static event System.Action OnDoorOpened;
    public static event System.Action OnGameOver;
    public static event System.Action OnGameWin;

    // ════════════════════════════════════════════════════════════════════════
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
        // Lock and hide cursor during gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible   = false;
    }

    // ════════════════════════════════════════════════════════════════════════
    #region Public API
    // ════════════════════════════════════════════════════════════════════════

    public void StartGame()
    {
        gameStarted = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(exteriorScene);
    }

    public void LoadMainMenu()
    {
        Time.timeScale   = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible   = true;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void RestartGame()
    {
        doorOpened = false;
        Time.timeScale = 1f;

        // Reload current active scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ─── Door / win flow ────────────────────────────────────────────────────

    public void NotifyDoorOpened()
    {
        if (doorOpened) return;
        doorOpened = true;
        OnDoorOpened?.Invoke();
    }

    public void TriggerWin()
    {
        OnGameWin?.Invoke();
        UIManager.Instance?.ShowWinScreen();
    }

    public void TriggerGameOver()
    {
        OnGameOver?.Invoke();
        UIManager.Instance?.ShowGameOver();
    }

    // ─── Scene transitions ──────────────────────────────────────────────────

    public void GoToInterior()
    {
        SaveSystem.Instance?.SaveGame();
        SceneManager.LoadScene(interiorScene);
    }

    public void GoToExterior()
    {
        SceneManager.LoadScene(exteriorScene);
    }

    #endregion
}
