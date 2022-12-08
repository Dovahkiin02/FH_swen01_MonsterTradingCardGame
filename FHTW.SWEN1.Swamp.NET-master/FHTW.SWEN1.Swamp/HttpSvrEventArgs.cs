using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;



namespace FHTW.SWEN1.Swamp
{
    /// <summary>This class provides event arguments for an HTTP server.</summary>
    public class HttpSvrEventArgs: EventArgs
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private members                                                                                          //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>TCP client.</summary>
        private TcpClient _Client;



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                             //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Creates a new instance of this class.</summary>
        /// <param name="tcp">HTTP message received from TCP listener.</param>
        public HttpSvrEventArgs(string tcp, TcpClient client)
        {
            _Client = client;

            string[] lines = tcp.Replace("\r\n", "\n").Replace("\r", "\n").Split("\n");
            bool inheaders = true;
            List<HttpHeader> headers = new List<HttpHeader>();

            for(int i = 0; i < lines.Length; i++)
            {
                if(i == 0)
                {
                    string[] inq = lines[0].Split(" ");
                    Method = inq[0];
                    Path = inq[1];
                }
                else if(inheaders)
                {
                    if(string.IsNullOrWhiteSpace(lines[i]))
                    {
                        inheaders = false;
                    }
                    else { headers.Add(new HttpHeader(lines[i])); }
                }
                else
                {
                    Payload += (lines[i] + "\r\n");
                }

                Headers = headers.ToArray();
            }
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                        //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>Get the HTTP method.</summary>
        public string Method
        {
            get; private set;
        }


        /// <summary>Gets the URL path.</summary>
        public string Path
        {
            get; private set;
        }


        /// <summary>Gets the HTTP headers.</summary>
        public HttpHeader[] Headers
        {
            get; private set;
        }


        /// <summary>Gets the HTTP payload.</summary>
        public string Payload
        {
            get; private set;
        }



        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public void Reply(int status, string payload = null)
        {
            string data;

            switch(status)
            {                                                                   // create response status string from code
                case 200:
                    data = "HTTP/1.1 200 OK\n";
                    break;
                case 400:
                    data = "HTTP/1.1 400 Bad Request\n";
                    break;
                default:
                    data = "HTTP/1.1 418 I'm a Teapot\n";
                    break;
            }

            if(string.IsNullOrEmpty(payload))
            {                                                                   // set Content-Length to 0 for empty content
                data += "Content-Length: 0\n";
            }
            data += "Content-Type: text/plain\n\n";

            if (payload != null) { data += payload; }

            byte[] dbuf = Encoding.ASCII.GetBytes(data);
            _Client.GetStream().Write(dbuf, 0, dbuf.Length);                    // send a response

            _Client.GetStream().Close();                                        // shut down the connection
            _Client.Dispose();
        }
    }
}
