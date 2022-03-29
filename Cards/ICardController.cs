using System;
using System.Collections.Generic;
using System.Text;

namespace KztekObject.Cards
{
    public interface ICardController
    {
        string GetCardHexNumber(string cardNumber);
        int CardLen();
    }
}
