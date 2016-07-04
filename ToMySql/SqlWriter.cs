
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
    class SqlWriter {
        public void WriteToPreFlopTable(HandHistory hand, MySqlConnection connection) { 
        }

        public void PlayerInfoStat(HandHistory hand, 
            Dictionary<string, decimal> playerProfitMap, Dictionary<string, int> vpCount, 
            Dictionary<string, int> raiseInCount, Dictionary<string, int> allCount
        ){
            // 这一handHistory中，入池的玩家以及加注入池玩家的登记表
            List<string> inpotPlayers = new List<string>();
            List<string> raiseInPreFlopPlayers = new List<string>();

            // 计算位置信息
            List<int> seatNumbers = new List<int>();
            foreach (Player p in hand.Players) {
                seatNumbers.Add(p.SeatNumber);
            }
            Dictionary<int, int> positionMap = Program.SeatPositionMap(seatNumbers, hand.DealerButtonPosition);
            
            // 遍历所有的行动，计算玩家的收益，以及入池的玩家
            foreach (HandAction ha in hand.HandActions) {
                string playerName = ha.PlayerName;
                decimal amount = ha.Amount;
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
    }
}
