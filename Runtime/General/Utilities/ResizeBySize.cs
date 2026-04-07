using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Outline))]
public class ResizeUiBySize : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, ICalibratableUI
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
    Vector2 initialSize;
    Vector2 initialMousePosition;
    bool isResizing;
    Outline outline;
    bool isDragging;

    Vector2 defaultPosition;
    Vector2 defaultSize;

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
        defaultSize = rectTransform.sizeDelta;

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

        Button button = buttonImage ? buttonImage.gameObject.GetComponent<Button>() : null;
        if (button)
            button.enabled = !enabled;

        UpdateOutlineVisibility();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!allowReposition && !allowResize) return;

        isDragging = true;
        initialPosition = rectTransform.anchoredPosition;
        initialSize = rectTransform.sizeDelta;
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
            Vector2 targetSize = initialSize + new Vector2(mouseDelta.x, -mouseDelta.y);
            rectTransform.sizeDelta = new Vector2(
                Mathf.Max(1f, targetSize.x),
                Mathf.Max(1f, targetSize.y)
            );
        }
        else if (allowReposition)
        {
            Vector2 targetPos = initialPosition + mouseDelta;
            if (UICalibrationManager.instance)
                targetPos = UICalibrationManager.instance.ApplySnap(targetPos);

            rectTransform.anchoredPosition = targetPos;
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
        PlayerPrefs.SetFloat(keyBase + "_width", rectTransform.sizeDelta.x);
        PlayerPrefs.SetFloat(keyBase + "_height", rectTransform.sizeDelta.y);
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

            rectTransform.sizeDelta = new Vector2(
                PlayerPrefs.GetFloat(keyBase + "_width"),
                PlayerPrefs.GetFloat(keyBase + "_height")
            );
        }
    }

    public UICalibrationState CaptureState()
    {
        return new UICalibrationState
        {
            id = StableId,
            mode = UICalibrationValueMode.Size,
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
        rectTransform.sizeDelta = state.sizeDelta;
    }

    public void ResetCalibrationDefault()
    {
        rectTransform.anchoredPosition = defaultPosition;
        rectTransform.sizeDelta = defaultSize;
    }

    public static void SaveAll()
    {
        PlayerPrefs.Save();
    }
}
