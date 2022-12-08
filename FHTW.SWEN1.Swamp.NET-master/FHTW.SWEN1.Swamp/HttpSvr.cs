using System;
using System.Net;
using System.Net.Sockets;
using System.Text;



namespace FHTW.SWEN1.Swamp {
    public delegate void IncomingEventHandler(object sender, HttpSvrEventArgs e);

    public sealed class HttpSvr {
        private TcpListener _Listener;

        public event IncomingEventHandler Incoming;

        public void Run() {
            _Listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 12000);
            _Listener.Start();

            byte[] buf = new byte[256];
            int n;
            string data;

            while(true) {
                TcpClient client = _Listener.AcceptTcpClient();                 // wait for a client to connect

                NetworkStream stream = client.GetStream();                      // get the client stream
                
                data = "";
                while(stream.DataAvailable || (data == ""))
                {                                                               // read and decode stream
                    n = stream.Read(buf, 0, buf.Length);
                    data += Encoding.ASCII.GetString(buf, 0, n);
                }

                Incoming?.Invoke(this, new HttpSvrEventArgs(data, client));
            }
        }
    }
}
