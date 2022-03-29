using System;
using System.Collections.Generic;
using System.Text;

namespace KztekObject.Cards
{
    public  class CardFactory
    {
        public enum EM_CardType
        {
            Proximity_Left,
            ProximityRight,
            Mifare,
            ultra,
        }

        public static ICardController CreateCardController(int CardType)
        {
            switch (CardType)
            {
                case (int)EM_CardType.Mifare:
                    return new MifareController(CardType);
                case (int)EM_CardType.Proximity_Left:
                    return new ProximityController(CardType);
                case (int)EM_CardType.ProximityRight:
                    return new ProximityController(CardType);
                default:
                    return null;
            }
        }
    }
}
