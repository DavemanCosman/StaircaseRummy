// #define NGUI // Uncomment this to use NGUI tweens instead of iTween

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

namespace FreeCell
{
    /// <summary>
    /// One of these should be placed on every deck GameObject.
    /// </summary>
    public class Deck : MonoBehaviour
    {
        public DeckType Type { get; set; }
        public int Index { get; set; }

        List<Card> cards = new List<Card>();

        /// <summary>
        /// The list of cards in this deck.
        /// </summary>
        public List<Card> Cards { get { return cards; } }

        /// <summary>
        /// The top most card that would be considered on the top of the stack of the deck. Not nessisarily the card nearest the top of the screen.
        /// </summary>
        public Card TopCard
        {
            get
            {
                if (Cards.Count > 0)
                    return Cards[Cards.Count - 1];
                else
                    return null;
            }
        }

        /// <summary>
        /// The bottom most card that would be considered on the bottom of the stack of the deck. Not nessisarily the card nearest the bottom of the screen.
        /// </summary>
        public Card BottomCard
        {
            get
            {
                if (Cards.Count > 0)
                    return Cards[0];
                else
                    return null;
            }
        }

        /// <summary>
        /// Is this deck empty?
        /// </summary>
        public bool HasCards
        {
            get
            {
                if (Cards.Count > 0)
                    return true;
                else
                    return false;
            }
        }

        /// <summary>
        /// Creates new cards in this deck.
        /// </summary>
        public void CreateCards(int numberOfSuits = 4)
        {
            UpdatesDisabled = true;
            for (int suit = 0; suit < numberOfSuits; suit++)
            {
                var realSuit = (suit % 4) + 1;
                for (int number = 1; number <= 13; number++)
                {
                    var card = (Instantiate(GameBehavior.Instance.CardPrefab) as GameObject).GetComponent<Card>();
                    card.Number = number;
                    card.Suit = (CardSuit)realSuit;
                    card.Deck = this;
                }
            }
            UpdatesDisabled = false;
            UpdateCards();
        }

        /// <summary>
        /// Check to see if this deck contains a card of specific number and suit.
        /// </summary>
        public bool Has(int number, CardSuit suit)
        {
            return Has((CardRank)number, suit);
        }

        /// <summary>
        /// Check to see if this deck contains a card of specific rank and suit.
        /// </summary>
        public bool Has(CardRank rank, CardSuit suit)
        {
            if (GetCard(rank, suit) != null)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Check to see if this deck contains a card of specific rank and color.
        /// </summary>
        public bool Has(CardRank rank, CardColor color)
        {
            if (color == CardColor.Black)
                return Has(rank, CardSuit.Clubs) || Has(rank, CardSuit.Spades);
            else
                return Has(rank, CardSuit.Hearts) || Has(rank, CardSuit.Diamonds);
        }

        /// <summary>
        /// Returns a card in this deck of a specific number and suit, otherwise null.
        /// </summary>
        public Card GetCard(int number, CardSuit suit)
        {
            return GetCard((CardRank)number, suit);
        }

        /// <summary>
        /// Returns a card in this deck of a specific rank and suit, otherwise null.
        /// </summary>
        public Card GetCard(CardRank rank, CardSuit suit)
        {
            foreach (Card card in Cards)
            {
                if ((card.Rank == rank) && (card.Suit == suit))
                    return card;
            }

            return null;
        }

        static System.Random random = new System.Random();

        /// <summary>
        /// Shuffles the deck of cards.
        /// </summary>
        public void Shuffle()
        {
            Shuffle(1, random);
        }

        /// <summary>
        /// Shuffles the deck of cards a specific number of times using a specified random number generator.
        /// </summary>
        public void Shuffle(int times, System.Random random)
        {
            for (int time = 0; time < times; time++)
            {
                for (int i = 0; i < Cards.Count; i++)
                {
                    Cards[i].Shuffle(random);
                }
            }
        }

        internal CardSuitComparer cardSuitComparer = new CardSuitComparer();

        /// <summary>
        /// Sort the cards in this deck.
        /// </summary>
        public void Sort()
        {
            Cards.Sort(cardSuitComparer);
        }

        /// <summary>
        /// Moves a number of cards from this deck to another.
        /// </summary>
        public void Draw(Deck toDeck, int count)
        {
            for (int i = 0; i < count; i++)
            {
                TopCard.Deck = toDeck;
            }
        }

        /// <summary>
        /// Flips all of the cards over.
        /// </summary>
        public void FlipAllCards()
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].Visible = !Cards[i].Visible;
            }
        }

        /// <summary>
        /// Makes all of the cards dragable or not.
        /// </summary>
        public void MakeAllCardsDragable(bool isDragable)
        {
            for (int i = 0; i < Cards.Count; i++)
            {
                Cards[i].IsDragable = isDragable;
            }
        }

        public override string ToString()
        {
            var output = new StringBuilder();

            output.Append("[" + Environment.NewLine);

            for (int i = 0; i < Cards.Count; i++)
            {
                output.Append(Cards[i].ToString() + Environment.NewLine);
            }

            output.Append("]" + Environment.NewLine);

            return output.ToString();
        }

        float cardSpacerX = 0;
        /// <summary>
        /// How much horizontal space should be placed between each card in the stack of this deck.
        /// </summary>
        public float CardSpacerX
        {
            get { return cardSpacerX; }
            set { cardSpacerX = value; }
        }

        float cardSpacerY = 0;
        /// <summary>
        /// How much vertical space should be placed between each card in the stack of this deck.
        /// </summary>
        public float CardSpacerY
        {
            get { return cardSpacerY; }
            set { cardSpacerY = value; }
        }

        private int maxCardsSpace = 0;
        /// <summary>
        /// The total height or width of the stack will be this number times the CardSpacer values.
        /// </summary>
        public int MaxCardsSpace
        {
            get
            {
                return maxCardsSpace;
            }
            set
            {
                maxCardsSpace = value;
            }
        }

        public static bool UpdatesDisabled;

        /// <summary>
        /// Recalculate all the card positions and animate them to the new positions
        /// Should be called when the deck change its cards order or count
        /// </summary>
        public void UpdateCards()
        {
            if (UpdatesDisabled)
                return;

            //print("Updating cards on deck " + (TopCard == null ? "Empty" : TopCard.ToString()));

            var localCardSpacerX = CardSpacerX;
            var localCardSpacerY = CardSpacerY;

            if ((MaxCardsSpace > 0) && (Cards.Count > MaxCardsSpace))
            {
                //override the spacers values to squeeze cards
                localCardSpacerX = (CardSpacerX * MaxCardsSpace) / Cards.Count;
                localCardSpacerY = (CardSpacerY * MaxCardsSpace) / Cards.Count;
            }

            //Loop on the Deck Cards (not playing cards)
            var lastTransform = transform;
            for (int i = 0; i < Cards.Count; i++)
            {
                //Get the card object
                var card = Cards[i];
                card.transform.parent = lastTransform;
                lastTransform = card.transform;

                var targetLocalPosition = new Vector3(0, 0, (-i - 1) * 0.001f); // z needs to be set for mouse hit detection
                if (i != 0)
                    targetLocalPosition += new Vector3(localCardSpacerX, localCardSpacerY);

#if NGUI
                TweenPosition.Begin(card.gameObject, 0.25f, targetLocalPosition);
#else
                iTween.MoveTo(card.gameObject, iTween.Hash("position", targetLocalPosition, "islocal", true, "time", 0.25f));
#endif
                card.GetComponent<Collider2D>().enabled = false; // disable the collider until the card is finished moving.

                var sortOrder = i + 20 * Index; // Right decks are on top of left decks when squeezing together
                StartCoroutine(Delay(() => // schedule this for when the card finishes moving
                {
                    card.GetComponent<Collider2D>().enabled = true;
                    card.GetComponent<Renderer>().sortingOrder = sortOrder; // sort order needs to be set for visual to render correctly
                }, 0.25f));
            }
        }

        /// <summary>
        /// This is runs an action after a delay. Must be called with StartCorotuine.
        /// </summary>
        public static IEnumerator Delay(Action action, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }

        /// <summary>
        /// Maintain MouseOverDeck reference for drag and drop.
        /// </summary>
        void OnMouseEnter()
        {
            GameBehavior.MouseOverDeck = this;
        }
    }
}