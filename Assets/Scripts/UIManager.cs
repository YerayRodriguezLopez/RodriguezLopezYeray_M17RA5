using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Main Menu")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button quitButton;

    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Game Over Menu")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button exitButton;

    [Header("HUD")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private Transform inventoryContainer;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Minimap")]
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private Transform playerTransform;

    private bool isPaused = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        SetupButtons();

        // Mostrar menú principal al inicio
        if (SceneManager.GetActiveScene().name == "MainMenu")
        {
            ShowMainMenu();
        }
        else
        {
            ShowHUD();
        }

        // Suscribirse al evento de inventario
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged += UpdateInventoryUI;
        }
    }

    private void Update()
    {
        // Detectar pausa (ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        // Actualizar minimap
        UpdateMinimap();
    }

    private void SetupButtons()
    {
        // Main Menu
        if (playButton != null)
            playButton.onClick.AddListener(StartGame);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);

        // Pause Menu
        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);
        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(BackToMainMenu);

        // Game Over
        if (retryButton != null)
            retryButton.onClick.AddListener(RestartGame);
        if (exitButton != null)
            exitButton.onClick.AddListener(BackToMainMenu);
    }

    #region Menu Management

    private void ShowMainMenu()
    {
        HideAllPanels();
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ShowHUD()
    {
        HideAllPanels();
        if (hudPanel != null)
            hudPanel.SetActive(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void PauseGame()
    {
        isPaused = true;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(true);
        if (hudPanel != null)
            hudPanel.SetActive(false);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AudioManager.Instance?.SetMusicVolume(0.3f);
    }

    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (hudPanel != null)
            hudPanel.SetActive(true);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        AudioManager.Instance?.SetMusicVolume(1f);
    }

    public void ShowGameOver()
    {
        HideAllPanels();

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AudioManager.Instance?.StopMusic();
        AudioManager.Instance?.PlaySFX("GameOver");
    }

    private void HideAllPanels()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);
        if (pauseMenuPanel != null)
            pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (hudPanel != null)
            hudPanel.SetActive(false);
    }

    #endregion

    #region Button Actions

    public void StartGame()
    {
        AudioManager.Instance?.PlaySFX("ButtonClick");
        SceneManager.LoadScene("GameScene");
    }

    public void RestartGame()
    {
        AudioManager.Instance?.PlaySFX("ButtonClick");
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        AudioManager.Instance?.PlaySFX("ButtonClick");
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        AudioManager.Instance?.PlaySFX("ButtonClick");
        Debug.Log("Saliendo del juego...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    #endregion

    #region HUD

    private void UpdateInventoryUI()
    {
        // Limpiar slots existentes
        foreach (Transform child in inventoryContainer)
        {
            Destroy(child.gameObject);
        }

        // Crear nuevos slots
        var inventory = InventoryManager.Instance.GetInventory();
        foreach (ItemData item in inventory)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, inventoryContainer);

            Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null && item.icon != null)
            {
                icon.sprite = item.icon;
                icon.enabled = true;
            }
        }
    }

    public void ShowMessage(string message, float duration = 3f)
    {
        if (messageText != null)
        {
            StopAllCoroutines();
            StartCoroutine(ShowMessageCoroutine(message, duration));
        }
    }

    private IEnumerator ShowMessageCoroutine(string message, float duration)
    {
        messageText.text = message;
        messageText.enabled = true;

        yield return new WaitForSeconds(duration);

        messageText.enabled = false;
    }

    private void UpdateMinimap()
    {
        if (minimapCamera != null && playerTransform != null)
        {
            // Mantener el minimap sobre el jugador
            Vector3 newPosition = playerTransform.position;
            newPosition.y = minimapCamera.transform.position.y;
            minimapCamera.transform.position = newPosition;

            // Rotar con el jugador (opcional)
            // minimapCamera.transform.rotation = Quaternion.Euler(90f, playerTransform.eulerAngles.y, 0f);
        }
    }

    #endregion

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.onInventoryChanged -= UpdateInventoryUI;
        }
    }
}