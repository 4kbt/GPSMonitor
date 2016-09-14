using System;
using System.IO.Ports;

namespace GPSMonitor
{
	public class GPSMonitor 
	{
		//Member variables
		public double GPSTime;
		public UInt32 GPSDate;
		public UInt16 satellitesInView;
		public UInt16 fixStatus; 
		private SerialPort mySerial;

		public static void Main (string[] args)
		{
			GPSMonitor GPS = new GPSMonitor ("/dev/ttyACM0");
			GPS.AcquireData ();
		}

		//Constructor
		public GPSMonitor( string portName ){

			//Close serial port if it's presently open
			if (mySerial != null)
				if (mySerial.IsOpen)
					mySerial.Close ();

			//Open and configure the serial port
			Console.WriteLine("Initializing!");
			mySerial = new SerialPort (portName, 9600);
			mySerial.Open ();
			mySerial.ReadTimeout = 4000;
			Console.WriteLine("Initialized!");
		}

		public void AcquireData ()
		{


			string RawString; 
			//Loop forever
			while (true) {
				RawString = ReadData ();
				Console.WriteLine (RawString);
				ParseString (RawString);
			}
		}

		//Function to parse the NMEA sentences
		private void ParseString (string s){
			string[] Words = s.Split (',');
			//string NMEAName = s.Substring (1, 5);
			switch (Words[0]) {
			case "$GPGSV":
				Console.WriteLine ("Number of satellites: " + Words [3]);
				break;
			default:
				break;
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

