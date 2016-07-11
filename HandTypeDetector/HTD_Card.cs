
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HandTypeDetector {
    /// <summary>
    /// HandTypeDetector -- Card
    /// </summary>
    class HTD_Card {
        
        // Spade, Heart, Clubs, Diamond
        public static char[] SUITS = "shcd".ToCharArray();

        public static char[] NUMS = "23456789TJQKA".ToCharArray();

        private HTD_Card() { }

        public int suitIndex = -1;
        public int numIndex = -1;

        public static int CharToNumIndex(char input) {
            int output = -1;
            for (int i = 0; i < NUMS.Length; i++) {
                if (NUMS[i] == input) {
                    output = i;
                    break;
                }
            }
            return output;
        }

        public static int CharToSuitIndex(char input) {
            int output = -1;
            for (int i = 0; i < SUITS.Length; i++) {
                if (SUITS[i] == input) {
                    output = i;
                    break;
                }
            }
            return output;
        }

        public static HTD_Card Create(string input) {
            if (string.IsNullOrEmpty(input)) {
                return null;
            }
            char[] inputAsCharArray = input.ToCharArray();
            if (inputAsCharArray.Length != 2) {
                return null;
            }
            int suitIndexTemp = CharToSuitIndex(inputAsCharArray[1]);
            int numIndexTemp = CharToNumIndex(inputAsCharArray[0]);
            if (suitIndexTemp > 0 && numIndexTemp > 0) {
                HTD_Card output = new HTD_Card();
                output.suitIndex = suitIndexTemp;
                output.numIndex = numIndexTemp;
                return output;
            } else {
                return null;
            }
        }
    }
}
