using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace FreeCell
{
    /// <summary>
    /// This class implements all of the freecell game logic. Edit this to implement another game type.
    /// </summary>
    public class StaircaseRummyBehavior : MonoBehaviour
    {
        #region Constants
        /// <summary>
        /// The max amount of players. For best results, keep this number between 2 and 4.
        /// </summary>
        public const int MaxPlayers = 4;

        /// <summary>
        /// The max amount of cards each player has per hand. Traditionally set to 7.
        /// Keep this number odd for best results.
        /// </summary>
        public const int MaxHandCards = 7;

        /// <summary>
        /// The amount of Staircase playing cells. Traditionally set to 4.
        /// </summary>
        public const int StairCells = 4;

        /// <summary>
        /// The max amount of decks being used.
        /// Keep it at this value for best results.
        /// </summary>
        public const int PlayingDecks = StairCells/2;

        /// <summary>
        /// The max amount of discard cells.
        /// </summary>
        public const int MaxJunkCells = 3;
        #endregion

        #region GameValues
        public static StaircaseRummyBehavior Instance;

        /// <summary>
        /// The max cards in a players staircase pile. For best results, keep this number odd.
        /// </summary>
        public static int MaxStaircaseStack = 13;

        /// <summary>
        /// Random number seed used to shuffle the cards.
        /// </summary>
        public static int Seed = 100;

        /// <summary>
        /// Number of suits that the player will be dealt. Traditionally this is four suits for a normal deck of cards.
        /// Staircase Rummy uses two decks, so a total of 8 suits are used.
        /// </summary>
        public static int Suits = 4 * PlayingDecks;

        /// <summary>
        /// Sets the distance between the cards for each play deck. Adjust this to suit your card faces.
        /// </summary>
        public float PlayStackVerticalSpace = -0.5f;

        /// <summary>
        /// True if the game has been won.
        /// </summary>
        public bool HasWon;

        #region Moves
        /// <summary>
        /// Maintains the list of moves for the undo system.
        /// </summary>
        public Stack<Move> Moves = new Stack<Move>();

        Stack<Move> redoMoves = new Stack<Move>();
        bool isRedoing;
        bool isAutocompleteRunning;
        #endregion

        #region Actions
        /// <summary>
        /// Called when the game is won. Play your victory screen from here.
        /// </summary>
        public Action OnWin;

        /// <summary>
        /// This is called when a card dragable flag is changed.
        /// </summary>
        public Action<Card, bool> OnCardDragableUpdate;
        #endregion

        /// <summary>
        /// Time that the game started. Use to calcuate play time.
        /// </summary>
        public float StartTime;
        #endregion

        #region Decks
        /// <summary>
        /// The list of cards from the drawing deck. Drawing is done automatically at the end of each turn.
        /// </summary>
        public Deck Dealer;

        /// <summary>
        /// The Staircase (Goal) stack of cards for S, W, N and E players.
        /// This is filled automatically by the game at the start. Top card is displayed.
        /// </summary>
        #region StaircaseStacks
        public Stack<Deck> Sstaircase;
        public Stack<Deck> Wstaircase;
        public Stack<Deck> Nstaircase;
        public Stack<Deck> Estaircase;
        #endregion

        /// <summary>
        /// The list of hand cards for South (S), West (W), North (N) and East (E) players.
        /// Filled automatically by the game at the start, and whenthe player ends their turn by discarding. 
        /// </summary>
        #region HandCards
        public List<Deck> ShandCards;
        public List<Deck> WhandCards;
        public List<Deck> NhandCards;
        public List<Deck> EhandCards;
        #endregion

        /// <summary>
        /// The Junk (Discard) pile of cards for S, W, N and E players.
        /// The players turn ends if they place a card here. Traditionally up to 3 cards only.
        /// </summary>
        #region JunkPiles
        public List<Deck> Sjunk;
        public List<Deck> Wjunk;
        public List<Deck> Njunk;
        public List<Deck> Ejunk;
        #endregion

        /// <summary>
        /// The list of play cells. This list will be filled as cards are played.
        /// </summary>
        #region PlayCells
        public List<Deck> SplayCell;
        public List<Deck> NplayCell; // A -> K
        public List<Deck> EplayCell;
        public List<Deck> WplayCell; // K -> A
        #endregion
        #endregion

        public StaircaseRummyBehavior()
        {
            Instance = this;
        }

        void Start()
        {
            StartCoroutine(NewGame(Seed));
        }

        #region UpdateAuto
        // Modify the right click or rid it altoghether
        void Update()
        {
            if (Input.GetMouseButtonDown(1)) // right click
                Autocomplete();
        }

        /// <summary>
        /// Moves allowed cards to the goal cells.
        /// </summary>
        /// <param name="onlySafe">If true it won't move cards that you still might need to place other cards on.</param>
        public void Autocomplete(bool onlySafe = false)
        {
            if (!isAutocompleteRunning)
                StartCoroutine(CheckForFoundationMoves(onlySafe));
        }
        #endregion

        /// <summary>
        /// Force the game to be won. Call this directly to test the win functionality.
        /// </summary>
        public void Win()
        {
            HasWon = true;
            GetComponent<AudioSource>().Play();

            StartCoroutine(DoWinAnimation());

            if (OnWin != null)
                OnWin();
        }
        
        public void ClearStaircases()
        {
            Sstaircase.Clear();
            Wstaircase.Clear();
            Nstaircase.Clear();
            Estaircase.Clear();
        }

        public void ClearHands()
        {
            ShandCards.Clear();
            WhandCards.Clear();
            NhandCards.Clear();
            EhandCards.Clear();
        }

        public void ClearJunk()
        {
            Sjunk.Clear();
            Wjunk.Clear();
            Njunk.Clear();
            Ejunk.Clear();
        }

        public void ClearPlayCells()
        {
            SplayCell.Clear();
            WplayCell.Clear();
            NplayCell.Clear();
            EplayCell.Clear();
        }

        /// <summary>
        /// Sets up the game and creates a new deal based on the given seed.
        /// </summary>
        public IEnumerator NewGame(int seed)
        {
            yield return new WaitForEndOfFrame(); // to give itween a chance to avoid it's initalization bug.

            GetComponent<AudioSource>().Play();
            HasWon = false;

            foreach (var card in FindObjectsOfType<Card>()) {
                Destroy(card.gameObject);
            }

            foreach (var deck in FindObjectsOfType<Deck>()) {
                if (deck != Dealer)
                    Destroy(deck.gameObject);
            }

            GameBehavior.Cards.Clear();

            Moves = new Stack<Move>();
            redoMoves = new Stack<Move>();

            ClearStaircases();
            for (int i = 0; i < Cells; i++)
            {
                CreateFreeCell();
            }

            GoalCells.Clear();
            var startPositionX = 0.625f + 0.05f;
            var xSpacing = Suits <= 6 ? 1.24f : 6.2f / (Suits - 1);
            for (int i = 0; i < Suits; i++)
            {
                var deck = (Instantiate(GameBehavior.Instance.DeckPrefab) as GameObject).GetComponent<Deck>();
                deck.CardSpacerY = 0.001f;
                deck.MaxCardsSpace = 15;
                deck.transform.position = new Vector3(startPositionX + (i * xSpacing), 3.3f, 0);
                deck.transform.parent = transform;

                deck.Type = DeckType.Goal;
                deck.Index = i;

                GoalCells.Add(deck);
            }

            PlayCells.Clear();
            startPositionX = ((Stacks - 1) / 2f) * -1.24f;
            for (int i = 0; i < Stacks; i++)
            {
                var deck = (Instantiate(GameBehavior.Instance.DeckPrefab) as GameObject).GetComponent<Deck>();
                deck.enabled = true;
                deck.CardSpacerY = PlayStackVerticalSpace;
                deck.MaxCardsSpace = 13;
                deck.transform.position = new Vector3(startPositionX + (i * 1.24f), 1.6f, 0);
                deck.transform.parent = transform;

                deck.Type = DeckType.Play;

                PlayCells.Add(deck);
            }

            Dealer.CreateCards(Suits);

            var random = new System.Random(seed);
            Dealer.Shuffle(3, random);
            Dealer.FlipAllCards();

            Dealer.CardSpacerX = 0;
            Dealer.MaxCardsSpace = 20;

            foreach (var card in Dealer.Cards)
            {
                card.Visible = true;
            }

            var count = 0;
            while (Dealer.Cards.Count > 0)
            {
                Dealer.Draw(PlayCells[count % PlayCells.Count], 1);
                count++;
            }

            if (Difficulty < 0)
            {
                for (int i = 0; i < -Difficulty * Suits; i++)
                {
                    // find all aces thru threes that aren't on top of deck already
                    var potentialCards = (from c in GameBehavior.Cards where c.Number <= 3 && c.Deck.TopCard != c select c).ToArray();

                    if (potentialCards.Length > 0)
                    {
                        var card = potentialCards[random.Next(0, potentialCards.Length)];
                        //print("Pushing up " + card.ToString());
                        card.SetDeck(card.Deck.Cards.IndexOf(card) + 1);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Difficulty * Suits; i++)
                {
                    // find all aces thru threes that aren't on bottom of deck already
                    var potentialCards = (from c in GameBehavior.Cards where c.Number <= 3 && c.Deck.BottomCard != c select c).ToArray();

                    if (potentialCards.Length > 0)
                    {
                        var card = potentialCards[random.Next(0, potentialCards.Length)];
                        //print("Pushing down " + card.ToString());
                        card.SetDeck(card.Deck.Cards.IndexOf(card) - 1);
                    }
                }
            }

            CalculateDragableCards();

            StartTime = Time.time;
        }

        /// <summary>
        /// Creates a new free cell deck.
        /// </summary>
        /// <returns>The created deck.</returns>
        public Deck CreateFreeCell()
        {
            if (FreeCellBehavior.Instance.FreeCells.Count >= FreeCellBehavior.MaxFreeCells)
                return null;

            var deck = (Instantiate(GameBehavior.Instance.DeckPrefab) as GameObject).GetComponent<Deck>();
            deck.CardSpacerY = 0.001f;
            deck.MaxCardsSpace = 15;
            deck.transform.parent = transform;
            deck.Type = DeckType.Free;
            deck.GetComponent<SpriteRenderer>().sprite = GameBehavior.Instance.CardSprites[53];

            FreeCells.Add(deck);
            LayoutFreeCells();

            return deck;
        }

        void LayoutFreeCells()
        {
            var startPositionX = 0.625f - 0.05f - 1.25f;

            var count = 0;
            foreach (var deck in FreeCells)
            {
                deck.transform.position = new Vector3(startPositionX - (count * 1.24f), 3.3f, 0);
                count++;
            }
        }

        /// <summary>
        /// Counts the number of availble slots to place cards.
        /// </summary>
        int CountEmpty(List<Deck> decks)
        {
            var result = 0;
            foreach (var deck in decks)
            {
                if (deck.Cards.Count == 0)
                    result++;
            }
            return result;
        }

        /// <summary>
        /// Calculates how many cards can be dragged at once.
        /// </summary>
        int MovableStackLimit(bool isTargetEmptyPlayCell)
        {
            // source http://www.solitairecentral.com/articles/FreecellPowerMovesExplained.html
            return (1 + CountEmpty(FreeCells)) * (int)Math.Pow(2, CountEmpty(PlayCells) - (isTargetEmptyPlayCell ? 1 : 0));
        }

        /// <summary>
        /// Most of the freecell logic is actually implemented here. This method restricts what cards can be dropped on each deck.
        /// </summary>
        /// <returns>True if the card was successfully dragged, false if the target deck is not a valid move.</returns>
        public bool CardDrag(Card card, Deck targetDeck)
        {
            if (card.Deck != targetDeck)
            {
                var isSingleCard = card.Deck.TopCard == card;

                // To Free Cells
                foreach (var deck in FreeCells)
                {
                    if (deck == targetDeck)
                    {
                        if (isSingleCard && targetDeck.Cards.Count == 0)
                        {
                            DoMove(Move.Single(card, targetDeck));
                            return true;
                        }
                    }
                }

                // To Play Cells
                foreach (var deck in PlayCells)
                {
                    if (deck == targetDeck)
                    {
                        if (targetDeck.Cards.Count == 0 || (targetDeck.TopCard.Color != card.Color && card.Number + 1 == targetDeck.TopCard.Number))
                        {
                            if (DoStackMove(card, targetDeck))
                                return true;
                        }
                    }
                }

                // To Goal Cells
                foreach (var deck in GoalCells)
                {
                    if (deck == targetDeck)
                    {
                        if (isSingleCard)
                        {
                            if (targetDeck.Cards.Count == 0)
                            {
                                if (card.Rank == CardRank.Ace)
                                {
                                    DoMove(Move.Single(card, targetDeck));
                                    return true;
                                }
                            }
                            else
                            {
                                if (targetDeck.TopCard.Suit == card.Suit && card.Number - 1 == targetDeck.TopCard.Number)
                                {
                                    DoMove(Move.Single(card, targetDeck));
                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        bool DoStackMove(Card card, Deck targetDeck)
        {
            var stackSize = card.Deck.Cards.Count - card.Deck.Cards.IndexOf(card);
            var limit = MovableStackLimit(targetDeck.Cards.Count == 0);
            if (stackSize <= limit)
            {
                var move = Move.Stack(card.Deck, targetDeck);
                for (int i = card.Deck.Cards.IndexOf(card); i < card.Deck.Cards.Count; i++)
                    move.Cards.Add(card.Deck.Cards[i]);
                DoMove(move);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Actually performs a move. All moves should be called thru this to maintain behavior and undo.
        /// </summary>
        public void DoMove(Move move)
        {
            move.Execute();
            Moves.Push(move);

            if (!HasWon)
            {
                CalculateDragableCards();
                if (!CheckWin())
                    Autocomplete(true);
            }
        }

        /// <summary>
        /// Called when a card is double clicked on.
        /// 
        /// If it can be moved to a goal deck it will be moved there. Other wise it will move to a free cell or an empty play cell.
        /// </summary>
        /// <param name="card"></param>
        public void OnDoubleClick(Card card)
        {
            if (card.IsDragable)
            {
                if (card.Deck.TopCard == card)
                {
                    // see if it can go on a goal deck
                    if (card.Rank == CardRank.Ace)
                    {
                        foreach (var goalDeck in GoalCells)
                        {
                            if (!goalDeck.HasCards)
                            {
                                DoMove(Move.Single(card, goalDeck));
                                return;
                            }
                        }
                    }
                    else
                    {
                        foreach (var goalDeck in GoalCells)
                        {
                            if (goalDeck.HasCards && goalDeck.TopCard.Suit == card.Suit && goalDeck.TopCard.Number == card.Number - 1)
                            {
                                DoMove(Move.Single(card, goalDeck));
                                return;
                            }
                        }
                    }
                }

                // then see if it can go on a play deck with cards
                foreach (var deck in PlayCells)
                {
                    if (deck != card.Deck &&
                        deck.HasCards &&
                        deck.TopCard.Number == card.Number + 1 &&
                        deck.TopCard.Color != card.Color)
                    {
                        if (DoStackMove(card, deck))
                            return;
                    }
                }

                if (card.Deck.TopCard == card && card.Deck.Type != DeckType.Free)
                {
                    // then see if it can go on a free cell
                    foreach (var deck in FreeCells)
                    {
                        if (!deck.HasCards)
                        {
                            DoMove(Move.Single(card, deck));
                            return;
                        }
                    }
                }

                // then see if it can go on an empty play deck
                foreach (var deck in PlayCells)
                {
                    if (!deck.HasCards)
                    {
                        if (DoStackMove(card, deck))
                            return;
                    }
                }
            }
        }

        /// <summary>
        /// Check to see if the game has been won.
        /// </summary>
        /// <returns>True if all the cards are in the goal cells.</returns>
        bool CheckWin()
        {
            foreach (var deck in GoalCells)
            {
                if (deck.Cards.Count < 13)
                    return false;
            }

            Win();

            return true;
        }

        /// <summary>
        /// Plays the win animation.
        /// </summary>
        IEnumerator DoWinAnimation()
        {
            yield return new WaitForSeconds(0.5f);

            redoMoves.Clear();

            while (HasWon)
            {
                if (isRedoing)
                {
                    if (redoMoves.Count > 0)
                        Redo();
                    else
                        isRedoing = false;
                }
                else
                {
                    if (Moves.Count > 0)
                        Undo();
                    else
                        isRedoing = true;
                }
                yield return new WaitForSeconds(0.15f);
            }
        }

        /// <summary>
        /// Check for correct running cards and adjust dragable cards and checks for completed full stacks
        /// </summary>
        protected void CalculateDragableCards()
        {
            var maxMovableStackLimit = MovableStackLimit(false);
            //print(maxMovableStackLimit);

            //Loop on each stack
            foreach (var playCell in PlayCells)
            {
                //Loop on each card from bottom up, and make it enabled until a wrong card placement or an invisible card occur
                var correctOrder = true;
                var correctCount = 0;
                for (int j = playCell.Cards.Count - 1; j >= 0; j--)
                {
                    playCell.Cards[j].IsDragable = correctOrder;

                    if (OnCardDragableUpdate != null)
                    {
                        OnCardDragableUpdate(playCell.Cards[j], correctOrder);
                    }

                    if (correctOrder) //If we still in a correct order state check for the next card
                    {
                        if ((j != 0) &&
                            (
                                (playCell.Cards[j - 1].Visible == false) ||
                                (IsWrongPlacement(playCell.Cards[j], playCell.Cards[j - 1])) ||
                                correctCount >= maxMovableStackLimit - 1
                            ))
                        {
                            correctOrder = false;
                        }
                        correctCount++;
                    }
                }
            }

            // Free cells and goal cells are draggable too (cancel attacks)
            foreach (var freecell in FreeCells)
            {
                if (freecell.TopCard != null)
                {
                    freecell.TopCard.IsDragable = true;

                    if (OnCardDragableUpdate != null)
                        OnCardDragableUpdate(freecell.TopCard, true);
                }
            }

            foreach (var goalCell in GoalCells)
            {
                if (goalCell.TopCard != null)
                {
                    goalCell.TopCard.IsDragable = true;

                    if (OnCardDragableUpdate != null)
                        OnCardDragableUpdate(goalCell.TopCard, true);
                }
            }
        }

        /// <summary>
        /// Return false if the two cards are not in the same suit and in ordered numbers
        /// </summary>
        /// <param name="card1"></param>
        /// <param name="card2"></param>
        /// <returns></returns>
        public bool IsWrongPlacement(Card card1, Card card2)
        {
            if ((card1.Color != card2.Color) && (card1.Number + 1 == card2.Number))
                return false;
            else
                return true;
        }

        /// <summary>
        /// Undoes the last move.
        /// </summary>
        public void Undo()
        {
            if (Moves.Count > 0)
            {
                var move = Moves.Pop();
                move.Undo();
                CalculateDragableCards();
                redoMoves.Push(move);
            }
        }

        /// <summary>
        /// Used by the win animation to replay the game moves that have been undone.
        /// </summary>
        void Redo()
        {
            if (redoMoves.Count > 0)
            {
                var move = redoMoves.Pop();
                DoMove(move);
            }
        }

        /// <summary>
        /// Check for and move all cards that can go to the goal decks.
        /// </summary>
        IEnumerator CheckForFoundationMoves(bool onlySafe)
        {
            isAutocompleteRunning = true;
            var recurse = false;

            foreach (var deck in FreeCells)
            {
                if (CheckDeck(deck, onlySafe))
                {
                    yield return new WaitForSeconds(0.1f);
                    recurse = true;
                }
            }

            foreach (var deck in PlayCells)
            {
                if (CheckDeck(deck, onlySafe))
                {
                    yield return new WaitForSeconds(0.1f);
                    recurse = true;
                }
            }

            if (recurse && !HasWon)
                StartCoroutine(CheckForFoundationMoves(onlySafe));
            else
                isAutocompleteRunning = false;
        }

        /// <summary>
        /// Check a deck for cards that can go to a goal deck and move them.
        /// </summary>
        bool CheckDeck(Deck deck, bool onlySafe)
        {
            if (deck.HasCards)
            {
                var cardToMove = deck.TopCard;
                var goalDeck = GetGoalDeckForCard(cardToMove);
                if (goalDeck != null)
                {
                    if (!onlySafe || IsCardSafeToMove(cardToMove))
                    {
                        DoMove(Move.Single(deck.TopCard, goalDeck));
                        return true;
                    }
                }
            }

            return false;
        }

        bool IsCardSafeToMove(Card cardToMove)
        {
            // Check for cards of opposite color and rank one lower
            var rank = (CardRank)cardToMove.Rank - 1;
            var color = cardToMove.Color == CardColor.Black ? CardColor.Red : CardColor.Black;
            foreach (var deck in PlayCells)
            {
                if (deck.Has(rank, color))
                    return false;
            }
            foreach (var cell in FreeCells)
            {
                if (cell.Has(rank, color))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Finds the goal deck that a card will go to.
        /// </summary>
        public Deck GetGoalDeckForCard(Card card)
        {
            foreach (var goalDeck in GoalCells)
            {
                if (!goalDeck.HasCards && card.Rank == CardRank.Ace)
                    return goalDeck;
                if (goalDeck.HasCards && goalDeck.TopCard.Suit == card.Suit && goalDeck.TopCard.Number == card.Number - 1)
                    return goalDeck;
            }
            return null;
        }
    }
}