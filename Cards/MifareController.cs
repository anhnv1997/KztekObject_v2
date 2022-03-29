using System;
using System.Collections.Generic;
using System.Text;

namespace KztekObject.Cards
{
    public class MifareController : ICardController
    {
        private int cardType;
        public MifareController(int _cardType)
        {
            this.cardType = _cardType;
        }

        public int CardLen()
        {
            return 4;
        }

        public string GetCardHexNumber(string cardNumber)
        {
            try
            {
                return Convert.ToInt32(cardNumber).ToString("X8");
            }
            catch (Exception)
            {
                return cardNumber;
            }
        }
    }
}
