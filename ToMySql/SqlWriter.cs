
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using HandHistories.Objects.Hand;
using HandHistories.Objects.Actions;
using HandHistories.Objects.Cards;
using HandHistories.Objects.Players;

namespace ToMySql {

    /// <summary>
    /// 统计
    /// </summary>
    class Statics {
        public static string[] RAISE_TYPES = new string[] { 
            "OPEN","3BET","4BET","5BET+"
        };
        public static string[] CALL_TYPES = new string[] { 
            "CALL", "CALL_OPEN", "CALL_3BET", "CALL_4BET", "CALL_5BET+"
        };

        public static string[] COMMUNITY_TYPES = new string[]{
            "DRY", "FLUSH_DRAW", "FLUSH", "STRAIGHT_DRAW", "ABB", "TRIPLE"
        };

        public static string[] HAND_TYPES = new string[] { 
            "TPTK", "TPSK", "TPWK", "2PAIR", "SET", "FLUSH", "STRAIGHT", "FULL_HOUSE+"
        };

        /// <summary>
        /// 根据解析好的手牌记录将玩家的翻牌前行为写入数据库
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="connection"></param>
        public static void WriteToPreFlopTable(HandHistory hand, MySqlConnection connection) {
            List<int> seatNumbers = new List<int>();
            foreach (Player p in hand.Players) {
                seatNumbers.Add(p.SeatNumber);
            }
            Dictionary<int, int> positionMap = SeatPositionMap(seatNumbers, hand.DealerButtonPosition);
            int raiseCount = 0;
            int callCount = 0;
            foreach (HandAction ha in hand.HandActions) {
                Player player = hand.Players[ha.PlayerName];
                string actionType = ha.HandActionType.ToString();
                if (actionType == "RAISE") {
                    actionType = RAISE_TYPES[raiseCount];
                    if (raiseCount < RAISE_TYPES.Length - 1) {
                        raiseCount++;
                    }
                    if (callCount < CALL_TYPES.Length - 1) {
                        callCount++;
                    }
                }
                if (actionType == "CALL") {
                    actionType = CALL_TYPES[callCount];
                    if (callCount < CALL_TYPES.Length - 1) {
                        callCount++;
                    }
                }
                if (player.HoleCards != null && ha.Street == Street.Preflop) {
                    // Only record data with holecards.
                    string holeCards = player.HoleCards.ToString();
                    decimal chips = player.StartingStack / hand.GameDescription.Limit.BigBlind;
                    int position = positionMap[player.SeatNumber];
                    decimal amount = ha.Amount / hand.GameDescription.Limit.BigBlind;
                    MySqlCommand cmdInsertHandAction = new MySqlCommand(
                        "INSERT INTO preFlop (PlayerName, HoleCards, Chips, Position, ActionType, Amount) " +
                        "VALUES (\'" + player.PlayerName.ToString() + "\',\'" + HoleCardsSimplify(holeCards) + "\', " + chips + ", " + position + ",\'" + actionType + "\'," + amount + " )",
                        connection);
                    cmdInsertHandAction.ExecuteNonQuery();
                }
            }
        }


        /// <summary>
        /// 根据解析好的hand统计玩家信息。
        /// 但是这里不包含写入数据库的代码，因为需要处理完所有的hands后，玩家的统计数据才算完成，才可以写入
        /// </summary>
        public static void PlayerInfoStat(HandHistory hand,
            Dictionary<string, decimal> playerProfitMap, Dictionary<string, int> vpCount,
            Dictionary<string, int> raiseInCount, Dictionary<string, int> allCount
        ) {
            // 这一handHistory中，入池的玩家以及加注入池玩家的登记表
            List<string> inpotPlayers = new List<string>();
            List<string> raiseInPreFlopPlayers = new List<string>();

            // 计算位置信息
            List<int> seatNumbers = new List<int>();
            foreach (Player p in hand.Players) {
                seatNumbers.Add(p.SeatNumber);
            }
            Dictionary<int, int> positionMap = SeatPositionMap(seatNumbers, hand.DealerButtonPosition);

            // 遍历所有的行动，计算玩家的收益，以及入池的玩家
            foreach (HandAction ha in hand.HandActions) {
                string playerName = ha.PlayerName;
                decimal amount = ha.Amount / hand.GameDescription.Limit.BigBlind;
                // 计算一局内玩家的收益
                if (!playerProfitMap.ContainsKey(playerName)) {
                    playerProfitMap.Add(playerName, amount);
                } else {
                    playerProfitMap[playerName] += amount;
                }
                // 计算翻牌前入池信息
                if (ha.Street == Street.Preflop) {
                    if (ha.HandActionType == HandActionType.CALL) {
                        if (!inpotPlayers.Contains(playerName)) {
                            inpotPlayers.Add(playerName);
                        }
                    } else if (ha.HandActionType == HandActionType.RAISE) {
                        if (!inpotPlayers.Contains(playerName)) {
                            inpotPlayers.Add(playerName);
                        }
                        if (!raiseInPreFlopPlayers.Contains(playerName)) {
                            raiseInPreFlopPlayers.Add(playerName);
                        }
                    }
                }
            }

            // 将统计信息记录
            foreach (Player p in hand.Players) {
                string playerName = p.PlayerName;
                if (allCount.ContainsKey(playerName)) {
                    allCount[playerName]++;
                } else {
                    allCount.Add(playerName, 1);
                }
                if (!vpCount.ContainsKey(playerName)) {
                    vpCount.Add(playerName, 0);
                }
                if (!raiseInCount.ContainsKey(playerName)) {
                    raiseInCount.Add(playerName, 0);
                }
            }
            foreach (string playerName in inpotPlayers) {
                if (vpCount.ContainsKey(playerName)) {
                    vpCount[playerName]++;
                } else {
                    vpCount.Add(playerName, 1);
                }
            }
            foreach (string playerName in raiseInPreFlopPlayers) {
                if (raiseInCount.ContainsKey(playerName)) {
                    raiseInCount[playerName]++;
                } else {
                    raiseInCount.Add(playerName, 1);
                }
            }
        }

        /// <summary>
        /// 座位与位置之间的映射
        /// </summary>
        /// <returns></returns>
        public static Dictionary<int, int> SeatPositionMap(List<int> seatNumbers, int buttonSeat) {
            Dictionary<int, int> output = new Dictionary<int, int>();
            List<int> outputList = new List<int>();
            List<int> listA = new List<int>();
            List<int> listB = new List<int>();
            for (int i = 0; i < seatNumbers.Count; i++) {
                if (i > buttonSeat) {
                    listA.Add(seatNumbers[i]);
                } else {
                    listB.Add(seatNumbers[i]);
                }
            }
            outputList.AddRange(listA);
            outputList.AddRange(listB);
            for (int i = 0; i < outputList.Count; i++) {
                output.Add(outputList[i], i);
            }
            return output;
        }

        public static string HoleCardsSimplify(string input) {
            char[] charArray = input.ToCharArray();
            string temp = charArray[0].ToString() + charArray[2].ToString();
            if (charArray[0] != charArray[2]) {
                if (charArray[1] == charArray[3]) {
                    temp += "s";
                } else {
                    temp += "o";
                }
            }
            return temp;
        }
    }
}
