using System;
using System.Collections.Generic;
using System.Text;

namespace FreeCell
{
    /// <summary>
    /// Used to sort the cards.
    /// </summary>
    class CardSuitComparer:IComparer<Card>
    {
        #region IComparer<Card> Members

        public int Compare(Card x, Card y)
        {
            if (x.Suit != y.Suit)
            {
                if (x.Suit < y.Suit)
                    return 1;
                else return -1;
            }
            else
            {
                return x.CompareTo(y);
            }
        }

        #endregion
    }
}
