using System.Globalization;
using TMPro;
using UnityEngine;

public class DesriptionPanelController : MonoBehaviour
{
    private TextMeshProUGUI DescriptionText;

    private void Awake()
    {
        DescriptionText = transform.Find("TextInfo").GetComponent<TextMeshProUGUI>();
    }

    public void SetDescription(string description)
    {
        DescriptionText.text = description;
    }
}
