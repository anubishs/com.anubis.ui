using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Keyboard : MonoBehaviour
{
   public static Keyboard instance; 

   [Header("Components")]
   public TMPro.TextMeshProUGUI nameField;
   public TMPro.TMP_InputField keyboardPhysicalField;
   public UnityEngine.UI.Button continueButton;

   [Header("Status")]
   public string typedName = "";
   public bool upper = true;
   public bool sent = false;
   public bool forced = false;
   private void Awake()
   {
       instance = this;
   } 
   private void Start()
   {
       upper = true;
   } 
   private void Update()
   {
       if (Input.anyKeyDown && !(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)))
       {
           if (upper) ToggleUpper();
       }
       //Debug.Log(typedName.Length);
       if (keyboardPhysicalField)
       {
          typedName = keyboardPhysicalField.text;
          if (keyboardPhysicalField.gameObject.activeSelf)
          {
             keyboardPhysicalField.ActivateInputField();
          }
       }

       if (typedName.Length <= 0)
       {
            typedName = "";
            nameField.text = "DIGITE SEU NOME";
            if (!upper) ToggleUpper();
       }
       else
       {
            nameField.text = typedName;
       }
   }
   private void LateUpdate()
   {
       if (keyboardPhysicalField) keyboardPhysicalField.MoveToEndOfLine(false, false);
   }
   private void OnEnable()
   {
       KeyboardKey.onKeyPress += AddCharToName;
   }
   private void OnDisable()
   {
       keyboardPhysicalField.text = "";  
       KeyboardKey.onKeyPress -= AddCharToName;
   }
   public void AddCharToName(char c)
   {
        if (keyboardPhysicalField) keyboardPhysicalField.text += c;
        else typedName += c;
        if (upper) upper = false;
   }
   public void RemoveCharFromName()
   {
       if (keyboardPhysicalField)
       {
            if (keyboardPhysicalField.text.Length == 0) return;
            keyboardPhysicalField.text = keyboardPhysicalField.text.Remove(keyboardPhysicalField.text.Length-1);
       } 
       else if (typedName.Length > 0) typedName = typedName.Remove(typedName.Length-1);
   }
   public void ToggleUpper()
   {
        upper = !upper;
   }
   public void SetUpperTrue()
   {
        if (!upper) upper = true;
   }
}
