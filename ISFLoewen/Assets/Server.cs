using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

namespace TcpServerClient
{
    /*Call it with in a new Thread
     * if (_workerThread != null)
        {
            if (_workerThread.IsAlive)
            {
                _workerThread.Abort();
            }
        }
        _server = new Server2(tbServerIP.Text, Convert.ToInt32(tbServerPort.Text));
        _workerThread = new Thread(_server.DoWork);

        _workerThread.Start();
        Console.WriteLine("main thread: Starting worker thread...");
     */
    class Server
    {
		public delegate void ReadCallbackType(byte[] data,int len);

        //private volatile bool _shouldStop;
        private IPAddress _ip;
        private int _port;
        private List<TcpClient> _clients;
        private TcpListener _tcpListener;
        private int _maxClients;
        private byte[] myReadBuffer = new byte[1024];
        private ReadCallbackType _readCallBackFunc;
        //private Thread _workerThread;

        public Server(IPAddress ip, int port, int maxClients, ReadCallbackType callback)
        {
            _clients = new List<TcpClient>();
            _maxClients = maxClients;
            _ip = ip;
            _port = port;
            _tcpListener = new TcpListener(_ip, port);
            _readCallBackFunc = new ReadCallbackType(callback);


            //_workerThread = new Thread(this.DoWork);
            //_workerThread.Start();


            _tcpListener.Start();

			this._tcpListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), this._tcpListener);
            Console.WriteLine("Server is listen for connections");
        }
        /*
        public void stop()
        {
            this._shouldStop = true;
        }

        private void DoWork()
        {
            StartListening(_ip.ToString(), _port);
            while (!_shouldStop)
            {
                //Console.WriteLine("worker thread: working...");
            }
        }
         * */

		public void disconnect()
		{
			try{
				if(_tcpListener!=null){
					if(getClients().Count>0){
						removeClient(getClients()[0]);
					}
					_tcpListener.Stop ();
				}
			}
			catch(Exception ex)
			{
			}
		}

        public List<TcpClient> getClients()
        {
            return _clients;
        }

        public void removeClient(TcpClient client)
        {
            if (_clients.Remove(client))
            {
				Console.WriteLine("INFO: Client was Removed from the List");
				if (this._clients.Count<this._maxClients)
					this._tcpListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), this._tcpListener);
            }
            else
            {
                Console.WriteLine("WARNING: Cannot remove Client from list, because it is not in the List");
            }

        }

        /*
        public void StartListening(string ip, int port)
        {
            Console.WriteLine("Start Listening");
            _tcpListener.Start();
            this._tcpListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), this._tcpListener);
        }

         * */

        private void DoAcceptTcpClientCallback(IAsyncResult ar)
        {
            TcpListener tcpListener = (TcpListener)ar.AsyncState;

            TcpClient client = tcpListener.EndAcceptTcpClient(ar);
            NetworkStream netStream = client.GetStream();
            netStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length, new AsyncCallback(myReadCallBack), netStream);
            _clients.Add(client);
            Console.WriteLine("New Client connected");
            
            if (this._clients.Count<this._maxClients)
                this._tcpListener.BeginAcceptTcpClient(new AsyncCallback(DoAcceptTcpClientCallback), this._tcpListener);
        }

        /*
        public bool sendStringToClient(TcpClient client, string str)
        {
            try
            {
                NetworkStream netStream = client.GetStream();
                if (netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(str);
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
         * */

        private void myReadCallBack(IAsyncResult ar)
        {
            try
			{
				NetworkStream myNetworkStream = (NetworkStream)ar.AsyncState;
				int numberOfBytesRead = myNetworkStream.EndRead(ar);
				_readCallBackFunc(myReadBuffer,numberOfBytesRead);

				myNetworkStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length,
				                          new AsyncCallback(myReadCallBack),
				                          myNetworkStream);
				/*
                String myCompleteMessage = "";
                int numberOfBytesRead;

                myCompleteMessage = String.Concat(myCompleteMessage, Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));
                _readCallBackFunc(myCompleteMessage);

                myNetworkStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length,
                                                           new AsyncCallback(myReadCallBack),
                                                           myNetworkStream);
				*/
                //Console.WriteLine("You received the following message(" + myCompleteMessage.Length + ") : " + myCompleteMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR CLIENT MYREADCALLBACK:" + ex.Message);
            }
        }

        public bool sendData(TcpClient client, string str)
        {
            try
            {

                NetworkStream netStream = client.GetStream();
                if (netStream.CanWrite)
                {
                    Byte[] sendBytes = Encoding.UTF8.GetBytes(str);
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool sendData(TcpClient client, byte[] data)
        {
            try
            {

                NetworkStream netStream = client.GetStream();
                if (netStream.CanWrite)
                {
                    Byte[] sendBytes = data;
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
