using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Warning : MonoBehaviour
{
    public Text TextElement;
    Color textColor;

    bool show = false;
    float currentOpacity = 0;

    public float showTime = 10;
    float CurrentShowTime = 10;

    private void Start()
    {
        textColor = TextElement.color;
    }

    // Start is called before the first frame update
    public void WarnPlayer()
    {
        show = true;
        CurrentShowTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (CurrentShowTime > 0)
        {
            CurrentShowTime -= Time.deltaTime;
            if (CurrentShowTime < 0)
            {
                CurrentShowTime = 0;
                show = false;
            }
        }

        if (show)
        {
            if (currentOpacity < 1)
            {
                currentOpacity += Time.deltaTime;
                if (currentOpacity > 1)
                {
                    CurrentShowTime = showTime;
                }
            }
        } else
        {
            if (currentOpacity > 0)
            {
                currentOpacity -= Time.deltaTime;
                if (currentOpacity < 0)
                {
                    currentOpacity = 0;
                }
            }
        }

        TextElement.color = new Color(textColor.r, textColor.g, textColor.b, currentOpacity);
    }
}
