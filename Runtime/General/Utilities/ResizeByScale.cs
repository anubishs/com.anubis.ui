using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Outline))]
public class ResizeByScale : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
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

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 initialPosition;
    private Vector3 initialScale;
    private Vector2 initialMousePosition;
    private bool isResizing;
    private Outline outline;
    private bool isDragging;

    void Awake()
    {
        objectName = gameObject.name;
        txtButton = GetComponentInChildren<TextMeshProUGUI>();
        txtButton.text = objectName;
        txtButton.gameObject.SetActive(false);
        buttonImage = GetComponent<Image>();
        outline = GetComponent<Outline>();
        SetupOutline();
    }

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        LoadButtonState();
        UpdateOutlineVisibility();
    }

    private void SetupOutline()
    {
        if (outline == null)
            outline = gameObject.AddComponent<Outline>();

        outline.effectDistance = new Vector2(outlineThickness, outlineThickness);
        outline.enabled = false;
    }

    private void UpdateOutlineVisibility()
    {
        if (!allowReposition && !allowResize)
        {
            outline.enabled = false;
            return;
        }

        outline.effectColor = moveColor;
        outline.enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ToggleEditMode();
        }
    }

    private void ToggleEditMode()
    {
        allowReposition = !allowReposition;
        allowResize = allowReposition;
        txtButton.gameObject.SetActive(allowReposition);

        buttonImage.sprite = allowReposition ? whiteImage : transparentImage;
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
            rectTransform.anchoredPosition = initialPosition + mouseDelta;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        UpdateOutlineVisibility();
    }

    void OnDisable()
    {
        isDragging = false;
        SaveButtonState();
        UpdateOutlineVisibility();
    }

    void OnEnable()
    {
        UpdateOutlineVisibility();
    }

    private Vector2 GetScaledMousePosition(PointerEventData eventData)
    {
        Vector2 mousePos = eventData.position;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePos,
                canvas.worldCamera,
                out mousePos
            );
        }

        return mousePos / canvas.scaleFactor;
    }

    public void SaveButtonState()
    {
        PlayerPrefs.SetFloat(objectName + "_x", rectTransform.anchoredPosition.x);
        PlayerPrefs.SetFloat(objectName + "_y", rectTransform.anchoredPosition.y);

        PlayerPrefs.SetFloat(objectName + "_sx", transform.localScale.x);
        PlayerPrefs.SetFloat(objectName + "_sy", transform.localScale.y);

        PlayerPrefs.Save();
    }

    public void LoadButtonState()
    {
        if (PlayerPrefs.HasKey(objectName + "_x"))
        {
            rectTransform.anchoredPosition = new Vector2(
                PlayerPrefs.GetFloat(objectName + "_x"),
                PlayerPrefs.GetFloat(objectName + "_y")
            );

            transform.localScale = new Vector3(
                PlayerPrefs.GetFloat(objectName + "_sx", 1f),
                PlayerPrefs.GetFloat(objectName + "_sy", 1f),
                1f
            );
        }
    }

    public static void SaveAll()
    {
        PlayerPrefs.Save();
    }
}