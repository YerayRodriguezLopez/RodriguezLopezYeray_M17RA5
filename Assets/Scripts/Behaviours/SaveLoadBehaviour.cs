using UnityEngine;

public class SaveLoadBehaviour : MonoBehaviour
{
    private const string POSITION_X_KEY = "PlayerPositionX";
    private const string POSITION_Y_KEY = "PlayerPositionY";
    private const string POSITION_Z_KEY = "PlayerPositionZ";
    private const string HAS_WEAPON_KEY = "PlayerHasWeapon";
    
    private PlayerController _playerController;

    private void Awake()
    {
        ClearSaveData();
    }
    
    private void Start()
    {
        _playerController = GetComponent<PlayerController>();
        
        if (!_playerController)
        {
            Debug.LogError("SaveLoadBehaviour: No PlayerController found on this GameObject!");
        }
    }
    
    public void SavePlayerData()
    {
        Vector3 position = transform.position;
        PlayerPrefs.SetFloat(POSITION_X_KEY, position.x);
        PlayerPrefs.SetFloat(POSITION_Y_KEY, position.y);
        PlayerPrefs.SetFloat(POSITION_Z_KEY, position.z);
        
        if (_playerController)
        {
            PlayerPrefs.SetInt(HAS_WEAPON_KEY, _playerController.HasWeapon ? 1 : 0);
        }
        
        PlayerPrefs.Save();
        // Debug.Log($"Player data saved! Position: {position}, Has Weapon: {_playerController?.HasWeapon}");
    }
    
    public bool LoadPlayerData()
    {
        if (PlayerPrefs.HasKey(POSITION_X_KEY))
        {
            float x = PlayerPrefs.GetFloat(POSITION_X_KEY);
            float y = PlayerPrefs.GetFloat(POSITION_Y_KEY);
            float z = PlayerPrefs.GetFloat(POSITION_Z_KEY);
            Vector3 savedPosition = new Vector3(x, y, z);
            
            bool hasWeapon = PlayerPrefs.GetInt(HAS_WEAPON_KEY, 0) == 1;

            CharacterController controller = GetComponent<CharacterController>();
            if (controller)
            {
                controller.enabled = false;
                transform.position = savedPosition;
                controller.enabled = true;
            }
            else
            {
                transform.position = savedPosition;
            }
            return hasWeapon;
        }
        else return false;

        // Debug.Log($"Player data loaded! Position: {savedPosition}, Has Weapon: {PlayerPrefs.GetInt(HAS_WEAPON_KEY, 0) == 1}");
    }
    
    // for debug, to clean the save.
    public void ClearSaveData()
    {
        PlayerPrefs.DeleteKey(POSITION_X_KEY);
        PlayerPrefs.DeleteKey(POSITION_Y_KEY);
        PlayerPrefs.DeleteKey(POSITION_Z_KEY);
        PlayerPrefs.DeleteKey(HAS_WEAPON_KEY);
        PlayerPrefs.Save();
        Debug.Log("Save data cleared!");
    }
}