using System;
using System.IO.Ports;

namespace GPSMonitor
{
	public class SerialPortTest
	{

		public static void Main (string[] args)
		{
			SerialPortTest myTest = new SerialPortTest ();
			myTest.Test ();
		}

		private SerialPort mySerial;
		// Constructor
		public SerialPortTest ()
		{
		}

		public void Test ()
		{
			if (mySerial != null)
			if (mySerial.IsOpen)
				mySerial.Close ();

			//Open and configure the serial port
			Console.WriteLine("Initializing!");
			mySerial = new SerialPort ("/dev/ttyACM0", 9600);
			mySerial.Open ();
			mySerial.ReadTimeout = 4000;
			Console.WriteLine("Initialized!");

			//Loop forever
			while (true) {
				Console.WriteLine (ReadData ());
			}
		}

		public string ReadData ()
		{
			byte tmpByte;
			string rxString = "";

			tmpByte = (byte)mySerial.ReadByte ();

			while (tmpByte != 10) {
				rxString += ((char)tmpByte);
				tmpByte = (byte)mySerial.ReadByte ();
			}

			return rxString;
		}

		public bool SendData (string Data)
		{
			mySerial.Write (Data);
			return true;
		}
	}
}

