using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerScore : MonoBehaviour
{
    public Text ScoreDisplayElement;
    public float Multiplier = 1;
    public string Preface = "Score: ";

    public static float Score = 0;

    // Start is called before the first frame update
    void Start()
    {
        Score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Score += Time.deltaTime * Multiplier;

        ScoreDisplayElement.text = Preface + Mathf.Floor(Score).ToString();
    }
}
