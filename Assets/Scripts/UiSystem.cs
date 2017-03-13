using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class ExtensionMethods
{
    public static bool MyEquals(this Resolution res, Resolution other)
    {
        return res.width == other.width && res.height == other.height;
    }
}
public class UiSystem : MonoBehaviour
{
    [HideInInspector]
    public GameSystem GameSystem
    {
        get { return m_gameSystem; }
        set
        {
            m_gameSystem = value;
            m_gameSystem.OnSaveGame += checkLoadGameButtonInteractable;
        }
    }

    public Button LoadGameButton;
    public GameObject MainMenu;
    public GameObject InGameMenu;
    public Toggle FullscreenToggle;
    public Dropdown ResolutionsDropdown;
    public Slider MusicVolumeSlider;
    public AudioMixer MusicAudioMixer;
    public Slider SfxVolumeSlider;
    public AudioMixer SfxAudioMixer;
    public AudioListener UiAudioListener;
    public AudioSource UiSound;

    private List <Resolution> m_possibleResolutions;
    private GameSystem m_gameSystem;


    private void activateBackgroundSound(bool val)
    {
        UiAudioListener.enabled = val;
        UiSound.enabled = val;
    }
    void Awake()
    {
        m_possibleResolutions = new List<Resolution>();
        checkLoadGameButtonInteractable();
        FullscreenToggle.isOn = Screen.fullScreen;
        createResolutionDropdownEntries();
    }

    private void createResolutionDropdownEntries()
    {
        int index = 0;
        foreach (var resolution in Screen.resolutions)
        {
            if (!m_possibleResolutions.Contains(resolution))
            {
                ResolutionsDropdown.options.Add(new Dropdown.OptionData(resolution.width + "x" + resolution.height));
                m_possibleResolutions.Add(resolution);
                if (Screen.currentResolution.MyEquals(resolution))
                    ResolutionsDropdown.value = index;

                ++index;
            }
        }
    }

    private void checkLoadGameButtonInteractable()
    {
        LoadGameButton.interactable = PlayerPrefs.GetInt(GameSystem.SavedGameAvailablePlayerPrefsKey) == 1;
    }

    public void SaveCurrentGame()
    {
        GameSystem.SaveData();
    }
    public void LoadMainScene(int index)
    {
        activateBackgroundSound(false);
        MainMenu.SetActive(false);
        GameSystem.LoadMainScene(index);
        PauseGame(false);
    }

    public void UnloadMainScene()
    {
        activateBackgroundSound(true);
        GameSystem.UnloadMainScene();
    }

    public void LoadGame()
    {
        activateBackgroundSound(false);
        GameSystem.LoadData();
        MainMenu.SetActive(false);
        PauseGame(false);
    }

    public void ReturnToMainMenu()
    {
        activateBackgroundSound(true);
        UnloadMainScene();
        InGameMenu.SetActive(false);
        MainMenu.SetActive(true);
        Cursor.visible = true;
    }

    public void PauseGame(bool val)
    {
        InGameMenu.SetActive(val);
        Time.timeScale = val ? 0 : 1;
        Cursor.visible = val;
    }

    public void SetResolution()
    {
        var res = m_possibleResolutions[ResolutionsDropdown.value];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = FullscreenToggle.isOn;
    }

    public void ChangeMusicVolume()
    {
        MusicAudioMixer.SetFloat("masterVolume", MusicVolumeSlider.value);
    }

    public void ChangeSfxVolume()
    {
        SfxAudioMixer.SetFloat("masterVolume", SfxVolumeSlider.value);
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
