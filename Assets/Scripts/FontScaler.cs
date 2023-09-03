using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FontScaler : MonoBehaviour
{
    private Text _text;
    [SerializeField] private int MinSize = 28;
    [SerializeField] private int MaxSize = 64;
    [SerializeField] private int referenceRes = 921600;

    private void Awake()
    {
        _text = GetComponent<Text>();
    }

    private void Update()
    {
        ScaleFont();
    }

    public void ScaleFont()
    {
        var fontSize = 0;
        var screenSize = Screen.width * Screen.height;
        fontSize = (int)Mathf.Lerp(MaxSize, MinSize, (float)referenceRes / (float)screenSize);
        _text.fontSize = fontSize;
    }
}
