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
            //string path = "D:\\PokerData_Test";
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
                    "(PlayerName varchar(255), Profit decimal(30,2), vpip decimal(30,2), pfr decimal(30,2), handsCount int);",
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
                    foreach (var hand in hands) {
                        var parsedHand = fastParser.ParseFullHandHistory(hand, true);
                        //Statics.PlayerInfoStat(parsedHand, playerProfitMap, vpCount, raiseInCount, allCount);
                        //Statics.WriteToPreFlopTable(parsedHand, connection);
                    }
                }
                foreach (string playerName in playerProfitMap.Keys) {
                    MySqlCommand cmdInsertProfit = new MySqlCommand(
                        "INSERT INTO playerInfo (PlayerName, Profit, Vpip, Pfr, HandsCount)" +
                        "VALUES (\'" + playerName + "\', " + (decimal)playerProfitMap[playerName] +
                        "," + (decimal)vpCount[playerName] / allCount[playerName] + "," +
                        (decimal)raiseInCount[playerName] / allCount[playerName] + "," + allCount[playerName] + ");"
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
    }
}
