using System;
using System.Collections.Generic;
using System.Text;
using static KztekObject.Cards.CardFactory;

namespace KztekObject.Objects
{
    public class Employee
    {
        public int CardType { get; set; } = (int)EM_CardType.Mifare;
        public string CardNumber { get; set; } = "";
        public List<int> Doors { get; set; } = new List<int>();
        public int MemoryID { get; set; } = 0;
        public int TimezoneID { get; set; } = 0;
        public List<string> fingerDatas { get; set; } = new List<string>();
        public string Name { get; set; } = "";
        public string Password { get; set; } = "";

    }
}
