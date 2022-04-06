using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public bool UsePauseMenu = false;
    public GameObject GamePauseMenu;

    public bool StopTime = false;

    public string MainMenuSceneName = "StartScene";

    [HideInInspector]
    public bool GamePaused;

    private GameObject PlayerObject;
    //private AudioSource song;

    public void UnPause()
    {
        TogglePauseGame();
    }

    public void ExitToMainMenu()
    {
        TogglePauseGame();
        //FadeAudioSource.StartFade(song, 10, 0);
        SceneManager.LoadScene(MainMenuSceneName);
    }

    private void TogglePauseGame()
    {
        GamePaused = !GamePaused;

        if (UsePauseMenu)
        {
            GamePauseMenu.SetActive(GamePaused);
        }

        if (StopTime)
        {
            if (GamePaused)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
    }
    private void Start()
    {
        if (UsePauseMenu)
        {
            PlayerObject = GameObject.Find("Player/Audio/AmbientSong");
        }
        //song = PlayerObject.GetComponent<AudioSource>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseGame();
        }
    }
}
