using System;
using UnityEngine;

public enum UICalibrationValueMode
{
    Size,
    Scale
}

[Serializable]
public class UICalibrationState
{
    public string id;
    public UICalibrationValueMode mode;
    public Vector2 anchoredPosition;
    public Vector2 sizeDelta;
    public Vector3 localScale;
}
