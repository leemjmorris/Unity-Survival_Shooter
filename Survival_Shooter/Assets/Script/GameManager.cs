using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.Audio;

public class GameManager : MonoBehaviour
{
    private const string gameManagerTag = "GameController";

    [Header("Game State")]
    public bool isGameOver = false;

    [Header("Score")]
    public int currentScore = 0;

    [Header("UI References")]
    public TextMeshProUGUI scoreText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI gameOverText;
    public TextMeshProUGUI finalScoreText;
    public TextMeshProUGUI waveReachedText;
    public TextMeshProUGUI currentWaveText;

    [Header("Fade Settings")]
    public CanvasGroup gameOverCanvasGroup;
    public float fadeInDuration = 2f;
    public float fadeDelay = 1f;

    [Header("Pause Menu")]
    public GameObject pauseMenuPanel;
    public bool isPaused = false;

    [Header("Audio Settings")]
    public Slider musicVolumeSlider;
    public Slider effectsVolumeSlider;
    public Toggle soundToggle;
    public Button pauseResumeButton;
    public Button pauseQuitButton;

    [Header("Audio Mixers")]
    public UnityEngine.Audio.AudioMixer masterMixer;
    public UnityEngine.Audio.AudioMixerSnapshot pausedSnapshot;
    public UnityEngine.Audio.AudioMixerSnapshot unpausedSnapshot;

    [Header("References")]
    public WaveSpawner waveSpawner;
    public PlayerHealth playerHealth;

    private void Start()
    {
        // LMJ: Play BGM (AudioClip should be set in AudioSource component in Inspector)
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && audio.clip != null)
        {
            audio.loop = true;
            audio.Play();
        }

        // LMJ: Subscribe to player death
        if (playerHealth != null)
        {
            playerHealth.OnDeath += OnPlayerDeath;
        }

        // LMJ: Setup game over panel
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);

            // LMJ: Get or add CanvasGroup for fade
            if (gameOverCanvasGroup == null)
            {
                gameOverCanvasGroup = gameOverPanel.GetComponent<CanvasGroup>();
                if (gameOverCanvasGroup == null)
                {
                    gameOverCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
                }
            }
            gameOverCanvasGroup.alpha = 0f;
        }

        // LMJ: Setup pause menu
        SetupPauseMenu();
        LoadAudioSettings();

        UpdateScoreUI();
        UpdateWaveUI();

        // LMJ: Subscribe to wave events
        if (waveSpawner != null)
        {
            waveSpawner.onWaveStart.AddListener(OnWaveStarted);
            waveSpawner.onWaveComplete.AddListener(OnWaveCompleted);
        }

        Time.timeScale = 1f;
    }

    private void Update()
    {
        // LMJ: ESC to toggle pause (only if not game over)
        if (!isGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // LMJ: Restart with R when game over
        if (isGameOver && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    public void AddScore(int points)
    {
        if (isGameOver) return;

        currentScore += points;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {currentScore}";
        }
    }

    private void UpdateWaveUI()
    {
        if (currentWaveText != null && waveSpawner != null)
        {
            currentWaveText.text = $"WAVE: {waveSpawner.currentWave}";
        }
    }

    private void OnWaveStarted(int waveNumber)
    {
        UpdateWaveUI();
    }

    private void OnWaveCompleted(int waveNumber)
    {
        UpdateWaveUI();
    }

    private void OnPlayerDeath()
    {
        if (isGameOver) return;

        isGameOver = true;
        StartCoroutine(ShowGameOver());
    }

    private IEnumerator ShowGameOver()
    {
        // LMJ: Wait before showing game over
        yield return new WaitForSecondsRealtime(fadeDelay);

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // LMJ: Update texts
            if (gameOverText != null)
            {
                gameOverText.text = "GAME OVER";
            }

            if (finalScoreText != null)
            {
                finalScoreText.text = $"Final Score: {currentScore}";
            }

            if (waveReachedText != null && waveSpawner != null)
            {
                waveReachedText.text = $"Wave Reached: {waveSpawner.currentWave}";
            }

            // LMJ: Fade in effect
            yield return StartCoroutine(FadeIn());
        }

        // LMJ: Slow down time after fade complete
        Time.timeScale = 0.3f;
    }

    private IEnumerator FadeIn()
    {
        if (gameOverCanvasGroup == null) yield break;

        float elapsedTime = 0f;
        gameOverCanvasGroup.alpha = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / fadeInDuration;

            // LMJ: Smooth fade curve
            gameOverCanvasGroup.alpha = Mathf.SmoothStep(0f, 1f, t);

            yield return null;
        }

        gameOverCanvasGroup.alpha = 1f;
        gameOverCanvasGroup.interactable = true;
        gameOverCanvasGroup.blocksRaycasts = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #region Pause Menu Methods

    private void SetupPauseMenu()
    {
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // LMJ: Setup slider listeners
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (effectsVolumeSlider != null)
        {
            effectsVolumeSlider.onValueChanged.AddListener(SetEffectsVolume);
        }

        // LMJ: Setup toggle listener
        if (soundToggle != null)
        {
            soundToggle.onValueChanged.AddListener(ToggleSound);
        }

        // LMJ: Setup button listeners
        if (pauseResumeButton != null)
        {
            pauseResumeButton.onClick.AddListener(Resume);
        }

        if (pauseQuitButton != null)
        {
            pauseQuitButton.onClick.AddListener(QuitGame);
        }
    }

    private void LoadAudioSettings()
    {
        // LMJ: Load saved values or use defaults
        float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float effectsVol = PlayerPrefs.GetFloat("EffectsVolume", 0.7f);
        bool soundOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        // LMJ: Apply to sliders
        if (musicVolumeSlider != null)
        {
            musicVolumeSlider.value = musicVol;
        }

        if (effectsVolumeSlider != null)
        {
            effectsVolumeSlider.value = effectsVol;
        }

        if (soundToggle != null)
        {
            soundToggle.isOn = soundOn;
        }

        // LMJ: Apply to mixers
        SetMusicVolume(musicVol);
        SetEffectsVolume(effectsVol);

        if (!soundOn)
        {
            if (masterMixer != null)
            {
                masterMixer.SetFloat("musicVol", -80f);
                masterMixer.SetFloat("sfxVol", -80f);
            }
        }
    }

    private void TogglePause()
    {
        isPaused = !isPaused;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(isPaused);
        }

        // LMJ: Stop or resume time
        Time.timeScale = isPaused ? 0f : 1f;

        // LMJ: Switch audio snapshot
        if (isPaused && pausedSnapshot != null)
        {
            pausedSnapshot.TransitionTo(0.1f);
        }
        else if (!isPaused && unpausedSnapshot != null)
        {
            unpausedSnapshot.TransitionTo(0.1f);
        }

        // LMJ: Lock/unlock cursor
        Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.None;
        Cursor.visible = isPaused;
    }

    public void Resume()
    {
        if (isPaused)
        {
            TogglePause();
        }
    }

    private void SetMusicVolume(float volume)
    {
        if (masterMixer != null)
        {
            // LMJ: Convert 0-1 to decibels (-80 to 0)
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("musicVol", dB);
        }
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    private void SetEffectsVolume(float volume)
    {
        if (masterMixer != null)
        {
            // LMJ: Convert 0-1 to decibels (-80 to 0)
            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            masterMixer.SetFloat("sfxVol", dB);
        }
        PlayerPrefs.SetFloat("EffectsVolume", volume);
    }

    private void ToggleSound(bool isOn)
    {
        if (masterMixer != null)
        {
            // LMJ: Mute both music and sfx
            float volume = isOn ? 0f : -80f;
            masterMixer.SetFloat("musicVol", volume);
            masterMixer.SetFloat("sfxVol", volume);
        }
        PlayerPrefs.SetInt("SoundOn", isOn ? 1 : 0);
    }

    #endregion
}