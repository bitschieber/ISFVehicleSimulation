using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Runtime.InteropServices;

namespace TcpServerClient
{
    class Client
    {
        TcpClient _tcpClient;
        byte[] myReadBuffer = new byte[1024];
        public List<string> _messages;
		public delegate void ReadCallbackType(DATA_SET_MONITOR_t Messasge);
        private ReadCallbackType _readCallBackFunc;
		public DATA_SET_MONITOR_t _dataInputSet;


        public Client(IPAddress ip, int port, ReadCallbackType callback)
        {
			_dataInputSet.lenkwinkel = 45;
			_dataInputSet.speed = 0;
            try
            {
                _readCallBackFunc = callback;
                _tcpClient = new TcpClient(ip.ToString(), port);
                _messages = new List<string>();
                initConnection();
            }
            catch (Exception ex)
            {
                //Debug.log(ex.Message);
            }
        }

        public Client(string ip, int port, ReadCallbackType callback)
        {
            try
            {
                _readCallBackFunc = callback;
                _tcpClient = new TcpClient(ip, port);
                _messages = new List<string>();
                initConnection();
            }
            catch (Exception ex)
            {
                //Debug.log(ex.Message);
            }
        }

        private void initConnection()
        {
            NetworkStream netStream = _tcpClient.GetStream();
            Console.WriteLine("Connected to Server");
            if (netStream.CanWrite)
            {
                //netStream.Write(sendBytes, 0, sendBytes.Length);
            }
            else
            {
                //Console.WriteLine("You cannot write data to this stream.");
                //_tcpClient.Close();

                // Closing the tcpClient instance does not close the network stream.
                //netStream.Close();
                //return;
            }

            if (netStream.CanRead)
            {
                netStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length, new AsyncCallback(myReadCallBack), netStream);

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

        private void myReadCallBack(IAsyncResult ar)
        {

            try
            {
                NetworkStream myNetworkStream = (NetworkStream)ar.AsyncState;
                //String myCompleteMessage = "";
                int numberOfBytesRead;

                numberOfBytesRead = myNetworkStream.EndRead(ar);
                if (numberOfBytesRead > 0)
                {

					
					//_dataInputSet = ByteArrayToNewStuff(myReadBuffer.ToArray());



					
					int BufferSize = Marshal.SizeOf(typeof(DATA_SET_MONITOR_t));
					byte[] buff = new byte[BufferSize];
					int setCounter = 0;
					while(numberOfBytesRead+setCounter<=System.Runtime.InteropServices.Marshal.SizeOf(_dataInputSet)){
						Array.Copy(myReadBuffer.ToArray(),BufferSize*setCounter,buff,0,BufferSize);
						//myReadBuffer.ToArray();
						_dataInputSet = ByteArrayToNewStuff(buff);
						setCounter += BufferSize;
					}

                    //myCompleteMessage = String.Concat(myCompleteMessage, Encoding.UTF8.GetString(myReadBuffer, 0, numberOfBytesRead));
                    
                    //_readCallBackFunc(myCompleteMessage);
					_readCallBackFunc(_dataInputSet);
                }

                myNetworkStream.BeginRead(myReadBuffer, 0, myReadBuffer.Length,
                                          new AsyncCallback(myReadCallBack),
                                          myNetworkStream);

            }
            catch (TimeoutException) { }

        }

		public bool sendData(DATA_SET_MONITOR_t data)
        {
            try
            {
                NetworkStream netStream = _tcpClient.GetStream();
                if (netStream.CanWrite)
                {
					Byte[] sendBytes = StructureToByteArray(data);
                    netStream.Write(sendBytes, 0, sendBytes.Length);
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

		DATA_SET_MONITOR_t ByteArrayToNewStuff(byte[] bytes)
		{
			GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			DATA_SET_MONITOR_t stuff = (DATA_SET_MONITOR_t)Marshal.PtrToStructure(
				handle.AddrOfPinnedObject(), typeof(DATA_SET_MONITOR_t));
			handle.Free();
			return stuff;
		}

		static byte [] StructureToByteArray(DATA_SET_MONITOR_t data)
			
		{
			
			int len = Marshal.SizeOf(data);
			
			byte [] arr = new byte[len];
			
			IntPtr ptr = Marshal.AllocHGlobal(len);
			
			Marshal.StructureToPtr(data, ptr, true);
			
			Marshal.Copy(ptr, arr, 0, len);
			
			Marshal.FreeHGlobal(ptr);
			
			return arr;
			
		}

    }
}
