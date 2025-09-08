using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBarColor : MonoBehaviour
{
    [SerializeField] private Image barImage;
    [SerializeField] private Color targetColor = Color.green;

    private void Start()
    {
        if (barImage != null)
            barImage.color = targetColor;
    }

    public void SetBarColor(Color newColor)
    {
        if (barImage != null)
            barImage.color = newColor;
    }
}