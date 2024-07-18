using System;

namespace UI.Signals
{
    public class ShowTextInputPopupCommand
    {
        public Func<string, string> OnTextEntered { get; }
        public string CurrentText { get; }
        public string Label { get; }

        public ShowTextInputPopupCommand(string currentText, string label, Func<string, string> onTextEntered)
        {
            CurrentText = currentText;
            Label = label;
            OnTextEntered = onTextEntered;
        }
    }
}