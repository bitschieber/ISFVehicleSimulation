using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Simulation;
using System.Runtime.InteropServices;
using SimulationUtils;

namespace TestClient
{
    class Client
    {
        TcpClient _tcpClient;
        byte[] myReadBuffer = new byte[1024];
		public List<string> _messages;
		private  static int _dataentry_t_size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DataEntry_t));
		public Dictionary<byte, DataEntry_t> data = new Dictionary<byte, DataEntry_t>();


        public Client(string ip, int port)
        {
            _tcpClient = new TcpClient(ip, port);
            _messages = new List<string>();
            initConnection();
        }

        private void initConnection()
		{
            NetworkStream netStream = _tcpClient.GetStream();
            Console.WriteLine("Connected to Server");
            if (netStream.CanWrite)
            {
                Byte[] sendBytes = Encoding.UTF8.GetBytes("Is anybody there?");
                //netStream.Write(sendBytes, 0, sendBytes.Length);
            }
            else
            {
                Console.WriteLine("You cannot write data to this stream.");
                _tcpClient.Close();

                // Closing the tcpClient instance does not close the network stream.
                netStream.Close();
                return;
            }

            if (netStream.CanRead)
            {
                netStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length,new AsyncCallback(myReadCallBack),netStream);
                
                /*
                // Reads NetworkStream into a byte buffer.
                byte[] bytes = new byte[_tcpClient.ReceiveBufferSize];

                // Read can return anything from 0 to numBytesToRead. 
                // This method blocks until at least one byte is read.
                netStream.Read(bytes, 0, (int)_tcpClient.ReceiveBufferSize);

                // Returns the data received from the host to the console.
                string returndata = Encoding.UTF8.GetString(bytes);

                Console.WriteLine("This is what the host returned to you: " + returndata);
                */
                
            }
            else
            {
                Console.WriteLine("You cannot read data from this stream.");
                _tcpClient.Close();

                // Closing the tcpClient instance does not close the network stream.
                netStream.Close();
                return;
            }
            //netStream.Close();
        }

        public void myReadCallBack(IAsyncResult ar)
        {
			/*
            try
            {
                NetworkStream myNetworkStream = (NetworkStream)ar.AsyncState;
                //byte[] myReadBuffer = new byte[1024];
                String myCompleteMessage = "";
                int numberOfBytesRead;

                numberOfBytesRead = myNetworkStream.EndRead(ar);
                myCompleteMessage = String.Concat(myCompleteMessage, Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));

                myNetworkStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length,
                                                           new AsyncCallback(myReadCallBack),
                                                           myNetworkStream);

                _messages.Add(myCompleteMessage);
                // Print out the received message to the console.
                Console.WriteLine("You received the following message(" + myCompleteMessage.Length + ") : " + myCompleteMessage);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR CLIENT MYREADCALLBACK:" + ex.Message);
            }
			*/


			
			byte[] values = new byte[_dataentry_t_size];
			try
			{
				NetworkStream myNetworkStream = (NetworkStream)ar.AsyncState;
				String myCompleteMessage = "";
				int numberOfBytesRead;
				
				numberOfBytesRead = myNetworkStream.EndRead(ar);
				
				//myCompleteMessage = String.Concat(myCompleteMessage, Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));
				myCompleteMessage = String.Concat(myCompleteMessage, Encoding.ASCII.GetString(myReadBuffer, 0, numberOfBytesRead));
				byte[] bytes = Utils.StringToByteArray(myCompleteMessage);
				//byte hb = (byte)_serialPort.ReadByte();
				//byte lb = (byte)_serialPort.ReadByte();
				
				//int count = (hb << 8) | lb;
				int count = numberOfBytesRead / _dataentry_t_size;
				DataEntry_t[] valuesArray = new DataEntry_t[count];
				
				for (int k = 0; k < count; k++)
				{
					for (int i = 0; i < _dataentry_t_size; i++)
					{
						values[i] = (byte)bytes[(k * _dataentry_t_size) + i];
					}
					DataEntry_t de = Utils.ByteArrayToNewStuff(values);
					if (data.ContainsKey(de.id))
					{
						DataEntry_t de_update;
						if (data.TryGetValue(de.id, out de_update))
						{
							de_update.value = de.value;
							data[de.id] = de_update;
						}
						else
							Console.WriteLine("ERROR: DATAIO Read: UpdateValue:" + de.id + "___count:" + count);
					}
					else
					{
						data.Add(de.id, de);
					}
				}

				myNetworkStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length,
				                          new AsyncCallback(myReadCallBack),
				                          myNetworkStream);
				
				int j = 0;
				j++;
				j--;
			}
			catch (TimeoutException) { }

        }

		public bool sendData(string str){
			try
			{
				NetworkStream netStream = _tcpClient.GetStream();
				if (netStream.CanWrite)
				{
					Byte[] sendBytes = Encoding.UTF8.GetBytes(str);
					netStream.Write(sendBytes, 0, sendBytes.Length);
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return false;
			}
			return true;
		}
	}
}
