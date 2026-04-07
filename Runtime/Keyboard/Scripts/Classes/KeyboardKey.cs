using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardKey : MonoBehaviour
{
    public delegate void KeyPress(char c);
    public static event KeyPress onKeyPress;

    [Header("Settings")]
    public char key;

    [Header("Components")]
    public TMPro.TextMeshProUGUI keyNameText;

    // Update is called once per frame
    void Update()
    {
        if (keyNameText)
        {
           if (!Keyboard.instance.upper) key = char.ToLower(key);
           else key = char.ToUpper(key);
           keyNameText.text = key.ToString();
        } 
    }
    public void AddKey()
    {
        if (onKeyPress != null) onKeyPress(key);
    }
}
