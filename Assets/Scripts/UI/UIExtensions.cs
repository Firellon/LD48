using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace UI
{
    public static class UIExtensions
    {
        public static bool IsPointerOverUIElement()
        {
            return IsPointerOverElement(it => it.gameObject.layer == LayerMask.NameToLayer("UI"), out var uiElements);
        }

        public static bool IsPointerOverElement(Func<RaycastResult, bool> predicate,
            out List<RaycastResult> filteredHoveredElements)
        {
            var mousePosition = Mouse.current.position;
            var hoverResults = new List<RaycastResult>();
            var pointerEventData = new PointerEventData(EventSystem.current)
            {
                position = new Vector2(mousePosition.x.value, mousePosition.y.value)
            };

            EventSystem.current.RaycastAll(pointerEventData, hoverResults);
            if (hoverResults.Any())
            {
                var hoverResult = hoverResults.First();
            }
            filteredHoveredElements = hoverResults.Where(predicate).ToList();

            return filteredHoveredElements.Any();
        }
    }
}