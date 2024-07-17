using UnityEngine;

namespace UI.Signals
{
    public class ShowHoverTooltipCommand
    {
        public Vector3 HoverPosition { get; }
        public string TooltipText { get; }

        public ShowHoverTooltipCommand(Vector3 hoverPosition, string tooltipText)
        {
            HoverPosition = hoverPosition;
            TooltipText = tooltipText;
        }
    }
    
    public class HideHoverTooltipCommand
    {
    }
}