using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CustomUI{
    public class ComplexShapeButton
    {
        [MenuItem("GameObject/UI/Complex Shape Button", false, 10)]
        static void CreateCustomButton(MenuCommand menuCommand)
        {
            // Create a new GameObject and set it up as a Button with a CustomImage component
            GameObject customButton = new GameObject("ComplexShapeButton");
            GameObjectUtility.SetParentAndAlign(customButton, menuCommand.context as GameObject);
            Undo.RegisterCreatedObjectUndo(customButton, "Create " + customButton.name);
            Selection.activeObject = customButton;

            // Add necessary components
            RectTransform rectTransform = customButton.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(160, 30); // Example size
            customButton.AddComponent<CanvasRenderer>();
            CustomImage customImage = customButton.AddComponent<CustomImage>();

            // Create the TextMeshPro child
            GameObject buttonText = new GameObject("ButtonText");
            GameObjectUtility.SetParentAndAlign(buttonText, customButton);
            RectTransform textRectTransform = buttonText.AddComponent<RectTransform>();
            textRectTransform.anchorMin = Vector2.zero;
            textRectTransform.anchorMax = Vector2.one;
            textRectTransform.sizeDelta = Vector2.zero;

            TextMeshProUGUI textMeshPro = buttonText.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = "Button";
            textMeshPro.color = Color.black;
            textMeshPro.alignment = TextAlignmentOptions.Center;

            // Add the Button component
            Button button = customButton.AddComponent<Button>();

            // Set up the Button transition
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = Color.gray;
            colors.pressedColor = Color.black;
            colors.selectedColor = Color.white;
            colors.disabledColor = Color.gray;
            button.colors = colors;
        }
    }
}