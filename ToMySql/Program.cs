using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HandHistories.Objects.GameDescription;
using HandHistories.Objects.Players;
using HandHistories.Objects.Actions;
using HandHistories.Parser.Parsers.Factory;
using System.Diagnostics;
using HandHistories.Parser.Parsers.FastParser.Base;
using HandHistories.Parser.Parsers.Exceptions;
using MySql.Data.MySqlClient;
using HandHistories.Objects.Cards;

namespace ToMySql {

    class Program {

        public static string[] RAISE_TYPES = new string[] { 
            "OPEN","3BET","4BET","5BET+"
        };

        static void Main(string[] args) {

            IHandHistoryParserFactory factory = new HandHistoryParserFactoryImpl();
            var handParser = factory.GetFullHandHistoryParser(SiteName.Pacific);
            string path = "D:\\PokerData_Downloaded";
            string[] fileNames = System.IO.Directory.GetFiles(path);

            // 玩家的盈利表
            Dictionary<string, decimal> playerProfitMap = new Dictionary<string, decimal>();
            // 玩家的入池计数
            Dictionary<string, int> vpCount = new Dictionary<string, int>(); 
            // 玩家加注入池计数
            Dictionary<string, int> raiseInCount = new Dictionary<string, int>(); 
            // 玩家全部牌局计数
            Dictionary<string, int> allCount = new Dictionary<string, int>();

            // db parameters
            string connectionString = @"server=localhost;userid=root;";
            MySqlConnection connection = null;
            try {
                connection = new MySqlConnection(connectionString);
                connection.Open();
                Console.WriteLine("MySQL version : {0}", connection.ServerVersion);

                // Create db, table
                MySqlCommand cmdCreateBase = new MySqlCommand(
                    "CREATE DATABASE IF NOT EXISTS pokerData;",
                    connection);
                cmdCreateBase.ExecuteNonQuery();
                MySqlCommand cmdUseDatabase = new MySqlCommand(
                    "USE pokerData;", connection
                    );
                cmdUseDatabase.ExecuteNonQuery();

                MySqlCommand cmdCreatePlayerProfitTable = new MySqlCommand(
                    "CREATE TABLE IF NOT EXISTS playerInfo" +
                    "(PlayerName varchar(255), Profit decimal(30,2), vpCount int, raiseCount int, allCount int);",
                    connection);

                cmdCreatePlayerProfitTable.ExecuteNonQuery();

                MySqlCommand cmdCreatePreFlopTable = new MySqlCommand(
                    "CREATE TABLE IF NOT EXISTS preFlop" +
                    "(PlayerName varchar(255)," +
                    "HoleCards varchar(255), Chips decimal(30,10), Position int, " +
                    "ActionType varchar(255), Amount decimal(30, 10));",
                    connection);
                cmdCreatePreFlopTable.ExecuteNonQuery();

                foreach (string fileName in fileNames) {
                    Console.WriteLine("Parsing : " + fileName);
                    string text = System.IO.File.ReadAllText(fileName);

                    HandHistoryParserFastImpl fastParser = handParser as HandHistoryParserFastImpl;

                    var hands = fastParser.SplitUpMultipleHandsToLines(text);
                    // --- PreFlop Data Record ---
                    foreach (var hand in hands) {
                        var parsedHand = fastParser.ParseFullHandHistory(hand, true);
                        List<int> seatNumbers = new List<int>();
                        foreach (Player p in parsedHand.Players) {
                            seatNumbers.Add(p.SeatNumber);
                        }
                        Dictionary<int, int> positionMap = SeatPositionMap(seatNumbers, parsedHand.DealerButtonPosition);
                        int raiseCount = 0;
                        foreach (HandAction ha in parsedHand.HandActions) {
                            Player player = parsedHand.Players[ha.PlayerName];
                            string actionType = ha.HandActionType.ToString();
                            if (actionType == "RAISE") {
                                actionType = RAISE_TYPES[raiseCount];
                                if (raiseCount < RAISE_TYPES.Length - 1) {
                                    raiseCount++;
                                }
                            }
                            if (player.HoleCards != null && ha.Street == Street.Preflop) {
                                // Only record data with holecards.
                                string holeCards = player.HoleCards.ToString();
                                decimal chips = player.StartingStack / parsedHand.GameDescription.Limit.BigBlind;
                                int position = positionMap[player.SeatNumber];
                                decimal amount = ha.Amount / parsedHand.GameDescription.Limit.BigBlind;
                                MySqlCommand cmdInsertHandAction = new MySqlCommand(
                                    "INSERT INTO preFlop (PlayerName, HoleCards, Chips, Position, ActionType, Amount) " +
                                    "VALUES (\'" + player.PlayerName.ToString() + "\',\'" + holeCards + "\', " + chips + ", " + position + ",\'" + actionType + "\'," + amount + " )",
                                    connection);
                                cmdInsertHandAction.ExecuteNonQuery();
                            }
                        }
                    }

                    // --- Player Info ---
                    
                    foreach (var hand in hands) {
                        var parsedHand = fastParser.ParseFullHandHistory(hand, true);
                        
                    }
                }
                foreach (string playerName in playerProfitMap.Keys) {
                    MySqlCommand cmdInsertProfit = new MySqlCommand(
                        "INSERT INTO playerInfo (PlayerName, Profit, vpCount int, raiseCount int, allCount int)" +
                        "VALUES (\'" + playerName + "\', " + playerProfitMap[playerName] + ")"
                        , connection);
                    cmdInsertProfit.ExecuteNonQuery();
                }
            } catch (Exception ex) {
                Console.WriteLine(ex.StackTrace);
                Console.Read();
                // DO NOTHING
            } finally {
                if (connection != null) {
                    connection.Close();
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
    }
}
