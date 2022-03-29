using KztekObject.enums;
using System;
using System.Collections.Generic;
using System.Text;
using static KztekObject.Cards.CardFactory;
using static KztekObject.enums.CommunicationType;
using static KztekObject.enums.Controller_Function;

namespace KztekObject.Objects
{
    public class Controller
    {
        public string Name { get; set; } = "";
        public string ID { get; set; } = "";

        #region: CONNECTION
        public string Comport { get; set; } = "";
        public int Baudrate { get; set; } = 100;
        public bool IsConnect { get; set; }
        #endregion: END CONNECTION

        public EM_DisplayMode DisplayMode { get; set; }
        public Dictionary<int, EM_CardType> ReaderCardTypes { get; set; } = new Dictionary<int, EM_CardType>();
        public EM_CommunicationType ControllerCommunicationType { get; set; }

        public int Comkey { get; set; } = 0;
        public int MachineID { get; set; } = 1;


    }
}


