using System.Collections;

namespace MonsterTradingCardGame {
    
    public class Collection : IEnumerable<Card> {
        public int Length => cards.Count;
        private List<Card> cards;      

        public Collection() {
            this.cards = new List<Card>();
        }

        public void addCard(Card card) {
            cards.Add(card);
        }

        public Card this[int index] {
            get { return cards[index]; }
        }

        public override string ToString() {
            string output = "";
            foreach (Card card in cards) {
                output += card.ToString();
            }
            return output;
        }

        public IEnumerator<Card> GetEnumerator() {
            return new CardEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new CardEnumerator(this);
        } 
    }

    public class CardEnumerator : IEnumerator<Card> {
        private Collection collection;
        private int currIndex;

        public CardEnumerator(Collection collection) {
            this.collection = collection!;
            currIndex = -1;
            Current = default;
        }
        public Card Current { get; private set; }

        Card IEnumerator<Card>.Current => Current;

        object IEnumerator.Current => Current;

        public void Dispose() {}

        public bool MoveNext() {
            if (++currIndex >= collection.Length) {
                return false;
            } else {
                Current = collection[currIndex];
            }
            return true;
        }

        public void Reset() {
            currIndex = -1;
        }
    }
}
