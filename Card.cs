using UnityEngine;
using System.Collections;
using System;
using System.Linq;

namespace FreeCell
{
    /// <summary>
    /// One of these should be placed on every card gameObject.
    /// </summary>
    public class Card : MonoBehaviour, IComparable<Card>
    {
        /// <summary>
        /// How big (in pixels) the dead zone is when dragging a card.
        /// </summary>
        const float dragDeadDistance = 10;

        /// <summary>
        /// Used for sort order.
        /// </summary>
        public static bool IsAceBiggest = true;

        /// <summary>
        /// The rank of the card.
        /// </summary>
        public CardRank Rank;

        /// <summary>
        /// The suit of the card.
        /// </summary>
        public CardSuit Suit;

        /// <summary>
        /// The color of the card. This is derived from the suit.
        /// </summary>
        public CardColor Color
        {
            get
            {
                if ((Suit == CardSuit.Spades) || (Suit == CardSuit.Clubs))
                    return CardColor.Black;
                else
                    return CardColor.Red;
            }
        }

        /// <summary>
        /// The sequential value of the card.
        /// </summary>
        public int Number
        {
            get { return (int)Rank; }
            set { Rank = (CardRank)value; }
        }

        /// <summary>
        /// A traditional short string that represents the rank of the card. (A, 2-10, J, Q, K)
        /// </summary>
        public string NumberString
        {
            get
            {
                switch (Rank)
                {
                    case CardRank.Ace:
                        return "A";
                    case CardRank.Jack:
                        return "J";
                    case CardRank.Queen:
                        return "Q";
                    case CardRank.King:
                        return "K";
                    default:
                        return Number.ToString();
                }
            }
        }

        Deck deck;
        /// <summary>
        /// The deck that this card belongs to.
        /// </summary>
        public Deck Deck
        {
            get
            {
                return deck;
            }
            set
            {
                if (deck != value)
                {
                    if (deck != null)
                        deck.Cards.Remove(this);
                    deck = value;
                    deck.Cards.Add(this);

                    Deck.UpdateCards();
                }
            }
        }

        bool visible = true;
        /// <summary>
        /// True if the card is face up. False if the card back is showing.
        /// </summary>
        public bool Visible
        {
            get
            {
                return visible;
            }
            set
            {
                if (visible != value)
                {
                    visible = value;
                    SetCardSprite();
                }
            }
        }

        bool isDragable = true;
        /// <summary>
        /// Can this card be dragged to another deck.
        /// </summary>
        public bool IsDragable
        {
            get { return isDragable; }
            set { isDragable = value; }
        }

        /// <summary>
        /// The position of the card in the deck.
        /// </summary>
        public int Index
        {
            get { return Deck.Cards.IndexOf(this); }
        }

        /*  The current location in the board
         *  Hand = the player hand
         *  Staircase = the players staircase pile
         *  Junk = the players junk pile
         *  InPlay = currently in play
         *  Deck = somewhere in the drawing deck
         *  Offscreen = In the offscreen deck
         */
        public String[] Location = { "Hand", "Staircase", "Junk", "InPlay", "Deck", "OffScreen" };


        public Card()
        {
            GameBehavior.Cards.Add(this);
        }

        public Card(CardRank rank, CardSuit suit, Deck deck)
        {
            Rank = rank;
            Suit = suit;
            this.deck = deck;
            GameBehavior.Cards.Add(this);
        }

        public Card(int number, CardSuit suit, Deck deck)
        {
            Rank = (CardRank)number;
            Suit = suit;
            this.deck = deck;
            GameBehavior.Cards.Add(this);
        }

        /// <summary>
        /// Used for sorting.
        /// </summary>
        public int CompareTo(Card other)
        {
            int value1 = this.Number;
            int value2 = other.Number;

            if (Card.IsAceBiggest)
            {
                if (value1 == 1)
                    value1 = 14;

                if (value2 == 1)
                    value2 = 14;
            }

            if (value1 > value2)
                return 1;
            else if (value1 < value2)
                return -1;
            else
                return 0;
        }

        /// <summary>
        /// Moves the card to the bottom of it's deck stack.
        /// </summary>
        public void MoveToFirst()
        {
            MoveToIndex(0);
        }

        /// <summary>
        /// Moves the card to the top of it's deck stack.
        /// </summary>
        public void MoveToLast()
        {
            MoveToIndex(Deck.Cards.Count);
        }

        /// <summary>
        /// Move this card to a random position in it's deck stack.
        /// </summary>
        public void Shuffle(System.Random random)
        {
            MoveToIndex(random.Next(0, Deck.Cards.Count));
        }

        /// <summary>
        /// Move this card to a specific position in it's deck stack.
        /// </summary>
        public void MoveToIndex(int index)
        {
            Deck.Cards.Remove(this);
            Deck.Cards.Insert(index, this);
        }

        public override string ToString()
        {
            return this.NumberString + " of " + this.Suit.ToString();
        }

        bool isDragging = false;
        Vector3 clickPosition;
        float doubleClickStart = 0;
        bool dragDisabled;

        public void Start()
        {
            var particleSystem = GetComponentInChildren<ParticleSystem>();
            if (particleSystem != null)
                particleSystem.GetComponent<Renderer>().sortingLayerName = "Foreground"; // see http://answers.unity3d.com/questions/577288/particle-system-rendering-behind-sprites.html
        }

        /// <summary>
        /// Sets the card's sprite to the appropriate card face.
        /// </summary>
        void SetCardSprite()
        {
            if (Visible)
                ((SpriteRenderer)GetComponent<Renderer>()).sprite = GameBehavior.Instance.CardSprites[CardIndex((int)Suit, Number)];
            else
                ((SpriteRenderer)GetComponent<Renderer>()).sprite = GameBehavior.Instance.CardSprites[52];
        }

        static int CardIndex(int suit, int number)
        {
            return ((suit - 1) * 13) + number - 1;
        }

        /// <summary>
        /// Maintain MouseOverDeck reference for drag and drop.
        /// </summary>
        void OnMouseEnter()
        {
            GameBehavior.MouseOverDeck = Deck;
            GameBehavior.MouseOverCard = this;
        }

        void OnMouseExit()
        {
            GameBehavior.MouseOverDeck = null;
            GameBehavior.MouseOverCard = null;
        }

        /// <summary>
        /// Drag cards if the card is marked draggable.
        /// </summary>
        void OnMouseDrag()
        {
            if ((clickPosition - Input.mousePosition).sqrMagnitude > dragDeadDistance * dragDeadDistance)
            {
                if (dragDisabled) // check for mobile drag on double click problem.
                    return;

                if (IsDragable && !isDragging)
                {
                    //                    doubleClickStart = -1; // clear double click timer to avoid mobile drag bug?

                    isDragging = true;
                    GetComponent<Renderer>().sortingOrder = 1000;
                    transform.parent = FindObjectOfType<MouseCursorBehavior>().transform;
                    transform.position += new Vector3(0, 0, -transform.position.z);
                    GetComponent<Collider2D>().enabled = false; // disable the collider so that detection of decks under the dragging card works.

                    for (int i = Deck.Cards.IndexOf(this) + 1; i < Deck.Cards.Count; i++)
                    {
                        var childCard = Deck.Cards[i];
                        childCard.GetComponent<Renderer>().sortingOrder = 1000 + i;
                    }
                }
            }
        }

        /// <summary>
        /// This does a double click check and calls FreeCellBehavior.Instance.OnDoubleClick
        /// </summary>
        void OnMouseDown()
        {
            if ((Time.time - doubleClickStart) < 0.3f)
            {
                dragDisabled = true;
                FreeCellBehavior.Instance.OnDoubleClick(this);
                doubleClickStart = -1;
                return;
            }
            else
            {
                doubleClickStart = Time.time;
                clickPosition = Input.mousePosition;
            }
        }

        /// <summary>
        /// This handles end dragging.
        /// </summary>
        void OnMouseUp()
        {
            dragDisabled = false;

            if (isDragging)
            {
                isDragging = false;
                GetComponent<Collider2D>().enabled = true;

                var mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                var deck = (from d in FindObjectsOfType<Deck>()
                            where d != Deck
                            orderby
                                d.TopCard == null ? // snap to deck if empty otherwise snap to top card position
                                    Vector2.SqrMagnitude(new Vector2(mousePosition.x - d.transform.position.x, mousePosition.y - d.transform.position.y)) :
                                    Vector2.SqrMagnitude(new Vector2(mousePosition.x - d.TopCard.transform.position.x, mousePosition.y - d.TopCard.transform.position.y))
                            select d).FirstOrDefault();
                //if (deck != null)
                //    print("Closest deck " + deck.Type);
                //if (deck != null && deck.TopCard != null)
                //    print("Closest deck " + deck.TopCard.ToString());
                if (deck != null && FreeCellBehavior.Instance.CardDrag(this, deck))
                    return;

                Deck.UpdateCards();
            }
        }

        /// <summary>
        /// Move this card to a specified position in a specified deck.
        /// </summary>
        public void SetDeck(int newPosition, Deck deck = null)
        {
            Deck.Cards.Remove(this);
            if (deck != null)
            {
                Deck.UpdateCards();
                this.deck = deck;
            }
            Deck.Cards.Insert(newPosition, this);
            Deck.UpdateCards();
        }
    }
}