using MetroAutomation.Controls;
using System;

namespace MetroAutomation.ViewModel
{
    public class StatusChangedEventArgs : EventArgs
    {
        internal StatusChangedEventArgs(LedState status, string statusText)
        {
            Status = status;
            StatusText = statusText;
        }

        public LedState Status { get; }

        public string StatusText { get; }
    }

    public class StatusAndText
    {
        public StatusAndText(LedState ledState, string text)
        {
            LedState = ledState;
            Text = text;
        }

        public LedState LedState { get; }

        public string Text { get; }
    }
}
