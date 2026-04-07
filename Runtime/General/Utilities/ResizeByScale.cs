using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Outline))]
public class ResizeByScale : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, ICalibratableUI
{
    [Header("Identification")]
    public string objectName;

    [Header("Drag Settings")]
    public bool allowReposition;
    public bool allowResize;
    public float resizeBorderWidth = 15f;

    [Header("Visual Settings")]
    public Image buttonImage;
    public TextMeshProUGUI txtButton;
    public Sprite transparentImage, whiteImage;
    public Color moveColor = Color.green;
    public Color dragColor = Color.red;
    public float outlineThickness = 2f;

    RectTransform rectTransform;
    Canvas canvas;
    Vector2 initialPosition;
    Vector3 initialScale;
    Vector2 initialMousePosition;
    bool isResizing;
    Outline outline;
    bool isDragging;

    Vector2 defaultPosition;
    Vector3 defaultScale;

    public string StableId => objectName;

    void Awake()
    {
        objectName = gameObject.name;
        txtButton = GetComponentInChildren<TextMeshProUGUI>();
        if (txtButton)
        {
            txtButton.text = objectName;
            txtButton.gameObject.SetActive(false);
        }

        buttonImage = GetComponent<Image>();
        outline = GetComponent<Outline>();
        SetupOutline();
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        defaultPosition = rectTransform.anchoredPosition;
        defaultScale = transform.localScale;

        LoadButtonState();
        UpdateOutlineVisibility();

        if (UICalibrationManager.instance)
            UICalibrationManager.instance.Register(this);
    }

    void OnEnable()
    {
        UpdateOutlineVisibility();

        if (UICalibrationManager.instance)
            UICalibrationManager.instance.Register(this);
    }

    void OnDisable()
    {
        isDragging = false;
        SaveButtonState();
        UpdateOutlineVisibility();

        if (UICalibrationManager.instance)
            UICalibrationManager.instance.Unregister(this);
    }

    void SetupOutline()
    {
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectDistance = new Vector2(outlineThickness, outlineThickness);
        outline.enabled = false;
    }

    void UpdateOutlineVisibility()
    {
        if (!allowReposition && !allowResize)
        {
            outline.enabled = false;
            return;
        }

        outline.effectColor = moveColor;
        outline.enabled = true;
    }

    public void SetCalibrationMode(bool enabled)
    {
        allowReposition = enabled;
        allowResize = enabled;

        if (txtButton)
            txtButton.gameObject.SetActive(enabled);

        if (buttonImage)
            buttonImage.sprite = enabled ? whiteImage : transparentImage;

        UpdateOutlineVisibility();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!allowReposition && !allowResize) return;

        isDragging = true;
        initialPosition = rectTransform.anchoredPosition;
        initialScale = transform.localScale;
        initialMousePosition = GetScaledMousePosition(eventData);

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        Rect rect = rectTransform.rect;
        isResizing = allowResize && (
            localPoint.x < rect.xMin + resizeBorderWidth ||
            localPoint.x > rect.xMax - resizeBorderWidth ||
            localPoint.y < rect.yMin + resizeBorderWidth ||
            localPoint.y > rect.yMax - resizeBorderWidth
        );

        outline.effectColor = dragColor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 currentMousePosition = GetScaledMousePosition(eventData);
        Vector2 mouseDelta = currentMousePosition - initialMousePosition;

        if (isResizing && allowResize)
        {
            float scaleFactor = (mouseDelta.x - mouseDelta.y) * 0.005f;
            Vector3 newScale = initialScale + Vector3.one * scaleFactor;

            newScale.x = Mathf.Max(0.1f, newScale.x);
            newScale.y = Mathf.Max(0.1f, newScale.y);

            transform.localScale = newScale;
        }
        else if (allowReposition)
        {
            Vector2 target = initialPosition + mouseDelta;
            if (UICalibrationManager.instance)
                target = UICalibrationManager.instance.ApplySnap(target);

            rectTransform.anchoredPosition = target;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        UpdateOutlineVisibility();
    }

    Vector2 GetScaledMousePosition(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.worldCamera,
                out mousePos
            );
        }

        return canvas != null ? mousePos / canvas.scaleFactor : mousePos;
    }

    public void SaveButtonState()
    {
        string keyBase = string.IsNullOrEmpty(objectName) ? gameObject.name : objectName;
        PlayerPrefs.SetFloat(keyBase + "_x", rectTransform.anchoredPosition.x);
        PlayerPrefs.SetFloat(keyBase + "_y", rectTransform.anchoredPosition.y);

        PlayerPrefs.SetFloat(keyBase + "_sx", transform.localScale.x);
        PlayerPrefs.SetFloat(keyBase + "_sy", transform.localScale.y);

        PlayerPrefs.Save();
    }

    public void LoadButtonState()
    {
        string keyBase = string.IsNullOrEmpty(objectName) ? gameObject.name : objectName;
        if (PlayerPrefs.HasKey(keyBase + "_x"))
        {
            rectTransform.anchoredPosition = new Vector2(
                PlayerPrefs.GetFloat(keyBase + "_x"),
                PlayerPrefs.GetFloat(keyBase + "_y")
            );

            transform.localScale = new Vector3(
                PlayerPrefs.GetFloat(keyBase + "_sx", 1f),
                PlayerPrefs.GetFloat(keyBase + "_sy", 1f),
                1f
            );
        }
    }

    public UICalibrationState CaptureState()
    {
        return new UICalibrationState
        {
            id = StableId,
            mode = UICalibrationValueMode.Scale,
            anchoredPosition = rectTransform.anchoredPosition,
            sizeDelta = rectTransform.sizeDelta,
            localScale = transform.localScale
        };
    }

    public void ApplyState(UICalibrationState state)
    {
        if (state == null)
            return;

        rectTransform.anchoredPosition = state.anchoredPosition;
        transform.localScale = state.localScale;
    }

    public void ResetCalibrationDefault()
    {
        rectTransform.anchoredPosition = defaultPosition;
        transform.localScale = defaultScale;
    }

    public static void SaveAll()
    {
        PlayerPrefs.Save();
    }
}
