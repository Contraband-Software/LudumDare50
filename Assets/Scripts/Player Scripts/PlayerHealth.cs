using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Debug")]
    public bool Invincible = false;

    [Header("Stats")]
    [Range(0.0f, 1000.0f)]
    public float MaxHealth = 100;
    public float KillHealthGain = 10.0f;

    [Range(0.0f, 100.0f)]
    public float HealthDecay = 0.1f;
    [Range(0.0f, 10.0f)]
    public float AliveWorldDecayMultiplier = 2.0f;

    [Header("Health Bar")]
    [Tooltip("UI element that fills the health bar (it is the health level).")]
    public GameObject HealthFillElement;
    [Tooltip("End area of the health bar image element that is not meant to be counted as part of the health level.")]
    public float HealthBarEndRegion = 0.0f;

    [Header("Game States")]
    //public PlayerScore PlayerScoreCounter;
    public string GameOverSceneName = "GameOverScene";

    private PlayerController playerController;

    float NormalizedCurrentHealth;
    float ElementWidth;
    Vector3 HealthElementTransform;

    //private AudioSource song;

    public void Damage(float amount)
    {
        if (!Invincible)
        {
            float currentHealth = Mathf.Clamp(NormalizedCurrentHealth * MaxHealth - amount, 0, MaxHealth);
            NormalizedCurrentHealth = currentHealth / MaxHealth;
        }
    }

    public void GameOver()
    {
        //FadeAudioSource.StartFade(song, 10, 0);
        SceneManager.LoadScene(GameOverSceneName);
    }

    public void GainHealth()
    {
        float currentHealth = Mathf.Clamp(NormalizedCurrentHealth * MaxHealth + KillHealthGain, 0, MaxHealth);
        NormalizedCurrentHealth = currentHealth / MaxHealth;
    }

    // Start is called before the first frame update
    void Start()
    {
        //song = gameObject.transform.Find("Audio/AmbientSong").gameObject.GetComponent<AudioSource>();
        playerController = GetComponent<PlayerController>();

        NormalizedCurrentHealth = 1;
        HealthElementTransform = HealthFillElement.transform.localPosition;
        ElementWidth = HealthFillElement.GetComponent<RectTransform>().rect.width - HealthBarEndRegion;
    }

    // Update is called once per frame
    void Update()
    {
        float currentHealth;

        if (!Invincible)
        {
            if (playerController.currentWorld == PlayerController.World.Alive)
            {
                currentHealth = Mathf.Clamp(NormalizedCurrentHealth * MaxHealth - HealthDecay * AliveWorldDecayMultiplier * Time.deltaTime, 0, MaxHealth);
            }
            else
            {
                currentHealth = Mathf.Clamp(NormalizedCurrentHealth * MaxHealth - HealthDecay * Time.deltaTime, 0, MaxHealth);
            }

            NormalizedCurrentHealth = currentHealth / MaxHealth;

            HealthFillElement.transform.localPosition = HealthElementTransform - new Vector3(ElementWidth, 0, 0) * (1 - NormalizedCurrentHealth);

            if (NormalizedCurrentHealth == 0)
            {
                GameOver();
            }
        }
    }
}
