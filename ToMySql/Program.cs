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
        static void Main(string[] args) {

            IHandHistoryParserFactory factory = new HandHistoryParserFactoryImpl();
            var handParser = factory.GetFullHandHistoryParser(SiteName.Pacific);
            string path = "D:\\PokerData_Downloaded";
            string[] fileNames = System.IO.Directory.GetFiles(path);

            // db parameters
            string connectionString = @"server=localhost;userid=root;";
            MySqlConnection connection = null;
            try {
                connection = new MySqlConnection(connectionString);
                connection.Open();
                Console.WriteLine("MySQL version : {0}", connection.ServerVersion);

                // Create db, table
                MySqlCommand cmdCreateBase = new MySqlCommand(
                    "CREATE DATABASE IF NOT EXISTS pokerData;" + "USE pokerData;" +
                    "CREATE TABLE IF NOT EXISTS preFlop" +
                    "(PlayerName varchar(255)" +
                    "HoleCards varchar(255), Chips decimal(255,10), Position int, " +
                    "ActionType varchar(255), Amount decimal(255, 10))",
                    connection);
                cmdCreateBase.ExecuteNonQuery();

                foreach (string fileName in fileNames) {
                    Console.WriteLine("Parsing : " + fileName);
                    string text = System.IO.File.ReadAllText(fileName);

                    HandHistoryParserFastImpl fastParser = handParser as HandHistoryParserFastImpl;

                    var hands = fastParser.SplitUpMultipleHandsToLines(text);
                    foreach (var hand in hands) {
                        var parsedHand = fastParser.ParseFullHandHistory(hand, true);
                        List<int> seatNumbers = new List<int>();
                        foreach (Player p in parsedHand.Players) {
                            seatNumbers.Add(p.SeatNumber);
                        }
                        Dictionary<int, int> positionMap = SeatPositionMap(seatNumbers, parsedHand.DealerButtonPosition);
                        // Find the best player
                        foreach (HandAction ha in parsedHand.HandActions) {
                            Player player = parsedHand.Players[ha.PlayerName];
                            string holeCards = player.HoleCards.ToString();
                            decimal chips = player.StartingStack;
                            int position = positionMap[player.SeatNumber];
                            string actionType = ha.HandActionType.ToString();
                            decimal amount = ha.Amount;
                            MySqlCommand cmdInsertHandAction = new MySqlCommand(
                                "INSERT INTO preFlop (PlayerName, HoleCards, Chips, Position, ActionType, Amount) " +
                                "VALUES (" + player.PlayerName + "," + holeCards + ", " + chips + ", " + position + "," + actionType + "," + amount + " )");
                            cmdInsertHandAction.ExecuteNonQuery();
                        }
                    }
                }
            } catch (Exception ex) {
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
