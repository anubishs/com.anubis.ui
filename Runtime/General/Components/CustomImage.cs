using UnityEngine;
using UnityEngine.UI;

namespace CustomUI{
    public class CustomImage : Image
    {
        public bool ComplexShapeButton = true;
        protected override void Start()
        {
            // Custom code to be added at the start
            if(ComplexShapeButton) this.alphaHitTestMinimumThreshold = 0.1f;
            
            // Call the base class Start method
            base.Start();
        }
    }
}