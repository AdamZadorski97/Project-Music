using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboController : MonoBehaviour
{
    public int currentCombo = 0;
    public int currrentScore;
    public TMP_Text comboText;
    public TMP_Text scoreText;
    public static ComboController Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        UpdateText();
    }


    public void AddCombo()
    {
        currentCombo++;
        currrentScore += currentCombo;
        UpdateText();
    }
    public void UpdateText()
    {
        comboText.text = $"Combo:{currentCombo}";
        scoreText.text = $"Score:{currrentScore}";
    }


}
