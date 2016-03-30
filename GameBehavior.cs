using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FreeCell
{
    /// <summary>
    /// This is a global class that is used to index cards, decks, the card sprites and also tracks the deck that the mouse is currently over (for drop detection).
    /// </summary>
    public class GameBehavior : MonoBehaviour
    {
        public static GameBehavior Instance;

        /// <summary>
        /// This is maintained as the deck that the mouse is currently over. It is used when dropping a card or stack of cards as the target deck.
        /// </summary>
        public static Deck MouseOverDeck;
        public static Card MouseOverCard;

        /// <summary>
        /// This must link to the card prefab so that they can be Instantiated at run time.
        /// </summary>
        public UnityEngine.Object CardPrefab;

        /// <summary>
        /// This must link to the Deck prefab so that they can be Instantiated at run time.
        /// </summary>
        public UnityEngine.Object DeckPrefab;

        /// <summary>
        /// This array must have 54 entries and link to each card sprite. The last one should be the card back image for when the card is flipped face down.
        /// </summary>
        [HideInInspector]
        public Sprite[] CardSprites;

        /// <summary>
        /// An unused alternate set of card faces. This should have exactly 54 items. 52 card faces in 0-51. A back card face at 52 and an empty deck image at 53.
        /// To use this set programatically run CardSprites = ComplexCardSprites;
        /// </summary>
        //public Sprite[] ComplexCardSprites;

        /// <summary>
        /// The set of card faces. This should have exactly 54 items. 52 card faces in 0-51. A back card face at 52 and an empty deck image at 53.
        /// </summary>
        /* For the purposes of Staircase Rummy, there are exactly 56 items. 54 total card faces from 0-53, where 52 and 53 are the two jokers.
         * The back card face and deck image are at 54 and 55 are for the card and deck image respectively  */
        public Sprite[] SimpleCardSprites;

        /// <summary>
        /// A list of all instantiated CardBehaviors.
        /// </summary>
        public static List<Card> Cards = new List<Card>();

        public GameBehavior()
        {
            Instance = this;
        }
        public void Awake()
        {
            CardSprites = SimpleCardSprites;
            //LoadSprites();
        }

        // Untested dynamic loading of card sprites. Use at your own risks or use different lists of card faces as shown above.
        //void LoadSprites()
        //{
        //    var loadSimple = true;
        //    for (var suit = 1; suit <= 4; suit++)
        //    {
        //        var suitName = ((CardSuit)suit).ToString().ToLower();
        //        for (var rank = 1; rank <= 13; rank++)
        //        {
        //            var rankName = rank.ToString();
        //            switch (rank)
        //            {
        //                case 1:
        //                    rankName = "ace";
        //                    break;
        //                case 11:
        //                    rankName = "jack";
        //                    break;
        //                case 12:
        //                    rankName = "queen";
        //                    break;
        //                case 13:
        //                    rankName = "king";
        //                    break;
        //            }
        //            var fileName = String.Format("{0}_of_{1}", rankName, suitName);
        //            CardSprites[Card.CardIndex(suit, rank)] = Resources.Load<Sprite>("Playing Cards/" + (loadSimple ? "Simple/" : "") + fileName);
        //        }
        //    }
        //}

        /// <summary>
        /// Toggles game pausing.
        /// </summary>
        public void TogglePause()
        {
            if (IsPaused)
            {
                foreach (var card in GameObject.FindObjectsOfType<Card>())
                {
                    card.GetComponent<SpriteRenderer>().enabled = true;
                }
                Time.timeScale = 1;
            }
            else
            {
                foreach (var card in GameObject.FindObjectsOfType<Card>())
                {
                    card.GetComponent<SpriteRenderer>().enabled = false;
                }
                Time.timeScale = 0;
            }
        }

        /// <summary>
        /// Is the game paused.
        /// </summary>
        public static bool IsPaused { get { return Time.timeScale == 0f; } }

        /// <summary>
        /// Unpause the game.
        /// </summary>
        public static void Unpause()
        {
            Time.timeScale = 1;
        }

        /// <summary>
        /// Pause the game.
        /// </summary>
        public static void Pause()
        {
            Time.timeScale = 0;
        }

        /// <summary>
        /// Find a specific card.
        /// </summary>
        public static Card Find(int number, CardSuit suit)
        {
            return (from c in Cards where c.Number == number && c.Suit == suit select c).FirstOrDefault();
        }

        /// <summary>
        /// Find a specific card in a specific type of deck.
        /// </summary>
        public static Card Find(int number, CardSuit suit, DeckType type)
        {
            return (from c in Cards where c.Deck.Type == type && c.Number == number && c.Suit == suit select c).FirstOrDefault();
        }
    }
}