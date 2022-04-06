using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreDisplay : MonoBehaviour
{
    public Text ScoreDisplayElement;
    public string Preface = "Score: ";

    // Start is called before the first frame update
    void Start()
    {
        ScoreDisplayElement.text = Preface + Mathf.Floor(PlayerScore.Score).ToString();
    }
}
