using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class UIPathFollower : MonoBehaviour
{
    [Header("Hierarchy")]
    public RectTransform rotator;     // rotates only
    public RectTransform visualRoot;  // flips only

    [Header("Visuals")]
    public Image followerImage;
    public TextMeshProUGUI followerName;

    [Header("Rotation")]
    public float rotationSpeed = 720f;

    static int globalCounter = 0;

    RectTransform rect;

    RectTransform[] path;
    float speed;

    EaseType easeType;
    float easePower;
    AnimationCurve customCurve;

    int index;
    Action<UIPathFollower> onFinish;

    Coroutine moveRoutine;
    bool useSpecialRotation;
    float specialRotationAngle;
    bool useTextFlip180;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
    }

    public void Initialize(
        RectTransform[] points,
        float moveSpeed,
        EaseType type,
        float power,
        AnimationCurve curve,
        Color routeColor,
        bool specialRotation,
        float fixedRotation,
        bool textFlip180,
        Action<UIPathFollower> finished)
    {
        path = points;
        speed = moveSpeed;

        easeType = type;
        easePower = power;
        customCurve = curve;

        onFinish = finished;

        index = 0;
        rect.position = path[0].position;
        
        useSpecialRotation = specialRotation;
        specialRotationAngle = fixedRotation;
        useTextFlip180 = textFlip180;

        SetupVisuals(routeColor);

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);

        moveRoutine = StartCoroutine(Move());
    }

    void SetupVisuals(Color routeColor)
    {
        globalCounter++;

        // if (followerName != null)
            // followerName.text = "Follower_" + globalCounter;

        if (followerImage != null)
            followerImage.color = routeColor;
            
        if (followerName != null)
        {
            followerName.rectTransform.localRotation = useTextFlip180 ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
        }
    }

    IEnumerator Move()
    {
        while (index < path.Length - 1)
        {
            Vector3 start = rect.position;
            Vector3 end = path[index + 1].position;

            float dist = Vector3.Distance(start, end);
            float t = 0;

            while (t < 1f)
            {
                t += Time.deltaTime * speed / dist;

                float v = EvaluateEase(t);

                Vector3 newPos = Vector3.Lerp(start, end, v);
                rect.position = newPos;

                // Direction toward next point (current position!)
                Vector2 dir = (end - rect.position).normalized;

                if (dir.sqrMagnitude > 0.001f)
                {
                    if (useSpecialRotation)
                    {
                        // Fixed rotation
                        Quaternion targetRot = Quaternion.Euler(0, 0, specialRotationAngle);

                        rotator.rotation = Quaternion.Slerp(
                            rotator.rotation,
                            targetRot,
                            Time.deltaTime * rotationSpeed * 0.01f
                        );
                    }
                    else
                    {
                        // Normal path-based rotation
                        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                        Quaternion targetRot = Quaternion.Euler(0, 0, angle);

                        rotator.rotation = Quaternion.Slerp(
                            rotator.rotation,
                            targetRot,
                            Time.deltaTime * rotationSpeed * 0.01f
                        );

                        if (visualRoot != null)
                        {
                            Vector3 scale = visualRoot.localScale;
                            scale.x = Mathf.Abs(scale.x) * (dir.x < 0 ? -1 : 1);
                            visualRoot.localScale = scale;
                        }
                    }
                }

                yield return null;
            }

            index++;
        }

        onFinish?.Invoke(this);
    }

    float EvaluateEase(float t)
    {
        switch (easeType)
        {
            case EaseType.Linear:
                return t;

            case EaseType.SmoothStep:
                return Mathf.SmoothStep(0, 1, t);

            case EaseType.EaseIn:
                return Mathf.Pow(t, easePower);

            case EaseType.EaseOut:
                return 1 - Mathf.Pow(1 - t, easePower);

            case EaseType.EaseInOut:
                return t < 0.5f
                    ? Mathf.Pow(t * 2, easePower) / 2
                    : 1 - Mathf.Pow((1 - t) * 2, easePower) / 2;

            case EaseType.CustomCurve:
                return customCurve.Evaluate(t);

            default:
                return t;
        }
    }
}