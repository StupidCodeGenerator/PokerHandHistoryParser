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

namespace ToMySql {
    class Program {
        static void Main(string[] args) {

            Dictionary<string, decimal> playerAmounts = new Dictionary<string, decimal>();
            Dictionary<string, int> playerHandCounts = new Dictionary<string, int>();
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

                // Create db
                MySqlCommand cmd = new MySqlCommand("CREATE DATABASE IF NOT EXISTS pokerData;", connection);
                cmd.ExecuteNonQuery();

                foreach (string fileName in fileNames) {
                    Console.WriteLine("Parsing : " + fileName);
                    string text = System.IO.File.ReadAllText(fileName);

                    HandHistoryParserFastImpl fastParser = handParser as HandHistoryParserFastImpl;

                    var hands = fastParser.SplitUpMultipleHandsToLines(text);
                    foreach (var hand in hands) {
                        var parsedHand = fastParser.ParseFullHandHistory(hand, true);
                        // Find the best player
                        foreach (HandAction ha in parsedHand.HandActions) {
                            string playerName = ha.PlayerName;
                            decimal amount = ha.Amount;
                            if (!playerAmounts.ContainsKey(playerName)) {
                                playerAmounts.Add(playerName, amount);
                            } else {
                                playerAmounts[playerName] += amount;
                            }
                            if (!playerHandCounts.ContainsKey(playerName)) {
                                playerHandCounts.Add(playerName, 1);
                            } else {
                                playerHandCounts[playerName] += 1;
                            }
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
    }
}
