using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using HSCF;
using HomeSeerAPI;
using Scheduler;
using HSCF.Communication.ScsServices.Service;
using System.Reflection;
using System.Text;
using HSPI_SIID.Modbus;
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



	public static bool bShutDown = false;

        #region "Externally Accessible Methods/Procedures - e.g. Script Commands"
        /*
            public int MyLocalFunction()
            {
                return 555;
            }

            public double MySquareFunction(object[] parms)
            {
                if (parms == null)
                    return 0;
                if (parms.Length < 1)
                    return 0;
                if (parms[0] == null)
                    return 0;
                if (!Information.IsNumeric(parms[0]))
                    return 0;

                System.Type NType = new object().GetType();
                //Default to object.
                try {
                    NType = parms[0].GetType();
                } catch (Exception ) {
                }
                try {
                return Math.Pow((double)parms[0], (double)2);
                } catch (Exception ex) {
                    Util.Log("MySquareFunction returned an exception - bad input perhaps? Type of input=" + NType.ToString() + ", Ex=" + ex.Message, Util.LogType.LOG_TYPE_ERROR);
                    return 0;
                }
            }*/

        #endregion

        #region "Common Interface"

        // For search demonstration purposes only.
        string[] Zone = new string[6];
        Scheduler.Classes.DeviceClass OneOfMyDevices = new Scheduler.Classes.DeviceClass();
        public HomeSeerAPI.SearchReturn[] Search(string SearchString, bool RegEx)
        {
              // Not yet implemented in the Sample
              //
              // Normally we would do a search on plug-in actions, triggers, devices, etc. for the string provided, using
              //   the string as a regular expression if RegEx is True.
              //
              System.Collections.Generic.List<SearchReturn> colRET = new System.Collections.Generic.List<SearchReturn>();
              SearchReturn RET;

              //So let's pretend we searched through all of the plug-in resources (triggers, actions, web pages, perhaps zone names, songs, etc.) 
              // and found a few matches....  

              //   The matches can be returned as just the string value...:
              RET = new SearchReturn();
              RET.RType = eSearchReturn.r_String_Other;
              RET.RDescription = "Found in the zone description for zone 4";
              RET.RValue = Zone[4];
              colRET.Add(RET);
              //   The matches can be returned as a URL:
              RET = new SearchReturn();
              RET.RType = eSearchReturn.r_URL;
              RET.RValue = Util.IFACE_NAME + Instance.name;
              // Could have put something such as /DeviceUtility?ref=12345&edit=1     to take them directly to the device properties of a device.
              colRET.Add(RET);
              //   The matches can be returned as an Object:
              //   This will be VERY infrequently used as it is restricted to object types that can go through the HomeSeer-Plugin interface.
              //   Normal data type objects (Date, String, Integer, Enum, etc.) can go through, but very few complex objects such as the 
              //       HomeSeer DeviceClass will make it through the interface unscathed.
              RET = new SearchReturn();
              RET.RType = eSearchReturn.r_Object;
              RET.RDescription = "Found in a device.";
              RET.RValue = Instance.host.DeviceName(OneOfMyDevices.get_Ref(Instance.host));
              //Returning a string in the RValue is optional since this is an object type return
              RET.RObject = OneOfMyDevices;
              colRET.Add(RET);

              return colRET.ToArray();
              
        }


    // a custom call to call a specific procedure in the plugin
    public object PluginFunction(string proc, object[] parms)
	{
		try {
			Type ty = this.GetType();
			MethodInfo mi = ty.GetMethod(proc);
			if (mi == null) {
				Util.Log("Method " + proc + " does not exist in this plugin.", Util.LogType.LOG_TYPE_ERROR);
				return null;
			}
			return (mi.Invoke(this, parms));
		} catch (Exception ex) {
			Util.Log("Error in PluginProc: " + ex.Message, Util.LogType.LOG_TYPE_ERROR);
		}

		return null;
	}

	public object PluginPropertyGet(string proc, object[] parms)
	{
		try {
			Type ty = this.GetType();
			PropertyInfo mi = ty.GetProperty(proc);
			if (mi == null) {
				Util.Log("Method " + proc + " does not exist in this plugin.", Util.LogType.LOG_TYPE_ERROR);
				return null;
			}
			return mi.GetValue(this, null);
		} catch (Exception ex) {
			Util.Log("Error in PluginProc: " + ex.Message, Util.LogType.LOG_TYPE_ERROR);
		}

		return null;
	}

	public void PluginPropertySet(string proc, object value)
	{
		try {
			Type ty = this.GetType();
			PropertyInfo mi = ty.GetProperty(proc);
			if (mi == null) {
				Util.Log("Property " + proc + " does not exist in this plugin.", Util.LogType.LOG_TYPE_ERROR);
			}
			mi.SetValue(this, value, null);
		} catch (Exception ex) {
			Util.Log("Error in PluginPropertySet: " + ex.Message, Util.LogType.LOG_TYPE_ERROR);
		}
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


	#if PlugDLL
	// These 2 functions for internal use only
	public HomeSeerAPI.IHSApplication HSObj {
		get { return hs; }
		set { hs = value; }
	}

	public HomeSeerAPI.IAppCallbackAPI CallBackObj {
		get { return callback; }
		set { callback = value; }
	}
	#endif

	public string InitIO(string port)
	{

		Console.WriteLine("InitIO called with parameter port as " + port);
            Instance = Program.AllInstances[InstanceFriendlyName()];
        string[] plugins = Instance.host.GetPluginsList();
		Util.gEXEPath = Instance.host.GetAppPath();
            

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
                wpd.link = "SIIDPage" + "?instance=" + Instance.name;
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
                case ("BACnet Device")://????
                    {
                        return Instance.bacnetDevices.BuildBACnetDeviceTab(dvRef);
                    }
                case ("Scratchpad")://????
                    {
                        return Instance.scrPage.BuildScratchDeviceTab(dvRef); 
                    }


            }

            StringBuilder stb = new StringBuilder();

		stb.Append("<br>Call: " + configcalls.ToString() + "<br><br>");
		clsJQuery.jqButton but = new clsJQuery.jqButton("button", "Press", "deviceutility", true);
		stb.Append(but.Build());

		stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("sample_div", ""));
		stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());


		configcalls += 1;

		return stb.ToString();
	}

	public Enums.ConfigDevicePostReturn ConfigDevicePost(int dvRef, string data, string user, int userRights) //this what we need to do?
	{ //changes made to the special tab do ajax callbacks to here
            Console.WriteLine("In Configure Device Post");
          /*  Scheduler.Classes.DeviceClass ourDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dvRef);
            var EDO = ourDevice.get_PlugExtraData_Get(Instance.host);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            switch (ourDevice.get_Device_Type_String(Instance.host))
            {
                case ("Modbus Gateway"):
                    {
                        ModbusDevicePage Instance.modPage = new ModbusDevicePage("ModbusDevicePage");
                         Instance.modPage.parseModbusGatewayTab(data);
                        break;
                    }
                case ("Modbus Device"):
                    {
                        ModbusDevicePage Instance.modPage = new ModbusDevicePage("ModbusDevicePage");
                         Instance.modPage.parseModbusDeviceTab( data);
                        break;
                    }
                case ("BacNet")://????
                    {
                        //?????
                        break;
                    }

                   

            }
            */

		return Enums.ConfigDevicePostReturn.DoneAndCancelAndStay;
	}

	// Web Page Generation - OLD METHODS
	// ================================================================================================
	public string GenPage(string link)
	{
            Console.WriteLine("ALSO AM HERE "+link); //for some reason, Ajax calls from other instances going here
            return "Generated from GenPage in plugin " + Util.IFACE_NAME;
	}
	public string PagePut(string data)
	{
            Console.WriteLine("In Page Put");
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
                Console.WriteLine("IN SIID PAGE");
                return (Instance.siidPage.GetPagePlugin(pageName, user, userRights, queryString));
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
            Console.WriteLine("PostBackProc pageName: " + pageName);
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

                return Instance.modPage.parseModbusDeviceTab(data);

            }
           else if(pageName == "Scratch" + Instance.ajaxName)
            {

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
                System.Threading.Tasks.Task.Factory.StartNew(() => Instance.modPage.ReadWriteIfMod(CC));
            


            }

        }

	private Scheduler.Classes.DeviceClass FindThermChildByType(int root_dv, DeviceTypeInfo_m.DeviceTypeInfo.eDeviceType_Thermostat dev_type)
	{

		// set therm operating mode
		// get the root device so we can find associated devices
		Scheduler.Classes.DeviceClass dv = (Scheduler.Classes.DeviceClass) Instance.host.GetDeviceByRef(root_dv);
		int[] list = null;
		DeviceTypeInfo_m.DeviceTypeInfo DT = default(DeviceTypeInfo_m.DeviceTypeInfo);

		if (dv != null) {
			// have the root device, get all associated devices
            list = dv.get_AssociatedDevices(Instance.host);
			for (int index = 0; index <= list.Length - 1; index++) {
				int childref = list[index];
                Scheduler.Classes.DeviceClass child_dv = (Scheduler.Classes.DeviceClass) Instance.host.GetDeviceByRef(childref);
				if (child_dv != null) {
					DT = child_dv.get_DeviceType_Get(null);
					if (DT.Device_Type == (int) dev_type) {
						return child_dv;
					}
				}
			}
		}
		return null;
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
		Util.strAction strAction = default(Util.strAction);



		if (ValidAct(ActInfo.TANumber)) {
			// This is a valid action number for the sample plug-in which offers 2 

			if (ActInfo.TANumber == 1) {
				strAction = GetActs(ActInfo, ref ActInfo.DataIn);
				if (strAction.Result && strAction.WhichAction == Util.eActionType.Weight && strAction.ActObj != null) {
                    Classes.MyAction1EvenTon Act1 = null;
                    Act1 = (Classes.MyAction1EvenTon)strAction.ActObj;
                    if (Act1.SetTo == Classes.MyAction1EvenTon.eSetTo.Not_Set)
                    {
						// NOTE: You must add sUnique to the name of your control!
						clsJQuery.jqDropList DL = new clsJQuery.jqDropList("Action1TypeList" + sUnique, "Events", true);
						DL.AddItem("(Not Set)", "0", true);
						DL.AddItem("Round Tonnage", "1", false);
						DL.AddItem("Unrounded Tonnage", "2", false);
						st.Append("Set Weight Option Mode:" + DL.Build());
					} else {
						clsJQuery.jqCheckBox CB1 = new clsJQuery.jqCheckBox("Action1Type" + sUnique, "", "Events", true, true);
						if (Act1.SetTo == Classes.MyAction1EvenTon.eSetTo.Rounded) {
							CB1.@checked = true;
							st.Append("Uncheck to revert to Unrounded weights:");
						} else {
							CB1.@checked = false;
							st.Append("Check to change to Rounded weights:");
						}
						st.Append(CB1.Build());
					}
				}
			}

			if (ActInfo.TANumber == 2) {
				clsJQuery.jqDropList DL2 = new clsJQuery.jqDropList("Act2SubActSelect" + sUnique, "Events", true);
				if (!ValidSubAct(ActInfo.TANumber, ActInfo.SubTANumber)) {
					DL2.AddItem(" ", "-1", true);
				}
				if (ActInfo.SubTANumber < 3 & ValidSubAct(ActInfo.TANumber, ActInfo.SubTANumber)) {
					mvarActionAdvanced = true;
				}
				if (mvarActionAdvanced) {
					DL2.AddItem("Action 2 SubAction 1 - Set Voltage to European", "1", Convert.ToBoolean((ActInfo.SubTANumber == 1 ? true : false)));
					DL2.AddItem("Action 2 SubAction 2 - Set Voltage to North American", "2", Convert.ToBoolean((ActInfo.SubTANumber == 2 ? true : false)));
				}
				DL2.AddItem("Action 2 SubAction 3 - Reset Average Voltage", "3", Convert.ToBoolean((ActInfo.SubTANumber == 3 ? true : false)));
				if (!ValidSubAct(ActInfo.TANumber, ActInfo.SubTANumber)) {
                    st.Append("Choose a Voltage Sub-Action: " + DL2.Build());
				} else {
                    st.Append("Change the Voltage Sub-Action: " + DL2.Build());
				}
			}


		} else {
			return "Error, Action number for plug-in " + Util.IFACE_NAME + " was not set.";

		}

		return st.ToString();
	}

	public bool ActionConfigured(IPlugInAPI.strTrigActInfo ActInfo)
	{
		Console.WriteLine("ActionConfigured Called");
		if (ValidAct(ActInfo.TANumber)) {
			return ValidSubAct(ActInfo.TANumber, ActInfo.SubTANumber);
		} else {
			return false;
		}
	}

	public bool ActionReferencesDevice(IPlugInAPI.strTrigActInfo ActInfo, int dvRef)
	{
		Console.WriteLine("ActionReferencesDevice Called");
		//
		// Actions in the sample plug-in do not reference devices, but for demonstration purposes we will pretend they do, 
		//   and that ALL actions reference our sample devices.
		//
		if (dvRef == Util.MyDevice)
			return true;
		return false;
	}

	public string ActionFormatUI(IPlugInAPI.strTrigActInfo ActInfo)
	{
		StringBuilder st = new StringBuilder();

		if (ValidAct(ActInfo.TANumber)) {
			Util.strAction strAction;
			if (ActInfo.TANumber == 1) {
				strAction = GetActs(ActInfo, ref ActInfo.DataIn);
				if (strAction.Result && strAction.WhichAction == Util.eActionType.Weight && strAction.ActObj != null) {
                    Classes.MyAction1EvenTon Act1 = null;
                    Act1 = (Classes.MyAction1EvenTon)strAction.ActObj;
					if (Act1.SetTo == Classes.MyAction1EvenTon.eSetTo.Not_Set) {
						st.Append("The Weight Options Action has not been configured yet.");
					} else {
                        if (Act1.SetTo == Classes.MyAction1EvenTon.eSetTo.Rounded)
                        {
							st.Append("Calculated weights will be rounded.");
						} else {
							st.Append("Calculated weights will not be rounded.");
						}
					}
				} else {
					st.Append("Error, " + Util.IFACE_NAME + " Weight option action was not recovered.");
				}
			}
			if (ActInfo.TANumber == 2) {
				if (!ValidSubAct(ActInfo.TANumber, ActInfo.SubTANumber)) {
					st.Append("The voltage options action has not been configured yet.");
				} else {
					switch (ActInfo.SubTANumber) {
						case 1:
							st.Append("Voltages will be European (220 @ 50Hz)");
							break;
						case 2:
							st.Append("Voltages will be North American (110 @ 60Hz)");
							break;
						case 3:
							st.Append("The average voltage calculation will be reset to zero.");
							break;
					}
				}
			}
		} else {
			st.Append("This action for plug-in " + Util.IFACE_NAME + " was not properly set by HomeSeer.");
		}
		return st.ToString();
	}

	public string get_ActionName(int ActionNumber)
    {
			if (!ValidAct(ActionNumber))
				return "";
			switch (ActionNumber) {
				case 1:
					return Util.IFACE_NAME + ": Set Weight Option";
				case 2:
					return Util.IFACE_NAME + ": Voltage Actions";
			}
			return "";
	}

	public IPlugInAPI.strMultiReturn ActionProcessPostUI(System.Collections.Specialized.NameValueCollection PostData, IPlugInAPI.strTrigActInfo ActInfoIN)
	{

		HomeSeerAPI.IPlugInAPI.strMultiReturn Ret = new HomeSeerAPI.IPlugInAPI.strMultiReturn();
		Ret.sResult = "";
		// We cannot be passed info ByRef from HomeSeer, so turn right around and return this same value so that if we want, 
		//   we can exit here by returning 'Ret', all ready to go.  If in this procedure we need to change DataOut or TrigInfo,
		//   we can still do that.
		Ret.DataOut = ActInfoIN.DataIn;
		Ret.TrigActInfo = ActInfoIN;

		if (PostData == null)
			return Ret;
		if (PostData.Count < 1)
			return Ret;
		System.Text.StringBuilder st = new System.Text.StringBuilder();
		string sKey = null;
		string sValue = null;
		EventWebControlInfo e = default(EventWebControlInfo);

		Classes.MyAction1EvenTon Act1 = null;
		Classes.MyAction2Euro Act2 = null;
		
		try {
			// Look for the Action and SubAction selections because if they changed, then 
			//   GetTrigs will create a new Action object before the other changes are applied.
			for (int i = 0; i <= PostData.Count - 1; i++) {
				sKey = PostData.GetKey(i);
				Instance.host.WriteLog(Util.IFACE_NAME + "Debug", sKey + "potatoes!");
				sValue = PostData[sKey].Trim();
				if (sKey == null)
					continue;
				if (string.IsNullOrEmpty(sKey.Trim()))
					continue;
				//       Instance.host.WriteLog(Util.IFACE_NAME & " DEBUG", sKey & "=" & sValue)
				if (sKey.Trim() == "id") {
					e = U_Get_Control_Info(sValue.Trim());
				} else {
					e = U_Get_Control_Info(sKey.Trim());
				}

				if (e.Decoded) {
					if (e.TrigActGroup == enumTAG.Group | e.TrigActGroup == enumTAG.Trigger)
						continue;

					if ((e.EvRef == ActInfoIN.evRef)) {
						switch (e.Name_or_ID) {
							case "Act2SubActSelect":
								Ret.TrigActInfo.SubTANumber = Convert.ToInt32(sValue.Trim());
								break;
							case "Action1TypeList":
								Ret.TrigActInfo.SubTANumber = Convert.ToInt32(sValue.Trim());
								break;
							case "Action1Type":
								switch (sValue.Trim().ToLower()) {
									case "checked":
										Ret.TrigActInfo.SubTANumber = 1;
										break;
									case "unchecked":
										Ret.TrigActInfo.SubTANumber = 2;
										break;
								}
								break;
						}
					}

				}
			}

			// This uses the event information or the data passed to us to get or create our
			//   action object.
			Util.strAction strAct = default(Util.strAction);
			strAct = GetActs(Ret.TrigActInfo, ref ActInfoIN.DataIn);
			if (strAct.Result == false) {
				// The action object was not found AND there is not enough info (ActionNumber)
				//   to create a new one, so there is really nothing we can do here!  We will 
				//   wipe out the data since it did not lead to recovery of the action object.
				Ret.DataOut = null;
				Ret.sResult = "No action object was created by " + Util.IFACE_NAME + " - not enough information provided.";
				return Ret;
			}


			//Check for a sub-Action change:
			if (strAct.WhichAction == Util.eActionType.Voltage && strAct.ActObj != null) {
				try {
					Act2 = (Classes.MyAction2Euro)strAct.ActObj;
				} catch (Exception) {
					Act2 = null;
				}
				if (Act2 != null) {
					if (ValidSubAct(Ret.TrigActInfo.TANumber, Ret.TrigActInfo.SubTANumber)) {
						switch (Ret.TrigActInfo.SubTANumber) {
							case 1:
								Act2.ThisAction = Classes.MyAction2Euro.eVAction.SetEuro;
								break;
							case 2:
								Act2.ThisAction = Classes.MyAction2Euro.eVAction.SetNorthAmerica;
								break;
							case 3:
								Act2.ThisAction = Classes.MyAction2Euro.eVAction.ResetAverage;
								break;
						}
					}
					if (!Util.SerializeObject(Act2, ref Ret.DataOut)) {
						Ret.sResult = Util.IFACE_NAME + " Error, Action type 2 was modified but serialization failed.";
						return Ret;
					}
				}
			} else if (strAct.WhichAction == Util.eActionType.Weight && strAct.ActObj != null) {
				try {
                    Act1 = (Classes.MyAction1EvenTon)strAct.ActObj;
				} catch (Exception) {
					Act1 = null;
				}
				if (ValidSubAct(Ret.TrigActInfo.TANumber, Ret.TrigActInfo.SubTANumber)) {
					switch (Ret.TrigActInfo.SubTANumber) {
						case 1:
                            Act1.SetTo = Classes.MyAction1EvenTon.eSetTo.Rounded;
							break;
						case 2:
                            Act1.SetTo = Classes.MyAction1EvenTon.eSetTo.Unrounded;
							break;
					}
				}
				if (Act1 != null) {
					if (!Util.SerializeObject(Act1, ref Ret.DataOut)) {
						Ret.sResult = Util.IFACE_NAME + " Error, Action type 1 was modified but serialization failed.";
						return Ret;
					}
				}
			}

        }
        catch (Exception ex)
        {
			Ret.sResult = "ERROR, Exception in Action UI of " + Util.IFACE_NAME + ": " + ex.Message;
			return Ret;
		}

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
		Util.strTrigger strRET = default(Util.strTrigger);
		Classes.MyTrigger2Shoe Trig2 = null;
		strRET = GetTrigs(TrigInfo, TrigInfo.DataIn);
		if (strRET.WhichTrigger != Util.eTriggerType.Unknown) {
			if (strRET.WhichTrigger == Util.eTriggerType.OneTon) {
				return false;
				// Trigger 1 cannot have a condition
			} else if (strRET.WhichTrigger == Util.eTriggerType.TwoVolts) {
				Trig2 = (Classes.MyTrigger2Shoe)strRET.TrigObj;
				if (Trig2 != null) {
					return Trig2.Condition;
				}
			}
		}
		return false;
	}
	public void set_Condition(IPlugInAPI.strTrigActInfo TrigInfo, bool Value)
    {
		Util.strTrigger strRET = default(Util.strTrigger);
		Classes.MyTrigger2Shoe Trig2 = null;
		strRET = GetTrigs(TrigInfo, TrigInfo.DataIn);
		if (strRET.WhichTrigger != Util.eTriggerType.Unknown) {
			if (strRET.WhichTrigger == Util.eTriggerType.OneTon) {
				// Trigger 1 cannot have a condition
				return;
			} else if (strRET.WhichTrigger == Util.eTriggerType.TwoVolts) {
				Trig2 = (Classes.MyTrigger2Shoe)strRET.TrigObj;
				if (Trig2 != null) {
					Trig2.Condition = Value;
				}
			}
		}
	}

	public bool get_HasConditions(int TriggerNumber) {
		
		switch (TriggerNumber) {
			case 1:
				return false;
			case 2:
				return true;
			default:
				return false;
		}
		
	}

	public bool HasTriggers {
		get { return true; }
	}

	public int TriggerCount {
		get { return 2; }
	}

	public string get_TriggerName(int TriggerNumber) {
		switch (TriggerNumber) {
			case 1:
				return "Trigger 1 - CS Weighs A Ton";
			case 2:
				return "Trigger 2 - CS Voltage for You";
			default:
				return "";
		}
	}

	public int get_SubTriggerCount(int TriggerNumber)
    {
		if (TriggerNumber == 2) {
			return 2;
		} else {
			return 0;
		}
	}

	public string get_SubTriggerName(int TriggerNumber, int SubTriggerNumber)
    { 
		
	    if (TriggerNumber != 2)
			return "";
		switch (SubTriggerNumber) {
			case 1:
				return "Trig 2 SubTrig 1 - Voltage";
			case 2:
				return "Trig 2 SubTrig 2 - Average Voltage";
			default:
				return "";
		}
	}

	public string TriggerBuildUI(string sUnique, HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
	{
		StringBuilder st = new StringBuilder();
		Util.strTrigger strTrigger = default(Util.strTrigger);



		if (ValidTrig(TrigInfo.TANumber)) {
			// This is a valid trigger number for the sample plug-in which offers 2 triggers (1 and 2)

			if (TrigInfo.TANumber == 1) {
				strTrigger = GetTrigs(TrigInfo, TrigInfo.DataIn);
				if (strTrigger.Result && strTrigger.WhichTrigger == Util.eTriggerType.OneTon && strTrigger.TrigObj != null) {
					Classes.MyTrigger1Ton Trig1 = null;
					Trig1 = (Classes.MyTrigger1Ton)strTrigger.TrigObj;
					if (Trig1 != null) {
						if (Trig1.Condition) {
						// This trigger is not valid for a Condition
						} else {
							clsJQuery.jqTextBox TB = new clsJQuery.jqTextBox("TriggerWeight" + sUnique, "number", Trig1.TriggerWeight.ToString(), "Events", 8, true);
							st.Append("  ");
							st.Append("Enter weight to be exceeded to trigger: " + TB.Build());
						}
					}
				}
			}

			if (TrigInfo.TANumber == 2) {
				Util.strTrigger strRET = default(Util.strTrigger);
				Classes.MyTrigger2Shoe Trig2 = null;
				strRET = GetTrigs(TrigInfo, TrigInfo.DataIn);
				if (strRET.WhichTrigger == Util.eTriggerType.TwoVolts) {
					Trig2 = (Classes.MyTrigger2Shoe)strRET.TrigObj;

					if (TrigInfo.SubTANumber == 1) {
						// Voltage
						Trig2.SubTrigger2 = false;
						clsJQuery.jqTextBox TB1 = new clsJQuery.jqTextBox("TriggerVolt" + sUnique, "number", Trig2.TriggerValue.ToString(), "Events", 8, true);
						st.Append("<br>");
						if (Trig2.Condition) {
							st.Append("Enter the instantaneous voltage (+/- 5V) for the condition to be true: " + TB1.Build());
						} else {
							st.Append("Enter the instantaneous voltage (exact) for trigger: " + TB1.Build());
						}
					} else if (TrigInfo.SubTANumber == 2) {
						// Average Voltage
						Trig2.SubTrigger2 = true;
						clsJQuery.jqTextBox TB1 = new clsJQuery.jqTextBox("TriggerAvgVolt" + sUnique, "number", Trig2.TriggerValue.ToString(), "Events", 8, true);
						st.Append("<br>");
						if (Trig2.Condition) {
							st.Append("Enter the average voltage (+/- 10V) for the condition to be true: " + TB1.Build());
						} else {
							st.Append("Enter the average voltage (exact) for trigger: " + TB1.Build());
						}

					}
				}

			}



		} else {
			return "Error, Trigger number for plug-in " + Util.IFACE_NAME + " was not set.";

		}

		return st.ToString();

	}

	public bool get_TriggerConfigured(IPlugInAPI.strTrigActInfo TrigInfo)
    {
			Util.strTrigger strRET = default(Util.strTrigger);
			Classes.MyTrigger1Ton Trig1 = null;
			Classes.MyTrigger2Shoe Trig2 = null;
			strRET = GetTrigs(TrigInfo, TrigInfo.DataIn);
			if (strRET.WhichTrigger != Util.eTriggerType.Unknown) {
				if (strRET.WhichTrigger == Util.eTriggerType.OneTon) {
					try {
						Trig1 = (Classes.MyTrigger1Ton)strRET.TrigObj;
					} catch (Exception) {
						Trig1 = null;
					}
					if (Trig1 != null) {
						if (Trig1.TriggerWeight > 0)
							return true;
					}
					return false;
				} else if (strRET.WhichTrigger == Util.eTriggerType.TwoVolts) {
					try {
						Trig2 = (Classes.MyTrigger2Shoe)strRET.TrigObj;
					} catch (Exception) {
						Trig2 = null;
					}
					if (Trig2 != null) {
						if (Trig2.TriggerValue > 0)
							return true;
					}
					return false;
				}
			}
			return false;
	}

	public bool TriggerReferencesDevice(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo, int dvRef)
	{
		//
		// Triggers in the sample plug-in do not reference devices, but for demonstration purposes we will pretend they do, 
		//   and that ALL triggers reference our sample devices.
		//
		if (dvRef == Util.MyDevice)
			return true;
		return false;
	}

	public string TriggerFormatUI(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
	{
		Util.strTrigger strRET = default(Util.strTrigger);
		Classes.MyTrigger1Ton Trig1 = null;
		Classes.MyTrigger2Shoe Trig2 = null;
		strRET = GetTrigs(TrigInfo, TrigInfo.DataIn);
		if (strRET.WhichTrigger != Util.eTriggerType.Unknown) {
			if (strRET.WhichTrigger == Util.eTriggerType.OneTon) {
				try {
					Trig1 = (Classes.MyTrigger1Ton)strRET.TrigObj;
				} catch (Exception) {
					Trig1 = null;
				}
				if (Trig1 != null) {
					if (Trig1.Condition)
						return "";
					return  "The weight exceeds " + Trig1.TriggerWeight.ToString()+ "lbs.";
				} else {
					return "ERROR (A) - Trigger 1 is not properly built yet.";
				}
			} else if (strRET.WhichTrigger == Util.eTriggerType.TwoVolts) {
				try {
					Trig2 = (Classes.MyTrigger2Shoe)strRET.TrigObj;
				} catch (Exception) {
					Trig2 = null;
				}
				if (Trig2 != null) {
					if (Trig2.SubTrigger2) {
						string sRet = "The average voltage ";
						if (Trig2.Condition) {
							sRet += "is within 10V of " + Trig2.TriggerValue.ToString() + "V";
							return sRet;
						} else {
							sRet += "is " + Trig2.TriggerValue.ToString() + "V";
							return sRet;
						}
					} else {
						string sRet = "The current voltage ";
						if (Trig2.Condition) {
							sRet += "is within 5V of " + Trig2.TriggerValue.ToString() + "V";
							return sRet;
						} else {
							sRet += "is " + Trig2.TriggerValue.ToString() + "V";
							return sRet;
						}
					}
				} else {
					return "ERROR (B) - Trigger 2 is not properly built yet.";
				}
			}
		}
		return "ERROR - The trigger is not properly built yet.";
	}

	public HomeSeerAPI.IPlugInAPI.strMultiReturn TriggerProcessPostUI(System.Collections.Specialized.NameValueCollection PostData, HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfoIn)
	{

		HomeSeerAPI.IPlugInAPI.strMultiReturn Ret = new HomeSeerAPI.IPlugInAPI.strMultiReturn();
		Ret.sResult = "";
		// We cannot be passed info ByRef from HomeSeer, so turn right around and return this same value so that if we want, 
		//   we can exit here by returning 'Ret', all ready to go.  If in this procedure we need to change DataOut or TrigInfo,
		//   we can still do that.
		Ret.DataOut = TrigInfoIn.DataIn;
		Ret.TrigActInfo = TrigInfoIn;

		if (PostData == null)
			return Ret;
        if (PostData.Count < 1)
			return Ret;
		System.Text.StringBuilder st = new System.Text.StringBuilder();
		string sKey = null;
		string sValue = null;
		EventWebControlInfo e = default(EventWebControlInfo);

		Classes.MyTrigger1Ton Trig1 = null;
		Classes.MyTrigger2Shoe Trig2 = null;
		
		try {
			// This uses the event information or the data passed to us to get or create our
			//   trigger object.
			Util.strTrigger strTrig = default(Util.strTrigger);
			strTrig = GetTrigs(Ret.TrigActInfo, TrigInfoIn.DataIn);
			if (strTrig.Result == false) {
				// The trigger object was not found AND there is not enough info (TriggerNumber)
				//   to create a new one, so there is really nothing we can do here!  We will 
				//   wipe out the data since it did not lead to recovery of the trigger object.
				Ret.DataOut = null;
				Ret.sResult = "No trigger object was created by " + Util.IFACE_NAME + " - not enough information provided.";
				return Ret;
			}

			// Now go through the data to see what specifics about the trigger may have been set.
			for (int i = 0; i <= PostData.Count - 1; i++) {
				sKey = PostData.GetKey(i);
				sValue = PostData[sKey].Trim();
				if (sKey == null)
					continue;
				if (string.IsNullOrEmpty(sKey.Trim()))
					continue;
				if (sKey.Trim() == "id") {
					e = U_Get_Control_Info(sValue.Trim());
				} else {
					e = U_Get_Control_Info(sKey.Trim());
				}

				if (e.Decoded) {
					if (e.TrigActGroup == enumTAG.Group | e.TrigActGroup == enumTAG.Action)
						continue;

					if ((e.EvRef == TrigInfoIn.evRef)) {
						switch (e.Name_or_ID) {
							//Case "SubtriggerSelect"

							case "TriggerWeight":
								if (strTrig.WhichTrigger == Util.eTriggerType.OneTon && strTrig.TrigObj != null) {
									try {
										Trig1 = (Classes.MyTrigger1Ton)strTrig.TrigObj;
                                    }
                                    catch (Exception ex)
                                    {
										Ret.sResult = Util.IFACE_NAME + " Error, Conversion of object to Trigger 1 failed: " + ex.Message;
										return Ret;
									}
									if (Trig1 != null) {
										Trig1.TriggerWeight = Conversion.Val(sValue.Trim());
									}
								}

								break;
							case "TriggerVolt":
								if (strTrig.WhichTrigger == Util.eTriggerType.TwoVolts && strTrig.TrigObj != null) {
									try {
										Trig2 = (Classes.MyTrigger2Shoe)strTrig.TrigObj;
                                    }
                                    catch (Exception ex)
                                    {
										Ret.sResult = Util.IFACE_NAME + " Error, Conversion of object to Trigger 2 failed: " + ex.Message;
										return Ret;
									}
									if (Trig2 != null) {
										Trig2.TriggerValue = Conversion.Val(sValue.Trim());
									}
								}

								break;
							case "TriggerAvgVolt":
								if (strTrig.WhichTrigger == Util.eTriggerType.TwoVolts && strTrig.TrigObj != null) {
									try {
										Trig2 = (Classes.MyTrigger2Shoe)strTrig.TrigObj;
                                    }
                                    catch (Exception ex)
                                    {
										Ret.sResult = Util.IFACE_NAME + " Error, Conversion of object to Trigger 2 failed: " + ex.Message;
										return Ret;
									}
									if (Trig2 != null) {
										Trig2.TriggerValue = Conversion.Val(sValue.Trim());
									}
								}

								break;
							default:
								Instance.host.WriteLog(Util.IFACE_NAME + " Warning", "MyPostData got unhandled key/value of " + e.Name_or_ID + "=" + sValue);
								break;
						}
					}

				}
			}


			//Check for a sub-Trigger change:
			if (strTrig.WhichTrigger == Util.eTriggerType.TwoVolts && strTrig.TrigObj != null) {
				try {
					Trig2 = (Classes.MyTrigger2Shoe)strTrig.TrigObj;
				} catch (Exception) {
					Trig2 = null;
				}
				if (Trig2 != null) {
					if (ValidSubTrig(Ret.TrigActInfo.TANumber, Ret.TrigActInfo.SubTANumber)) {
						if (Ret.TrigActInfo.SubTANumber == 2) {
							Trig2.SubTrigger2 = true;
						}
					}
					if (!Util.SerializeObject(Trig2, ref Ret.DataOut)) {
						Ret.sResult = Util.IFACE_NAME + " Error, Trigger type 2 was modified but serialization failed.";
						return Ret;
					}
				}
			} else if (strTrig.WhichTrigger == Util.eTriggerType.OneTon && strTrig.TrigObj != null) {
				try {
					Trig1 = (Classes.MyTrigger1Ton)strTrig.TrigObj;
				} catch (Exception) {
					Trig1 = null;
				}
				if (Trig1 != null) {
                    if (!Util.SerializeObject(Trig1, ref Ret.DataOut))
                    {
                        Ret.sResult = Util.IFACE_NAME + " Error, Trigger type 1 was modified but serialization failed.";
						return Ret;
					}
				}
			}

        }
        catch (Exception ex)
        {
			Ret.sResult = "ERROR, Exception in Trigger UI of " + Util.IFACE_NAME + ": " + ex.Message;
			return Ret;
		}

		// All OK
		Ret.sResult = "";
		return Ret;

	}

	public bool TriggerTrue(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
	{
	
		return false;

	}

	#endregion

	private Util.strTrigger GetTrigs(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo, byte[] DataIn)
	{
            var strRET = new Util.strTrigger();
            strRET.TrigObj = null;
		strRET.WhichTrigger = Util.eTriggerType.Unknown;
		strRET.Result = false;
		return strRET;

	}

	private Util.strAction GetActs(HomeSeerAPI.IPlugInAPI.strTrigActInfo ActInfo, ref byte[] DataIn)
	{
		


		var strRET = new Util.strAction();
		strRET.ActObj = null;
		strRET.WhichAction = Util.eActionType.Unknown;
		strRET.Result = false;
		return strRET;

	}

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

	static internal EventWebControlInfo U_Get_Control_Info(string sIN)
	{
		EventWebControlInfo e = new EventWebControlInfo();
		e.EventTriggerGroupID = -1;
		e.GroupID = -1;
		e.EvRef = -1;
		e.Name_or_ID = "";
		e.TriggerORActionID = -1;
		e.Decoded = false;
		e.Additional = "";
		e.TrigActGroup = enumTAG.Unknown;

		if (sIN == null)
			return e;
		if (string.IsNullOrEmpty(sIN.Trim()))
			return e;
		if (!sIN.Contains("_"))
			return e;
		string[] s = null;
		string[] ch = new string[1];
		ch[0] = "_";
		s = sIN.Split(ch, StringSplitOptions.None);
		if (s == null)
			return e;
		if (s.Length < 1)
			return e;
		if (s.Length == 1) {
			e.Name_or_ID = s[0].Trim();
			e.Decoded = true;
			return e;
		}
		string sTemp = null;
		for (int i = 0; i <= s.Length - 1; i++) {
			if (s[i] == null)
				continue;
			if (string.IsNullOrEmpty(s[i].Trim()))
				continue;
			if (i == 0) {
				e.Name_or_ID = s[0].Trim();
			} else {
				if (s[i].Trim() == "ID")
					continue;
				if (s[i].Trim().StartsWith("G")) {
					sTemp = s[i].Substring(1).Trim();
					if (Information.IsNumeric(sTemp)) {
						e.EventTriggerGroupID = Convert.ToInt32(Conversion.Val(sTemp));
						e.TrigActGroup = enumTAG.Trigger;
					}
				} else if (s[i].Trim().StartsWith("L")) {
					sTemp = s[i].Substring(1).Trim();
					if (Information.IsNumeric(sTemp)) {
						e.GroupID = Convert.ToInt32(Conversion.Val(sTemp));
						e.TrigActGroup = enumTAG.Group;
					}
				} else if (s[i].Trim().StartsWith("T")) {
					sTemp = s[i].Substring(1).Trim();
					if (Information.IsNumeric(sTemp)) {
						e.TriggerORActionID = Convert.ToInt32(Conversion.Val(sTemp));
						e.TrigActGroup = enumTAG.Trigger;
					}
				} else if (s[i].Trim().StartsWith("A")) {
					sTemp = s[i].Substring(1).Trim();
					if (Information.IsNumeric(sTemp)) {
						e.TriggerORActionID = Convert.ToInt32(Conversion.Val(sTemp));
						e.TrigActGroup = enumTAG.Action;
					}
				} else {
					if (Information.IsNumeric(s[i].Trim())) {
						e.EvRef = Convert.ToInt32(Conversion.Val(s[i].Trim()));
					} else {
						if (string.IsNullOrEmpty(e.Additional)) {
							e.Additional = s[i].Trim();
						} else {
							e.Additional += "_" + s[i].Trim();
						}
					}
				}
			}
		}
		e.Decoded = true;
		return e;
	}
	internal bool ValidTrigInfo(HomeSeerAPI.IPlugInAPI.strTrigActInfo TrigInfo)
	{
		if (TrigInfo.evRef > 0) {
		} else {
			return false;
		}
		if (TrigInfo.TANumber > 0 && TrigInfo.TANumber < 3) {
			if (TrigInfo.TANumber == 1)
				return true;
			if (TrigInfo.SubTANumber > 0 && TrigInfo.SubTANumber < 3)
				return true;
		}
		return false;
	}
	internal bool ValidActInfo(HomeSeerAPI.IPlugInAPI.strTrigActInfo ActInfo)
	{
		if (ActInfo.evRef > 0) {
		} else {
			return false;
		}
		if (ActInfo.TANumber > 0 && ActInfo.TANumber < 3) {
			if (ActInfo.TANumber == 1)
				return true;
			if (ActInfo.SubTANumber > 0 && ActInfo.SubTANumber < 4)
				return true;
		}
		return false;
	}
	internal bool ValidTrig(int TrigIn)
	{
		if (TrigIn > 0 && TrigIn < 3)
			return true;
		return false;
	}
	internal bool ValidAct(int ActIn)
	{
		if (ActIn > 0 && ActIn < 3)
			return true;
		return false;
	}
	internal bool ValidSubTrig(int TrigIn, int SubTrigIn)
	{
		if (TrigIn > 0 && TrigIn < 3) {
			if (TrigIn == 1)
				return true;
			if (SubTrigIn > 0 && SubTrigIn < 3)
				return true;
		}
		return false;
	}
	internal bool ValidSubAct(int ActIn, int SubActIn)
	{
		if (ActIn > 0 && ActIn < 3) {
			if (ActIn == 1) {
				if (SubActIn > 0 && SubActIn < 3)
					return true;
				return false;
			}
			if (SubActIn > 0 && SubActIn < 4)
				return true;
		}
		return false;
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
