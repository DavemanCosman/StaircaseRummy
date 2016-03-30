using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FreeCell
{
    /// <summary>
    /// This class is used by the undo system to record the moves so that they can be reversed.
    /// </summary>
    public class Move
    {
        public List<Card> Cards { get; set; }

        Deck SourceDeck { get; set; }
        Deck TargetDeck { get; set; }
        int CardIndex { get; set; }
        MoveType Type;
        List<Move> submoves;
        int oldIndex;

        /// <summary>
        /// Move an entire stack of cards from one deck to another.
        /// </summary>
        public static Move Stack(Deck source, Deck target)
        {
            var result = new Move()
            {
                Cards = new List<Card>(),
                SourceDeck = source,
                TargetDeck = target,
                Type = MoveType.Stack
            };

            return result;
        }

        /// <summary>
        /// Move a card from one deck to another.
        /// Optionally specify the position in the target deck that the card is moved to.
        /// </summary>
        public static Move Single(Card card, Deck target, int targetIndex = -1)
        {
            var result = new Move()
            {
                Cards = new List<Card>(),
                SourceDeck = card.Deck,
                TargetDeck = target,
                CardIndex = targetIndex,
                Type = MoveType.Single
            };

            result.Cards.Add(card);
            return result;
        }

        /// <summary>
        /// Swap to cards.
        /// </summary>
        public static Move Swap(Card card1, Card card2)
        {
            var result = new Move()
            {
                Cards = new List<Card>(),
                SourceDeck = card1.Deck,
                TargetDeck = card2.Deck,
                Type = MoveType.Swap
            };

            result.Cards.Add(card1);
            result.Cards.Add(card2);

            return result;
        }

        /// <summary>
        /// Creates a set of moves that will be undone as a single Undo/Redo operation.
        /// </summary>
        public static Move Multi()
        {
            Deck.UpdatesDisabled = true;
            return new Move()
            {
                submoves = new List<Move>(),
                Type = MoveType.Multi
            };
        }

        /// <summary>
        /// Add a move to a multi move.
        /// </summary>
        public void AddSubmove(Move sub)
        {
            sub.Execute();
            submoves.Add(sub);
        }

        /// <summary>
        /// Executes a move.
        /// </summary>
        public void Execute()
        {
            switch (Type)
            {
                case MoveType.Stack:
                case MoveType.Single:
                    foreach (var card in Cards)
                    {
                        var particleSystem = card.GetComponentInChildren<ParticleSystem>();

                        if (!FreeCellBehavior.Instance.HasWon)
                        {
                            if (TargetDeck.Type == DeckType.Goal)
                                particleSystem.GetComponent<AudioSource>().Play();
                            else
                                card.GetComponent<AudioSource>().Play();
                        }

                        if (FreeCellBehavior.Instance.HasWon || TargetDeck.Type == DeckType.Goal)
                            particleSystem.Play();

                        card.GetComponent<Renderer>().sortingOrder = 1000;

                        if (Type == MoveType.Single)
                        {
                            oldIndex = card.Index;

                            if (SourceDeck != TargetDeck)
                            {
                                CardIndex = card.Index;
                                card.Deck.Cards.Remove(card);
                                card.Deck.UpdateCards();
                                card.Deck = TargetDeck;
                            }
                            else
                            {
                                if (oldIndex != CardIndex)
                                {
                                    card.Deck.Cards.Remove(card);
                                    SourceDeck.Cards.Insert(CardIndex, card);
                                    card.Deck.UpdateCards();
                                }
                            }
                        }
                        else
                        {
                            card.Deck = TargetDeck;
                        }
                    }
                    break;
                case MoveType.Swap:
                    var card1 = Cards[0];
                    var card2 = Cards[1];
                    //Debug.Log("swapping " + card1 + " and " + card2);
                    var card1Index = card1.Index;
                    var card1Deck = card1.Deck;
                    card1.SetDeck(card2.Index, card2.Deck);
                    card2.SetDeck(card1Index, card1Deck);

                    //card1.Deck.UpdateCards();
                    //card2.Deck.UpdateCards();
                    break;
                case MoveType.Multi:
                    Deck.UpdatesDisabled = false;
                    foreach (var deck in GameObject.FindObjectsOfType<Deck>())
                        deck.UpdateCards();
                    break;
            }
        }

        /// <summary>
        /// Undoes a move.
        /// </summary>
        public void Undo()
        {
            switch (Type)
            {
                case MoveType.Stack:
                case MoveType.Single:
                    foreach (var card in Cards)
                    {
                        if (!FreeCellBehavior.Instance.HasWon)
                            card.GetComponent<AudioSource>().Play();

                        if (FreeCellBehavior.Instance.HasWon)
                        {
                            var particleSystem = card.GetComponentInChildren<ParticleSystem>();
                            particleSystem.Play();
                        }

                        card.GetComponent<Renderer>().sortingOrder = 1000;
                        if (Type == MoveType.Single)
                        {
                            card.Deck = SourceDeck;
                            card.SetDeck(oldIndex);
                        }
                        else
                            card.Deck = SourceDeck;
                    }
                    break;
                case MoveType.Swap:
                    var card1 = Cards[0];
                    var card2 = Cards[1];
                    var card1Index = card1.Index;
                    var card1Deck = card1.Deck;
                    card1.SetDeck(card2.Index, card2.Deck);
                    card2.SetDeck(card1Index, card1Deck);
                    break;
                case MoveType.Multi:
                    Deck.UpdatesDisabled = true;
                    foreach (var sub in submoves.Reverse<Move>())
                        sub.Undo();
                    Deck.UpdatesDisabled = false;
                    foreach (var deck in GameObject.FindObjectsOfType<Deck>())
                        deck.UpdateCards();
                    break;
            }
        }
    }
}
