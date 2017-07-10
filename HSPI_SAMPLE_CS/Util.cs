using System;


using System.IO;


namespace HSPI_SIID
{

    static class Util
	{


	// interface status
	// for InterfaceStatus function call
	public const  int ERR_NONE = 0;
	public const  int ERR_SEND = 1;

	public const  int ERR_INIT = 2;
        //	public static HomeSeerAPI.IHSApplication hs;  //Changing Util.hs (plugin specific plugin interface) to AllInstances[instanceName].host
        //public static HomeSeerAPI.IAppCallbackAPI callback;
        public const string IFACE_NAME = "SIID";
	//public const string IFACE_NAME = "Sample Plugin";
		// set when SupportMultipleInstances is TRUE
	//public static string Instance = "";  //Changing way this is done
	public static string gEXEPath = "";

	public static bool gGlobalTempScaleF = true;
	public static System.Collections.SortedList colTrigs_Sync;
	public static System.Collections.SortedList colTrigs;
	public static System.Collections.SortedList colActs_Sync;

	public static System.Collections.SortedList colActs;


	private static System.Threading.Thread Demo_Thread;
	public static bool StringIsNullOrEmpty(ref string s)
	{
		if (string.IsNullOrEmpty(s))
			return true;
		return string.IsNullOrEmpty(s.Trim());
	}



	static internal int GetDecimals(double D)
	{
		string s = "";
		char[] c = new char[1];
		c[0] = '0';
		// Trailing zeros to be removed.
		D = Math.Abs(D) - Math.Abs(Math.Truncate(D));
		// Remove the whole number so the result always starts with "0." which is a known quantity.
		s = D.ToString("F30");
		s = s.TrimEnd(c);
		return s.Length - 2;
		// Minus 2 because that is the length of "0."
	}




	public enum LogType
	{
		LOG_TYPE_INFO = 0,
		LOG_TYPE_ERROR = 1,
		LOG_TYPE_WARNING = 2
	}

	public static void Log(string msg, LogType logType)
	{
		/*try {
			if (msg == null)
				msg = "";
			if (!Enum.IsDefined(typeof(LogType), logType)) {
				logType = Util.LogType.LOG_TYPE_ERROR;
			}
			Console.WriteLine(msg);
			switch (logType) {
				case LogType.LOG_TYPE_ERROR:
					hs.WriteLog(Util.IFACE_NAME + " Error", msg);
					break;
				case LogType.LOG_TYPE_WARNING:
					hs.WriteLog(Util.IFACE_NAME + " Warning", msg);
					break;
				case LogType.LOG_TYPE_INFO:
					hs.WriteLog(Util.IFACE_NAME, msg);
					break;
			}
		} catch (Exception ex) {
			Console.WriteLine("Exception in LOG of " + Util.IFACE_NAME + ": " + ex.Message);
		}*/

	}

	internal enum eTriggerType
	{
		OneTon = 1,
		TwoVolts = 2,
		Unknown = 0
	}
	internal enum eActionType
	{
		Unknown = 0,
		Weight = 1,
		Voltage = 2
	}

	internal struct strTrigger
	{
		public eTriggerType WhichTrigger;
		public object TrigObj;
		public bool Result;
	}
	internal struct strAction
	{
		public eActionType WhichAction;
		public object ActObj;
		public bool Result;
	}

	static internal strTrigger TriggerFromData(byte[] Data)
	{
		strTrigger ST = new strTrigger();
		ST.WhichTrigger = eTriggerType.Unknown;
		ST.Result = false;
		if (Data == null)
			return ST;
		if (Data.Length < 1)
			return ST;

		bool bRes = false;
		Classes.MyTrigger1Ton Trig1 = new Classes.MyTrigger1Ton();
		Classes.MyTrigger2Shoe Trig2 = new Classes.MyTrigger2Shoe();
		try {
			object objTrig1 = Trig1;
			bRes = DeSerializeObject(Data, ref objTrig1, Trig1.GetType());
			if (bRes) Trig1 = (Classes.MyTrigger1Ton) objTrig1; 
		} catch (Exception) {
			bRes = false;
		}
		if (bRes & Trig1 != null) {
			ST.WhichTrigger = eTriggerType.OneTon;
			ST.TrigObj = Trig1;
			ST.Result = true;
			return ST;
		}
		try {
			object objTrig2 = Trig2;
			bRes = DeSerializeObject(Data, ref objTrig2, Trig2.GetType());
			if (bRes) Trig2 = (Classes.MyTrigger2Shoe)objTrig2; 
		} catch (Exception) {
			bRes = false;
		}
		if (bRes & Trig2 != null) {
			ST.WhichTrigger = eTriggerType.TwoVolts;
			ST.TrigObj = Trig2;
			ST.Result = true;
			return ST;
		}
		ST.WhichTrigger = eTriggerType.Unknown;
		ST.TrigObj = null;
		ST.Result = false;
		return ST;
	}

	static internal strAction ActionFromData(byte[] Data)
	{
		strAction ST = new strAction();
		ST.WhichAction = eActionType.Unknown;
		ST.Result = false;
		if (Data == null)
			return ST;
		if (Data.Length < 1)
			return ST;

		bool bRes = false;
		Classes.MyAction1EvenTon Act1 = new Classes.MyAction1EvenTon();
		Classes.MyAction2Euro Act2 = new Classes.MyAction2Euro();
		try {
			object objAct1 = Act1;
			bRes = DeSerializeObject(Data, ref objAct1, Act1.GetType());
			if (bRes) Act1 = (Classes.MyAction1EvenTon)objAct1; 
		} catch (Exception) {
			bRes = false;
		}
		if (bRes & Act1 != null) {
			ST.WhichAction = eActionType.Weight;
			ST.ActObj = Act1;
			ST.Result = true;
			return ST;
		}
		try {
			object objAct2 = Act2;
			bRes = DeSerializeObject(Data, ref objAct2, Act2.GetType());
			if (bRes) Act2 = (Classes.MyAction2Euro)objAct2; 
		} catch (Exception) {
			bRes = false;
		}
		if (bRes & Act2 != null) {
			ST.WhichAction = eActionType.Voltage;
			ST.ActObj = Act2;
			ST.Result = true;
			return ST;
		}
		ST.WhichAction = eActionType.Unknown;
		ST.ActObj = null;
		ST.Result = false;
		return ST;
	}

	public static void Add_Update_Trigger(object Trig)
	{
		if (Trig == null)
			return;
		string sKey = "";
		if (Trig is Classes.MyTrigger1Ton) {
			Classes.MyTrigger1Ton Trig1 = null;
			try {
				Trig1 = (Classes.MyTrigger1Ton)Trig;
			} catch (Exception) {
				Trig1 = null;
			}
			if (Trig1 != null) {
				if (Trig1.TriggerUID < 1)
					return;
				sKey = "K" + Trig1.TriggerUID.ToString();
				if (colTrigs.ContainsKey(sKey)) {
					lock (colTrigs.SyncRoot) {
						colTrigs.Remove(sKey);
					}
				}
				colTrigs.Add(sKey, Trig1);
			}
		} else if (Trig is Classes.MyTrigger2Shoe) {
			Classes.MyTrigger2Shoe Trig2 = null;
			try {
				Trig2 = (Classes.MyTrigger2Shoe)Trig;
			} catch (Exception) {
				Trig2 = null;
			}
			if (Trig2 != null) {
				if (Trig2.TriggerUID < 1)
					return;
				sKey = "K" + Trig2.TriggerUID.ToString();
				if (colTrigs.ContainsKey(sKey)) {
					lock (colTrigs.SyncRoot) {
						colTrigs.Remove(sKey);
					}
				}
				colTrigs.Add(sKey, Trig2);
			}
		}
	}

	public static void Add_Update_Action(object Act)
	{
		if (Act == null)
			return;
		string sKey = "";
		if (Act is Classes.MyAction1EvenTon) {
			Classes.MyAction1EvenTon Act1 = null;
			try {
				Act1 = (Classes.MyAction1EvenTon)Act;
			} catch (Exception) {
				Act1 = null;
			}
			if (Act1 != null) {
				if (Act1.ActionUID < 1)
					return;
				sKey = "K" + Act1.ActionUID.ToString();
				if (colActs.ContainsKey(sKey)) {
					lock (colActs.SyncRoot) {
						colActs.Remove(sKey);
					}
				}
				colActs.Add(sKey, Act1);
			}
		} else if (Act is Classes.MyAction2Euro) {
			Classes.MyAction2Euro Act2 = null;
			try {
				Act2 = (Classes.MyAction2Euro)Act;
			} catch (Exception) {
				Act2 = null;
			}
			if (Act2 != null) {
				if (Act2.ActionUID < 1)
					return;
				sKey = "K" + Act2.ActionUID.ToString();
				if (colActs.ContainsKey(sKey)) {
					lock (colActs.SyncRoot) {
						colActs.Remove(sKey);
					}
				}
				colActs.Add(sKey, Act2);
			}
		}
	}

	public static int MyDevice = -1;

	public static int MyTempDevice = -1;
	
	



	static internal bool SerializeObject(object ObjIn, ref byte[] bteOut)
	{
		if (ObjIn == null)
			return false;
		MemoryStream str = new MemoryStream();
		System.Runtime.Serialization.Formatters.Binary.BinaryFormatter sf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

		try {
			sf.Serialize(str, ObjIn);
			bteOut = new byte[Convert.ToInt32(str.Length - 1) + 1];
			bteOut = str.ToArray();
			return true;
		} catch (Exception ex) {
			Log(IFACE_NAME + " Error: Serializing object " + ObjIn.ToString() + " :" + ex.Message, LogType.LOG_TYPE_ERROR);
			return false;
		}

	}

	static internal bool SerializeObject(object ObjIn, ref string HexOut)
	{
		if (ObjIn == null)
			return false;
		MemoryStream str = new MemoryStream();
		System.Runtime.Serialization.Formatters.Binary.BinaryFormatter sf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
		byte[] bteOut = null;

		try {
			sf.Serialize(str, ObjIn);
			bteOut = new byte[Convert.ToInt32(str.Length - 1) + 1];
			bteOut = str.ToArray();
			HexOut = "";
			for (int i = 0; i <= bteOut.Length - 1; i++) {
				HexOut += bteOut[i].ToString("x2").ToUpper();
			}
			return true;
		} catch (Exception ex) {
			Log(IFACE_NAME + " Error: Serializing (Hex) object " + ObjIn.ToString() + " :" + ex.Message, LogType.LOG_TYPE_ERROR);
			return false;
		}

	}

	public static bool DeSerializeObject(byte[] bteIn, ref object ObjOut, System.Type OType)
	{
		// Almost immediately there is a test to see if ObjOut is NOTHING.  The reason for this
		//   when the ObjOut is suppose to be where the deserialized object is stored, is that 
		//   I could find no way to test to see if the deserialized object and the variable to 
		//   hold it was of the same type.  If you try to get the type of a null object, you get
		//   only a null reference exception!  If I do not test the object type beforehand and 
		//   there is a difference, then the InvalidCastException is thrown back in the CALLING
		//   procedure, not here, because the cast is made when the ByRef object is cast when this
		//   procedure returns, not earlier.  In order to prevent a cast exception in the calling
		//   procedure that may or may not be handled, I made it so that you have to at least 
		//   provide an initialized ObjOut when you call this - ObjOut is set to nothing after it 
		//   is typed.
		if (bteIn == null)
			return false;
		if (bteIn.Length < 1)
			return false;
		MemoryStream str = null;
		System.Runtime.Serialization.Formatters.Binary.BinaryFormatter sf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
		object ObjTest = null;
		System.Type TType = null;
		try {
			ObjOut = null;
			str = new MemoryStream(bteIn);
			ObjTest = sf.Deserialize(str);
			if (ObjTest == null)
				return false;
			TType = ObjTest.GetType();
			if (!TType.Equals(OType))
				return false;
			ObjOut = ObjTest;
			if (ObjOut == null)
				return false;
			return true;
		} catch (InvalidCastException) {
			return false;
		} catch (Exception ex) {
			Log(IFACE_NAME + " Error: DeSerializing object: " + ex.Message, LogType.LOG_TYPE_ERROR);
			return false;
		}

	}

    



	public static bool DeSerializeObject(string HexIn, ref object ObjOut, System.Type OType)
	{
		// Almost immediately there is a test to see if ObjOut is NOTHING.  The reason for this
		//   when the ObjOut is suppose to be where the deserialized object is stored, is that 
		//   I could find no way to test to see if the deserialized object and the variable to 
		//   hold it was of the same type.  If you try to get the type of a null object, you get
		//   only a null reference exception!  If I do not test the object type beforehand and 
		//   there is a difference, then the InvalidCastException is thrown back in the CALLING
		//   procedure, not here, because the cast is made when the ByRef object is cast when this
		//   procedure returns, not earlier.  In order to prevent a cast exception in the calling
		//   procedure that may or may not be handled, I made it so that you have to at least 
		//   provide an initialized ObjOut when you call this - ObjOut is set to nothing after it 
		//   is typed.
		if (HexIn == null)
			return false;
		if (string.IsNullOrEmpty(HexIn.Trim()))
			return false;
		MemoryStream str = null;
		System.Runtime.Serialization.Formatters.Binary.BinaryFormatter sf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
		object ObjTest = null;
		System.Type TType = null;

		byte[] bteIn = null;
		int HowMany = 0;

		try {
			HowMany = Convert.ToInt32((HexIn.Length / 2) - 1);
			bteIn = new byte[HowMany + 1];
			for (int i = 0; i <= HowMany; i++) {
				//bteIn(i) = CByte("&H" & HexIn.Substring(i * 2, 2))
				bteIn[i] = byte.Parse(HexIn.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);
			}
			ObjOut = null;
			str = new MemoryStream(bteIn);
			ObjTest = sf.Deserialize(str);
			if (ObjTest == null)
				return false;
			TType = ObjTest.GetType();
			if (!TType.Equals(OType))
				return false;
			ObjOut = ObjTest;
			if (ObjOut == null)
				return false;
			return true;
		} catch (InvalidCastException) {
			return false;
		} catch (Exception ex) {
			Log(IFACE_NAME + " Error: DeSerializing object: " + ex.Message, LogType.LOG_TYPE_ERROR);
			return false;
		}

	}
}

}
