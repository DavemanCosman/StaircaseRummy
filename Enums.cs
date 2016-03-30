using System;
using System.Collections.Generic;
using System.Text;

namespace FreeCell
{
    [Serializable]
    public enum CardSuit
    {
        Spades = 1, 
        Hearts, 
        Diamonds, 
        Clubs
    }

    [Serializable]
    public enum CardRank
    {
        Ace = 1, 
        Deuce, 
        Three, 
        Four, 
        Five, 
        Six, 
        Seven, 
        Eight, 
        Nine, 
        Ten, 
        Jack,
        Queen, 
        King
    }

    [Serializable]
    public enum CardColor
    {
        Black,
        Red
    }

    [Serializable]
    public enum DeckType
    {
        Hand,
        Staircase,
        Junk,
        Play,
        Dealer,
        OffScreen
    }

    [Serializable]
    public enum PlayerType
    {
        South = 1,
        West,
        North,
        East
    }

    [Serializable]
    public enum MoveType
    {
        Single,
        Stack,
        Swap,
        Multi,
    }
}
