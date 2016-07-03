using System;
using MySql.Data.MySqlClient;

public class Example {

    static void Main() {
        string cs = @"server=localhost;userid=root;";

        MySqlConnection conn = null;

        try {
            conn = new MySqlConnection(cs);
            conn.Open();
            Console.WriteLine("MySQL version : {0}", conn.ServerVersion);
            Console.Read();
        } catch (MySqlException ex) {
            Console.WriteLine("Error: {0}", ex.ToString());
            Console.Read();
        } finally {
            if (conn != null) {
                conn.Close();
            }
        }
    }
}