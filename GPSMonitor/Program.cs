using System;
using System.IO.Ports;

namespace GPSMonitor
{
	public class GPSMonitor 
	{

		public static void Main (string[] args)
		{
			GPSMonitor GPS = new GPSMonitor ("/dev/ttyACM0");
			GPS.AcquireData ();
		}

		//Member variables
		public double GPSTime; //time of day as presented by the GPS (A la 130532.000 for 13:05:32.000)
		public UInt64 SystemFixTime; //system time at the instant that GPSTime was set.
		public UInt32 GPSDate; //date as presented by the GPS (140916 is September 14, 2016);
		public UInt16 satellitesInView; //number of satellites most recently in view
		public UInt16 fixStatus; //NMEA fix status (1 = no fix, 2 = 2D fix, 3 = 3D fix)
		public UInt16[] satelliteSNR;
		public int NumberOfGPSSatellitesInOrbit = 20;
		public int numSatellitesWithActualSignal;

		private SerialPort mySerial; //The SerialPort object connected to the GPS.

		//Constructor
		public GPSMonitor( string portName ){

			satelliteSNR = new UInt16[NumberOfGPSSatellitesInOrbit];

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

		//main acquisition loop.
		public void AcquireData ()
		{
			string RawString; 
			//Loop forever
			while (true) {
				RawString = ReadData ();
//				Console.WriteLine (RawString);
				ParseString (RawString);


			}
		}

		//Function to parse the NMEA sentences
		//This function is amenable to unit testing.
		private void ParseString (string s){

			char[] delimiterChars = { ',', '*'};
			string[] Words = s.Split ( delimiterChars  );

			//Switch on NMEA sentence-type.
			switch (Words[0]) {

			case "$GPGSA":
				fixStatus = UInt16.Parse (Words [2]);
				break;

			case "$GPGSV": //Signal strength and satellite information
				satellitesInView = UInt16.Parse (Words [3]);
				int sentenceNumber = Int16.Parse (Words [2]);

				//Figure out how many satellites are in this NMEA sentence
				int satellitesInThisSentence = satellitesInView - 4 * (sentenceNumber -1 );
				if( satellitesInThisSentence > 4 ){
					satellitesInThisSentence = 4;
				}

				//Update the relevant satellite's SNR
				for (int satCounter = 0; satCounter < satellitesInThisSentence; satCounter++){

					//Number of the satellite in the SNR array
					int satIndex = (4*(sentenceNumber-1) + satCounter);

					//Isolate SNR word
					string satString = Words[ 7 + 4*satCounter];

					//Save SNR to SNR array
					if (satString.Equals("")){
						satelliteSNR[satIndex] = 0;
					}
					else{
						satelliteSNR[satIndex] = UInt16.Parse(satString);
					}
				}

				//Flush any nonexistent satellits
				for (int satCounter = satellitesInView; satCounter < NumberOfGPSSatellitesInOrbit; satCounter++){
					satelliteSNR[satCounter] = 0;
				}

				//Count non-zero SNRs
				numSatellitesWithActualSignal = 0;
				for (int satCounter  = 0 ; satCounter < NumberOfGPSSatellitesInOrbit; satCounter++){
					if(satelliteSNR[satCounter] > 0){
						numSatellitesWithActualSignal++;
					}
				}
				break;

			case "$GPRMC": //Position and timing information
				SystemFixTime = GetCurrentUnixTimestampMillis (); //Do this first to minimize latency
				GPSTime = Double.Parse (Words [1]);
				GPSDate = UInt32.Parse (Words [9]);
				Console.WriteLine (SystemFixTime + "\t" + GPSTime + "\t" + GPSDate + "\t" + satellitesInView + "\t" + fixStatus + "\t" + numSatellitesWithActualSignal);

				//Debug output for satellite SNR
				//for( int satCounter = 0; satCounter < NumberOfGPSSatellitesInOrbit; satCounter++){
				//	Console.Write("\t" + satelliteSNR[satCounter]);
				//}
				//Console.WriteLine("");

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


		//Borrowed from http://stackoverflow.com/questions/7983441/unix-time-conversions-in-c-sharp
		private static readonly DateTime UnixEpoch =
			new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

		public static UInt64 GetCurrentUnixTimestampMillis()
		{
			return (UInt64) (DateTime.UtcNow - UnixEpoch).TotalMilliseconds;
		}

	}
}

