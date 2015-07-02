//------------------------------------------------------------------------------
// <auto-generated>
//     Dieser Code wurde von einem Tool generiert.
//     Laufzeitversion:4.0.30319.0
//
//     Änderungen an dieser Datei können falsches Verhalten verursachen und gehen verloren, wenn
//     der Code erneut generiert wird.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using System.Runtime.InteropServices;


namespace SimulationUtils
{
	public struct DataEntry_t
	{
		public byte id;
		public int value;
	}

	public class Utils
	{
		private  static int _dataentry_t_size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DataEntry_t));

		public static byte[] StructureToByteArray(DataEntry_t[] val)		
		{	
			//int len = Marshal.SizeOf(_dataentry_t_size);		
			int len = _dataentry_t_size;
			byte [] arr = new byte[(val.Length*len)];
			
			for (int i = 0; i< val.Length; i++) {
				byte[] ba = SingleStructureToByteArray(val[i]);
				System.Buffer.BlockCopy( ba, 0, arr, i*len, len );
			}
			/*
			int len = Marshal.SizeOf(obj);
			
			byte [] arr = new byte[len];
			
			IntPtr ptr = Marshal.AllocHGlobal(len);
			
			Marshal.StructureToPtr(obj, ptr, true);
			
			Marshal.Copy(ptr, arr, 0, len);
			
			Marshal.FreeHGlobal(ptr);
			*/
			
			return arr;		
		}
		
		public static byte [] SingleStructureToByteArray(object obj)		
		{		
			int len = Marshal.SizeOf(obj);
			
			byte [] arr = new byte[len];
			
			IntPtr ptr = Marshal.AllocHGlobal(len);
			
			Marshal.StructureToPtr(obj, ptr, true);
			
			Marshal.Copy(ptr, arr, 0, len);
			
			Marshal.FreeHGlobal(ptr);
			
			return arr;		
		}
		
		public static byte[] StringToByteArray(string str)
		{
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
			return enc.GetBytes(str);
		}

		public static string ByteArrayToString(byte[] arr)
		{
			System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
			string result = System.Text.Encoding.UTF8.GetString(arr);
			string result2 = enc.GetString(arr);
			string s3 = Convert.ToBase64String(arr);
			return s3;
		}		
		
		public static DataEntry_t ByteArrayToNewStuff(byte[] bytes)
		{
			GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
			DataEntry_t stuff = (DataEntry_t)Marshal.PtrToStructure(
				handle.AddrOfPinnedObject(), typeof(DataEntry_t));
			handle.Free();
			return stuff;
		}
	}
}
