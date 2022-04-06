using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtons : MonoBehaviour
{
    public GameObject TutorialPopupMenu;

    public string GameSceneName = "GameScene";

    //private AudioSource song;

    private void OnSceneLoaded(Scene scene)
    {
        /*
        Animator ani = GameObject.Find("/Canvas/Image").GetComponent<Animator>();
        if (scene.name == "StartScene")
        {
            ani.Play("CompassSpin");
        }
        */
    }

    private void Start()
    {
        //Animator ani = GameObject.Find("/Canvas/Image").GetComponent<Animator>();
        //song = gameObject.transform.Find("/Main Camera").gameObject.GetComponent<AudioSource>();
        //ani.Play("CompassSpin");
        print("smeed");
    }

    public void PlayGame()
    {
        //FadeAudioSource.StartFade(song, 100, 0);
        SceneManager.LoadScene(GameSceneName);
    }
    public void ShowTutorial()
    {
        TutorialPopupMenu.SetActive(!TutorialPopupMenu.activeSelf);
    }
    public void Exit()
    {
        Application.Quit();
    }
}
