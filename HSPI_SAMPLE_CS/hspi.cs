using Microsoft.VisualBasic;
using System;
using HomeSeerAPI;
using Scheduler;
using System.Reflection;
using System.Text;
using HSPI_SIID.BACnet;
using System.Web;

namespace HSPI_SIID
{
    public class HSPI : IPlugInAPI
    {

        /*  public HSPI() : base() //This is called by the main program
         {

             // Create a thread-safe collection by using the .Synchronized wrapper.
            Util.colTrigs_Sync = new System.Collections.SortedList();
             Util.colTrigs = System.Collections.SortedList.Synchronized(Util.colTrigs_Sync);

             Util.colActs_Sync = new System.Collections.SortedList();
             Util.colActs = System.Collections.SortedList.Synchronized(Util.colActs_Sync);
    }*/


    // this API is required for ALL plugins

    public InstanceHolder Instance { get; set; }

      public string OurInstanceFriendlyName { get; set; }


      

        public  string MainSiidPageName = "";


        //  public string Instance = "";






        public enum LogType
        {
            LOG_TYPE_INFO = 0,
            LOG_TYPE_ERROR = 2,
            LOG_TYPE_WARNING = 1,
            LOG_TYPE_NONE = 3,
        }

        public LogType LogLevel = LogType.LOG_TYPE_ERROR;


        public void Log(Exception E)
        {
            Log(E.Message, 2);
        }

        public void Log(string msg)
        {
            Log(msg, 0);
        }

        public void Log(string msg, int logType)
        {
            Log(msg, (LogType)logType);
        }

        public void Log(string msg, LogType logType)
        {

            try
            {
                if (msg == null)
                    msg = "";
                if (!Enum.IsDefined(typeof(LogType), logType))
                {
                    logType = LogType.LOG_TYPE_ERROR;
                }
                Console.WriteLine(msg);
                if (logType >= LogLevel)
                {
                    switch (logType)
                    {
                        case LogType.LOG_TYPE_ERROR:
                            Instance.host.WriteLog(Util.IFACE_NAME + " Error", msg);
                            break;
                        case LogType.LOG_TYPE_WARNING:
                            Instance.host.WriteLog(Util.IFACE_NAME + " Warning", msg);
                            break;
                        case LogType.LOG_TYPE_INFO:
                            Instance.host.WriteLog(Util.IFACE_NAME, msg);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in LOG of " + Util.IFACE_NAME + ": " + ex.Message);
            }

        }

        public static bool bShutDown = false;

        #region "Externally Accessible Methods/Procedures - e.g. Script Commands"
       

        #endregion

        #region "Common Interface"

        // For search demonstration purposes only.
     
        public HomeSeerAPI.SearchReturn[] Search(string SearchString, bool RegEx)
        {
            
              System.Collections.Generic.List<SearchReturn> colRET = new System.Collections.Generic.List<SearchReturn>();
              SearchReturn RET;

              //So let's pretend we searched through all of the plug-in resources (triggers, actions, web pages, perhaps zone names, songs, etc.) 
              // and found a few matches....  

              //   The matches can be returned as just the string value...:
              RET = new SearchReturn();
   
              colRET.Add(RET);
          

              return colRET.ToArray();
              
        }


    // a custom call to call a specific procedure in the plugin
    public object PluginFunction(string proc, object[] parms)
	{
		

		return null;
	}

	public object PluginPropertyGet(string proc, object[] parms)
	{
		

		return null;
	}

	public void PluginPropertySet(string proc, object value)
	{
		
	}


	public string Name {
		get { return Util.IFACE_NAME; }// +Instance.name; }
	}

	public int Capabilities()
	{
            return 4;// (int)(HomeSeerAPI.Enums.eCapabilities.CA_IO | HomeSeerAPI.Enums.eCapabilities.CA_Thermostat);
	}

	// return 1 for a free plugin
	// return 2 for a licensed (for pay) plugin
	public int AccessLevel()
	{
		return 1; //setting this to 2 causes null reference exception currently. Maybe can't run remotely when set to licensed
	}

	public bool HSCOMPort {
			//We want HS to give us a com port number for accessing the hardware via a serial port
		get { return false; }
	}
        //Clearly it is possible to add a button on the instance column of the manage plugins which when clicked adds a new instance.
        //Don't know how to do that though
        public bool SupportsMultipleInstances()  //If set to false, then the Uninstall this plugin button is active on the instance tab
	{
            return  true;
	}

	public bool SupportsMultipleInstancesSingleEXE()
	{
            return false;// true;  //If true we cannot use the interfaces page
	}



	public string InstanceFriendlyName()
	{
            return OurInstanceFriendlyName;
	}




	public string InitIO(string port)
	{

		Console.WriteLine("InitIO called with parameter port as " + port);
            Instance = Program.AllInstances[InstanceFriendlyName()];
        string[] plugins = Instance.host.GetPluginsList();
	    

            try {

                Instance.siidPage.LoadINISettings();
                Console.WriteLine("Instance " + Instance.name);

                //All may not be needed or used, is for ajax callbacks
                //Instance.host.RegisterPage(MainSiidPageName, Util.IFACE_NAME, "");
                //Instance.host.RegisterPage("SIIDPage" + Instance.name, Util.IFACE_NAME,"");
                // Instance.host.RegisterPage("SIIDPage" + Instance.name, "", Instance.name);

                //Instance.host.RegisterPage("SIIDPage" + Instance.name, Util.IFACE_NAME, Instance.name); //Necessary to do the GetPagePlugin  Want also for postbackproc
                                                                                                     //Doesn't seem to work for multiple instances for postback
                   Instance.host.RegisterPage("SIIDPage", Util.IFACE_NAME, Instance.name);
                Instance.host.RegisterPage("ModBus", Util.IFACE_NAME, Instance.name);                                                                                                                                                                       //  Console.WriteLine(MainSiidPageName + "  " + Util.IFACE_NAME+"  "+ Instance.name);

                Instance.host.RegisterPage("Scratch", Util.IFACE_NAME, Instance.name);


                Instance.host.RegisterPage("ModbusDevicePage" , Util.IFACE_NAME, Instance.name); //MODBUS specifc ajax callback.  used in the PostBackPlugin switch area

//FigureOut These ones
                Instance.host.RegisterPage("AddModbusGate" , Util.IFACE_NAME, Instance.name);
                Instance.host.RegisterPage("ModBusGateTab" , Util.IFACE_NAME, Instance.name);
                Instance.host.RegisterPage("ModBusDevTab" , Util.IFACE_NAME, Instance.name);
                Instance.host.RegisterPage("AddModbusDevice" , Util.IFACE_NAME, Instance.name);


                //Instance.host.RegisterPage("BACnet", Util.IFACE_NAME, Instance.name); //Ajax calls from BACnet builder
                //Instance.host.RegisterPage("discoverBACnetDevices", Util.IFACE_NAME, Instance.name);//Redirect from the Gobutton for discoverBACnetDevices
                //Instance.host.RegisterPage("addBACnetDevice", Util.IFACE_NAME, Instance.name);

                Instance.host.RegisterPage(BACnetDataService.BaseUrl, Util.IFACE_NAME, Instance.name);
                Instance.host.RegisterPage(BACnetDevices.BaseUrl, Util.IFACE_NAME, Instance.name);
                Instance.host.RegisterPage(BACnetHomeSeerDevices.BaseUrl, Util.IFACE_NAME, Instance.name);
                //so don't register page with the instance ajax name.  But the request URL's need it in order for it to be routed to the correct plugin instance



                //Instance.host.RegisterPage(Instance.bacnetDataService.PageName, Util.IFACE_NAME, Instance.name);
                //Instance.host.RegisterPage(Instance.bacnetDevices.PageName, Util.IFACE_NAME, Instance.name);
                //Instance.host.RegisterPage(Instance.bacnetHomeSeerDevices.PageName, Util.IFACE_NAME, Instance.name);


                //Instance.host.RegisterPage(BACnetDataService.BaseUrl + Instance.ajaxName.Replace(":", "_"), Util.IFACE_NAME, Instance.name);
                //Instance.host.RegisterPage(BACnetDevices.BaseUrl + Instance.ajaxName.Replace(":", "_"), Util.IFACE_NAME, Instance.name);
                //Instance.host.RegisterPage(BACnetHomeSeerDevices.BaseUrl + Instance.ajaxName.Replace(":", "_"), Util.IFACE_NAME, Instance.name);




                //Instance.host.RegisterPage("BACnetDataService", Util.IFACE_NAME, Instance.name);
                //Instance.host.RegisterPage("BACnetDevices", Util.IFACE_NAME, Instance.name);
                //Instance.host.RegisterPage("BACnetHomeSeerDevices", Util.IFACE_NAME, Instance.name);




                //Instance.host.RegisterPage("BACnetHomeSeerDevicesEdit", Util.IFACE_NAME, Instance.name);


                Instance.host.RegisterPage("file", Util.IFACE_NAME, Instance.name);


                //Figure out these ones

                // Instance.host.RegisterPage(MainSiidPageName+"SIIDConfPage", Util.IFACE_NAME, Instance.name); //Need unique pagenames for each instance?
                //     Console.WriteLine(MainSiidPageName + "SIIDConfPage");

                // register a normal page to appear in the HomeSeer menu
                WebPageDesc wpd = new WebPageDesc();
                wpd.link = "SIIDPage";



                if (!string.IsNullOrEmpty(Instance.name))
                {
                    wpd.linktext = Util.IFACE_NAME + " SIID main page instance " + Instance.name;
              
                }
                else
                {
                    wpd.linktext = Util.IFACE_NAME + " SIID main page";
                }
                wpd.page_title = "SIIDPage" + Instance.name;
                wpd.plugInName = Util.IFACE_NAME;
                wpd.plugInInstance = Instance.name;
                Instance.callback.RegisterLink(wpd); //THis page used in the GenPagePlugin function.  Returns our webpage when the address goes to the one we registered
                
                
          
                wpd = new WebPageDesc();

                  if (!string.IsNullOrEmpty(Instance.name))
                  {
                    wpd.link = "SIIDPage";// + "?instance=" + Instance.name;
                    wpd.linktext = Util.IFACE_NAME + " SIID main page instance " + Instance.name;
                }
                else
                {
                    wpd.link = "SIIDPage";
                    wpd.linktext = Util.IFACE_NAME + " SIID main page";
                }
                wpd.page_title = "SIIDPage" + Instance.name;
                wpd.plugInName = Util.IFACE_NAME;
                wpd.plugInInstance =  Instance.name;
           
                Instance.callback.RegisterConfigLink(wpd); //Looks like the plugin config link doesn't go to specific instances


         

			Instance.host.WriteLog(Util.IFACE_NAME, "InitIO called, plug-in is being initialized...");

		
        }
        catch (Exception ex)
        {
			bShutDown = true;
			return "Error on InitIO: " + ex.Message;
		}

		bShutDown = false;
		return "";
		// return no error, or an error message

	}


	private int configcalls;
	public string ConfigDevice(int dvRef, string user, int userRights, bool newDevice)
	{
            //OK, I think if we do a switch here based on Device Type String, this will work. For Modbus I'm setting the string to "Modbus". 
            //Of course the user can change this string and mess this up so we'll see

            Scheduler.Classes.DeviceClass ourDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dvRef);

            var EDO = ourDevice.get_PlugExtraData_Get(Instance.host);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());


            switch (parts["Type"])
            {
                case ("Modbus Gateway"):
                    {
                    

                        return Instance.modPage.BuildModbusGatewayTab(dvRef);
                    }
                case ("Modbus Device"):
                    {
                       
                        return Instance.modPage.BuildModbusDeviceTab(dvRef);
                    }
                case ("BACnet Device") : case ("BACnet Object"):
                    {
                        return Instance.bacnetDevices.BuildBACnetDeviceTab(dvRef);
                    }
                case ("Scratchpad")://????
                    {
                        return Instance.scrPage.BuildScratchDeviceTab(dvRef); 
                    }


            }

      

		return "ERROR";
	}

	public Enums.ConfigDevicePostReturn ConfigDevicePost(int dvRef, string data, string user, int userRights) //this what we need to do?
	{

		return Enums.ConfigDevicePostReturn.DoneAndCancelAndStay;
	}

	// Web Page Generation - OLD METHODS
	// ================================================================================================
	public string GenPage(string link)
	{
          
            return "Generated from GenPage in plugin " + Util.IFACE_NAME;
	}
	public string PagePut(string data)
	{
          
		return "";
	}
	// ================================================================================================

	// Web Page Generation - NEW METHODS
	// ================================================================================================
	public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
	{
        //TODO: do we need to add instance name to these pageNames?



		//If you have more than one web page, use pageName to route it to the proper GetPagePlugin
		Console.WriteLine("GetPagePlugin pageName: " + pageName +" "+queryString);
            // get the correct page
            if (pageName == "SIIDPage")
            {
                //Console.WriteLine("IN SIID PAGE");
                return (Instance.siidPage.GetPagePlugin(pageName, user, userRights, queryString));
            }
            else if (pageName == "Scratch" + Instance.ajaxName)
            {

                return Instance.scrPage.addSubrule(queryString);

            }
            else if (pageName == "AddModbusGate")
            {
                return Instance.modPage.MakeGatewayRedirect(pageName, user, userRights, queryString);

            }
            else if (pageName == "AddModbusDevice")
            {
                return Instance.modPage.MakeSubDeviceRedirect(pageName, user, userRights, queryString);

            }

            else if (pageName == Instance.bacnetHomeSeerDevices.PageName)
            {

                return Instance.bacnetHomeSeerDevices.addOrEditBacnetHomeseerDevice(queryString);


                //TODO: check if device already exists in homeseer.  If so, fine, just point to device config page anyway.


                //var bacnetDevice = Instance.bacnetDataService.GetBacnetDevice(queryString);

                //return Instance.bacPage.MakeBACnetRedirect(pageName, user, userRights, queryString);

            }

            //else if (pageName == "addBACnetObject")     //this comes from a separate button
            //{

            //    //TODO: check if device already exists in homeseer.  If so, fine, just point to device config page anyway.

            //    return Instance.bacPage.MakeBACnetRedirect(pageName, user, userRights, queryString);

            //}

            else if (pageName == "BACnetDataService")
            {

                return Instance.bacnetDataService.GetTreeData(queryString);

            }


            else if (pageName == "file")
            {

                //return Instance.bacnetDataService.GetData(queryString);

            }





       

            return "page not registered";
	}

	public string PostBackProc(string pageName, string data, string user, int userRights)
	{
            //If you have more than one web page, use pageName to route it to the proper postBackProc
            //  Console.WriteLine("PostBackProc pageName: " + pageName);
            Log(data, 0);
            if (pageName == "SIIDPage"+Instance.ajaxName)
            {
                
                return Instance.siidPage.postbackSSIDConfigPage(pageName, data, user, userRights);
            }
            else if (pageName == "ModBus" + Instance.ajaxName)
            {
                return Instance.modAjax.postBackProcModBus(pageName, data, user, userRights);

            }
            else if (pageName == "ModBusGateTab" + Instance.ajaxName)
            {

                return Instance.modPage.parseModbusGatewayTab(data);

            }
            else if (pageName == "ModBusDevTab" + Instance.ajaxName)
            {
                data = data.Replace("+", "%2B");
                return Instance.modPage.parseModbusDeviceTab(data);

            }
           else if(pageName == "Scratch" + Instance.ajaxName)
            {
                
                data = data.Replace("+", "%2B");

                return Instance.scrPage.parseInstances(data);

            }


            else if (pageName == Instance.bacnetDevices.PageName)
            {

                //TODO: should really add a separate page for this, since separate post URL, but...

                //return "";

                return Instance.bacnetDevices.parseBacnetDeviceTab(data);

            }




            else if (pageName == Instance.bacnetHomeSeerDevices.PageName)
            {

                return Instance.bacnetHomeSeerDevices.addOrEditBacnetHomeseerDevice(data);


                //TODO: check if device already exists in homeseer.  If so, fine, just point to device config page anyway.


                //var bacnetDevice = Instance.bacnetDataService.GetBacnetDevice(queryString);

                //return Instance.bacPage.MakeBACnetRedirect(pageName, user, userRights, queryString);

            }

            
            else if (pageName == "BACnetDataService")
            {

                return Instance.bacnetDataService.GetTreeData(data);

            }



            return "";
	}

	// ================================================================================================

	public void HSEvent(Enums.HSEvent EventType, object[] parms)
	{
        Console.WriteLine("HSEvent: " + EventType.ToString());
		switch (EventType) {
			case Enums.HSEvent.VALUE_CHANGE:
				break;
		}
	}



	public HomeSeerAPI.IPlugInAPI.strInterfaceStatus InterfaceStatus()
	{
		IPlugInAPI.strInterfaceStatus es = new IPlugInAPI.strInterfaceStatus();
          
		es.intStatus = IPlugInAPI.enumInterfaceStatus.OK;
		return es;
	}

	public IPlugInAPI.PollResultInfo PollDevice(int dvref)
	{
		Console.WriteLine("PollDevice");
		IPlugInAPI.PollResultInfo ri = default(IPlugInAPI.PollResultInfo);
		ri.Result = IPlugInAPI.enumPollResult.OK;
		return ri;
	}

	public bool RaisesGenericCallbacks()
	{
		return false;
	}

        public void SetIOMulti(System.Collections.Generic.List<HomeSeerAPI.CAPI.CAPIControl> colSend)
        {



            //OK, we will take this function over for modbus actions.
            foreach (CAPI.CAPIControl CC in colSend)
            {

                var devID = CC.Ref;
                try
                {
                    var NewDevice = HSPI_SIID.General.SiidDevice.GetFromListByID(Instance.Devices, devID);
                    var EDO = NewDevice.Extra;
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    switch (parts["type"]) {
                        case "Scratchpad":
                            {
                              

                                    System.Threading.Tasks.Task.Factory.StartNew(() => Instance.scrPage.scratchpadCommandIn(CC));
                                


                                break;
                            }
                        case "Modbus Device":
                            {
                                System.Threading.Tasks.Task.Factory.StartNew(() => Instance.modPage.ReadWriteIfMod(CC));
                                break;
                            }
                        case "BACnet Object":
                            {

                                //Instance.bacnetHomeSeerDevices.testDev();
                                //return;

                                //Instance.bacnetHomeseerDevices.testDev();

                                var hsDev = NewDevice.Device;


                                if (CC.Label.StartsWith("Release"))
                                    Instance.bacnetDevices.ReadWriteBacnet(hsDev, null);
                                else
                                {
                                    if (CC.ControlString == "")     //changing with drop-down...only for multi-state value devices
                                        Instance.bacnetDevices.ReadWriteBacnet(hsDev, CC.ControlValue.ToString());
                                    else
                                        Instance.bacnetDevices.ReadWriteBacnet(hsDev, CC.ControlString);
                                }



                                //Instance.host.SetDeviceValueByRef(devID, 0.0, true);
                                //Instance.host.SetDeviceString(devID, "blah", true);
                                //NewDevice.Device.
                                //System.Threading.Tasks.Task.Factory.StartNew(() => Instance.modPage.ReadWriteIfMod(CC));
                                break;
                            }
                        case "BACnet Object (write priority)":
                            {

                                Instance.bacnetDevices.ChangeWritePriority(NewDevice, CC.ControlValue);

                                break;
                            }



                    }

                    
                }
                catch { }
                
            


            }

        }


	public void ShutdownIO()
	{
            // do your shutdown stuff here
           // Program.RemoveInstance(Instance.name);
            bShutDown = true;
		// setting this flag will cause the plugin to disconnect immediately from HomeSeer
	}

	public bool SupportsConfigDevice()
	{
		return true;
	}

	public bool SupportsConfigDeviceAll()
	{
		return false;
	}

	public bool SupportsAddDevice()
	{
		return false;
	}


	#endregion

	#region "Actions Interface"

	public int ActionCount()
	{
		return 0;
	}

	private bool mvarActionAdvanced;
	public bool ActionAdvancedMode {
		get { return mvarActionAdvanced; }
		set { mvarActionAdvanced = value; }
	}

	public string ActionBuildUI(string sUnique, IPlugInAPI.strTrigActInfo ActInfo)
	{
		StringBuilder st = new StringBuilder();
		

		return st.ToString();
	}

	public bool ActionConfigured(IPlugInAPI.strTrigActInfo ActInfo)
	{
	
			return false;
		
	}

	public bool ActionReferencesDevice(IPlugInAPI.strTrigActInfo ActInfo, int dvRef)
	{
		
		return false;
	}

	public string ActionFormatUI(IPlugInAPI.strTrigActInfo ActInfo)
	{
		StringBuilder st = new StringBuilder();

	
		return st.ToString();
	}

	public string get_ActionName(int ActionNumber)
    {
		
			return "";
	}

	public IPlugInAPI.strMultiReturn ActionProcessPostUI(System.Collections.Specialized.NameValueCollection PostData, IPlugInAPI.strTrigActInfo ActInfoIN)
	{

		HomeSeerAPI.IPlugInAPI.strMultiReturn Ret = new HomeSeerAPI.IPlugInAPI.strMultiReturn();
	
		// All OK
		Ret.sResult = "";
		return Ret;


	}

	public bool HandleAction(IPlugInAPI.strTrigActInfo ActInfo)
	{
            return false;

	}

	#endregion

	#region "Trigger Interface"

	/// <summary>
	/// Indicates (when True) that the Trigger is in Condition mode - it is for triggers that can also operate as a condition
	///    or for allowing Conditions to appear when a condition is being added to an event.
	/// </summary>
	/// <param name="TrigInfo">The event, group, and trigger info for this particular instance.</param>
	/// <value></value>
	/// <returns>The current state of the Condition flag.</returns>
	/// <remarks></remarks>
	public bool get_Condition(IPlugInAPI.strTrigActInfo TrigInfo)
	{
	
		return false;
	}
	public void set_Condition(IPlugInAPI.strTrigActInfo TrigInfo, bool Value)
    {
		
	}

	public bool get_HasConditions(int TriggerNumber) {
		
			return false;
		
		
	}

	public bool HasTriggers {
		get { return true; }
	}

	public int TriggerCount {
		get { return 1; }
	}

	public string get_TriggerName(int TriggerNumber) {
		
				return "";
		
	}

	public int get_SubTriggerCount(int TriggerNumber)
    {
	
			return 0;
		
	}

	public string get_SubTriggerName(int TriggerNumber, int SubTriggerNumber)
    { 
		
	   
				return "";
		
	}

	public string TriggerBuildUI(string sUnique, HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
	{
		StringBuilder st = new StringBuilder();
		

		return st.ToString();

	}

	public bool get_TriggerConfigured(IPlugInAPI.strTrigActInfo TrigInfo)
    {
			
			return false;
	}

	public bool TriggerReferencesDevice(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo, int dvRef)
	{
	
		return false;
	}

	public string TriggerFormatUI(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
	{
		
		return "ERROR - The trigger is not properly built yet.";
	}

	public HomeSeerAPI.IPlugInAPI.strMultiReturn TriggerProcessPostUI(System.Collections.Specialized.NameValueCollection PostData, HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfoIn)
	{

		HomeSeerAPI.IPlugInAPI.strMultiReturn Ret = new HomeSeerAPI.IPlugInAPI.strMultiReturn();
		Ret.sResult = "";
		
		// All OK
		Ret.sResult = "";
		return Ret;

	}

	public bool TriggerTrue(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
	{
	
		return false;

	}

	#endregion


	

	public enum enumTAG
	{
		Unknown = 0,
		Trigger = 1,
		Action = 2,
		Group = 3
	}

	public struct EventWebControlInfo
	{
		public bool Decoded;
		public int EventTriggerGroupID;
		public int GroupID;
		public int EvRef;
		public int TriggerORActionID;
		public string Name_or_ID;
		public string Additional;
		public enumTAG TrigActGroup;
	}

    


	// called if speak proxy is installed
	public void SpeakIn(int device, string txt, bool w, string host)
	{
		Console.WriteLine("Speaking from HomeSeer, txt: " + txt);
		// speak back
		Instance.host.SpeakProxy(device, txt + " the plugin added this", w, host);
	}

	// save an image file to HS, images can only be saved in a subdir of html\images so a subdir must be given
	// save an image object to HS
	private void SaveImageFileToHS(string src_filename, string des_filename)
	{
		System.Drawing.Image im = System.Drawing.Image.FromFile(src_filename);
		Instance.host.WriteHTMLImage(im, des_filename, true);
	}

	// save a file as an array of bytes to HS
	private void SaveFileToHS(string src_filename, string des_filename)
	{
		byte[] bytes = System.IO.File.ReadAllBytes(src_filename);
		if (bytes != null) {
			Instance.host.WriteHTMLImageFile(bytes, des_filename, true);
		}
	}
    }
}
