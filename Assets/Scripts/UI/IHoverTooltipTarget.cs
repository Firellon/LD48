using UnityEngine;

namespace UI
{
    public interface IHoverTooltipTarget
    {
        string TooltipText { get; }
        Vector2 LeftBottomTooltipOffset { get;  }
        Vector2 RightTopTooltipOffset { get; }
    }
}