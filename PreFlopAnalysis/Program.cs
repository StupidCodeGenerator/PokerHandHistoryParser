/*
 * The Plan :
 * 
 * 1. Get all players that profit > 1000;
 * 2. For all the players, find there preflop datas.
 * 3. Calculate the probabilities of OPEN, 3BET, 4BET, 5BET+, CALL, CALL_OPEN, CALL_3BET, CALL_4BET, CALL_5BET+
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace PreFlopAnalysis {
    class Program {
        public static string[] ALL_NUMS = new string[] { "A", "K", "Q", "J", "T", "9", "8", "7", "6", "5", "4", "3", "2" };
        public static string[] ALL_HANDS = AllHandStrings().ToArray();

        public static decimal PLAYER_PROFIT_LIMIT = 100; // Only consider players that above water 100 bbs

        static void Main(string[] args) {

            string connectionString = @"server=localhost;userid=root;";
            MySqlConnection connection = new MySqlConnection(connectionString);
            MySqlDataReader reader;
            connection.Open();

            MySqlCommand cmdUseDatabase = new MySqlCommand("USE pokerData;", connection);
            cmdUseDatabase.ExecuteNonQuery();
            
            // For that players, find frequencies about each kind of actions. 
            // for each holeCard discription (AKs, AQo .. etc), 
            // Search the db for actions. then count frequencies of the actions
            // the vector of each data record will be:
            // / HoleCards / CALL / OPEN(CALL_OPEN) / 3BET(CALL_3BET) / 4BET(CALL_4BET) / 5BET+(CALL_5BET+) /
            Dictionary<string, int> allCount = new Dictionary<string, int>();
            Dictionary<string, int> call = new Dictionary<string, int>();
            Dictionary<string, int> open = new Dictionary<string, int>();
            Dictionary<string, int> bet3 = new Dictionary<string, int>();
            Dictionary<string, int> bet4 = new Dictionary<string, int>();
            Dictionary<string, int> bet5p = new Dictionary<string, int>();
            foreach (string holeCards in ALL_HANDS) {
                if (!allCount.ContainsKey(holeCards)) {
                    allCount.Add(holeCards, 0);
                }
                if (!call.ContainsKey(holeCards)) {
                    call.Add(holeCards, 0);
                }
                if (!open.ContainsKey(holeCards)) {
                    open.Add(holeCards, 0);
                }
                if (!bet3.ContainsKey(holeCards)) {
                    bet3.Add(holeCards, 0);
                }
                if (!bet4.ContainsKey(holeCards)) {
                    bet4.Add(holeCards, 0);
                }
                if (!bet5p.ContainsKey(holeCards)) {
                    bet5p.Add(holeCards, 0);
                }
                string cmdStr = "SELECT ActionType FROM PreFlop INNER JOIN PlayerInfo ON PreFlop.PlayerName = PlayerInfo.PlayerName WHERE Profit > "+PLAYER_PROFIT_LIMIT+" and HoleCards = \'"+holeCards+"\';";
                Console.WriteLine(cmdStr);
                MySqlCommand cmdSelectActions = new MySqlCommand(cmdStr, connection);
                reader = cmdSelectActions.ExecuteReader();
                while (reader.Read()) {
                    allCount[holeCards]++;
                    string actionType = reader[0].ToString();
                    switch (actionType) {
                        case "CALL":
                            call[holeCards]++;
                            break;
                        case "OPEN":
                        case "CALL_OPEN":
                            open[holeCards]++;
                            break;
                        case "3BET":
                        case "CALL_3BET":
                            bet3[holeCards]++;
                            break;
                        case "4BET":
                        case "CALL_4BET":
                            bet4[holeCards]++;
                            break;
                        case "5BET+":
                        case "CALL_5BET+":
                            bet5p[holeCards]++;
                            break;
                    }
                }
                reader.Close();
            }

            System.IO.File.WriteAllText("result.csv", "HoleCards,CALL,3BET,4BET,5BETp,ALL\r\n");
            foreach (string holeCards in allCount.Keys) {
                string oneLine = holeCards + "," + call[holeCards] + "," + bet3[holeCards] + "," + bet4[holeCards] + "," + bet5p[holeCards] + ","+allCount[holeCards]+"\r\n";
                System.IO.File.AppendAllText("result.csv", oneLine);
            }

            connection.Close();
            Console.WriteLine("Press any key to exit.");
            Console.Read();
        }

        /// <summary>
        /// Generate AA,AKs ... 27o
        /// </summary>
        public static List<string> AllHandStrings() {
            List<string> output = new List<string>();
            for (int i = 0; i < ALL_NUMS.Length; i++) {
                for (int j = 0; j < ALL_NUMS.Length; j++) {
                    string temp = ALL_NUMS[i] + ALL_NUMS[j];
                    if (i > j) {
                        temp += "s";
                    } else if (i < j) {
                        temp += "o";
                    }
                    output.Add(temp);
                }
            }
            return output;
        }
    }
}
