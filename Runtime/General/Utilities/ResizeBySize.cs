using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Outline))]
public class ResizeUiBySize : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
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
    private Vector2 initialSize;
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
        
        // Green outline when element can be moved/resized
        outline.effectColor = moveColor;
        outline.enabled = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            ToggleEditMode();
            buttonImage.gameObject.GetComponent<Button>().enabled = !buttonImage.gameObject.GetComponent<Button>().enabled;
        }
    }

    private void ToggleEditMode()
    {
        allowReposition = !allowReposition;
        allowResize = allowReposition; // Link resize to reposition mode
        txtButton.gameObject.SetActive(allowReposition);

        buttonImage.sprite = allowReposition ? whiteImage : transparentImage;
        UpdateOutlineVisibility();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!allowReposition && !allowResize) return;

        isDragging = true;
        initialPosition = rectTransform.anchoredPosition;
        initialSize = rectTransform.sizeDelta;
        initialMousePosition = GetScaledMousePosition(eventData);

        // Check if we're clicking on resize border
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

        // Change to red during interaction
        outline.effectColor = dragColor;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector2 currentMousePosition = GetScaledMousePosition(eventData);
        Vector2 mouseDelta = currentMousePosition - initialMousePosition;

        if (isResizing && allowResize)
        {
            // Apply resizing
            rectTransform.sizeDelta = initialSize + new Vector2(
                mouseDelta.x, 
                -mouseDelta.y
            );
        }
        else if (allowReposition)
        {
            // Apply repositioning
            rectTransform.anchoredPosition = initialPosition + mouseDelta;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        // Return to green after interaction
        UpdateOutlineVisibility();
    }

    void OnDisable()
    {
        isDragging = false;
        SaveButtonState();
        // Ensure green outline when disabled
        UpdateOutlineVisibility();
    }

    void OnEnable()
    {
        // Ensure green outline when enabled
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
        PlayerPrefs.SetFloat(objectName + "_width", rectTransform.sizeDelta.x);
        PlayerPrefs.SetFloat(objectName + "_height", rectTransform.sizeDelta.y);
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
            
            rectTransform.sizeDelta = new Vector2(
                PlayerPrefs.GetFloat(objectName + "_width"),
                PlayerPrefs.GetFloat(objectName + "_height")
            );
        }
    }

    // For easy saving from other scripts
    public static void SaveAll()
    {
        PlayerPrefs.Save();
    }
}