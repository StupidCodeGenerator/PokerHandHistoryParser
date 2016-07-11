using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandTypeDetector {
    public class HandTypeDetector {
        public static string[] FLOP_TYPES = new string[]{
            "DRY", "FLUSH_DRAW", "FLUSH", "STRAIGHT_DRAW", "FLUSH_STRAIGHT_DRAW", "ABB", "TRIPLE"
        };

        public static string[] HAND_TYPES = new string[] { 
            "TPTK", "TPSK", "TPWK", "2PAIR", "SET", "FLUSH", "STRAIGHT", "FULL_HOUSE", "QUADS", "FLUSH_STRAIGHT"
        };

        /// <summary>
        /// Community type in flop street
        /// </summary>
        public static string GetFlopType(string input) {
            char[] inputAsCharArray = input.ToCharArray();
            List<HTD_Card> cards = new List<HTD_Card>();
            for (int i = 0; i < inputAsCharArray.Length - 1; i += 2) {
                string cardStr = "";
                cardStr += inputAsCharArray[i];
                cardStr += inputAsCharArray[i + 1];
                HTD_Card card = HTD_Card.Create(cardStr);
                cards.Add(card);
            }
            if (cards.Count != 3) {
                return "INVALID";
            } else {
                if (cards[0].suitIndex == cards[1].suitIndex && cards[1].suitIndex == cards[2].suitIndex) {
                    cards = new List<HTD_Card>(from c in cards orderby c.numIndex ascending select c);
                    if (cards[1].numIndex - cards[0].numIndex == 1 && cards[2].numIndex - cards[1].numIndex == 1) {
                        return "FLUSH_STRAIGHT_DRAW";
                    } else {
                        return "FLUSH_DRAW";
                    }
                }
                // TODO 07-11
            }
        }
    }
}
