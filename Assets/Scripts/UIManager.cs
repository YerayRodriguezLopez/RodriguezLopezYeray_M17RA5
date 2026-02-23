using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ─── Panels ─────────────────────────────────────────────────────────────
    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject hudPanel;

    // ─── HUD ────────────────────────────────────────────────────────────────
    [Header("HUD")]
    [SerializeField] private Transform  inventoryContainer;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private TMP_Text   messageText;
    [SerializeField] private float      messageDuration = 3f;

    // ─── Minimap ────────────────────────────────────────────────────────────
    [Header("Minimap")]
    [SerializeField] private RawImage   minimapImage;
    [SerializeField] private Camera     minimapCamera;
    [SerializeField] private RectTransform playerMarker;

    // ─── Cached slots ────────────────────────────────────────────────────────
    private List<GameObject> inventorySlots = new List<GameObject>();
    private Coroutine        messageCoroutine;

    // ════════════════════════════════════════════════════════════════════════
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Default state: show only HUD (assuming we start in-game)
        // MainMenu scene will call ShowMainMenu() explicitly.
        ShowMainMenu(false);
        ShowPauseMenu(false);
        ShowGameOver(false);
        ShowWinScreen(false);

        if (hudPanel != null) hudPanel.SetActive(true);

        // Subscribe to inventory changes
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += RefreshInventoryHUD;
    }

    private void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= RefreshInventoryHUD;
    }

    // ════════════════════════════════════════════════════════════════════════
    #region Panel visibility
    // ════════════════════════════════════════════════════════════════════════

    public void ShowMainMenu(bool visible = true)
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(visible);
        if (hudPanel       != null) hudPanel.SetActive(!visible);

        Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible   = visible;
    }

    public void ShowPauseMenu(bool visible)
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(visible);
    }

    public void ShowGameOver(bool visible = true)
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(visible);
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            Time.timeScale   = 0f;
        }
    }

    public void ShowWinScreen(bool visible = true)
    {
        if (winPanel != null) winPanel.SetActive(visible);
        if (visible)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
        }
    }

    // ─── Shorthand called from PlayerController ──────────────────────────────
    public void ShowGameOver() => ShowGameOver(true);

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    #region Button callbacks (assign in Inspector)
    // ════════════════════════════════════════════════════════════════════════

    // Main menu buttons
    public void OnPlayButton()    => GameManager.Instance?.StartGame();
    public void OnQuitButton()    => GameManager.Instance?.QuitGame();

    // Pause menu buttons
    public void OnResumeButton()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        player?.TogglePause();
    }

    public void OnRestartButton()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.RestartGame();
    }

    public void OnMainMenuButton()
    {
        Time.timeScale = 1f;
        GameManager.Instance?.LoadMainMenu();
    }

    // Game over / win buttons
    public void OnPlayAgainButton() => OnRestartButton();

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    #region HUD – Inventory
    // ════════════════════════════════════════════════════════════════════════

    private void RefreshInventoryHUD()
    {
        if (inventoryContainer == null || inventorySlotPrefab == null) return;

        // Clear old slots
        foreach (GameObject slot in inventorySlots)
            Destroy(slot);
        inventorySlots.Clear();

        // Rebuild
        List<ItemData> items = InventoryManager.Instance?.GetAllItems();
        if (items == null) return;

        foreach (ItemData item in items)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, inventoryContainer);
            inventorySlots.Add(slot);

            // Set icon if the prefab has an Image component
            Image iconImage = slot.GetComponentInChildren<Image>();
            if (iconImage != null && item.icon != null)
                iconImage.sprite = item.icon;

            // Set name label if present
            TMP_Text label = slot.GetComponentInChildren<TMP_Text>();
            if (label != null)
                label.text = item.itemName;
        }
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    #region HUD – Messages
    // ════════════════════════════════════════════════════════════════════════

    public void ShowMessage(string msg)
    {
        if (messageText == null) return;

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageCoroutine(msg));
    }

    private IEnumerator ShowMessageCoroutine(string msg)
    {
        messageText.text    = msg;
        messageText.enabled = true;
        yield return new WaitForSeconds(messageDuration);
        messageText.enabled = false;
        messageText.text    = string.Empty;
    }

    #endregion

    // ════════════════════════════════════════════════════════════════════════
    #region Minimap
    // ════════════════════════════════════════════════════════════════════════

    private void LateUpdate()
    {
        UpdateMinimap();
    }

    private void UpdateMinimap()
    {
        if (minimapCamera == null || playerMarker == null) return;

        PlayerController player = FindObjectOfType<PlayerController>();
        if (player == null) return;

        // Keep minimap camera above player
        Vector3 camPos = minimapCamera.transform.position;
        camPos.x = player.transform.position.x;
        camPos.z = player.transform.position.z;
        minimapCamera.transform.position = camPos;

        // Rotate player marker to match player yaw
        playerMarker.rotation = Quaternion.Euler(
            0, 0, -player.transform.eulerAngles.y);
    }

    #endregion
}
