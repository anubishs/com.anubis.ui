using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "UI Pathing/UI Route")]
public class UIRoute : ScriptableObject
{
    public List<string> pointNames = new List<string>();

    [Header("Movement")]
    public float speed = 200f;

    public EaseType easeType = EaseType.SmoothStep;
    public float easePower = 2f;

    public AnimationCurve customCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Spawning")]
    public float minSpawnInterval = 2f;
    public float maxSpawnInterval = 4f;
    public int maxActive = 3;

    [Header("Editor")]
    public Color gizmoColor = Color.cyan;
    [Header("Special Rotation")]
    public bool specialRotation = false;
    public float fixedRotation = -90f;
    [Header("Text")]
    public bool textFlip180;

    [HideInInspector] public int active;
    [HideInInspector] public float timer;
    [HideInInspector] public float nextListSpawnTime;
    [HideInInspector] public float currentSpawnInterval;
}

public enum EaseType
{
    Linear,
    SmoothStep,
    EaseIn,
    EaseOut,
    EaseInOut,
    CustomCurve
}