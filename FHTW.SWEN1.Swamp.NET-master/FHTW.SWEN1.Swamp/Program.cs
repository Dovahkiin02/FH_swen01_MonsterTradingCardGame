using System;
using System.Data;
using System.Data.SQLite;

namespace FHTW.SWEN1.Swamp
{
    /// <summary>This is the program class for the application.</summary>
    public static class Program
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private static members                                                                                   //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Database connection.</summary>
        private static IDbConnection _Cn;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // main entry point                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Entry point.</summary>
        /// <param name="args">Command line arguments.</param>
        static void Main(string[] args)
        {
            _Cn = new SQLiteConnection("Data Source=swamp.db;Version=3;");
            _Cn.Open();

            HttpSvr svr = new HttpSvr();
            svr.Incoming += _Svr_Incoming;

            svr.Run();
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // event handlers                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        /// <summary>Processes an incoming HTTP request.</summary>
        /// <param name="sender">Object that raised the event.</param>
        /// <param name="e">Event arguments.</param>
        private static void _Svr_Incoming(object sender, HttpSvrEventArgs e)
        {
            if(e.Path == "/messages")
            {                                                                   // no message id provided
                if(e.Method == "POST")
                {                                                               // POST: add new message
                    IDbCommand cmd = _Cn.CreateCommand();                       // create database command, insert message into database
                    cmd.CommandText = "INSERT INTO MESSAGES (DATA) VALUES (:m)";
                    IDataParameter p = cmd.CreateParameter();                   // make and bind parameter for message body
                    p.ParameterName = ":m";
                    p.Value = e.Payload;                    
                    cmd.Parameters.Add(p);
                    cmd.ExecuteNonQuery();
                    cmd.Dispose();

                    cmd = _Cn.CreateCommand();                                  // read autoincremented message ID
                    cmd.CommandText = "SELECT LAST_INSERT_ROWID()";
                    int id = Convert.ToInt32(cmd.ExecuteScalar());              // put ID into variable
                    cmd.Dispose();

                    Console.WriteLine("Saved message \"{0}\" as {1}", e.Payload.Replace("\n", "").Replace("\r", ""), id);
                    e.Reply(200, id.ToString());                                // create reply
                }
            }
            else
            {
                Console.WriteLine("Rejected message.");
                e.Reply(400);
            }
        }
    }
}
