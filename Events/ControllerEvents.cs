using KztekObject.enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace KztekObject.Events
{
    public class ControllerEvent
    {
        public delegate void ErrorEventHandler(object sender, ErrorEventArgs e);
        public class ErrorEventArgs : EventArgs
        {
            public string ErrorString { get; set; } = string.Empty;
            public string ErrorFunc { get; set; } = string.Empty;
            public string ErrorLine { get; set; } = string.Empty;
            public string CMD { get; set; } = string.Empty;
        }

        public delegate void InputEventHandler(object sender, InputEventArgs e);
        public class InputEventArgs : EventArgs {
            public string InputIndex { get; set; }
            public DateTime Time { get; set; }
            public string InputName { get; set; }
            public string Status { get; set; }
        }

        public delegate void CardEventHandler(object sender, CardEventArgs e);
        public class CardEventArgs : EventArgs
        {
            public string CardNumber { get; set; }
            public string ControllerID { get; set; }
            public int ReaderIndex { get; set; }
            public string Date { get; set; }
            public string Time { get; set; }
            public EM_EventStatus EventStatus { get; set; }
            public string UserID { get; set; }
            public EM_EventType EventType { get; set; }
        }

        public delegate void ConnectStatusChangeEventHandler(object sender, ConnectStatusCHangeEventArgs e);
        public class ConnectStatusCHangeEventArgs : EventArgs
        {
            public bool CurrentStatus { get; set; }
            public string ControllerID { get; set; }
        }


    }
}
