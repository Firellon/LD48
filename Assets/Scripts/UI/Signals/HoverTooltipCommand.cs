using UnityEngine;

namespace UI.Signals
{
    public class ShowHoverTooltipCommand
    {
        public Vector3 TargetScreenPosition { get; }
        public string TooltipText { get; }
        public Vector3 TooltipOffset { get; }
        
        public ShowHoverTooltipCommand(Vector3 targetScreenPosition, string tooltipText, Vector3 tooltipOffset)
        {
            TargetScreenPosition = targetScreenPosition;
            TooltipText = tooltipText;
            TooltipOffset = tooltipOffset;
        }
    }
    
    public class HideHoverTooltipCommand
    {
        public HideHoverTooltipCommand(){ }
    }
}