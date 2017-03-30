using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler;
using System.Web;

using System.Threading;
using HSPI_SIID_ModBusDemo.Modbus;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using HomeSeerAPI;
using Microsoft.VisualBasic.FileIO;
using HSPI_SIID.ScratchPad;
using HSPI_SIID.General;
using System.Reflection;

namespace HSPI_SIID_ModBusDemo
{
    public class SIID_Page : PageBuilderAndMenu.clsPageBuilder

    {
   
        public InstanceHolder Instance { get; set; }



        public string OurPageName {get;set;}
     //   public HSPI hspi { get; set; }
    


        public static Dictionary<int, System.Threading.Timer> PluginTimerDictionary = new Dictionary<int, Timer>(); //Indexed by device ID, value is timers, intended for modbus gateway polls. idea is one per gateway ID
                                                                                                                    //Needs to instance this when the plugin initializes, needs to update when a new gateway is added or when the polling interval changes
        private System.Threading.Timer ScratchTimer { get; set; }

        public SIID_Page(string pagename, InstanceHolder instance) : base(pagename)
        {
   
            Instance = instance;
        
           
        OurPageName=pagename;
           InitializeModbusGatewayTimers();
            InitializeScratchpadTimer();
        }

       

        public int selectedPlugin { get; set; }

    


        public void LoadINISettings()
        {
            Console.WriteLine("IN LOAD "+ OurPageName);
            selectedPlugin = Convert.ToInt32(Instance.host.GetINISetting("CONFIG", "Selected_Plugin", "1", OurPageName.Replace(":", "") + ".INI"));
            Instance.modAjax.loadModbusConfig();


            SaveAllINISettings();
        }

        public void SaveAllINISettings()
        {
            Console.WriteLine("IN SAVE " + OurPageName);
            Instance.host.SaveINISetting("CONFIG", "Selected_Plugin", selectedPlugin.ToString(), OurPageName.Replace(":","") + ".INI");

            Instance.modAjax.saveModbusConfig();
            //modbus specific config



        }
        

        public void SaveSpecificINISetting(string section, string key, string value)
        {
            Instance.host.SaveINISetting(section, key , value , OurPageName.Replace(":", "") + ".INI");

        }


        public void ImportDevices(string RawCsv)
        {
          
            try
            {
                //    int commonrowOffset =  new HSPI_SIID.HomeSeerDevice().listOfAttributes.Count();
                List<Tuple<int, int, string>> DevicesToImport = new List<Tuple<int, int, string>>(); //Dv to replace, header index, row

                Dictionary<int, List<string>> RowByHeaderID = new Dictionary<int, List<string>>();
                Dictionary<int, int> OldToNew = new Dictionary<int, int>();
                string[] CSVRows = RawCsv.Split('\n');
                List<string> Headers = new List<string>();
                bool HasHeader = false;

                foreach (string row in CSVRows) //keep track of subsection's headers. we must use those
                {
                    //Is a valid device if the first cell is an integer. Is a header for the following valid rows if the first cell is not an integer but there are more than one cells in the line.
                    int CellCount = row.Split(',').Count();
                    if (CellCount > 1) //Then either a header row or a Device row
                    {
                        int ID = 0;
                        string T = row.Split(',')[0]; //May or may not have quotes and \ around number
                        int.TryParse(System.Text.RegularExpressions.Regex.Replace(row.Split(',')[0], "[^.0-9]", ""), out ID); //First cell is always ID or a header, If it parses as a number 
                        if (ID != 0 && HasHeader) //ID was a valid number
                        {
                            var dv = Instance.host.NewDeviceRef("ImportingDevice");
                            //DevicesToImport.Add(new Tuple<int, int, string>(dv, Headers.Count - 1, row.Replace("\r", "")));
                            RowByHeaderID[Headers.Count - 1].Add(row + "\n");
                            OldToNew[ID] = dv;
                        }
                        else
                        {
                            HasHeader = true;
                            Headers.Add(row.ToLower()+"\n");
                            RowByHeaderID[Headers.Count - 1] = new List<string>();

                        }

                    }


                }

                //Running into problem when parsing csv rows.
                //Seems way to do it is to create a csv file
                //Then hit those csv files with a file parser...

                int count = 0;
                foreach(string row in Headers)
                {
                    StringBuilder FileString = new StringBuilder();
                    FileString.Append(row + "\n");
                    foreach (string Entry in RowByHeaderID[count])
                    {
                        
                            FileString.Append(Entry);
                        

                    }
                    byte[] byteArray = Encoding.ASCII.GetBytes(FileString.ToString());
                    MemoryStream stream = new MemoryStream(byteArray);
                    using (TextFieldParser parser = new TextFieldParser(stream))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");
                        parser.TrimWhiteSpace = true;
                        bool hasHeader = false;
                        string[] HeaderArray = null;
                        while (!parser.EndOfData)
                        {
                            string[] fieldRow = parser.ReadFields();
                            
                            if (!hasHeader)
                            {
                             
                                HeaderArray = fieldRow;
                                hasHeader = true;
                            }
                            else
                            {
                                int FRind = 0;
                                Dictionary<string, string> CodeLookup = new Dictionary<string, string>();
                                foreach (string fieldRowCell in fieldRow)
                                {
                                    CodeLookup[HeaderArray[FRind].ToLower()] = fieldRowCell;
                                    FRind++;
                                }
                                try
                                {
                                    Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(OldToNew[int.Parse(CodeLookup["id"])]);
                                    newDevice.set_Name(Instance.host, FetchAttribute(CodeLookup, "Name"));
                                    newDevice.set_Location2(Instance.host, FetchAttribute(CodeLookup, "Floor"));
                                    newDevice.set_Location(Instance.host, FetchAttribute(CodeLookup, "Room"));
                                    //newDevice.set_Interface(Instance.host, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
                                    newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function
                                    newDevice.set_InterfaceInstance(Instance.host, Instance.name);
                                    newDevice.set_Address(Instance.host, FetchAttribute(CodeLookup, "address"));
                                    newDevice.set_Code(Instance.host, FetchAttribute(CodeLookup, "code"));
                                    //newDevice.set_InterfaceInstance()''  SET INTERFACE INSTANCE
                                    newDevice.set_Status_Support(Instance.host, bool.Parse(FetchAttribute(CodeLookup, "statusOnly")));
                                    newDevice.set_Can_Dim(Instance.host, bool.Parse(FetchAttribute(CodeLookup, "CanDim")));
                                    newDevice.set_UserAccess(Instance.host, FetchAttribute(CodeLookup, "useraccess"));
                                    newDevice.set_UserNote(Instance.host, FetchAttribute(CodeLookup, "notes"));
                                    newDevice.set_Device_Type_String(Instance.host, FetchAttribute(CodeLookup, "deviceTypeString"));

                                    
                                    switch(FetchAttribute(CodeLookup, "RelationshipStatus"))
                                    {
                                        case ("Not_Set"):
                                            {
                                                newDevice.set_Relationship(Instance.host,Enums.eRelationship.Not_Set);
                                                break;
                                            }
                                        case ("Indeterminate"):
                                            {
                                                newDevice.set_Relationship(Instance.host, Enums.eRelationship.Indeterminate);
                                                break;
                                            }
                                        case ("Child"):
                                            {
                                                newDevice.set_Relationship(Instance.host, Enums.eRelationship.Child);
                                                break;
                                            }

                                        case ("Parent_Root"):
                                            {
                                                newDevice.set_Relationship(Instance.host, Enums.eRelationship.Parent_Root);
                                                break;
                                            }
                                        case ("Standalone"):
                                            {
                                                newDevice.set_Relationship(Instance.host, Enums.eRelationship.Standalone);
                                                break;
                                            }


                                    }


                                    if (bool.Parse(FetchAttribute(CodeLookup, "donotlog")))
                                    {
                                        newDevice.MISC_Set(Instance.host, Enums.dvMISC.NO_LOG);

                                    }
                                    try
                                    {
                                        string[] DeviceTypes = FetchAttribute(CodeLookup, "DeviceType").Split('_');

                                        if (DeviceTypes.Count() == 6)
                                        {
                                            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
                                            DevINFO.Device_API = (DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI)int.Parse(DeviceTypes[0]);
                                            DevINFO.Device_SubType = int.Parse(DeviceTypes[2]);
                                            DevINFO.Device_SubType_Description = DeviceTypes[3];
                                            DevINFO.Device_Type = int.Parse(DeviceTypes[4]);
                                            newDevice.set_DeviceType_Set(Instance.host, DevINFO);
                                        }
                                    }
                                    catch
                                    {

                                    }
                                    //Now replace associated devices with their new ID:
                                    string[] OldAssocDeviceList = FetchAttribute(CodeLookup, "associatedDevicesList").Split(',');
                                    foreach (string Old in OldAssocDeviceList)
                                    {
                                        int T = 0;
                                        int.TryParse(Old, out T);
                                        if (T != 0)
                                        {
                                            newDevice.AssociatedDevice_Add(Instance.host, OldToNew[T]);
                                        }
                                      
                                    }
                                    HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();
                                    switch (CodeLookup["type"])
                                    {
                                        case ("BACnet"):
                                            {

                                                break;
                                            }
                                        case ("Scratchpad"):
                                            {
                                                var parts = HttpUtility.ParseQueryString(string.Empty);
                                                parts["Type"] = FetchAttribute(CodeLookup, "type"); ;
                                                parts["IsEnabled"] = FetchAttribute(CodeLookup, "isenabled"); ;
                                                parts["IsAccumulator"] = FetchAttribute(CodeLookup, "isaccumulator"); ;
                                                //    parts["UpdateInterval"] = "30000"; //Global every 30 seconds for all rules
                                                parts["ResetType"] = FetchAttribute(CodeLookup, "resettype"); ;
                                                parts["ResetInterval"] = FetchAttribute(CodeLookup, "resetinterval"); ;

                                                parts["ResetTime"] = FetchAttribute(CodeLookup, "resettime"); ;
                                                parts["DayOfWeek"] = FetchAttribute(CodeLookup, "dayofweek"); ;
                                                parts["DayOfMonth"] = FetchAttribute(CodeLookup, "dayofmonth"); ;

                                                string ScratchString = FetchAttribute(CodeLookup, "ScratchpadString");
                                                foreach (KeyValuePair<int, int> OLDTONEW in OldToNew)
                                                {
                                                    string old = "$(" + OLDTONEW.Key + ")";
                                                    string NN = "$(" + OLDTONEW.Value + ")";
                                                    ScratchString = ScratchString.Replace(old, NN);
                                                    ScratchString = ScratchString.Replace("#(" + OLDTONEW.Key + ")", "#(" + OLDTONEW.Value + ")");
                                                }

                                                parts["ScratchPadString"] = ScratchString;
                                                parts["DisplayString"] = FetchAttribute(CodeLookup, "displaystring"); ;
                                                parts["OldValue"] = FetchAttribute(CodeLookup, "oldvalue"); ;
                                                parts["NewValue"] = FetchAttribute(CodeLookup, "newvalue"); ;
                                                parts["DisplayedValue"] = FetchAttribute(CodeLookup, "displayedvalue"); ;
                                                parts["DateOfLastReset"] = FetchAttribute(CodeLookup, "dateoflastreset");
                                                EDO.AddNamed("SSIDKey", parts.ToString());
                                                break;
                                            }
                                        case ("Modbus Gateway"):
                                            {
                                                Instance.modPage.MakeGatewayGraphicsAndStatus(OldToNew[int.Parse(CodeLookup["id"])]);
                                                newDevice.MISC_Set(Instance.host, Enums.dvMISC.SHOW_VALUES);



                                                var parts = HttpUtility.ParseQueryString(string.Empty);


                                                parts["Type"] = FetchAttribute(CodeLookup, "type");
                                                parts["Gateway"] = FetchAttribute(CodeLookup, "gateway");
                                                parts["TCP"] = FetchAttribute(CodeLookup, "tcp");
                                                parts["Poll"] = FetchAttribute(CodeLookup, "poll");
                                                parts["Enabled"] = FetchAttribute(CodeLookup, "enabled");
                                                parts["BigE"] = FetchAttribute(CodeLookup, "bige");
                                                parts["ZeroB"] = FetchAttribute(CodeLookup, "zerob");
                                                parts["RWRetry"] = FetchAttribute(CodeLookup, "rwretry");
                                                parts["RWTime"] = FetchAttribute(CodeLookup, "rwtime");
                                                parts["Delay"] = FetchAttribute(CodeLookup, "delay");
                                                parts["RegWrite"] = FetchAttribute(CodeLookup, "regwrite");

                                                StringBuilder NewRef = new StringBuilder();
                                                foreach (string old in FetchAttribute(CodeLookup, "LinkedDevices").Split(','))
                                                {
                                                    try
                                                    {
                                                        NewRef.Append(OldToNew[int.Parse(old)] + ",");
                                                    }
                                                    catch
                                                    {

                                                    }
                                                }

                                                parts["LinkedDevices"] = NewRef.ToString();
                                                parts["RawValue"] = FetchAttribute(CodeLookup, "RawValue");
                                                parts["ProcessedValue"] = FetchAttribute(CodeLookup, "ProcessedValue");
                                                EDO.AddNamed("SSIDKey", parts.ToString());



                                                break;
                                            }
                                        case ("Modbus Device"):
                                            {
                                                Instance.modPage.MakeSubDeviceGraphicsAndStatus(OldToNew[int.Parse(CodeLookup["id"])]);
                                                newDevice.MISC_Set(Instance.host, Enums.dvMISC.SHOW_VALUES);
                                                var parts = HttpUtility.ParseQueryString(string.Empty);
                                                parts["Type"] = FetchAttribute(CodeLookup, "type");

                                                parts["GateID"] = OldToNew[int.Parse(FetchAttribute(CodeLookup, "GateID"))].ToString(); //Replace


                                                parts["Gateway"] = FetchAttribute(CodeLookup, "Gateway");
                                                parts["RegisterType"] = FetchAttribute(CodeLookup, "RegisterType");//Instance.modAjax.modbusDefaultPoll.ToString(); //0 is discrete input, 1 is coil, 2 is InputRegister, 3 is Holding Register
                                                parts["SlaveId"] = FetchAttribute(CodeLookup, "SlaveId"); //get number of slaves from gateway?
                                                parts["ReturnType"] = FetchAttribute(CodeLookup, "ReturnType");
                                                //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
                                                //tells us how many registers to read/write and also how to parse returns
                                                //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
                                                //So coil and discrete are bool ONLY
                                                //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read
                                                parts["SignedValue"] = FetchAttribute(CodeLookup, "SignedValue");


                                                string ScratchString = FetchAttribute(CodeLookup, "ScratchpadString");
                                                foreach (KeyValuePair<int, int> OLDTONEW in OldToNew)
                                                {
                                                    string old = "$(" + OLDTONEW.Key + ")";
                                                    string NN = "$(" + OLDTONEW.Value + ")";
                                                    ScratchString=ScratchString.Replace(old,NN);
                                                    ScratchString=ScratchString.Replace("#(" + OLDTONEW.Key + ")", "#(" + OLDTONEW.Value + ")");
                                                }
                                                parts["ScratchpadString"] = ScratchString; //Replace
                                                parts["DisplayFormatString"] = FetchAttribute(CodeLookup, "DisplayFormatString");
                                                parts["ReadOnlyDevice"] = FetchAttribute(CodeLookup, "ReadOnlyDevice");
                                                parts["DeviceEnabled"] = FetchAttribute(CodeLookup, "DeviceEnabled");
                                                parts["RegisterAddress"] = FetchAttribute(CodeLookup, "RegisterAddress");
                                                parts["RawValue"] = FetchAttribute(CodeLookup, "RawValue");
                                                parts["ProcessedValue"] = FetchAttribute(CodeLookup, "ProcessedValue");

                                                EDO.AddNamed("SSIDKey", parts.ToString());
                                                break;
                                            }

                                    }
                                    newDevice.set_PlugExtraData_Set(Instance.host, EDO);




                                }
                                catch
                                {


                                }







                                }
                           
                        }

                    }

                    count++;
                    }


              

            }
            catch //Fails for some reason
            {


            }

        }
        public string FetchAttribute(Dictionary<string, string> CodeLookup, string key)
        {
           try
            {

                return CodeLookup[key.ToLower()];
            }
            catch
            {
                return "False";
            }

        }

        public string ReturnDevicesInExportForm()
        {
            string FileName = Instance.hspi.InstanceFriendlyName() + "_" + "Export_Devices.CSV";
            StringBuilder FileContent = new StringBuilder();
  
            Scheduler.Classes.clsDeviceEnumeration DevNum = (Scheduler.Classes.clsDeviceEnumeration)Instance.host.GetDeviceEnumerator();
            var Dev = DevNum.GetNext();
            List<int> ModGateways = new List<int>();
            List<int> ModDevices = new List<int>();
            List<int> BackNetDevices = new List<int>();
            List<int> ScratchDevices = new List<int>();
            while (Dev != null)
            {
                try
                {
                    
                    //StringBuilder Row = new StringBuilder();
                 


                    var EDO = Dev.get_PlugExtraData_Get(Instance.host);
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString()); //So it is a SIID device
                   if (Dev.get_Interface(Instance.host).ToString() == Util.IFACE_NAME.ToString() && Dev.get_InterfaceInstance(Instance.host).ToString() == Instance.name) //Then it's one of ours (Need maybe an extra chack for Instance
                        {
                        switch (parts["Type"])
                        {
                            case ("BACnet Device"):
                                {
                                    BackNetDevices.Add(Dev.get_Ref(Instance.host));
                                    break;
                                }
                            case ("Modbus Gateway"):
                                {
                                    ModGateways.Add(Dev.get_Ref(Instance.host));
                                    break;
                                }
                            case ("Modbus Device"):
                                {
                                    ModDevices.Add(Dev.get_Ref(Instance.host));
                                    break;
                                }
                            case ("Scratchpad"):
                                {
                                    ScratchDevices.Add(Dev.get_Ref(Instance.host));
                                    break;
                                }
                             


                                  }
                                  


                        }


                    
                    //   if (parts["Type"] == "Modbus Device")
                    //     {
                    //        ModbusDevs.Add(Dev.get_Ref(Instance.host));
                    //    }

                }
                catch(Exception e)
                {
                   
                }
                Dev = DevNum.GetNext();


            }

            if (BackNetDevices.Count > 0)
            {
                FileContent.Append("Backnet Devices\r\n");
                FileContent.Append(new HSPI_SIID.SIIDDevice(BackNetDevices[0],Instance).ReturnCSVHead());
                foreach (int ID in BackNetDevices)
                {
                    FileContent.Append(new HSPI_SIID.SIIDDevice(ID,Instance).ReturnCSVRow());
                }
            }
            if (ModGateways.Count > 0)
            {
                FileContent.Append("Modbus Gateways\r\n");
                FileContent.Append(new HSPI_SIID.SIIDDevice(ModGateways[0],Instance).ReturnCSVHead());
                foreach (int ID in ModGateways)
                {
                    FileContent.Append(new HSPI_SIID.SIIDDevice(ID,Instance).ReturnCSVRow());
                }
            }
            if (ModDevices.Count > 0)
            {
                FileContent.Append("Modbus Devices\r\n");
                FileContent.Append(new HSPI_SIID.SIIDDevice(ModDevices[0],Instance).ReturnCSVHead());
                foreach (int ID in ModDevices)
                {
                    FileContent.Append(new HSPI_SIID.SIIDDevice(ID,Instance).ReturnCSVRow());
                }
            }
            if (ScratchDevices.Count > 0)
            {
                FileContent.Append("Scratchpad Rules\r\n");
                FileContent.Append(new HSPI_SIID.SIIDDevice(ScratchDevices[0], Instance).ReturnCSVHead());
                foreach (int ID in ScratchDevices)
                {
                    FileContent.Append(new HSPI_SIID.SIIDDevice(ID, Instance).ReturnCSVRow());
                }
            }







            return FileName+"_)(*&^%$#@!"+ FileContent.ToString();
        }

        public string postbackSSIDConfigPage(string page, string data, string user, int userRights)
        {
            Console.WriteLine("AM in the SIID postbackConfig for some reason");
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);
            string ID = parts["id"].Split('_')[0];
            switch (ID)
            {
                case "Import":
                    {
                        ImportDevices(parts["value"]);
                        return ""; 
                      
                    }
                case "Export":
                    {
                        return ReturnDevicesInExportForm();   
                     
                    }
                case "Scratchpad":
                    {

                        return Instance.scrPage.MakeNewRule();
                       
                    }
                /*    case "Instance":
                    {

                      Util.Instance = new Random().Next().ToString();
                        break;
                    }*/
                    
                case "SelectPlugin":
                    {
                        selectedPlugin = Convert.ToInt32(parts["pluginSelection"]);
                        SaveAllINISettings();
                        break;
                    }
            }

            //So I guess AJAX calls come back here?
            //need to register

          
            return base.postBackProc(page, data, user, userRights);
        }



        public void InitializeScratchpadTimer()
        {

          ScratchTimer = new System.Threading.Timer(Instance.scrPage.DoScratchRules, true, 30000,30000);

        }

        public void InitializeModbusGatewayTimers()
        {
            List<int> ModbusGates = Instance.modPage.getAllGateways().ToList();
            List<Scheduler.Classes.DeviceClass> ModbusDevs = new List<Scheduler.Classes.DeviceClass>();

            foreach (int GID in ModbusGates)
            {
                Scheduler.Classes.DeviceClass Dev = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(Convert.ToInt32(GID));
                var EDO = Dev.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

                if (!PluginTimerDictionary.ContainsKey(Convert.ToInt32(GID)))
                {

                    if (Dev.get_InterfaceInstance(Instance.host) == Instance.name)
                    {
                        System.Threading.Timer GateTimer = new System.Threading.Timer(Instance.modPage.PollActiveFromGate, GID, 10000, Convert.ToInt32(parts["Poll"]));

                        Console.WriteLine("Starting Polling timer for gateway: " + GID);
                        PluginTimerDictionary.Add(Convert.ToInt32(GID), GateTimer);
                    }
                    
                }
            }

            
        }

        public string AllBacnetDevices()
        {//gets list of all associated devices. 
         //Get the collection of these devices which are bacnet
         //build Devices table with the appropriate data displays
        
            StringBuilder sb = new StringBuilder();
            htmlBuilder BACnetBuilder = new htmlBuilder("BacnetDev" + Instance.ajaxName);

            htmlTable ModbusConfHtml = BACnetBuilder.htmlTable(800);

            List<Scheduler.Classes.DeviceClass> BACnetDevs = Instance.bacPage.getAllBacnetDevices().ToList();
            //HERE

            foreach (Scheduler.Classes.DeviceClass device in BACnetDevs)
            {
                var EDO = device.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

                ModbusConfHtml.addDevMain(BACnetBuilder.MakeImage(16, 16, device.get_Image(Instance.host)).print() +
                    BACnetBuilder.MakeLink("/deviceutility?ref=" + device.get_Ref(Instance.host)
                    + "&edit=1", device.get_Name(Instance.host)).print(), EDO.GetNamed("SSIDKey").ToString());
              

            }
            sb.Append(ModbusConfHtml.print());


            
            return sb.ToString();
        }


        public string AllModbusDevices()
        {//gets list of all associated devices. 
         //Get the collection of these devices which are modbus gateways or devices
         //build Gateway / Devices table with the appropriate links and the appropriate Add Device buttons
         //returns the built html string
            StringBuilder sb = new StringBuilder();
            htmlBuilder ModbusBuilder = new htmlBuilder("AddModbusDevice" + Instance.ajaxName);

          List<int>  ModbusGates= Instance.modPage.getAllGateways().ToList();
            List<Scheduler.Classes.DeviceClass> ModbusDevs = new List<Scheduler.Classes.DeviceClass>();

            foreach (int GID in ModbusGates)
            {
                Scheduler.Classes.DeviceClass Dev = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(Convert.ToInt32(GID));
                 var EDO = Dev.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                StringBuilder updatedList = new StringBuilder();
                foreach (var subId in parts["LinkedDevices"].Split(','))
                {
                    try
                    {
                        Scheduler.Classes.DeviceClass MDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(Convert.ToInt32(subId));
                        if (MDevice != null)
                        {
                            ModbusDevs.Add(MDevice);
                            updatedList.Append(subId + ",");
                        }
                    }
                    catch
                    {

                    }


                }
                parts["LinkedDevices"] = updatedList.ToString();
                EDO.RemoveNamed("SSIDKey");
                EDO.AddNamed("SSIDKey", parts.ToString());
                Dev.set_PlugExtraData_Set(Instance.host, EDO);
            }

            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable(800);
            sb.Append("<br>");
            foreach (int GateRef in ModbusGates)
            {
                Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(GateRef);
                ModbusConfHtml.addDevHeader("Gateway");
                Gateway.get_Image(Instance.host);
                Gateway.get_Name(Instance.host);
                ModbusConfHtml.addDevMain(ModbusBuilder.MakeImage(16,16, Gateway.get_Image(Instance.host)).print()+
                    ModbusBuilder.MakeLink("/deviceutility?ref="+GateRef
                    +"&edit=1", Gateway.get_Name(Instance.host)).print(), ModbusBuilder.Qbutton("G_"+GateRef,"Add Device").print());
                sb.Append(ModbusConfHtml.print());
                ModbusConfHtml = ModbusBuilder.htmlTable(800);
                ModbusConfHtml.addSubHeader("Enabled","Device Name","Address","Type","Format");
               
                
                foreach (Scheduler.Classes.DeviceClass MDevice in ModbusDevs)
                {
                 
                    if (MDevice != null)
                    {
                        var EDO = MDevice.get_PlugExtraData_Get(Instance.host);
                        var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                        if (Convert.ToInt32(parts["GateID"]) == GateRef)
                        {
                            ModbusConfHtml.addSubMain(ModbusBuilder.MakeImage(16, 16, MDevice.get_Image(Instance.host)).print(),
                               ModbusBuilder.MakeLink("/deviceutility?ref=" + MDevice.get_Ref(Instance.host) + "&edit=1", MDevice.get_Name(Instance.host)).print(),
                               parts["SlaveId"],
                               Instance.modPage.GetReg(parts["RegisterType"]),
                               Instance.modPage.GetRet(parts["ReturnType"]));

                        }
                    }


                }
                sb.Append(ModbusConfHtml.print());
                sb.Append("<br>");
                ModbusConfHtml = ModbusBuilder.htmlTable(800);
            }

            
            Instance.modPage.UpdateGateList(ModbusGates.ToArray());
            return sb.ToString();
        }

        public string ScratchpadRules()
        {
            StringBuilder sb = new StringBuilder();
           List<Scheduler.Classes.DeviceClass> Rules= Instance.scrPage.getAllRules();
            if (Rules.Count > 0)
            {
                htmlBuilder ScratchBuilder = new htmlBuilder("Scratch" + Instance.ajaxName);
                sb.Append("<div><h2>ScratchPad Rules:<h2><hl>");
                htmlTable ScratchTable = ScratchBuilder.htmlTable();

                ScratchTable.addHead(new string[] { "Rule Name", "Value","Enable Rule", "Is Accumulator", "Reset Type", "Reset Interval", "Rule String", "Rule Formatting" }); //0,1,2,3,4,5

                foreach (Scheduler.Classes.DeviceClass Dev in Rules)
                {
                    int ID = Dev.get_Ref(Instance.host);
                    List<string> Row = new List<string>();
                    var EDO = Dev.get_PlugExtraData_Get(Instance.host);
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    Row.Add(ScratchBuilder.stringInput("Name_" + ID,Dev.get_Name(Instance.host)).print());
                    Row.Add(parts["DisplayedValue"]);
                    Row.Add(ScratchBuilder.checkBoxInput("IsEnabled_" + ID, bool.Parse(parts["IsEnabled"])).print());
                    Row.Add(ScratchBuilder.checkBoxInput("IsAccumulator_"+ID, bool.Parse(parts["IsAccumulator"]) ).print());

                    //Reset type is 0=periodically, 1=daily,2=weekly,3=monthly
                    
                    Row.Add(ScratchBuilder.selectorInput(ScratchpadDevice.ResetType, "ResetType_"+ID, "ResetType_" + ID, Convert.ToInt32(parts["ResetType"]) ).print());
                    //Based on what selector input, this next cell will be crowded with the different input possibilities where all but one have display none



                    StringBuilder ComplexCell = new StringBuilder();
         
                    ComplexCell.Append("<div id=0_" + ID + " style=display:none>Interval in minutes: " + ScratchBuilder.numberInput("ResetInterval_" + ID + "_0", Convert.ToInt32(parts["ResetInterval"])).print() + "</div>");
                    ComplexCell.Append("<div id=1_" + ID + " style=display:none>" + ScratchBuilder.timeInput("ResetTime_" + ID + "_1", parts["ResetTime"]).print() + "</div>");
                    ComplexCell.Append("<div id=2_" + ID + " style=display:none>" + ScratchBuilder.selectorInput(GeneralHelperFunctions.DaysOfWeek, "DayOfWeek_" + ID + "_2", "DayOfWeek_" + ID + "_2", Convert.ToInt32(parts["DayOfWeek"])).print() + "</div>");
                    ComplexCell.Append("<div id=3_" + ID + " style=display:none>Day of the month: " + ScratchBuilder.numberInput("DayOfMonth_" + ID + "_3", Convert.ToInt32(parts["DayOfMonth"])).print() + "</div>");
                    ComplexCell.Append(@"<script>
UpdateDisplay=function(id){
console.log('UPDATING DISPLAY '+id);
$('#0_'+id)[0].style.display='none';
$('#1_'+id)[0].style.display='none';
$('#2_'+id)[0].style.display='none';
$('#3_'+id)[0].style.display='none';
V = $('#ResetType_'+id)[0].value;
$('#'+V+'_'+id)[0].style.display='';
}
DoChange=function(){
console.log('DoChange');
console.log(this);
UpdateDisplay(this.id.split('_')[1]);
}
UpdateDisplay("+ID+ @");
$('#ResetType_" + ID + @"').change(DoChange); //OK HERE

</script>");
                    Row.Add(ComplexCell.ToString());

                    Row.Add(ScratchBuilder.stringInput("ScratchPadString_" + ID, parts["ScratchPadString"]).print());
                    Row.Add(ScratchBuilder.stringInput("DisplayString_" + ID, parts["DisplayString"]).print());

                    
                    ScratchTable.addArrayRow(Row.ToArray());
                }

                sb.Append(ScratchTable.print());

                sb.Append("</div>");
            }

            return sb.ToString();

        }

        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            Console.WriteLine("started the genpage");
            StringBuilder stb = new StringBuilder();
            SIID_Page page = this;
            htmlBuilder ModbusBuilder = new htmlBuilder("ModBus" + Instance.ajaxName);
            htmlBuilder BacnetBuilder = new htmlBuilder("BACnet" + Instance.ajaxName);
            InitializeModbusGatewayTimers();
            try
            {
                page.reset();

                page.AddHeader(Instance.host.GetPageHeader(pageName, Util.IFACE_NAME + " main plugin page", "", "", false, true));

                htmlBuilder GeneralPageStuff = new htmlBuilder(OurPageName + Instance.ajaxName);
                stb.Append("<h2>SIID Page "+ OurPageName + "</h2><br><br>");
                stb.Append("<hr>SIID Options<br><br>");
                stb.Append("<div>");
                stb.Append(GeneralPageStuff.Uploadbutton("Import", "Import SIID Devices from CSV File").print());
                stb.Append(GeneralPageStuff.Downloadbutton("Export", "Export SIID Devices to CSV File").print());
                stb.Append(GeneralPageStuff.button("Scratchpad", "Make new Scratchpad Rule").print());
                //stb.Append(GeneralPageStuff.button("Instance", "Switch Instances").print());
                stb.Append("</div>");


                stb.Append(ScratchpadRules());



                stb.Append("<hr>Select plugin API<br><br>");
               clsJQuery.jqRadioButton rb = new clsJQuery.jqRadioButton("pluginSelection", OurPageName, false);
               
                //rb.buttonset = False
                 rb.id = "SelectPlugin";
                rb.values.Add("Select an API", "1");
                rb.values.Add("ModBus", "2");
                rb.values.Add("BACnet", "3");
                rb.@checked = selectedPlugin.ToString();
                stb.Append(rb.Build());
                //

                //Modbus
                // ******************************************
                if (selectedPlugin == 2)
                {
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("modbus", "style=''"));
                }
                else
                {
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("modbus", "style='display:none;'"));
                }
               

                clsJQuery.jqTabs jqtabs = new clsJQuery.jqTabs("tab1id", this.PageName+ Instance.ajaxName);
                clsJQuery.Tab tab = new clsJQuery.Tab();
                tab.tabTitle = "Devices";
                tab.tabDIVID = "modBusDevTab";

                //have the ModBus Add device button
                //Also list all associated modbus devices
                htmlBuilder AddModbusDevBuilder = new htmlBuilder("AddModbusGate" + Instance.ajaxName);
                StringBuilder ModbusDevPage = new StringBuilder();
                ModbusDevPage.Append("<br><br><div style='display:block;'>");
                ModbusDevPage.Append(AddModbusDevBuilder.Gobutton("addModGateway", "Add Modbus IP Gateway").print()); 
                ModbusDevPage.Append("</div><br>");
                ModbusDevPage.Append(AllModbusDevices());
                tab.tabContent = ModbusDevPage.ToString();

                jqtabs.postOnTabClick = true;
                jqtabs.tabs.Add(tab);

                tab = new clsJQuery.Tab();
                tab.tabTitle = "Configuration";
                tab.tabDIVID = "modBusConfTab";


          


                    htmlTable ModbusConfHtml =  ModbusBuilder.htmlTable();
                ModbusConfHtml.add(" Configuration:");
                ModbusConfHtml.add(" Default Poll Interval in miliseconds<br>(can be overridden per gateway):", ModbusBuilder.numberInput("polltime", Instance.modbusDefaultPoll).print());
                selectorInput loglevel = ModbusBuilder.selectorInput(new string[] { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" },"logL","Log Level", Instance.modbusLogLevel);
                ModbusConfHtml.add(" Log Level:", loglevel.print());
                checkBoxInput logTF = ModbusBuilder.checkBoxInput("modlog", Instance.modbusLogToFile);
                ModbusConfHtml.add(" Log To File:", logTF.print());

                 string ConfigTable = "<div id=confTab style='display:block;'>" + ModbusConfHtml.print() + "</div>";


               // string TestStuff = new numberInput().print() + loglevel.print() + logTF.print();
               // tab.tabContent = TestStuff;

                  tab.tabContent = ConfigTable;

                jqtabs.tabs.Add(tab);

                stb.Append(jqtabs.Build());


                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());
                // End Modbus ***********************************

                //Start BACnet ***********************************
                if (selectedPlugin == 3)
                {
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("bacnet", "style=''"));
                }
                else
                {
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("bacnet", "style='display:none;'"));
                }


                 jqtabs = new clsJQuery.jqTabs("tab2id", this.PageName + Instance.ajaxName);
                 tab = new clsJQuery.Tab();
                tab.tabTitle = "Homeseer BACnet devices";
                tab.tabDIVID = "bacNetDevTab";
                StringBuilder DeviceTab = new StringBuilder();
                DeviceTab.Append("<h3>BACnet Devices in Homeseer</h3>");
                //have the ModBus Add device button
                //Also list all associated modbus devices

                DeviceTab.Append(AllBacnetDevices());

                tab.tabContent = DeviceTab.ToString();

                jqtabs.postOnTabClick = false;
                jqtabs.tabs.Add(tab);

                tab = new clsJQuery.Tab();
                tab.tabTitle = "Discover BACnet devices";
                tab.tabDIVID = "BacDiscoverTab";
                StringBuilder DiscoverTab = new StringBuilder();
                DiscoverTab.Append("<h3>Discover and add BACnet devices to Homeseer</h3>");



                DiscoverTab.Append("<script>$('#BacDiscoverTab').css('height', '500px');</script>");

                DiscoverTab.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("bacnetGlobalNetworkTree", "style='height:400px; width: 400px; float:left; margin-bottom: 10px; '")); //, "style='display:none;'")


                //DiscoverTab.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("bacnetGlobalNetworkTree", "style='height:370px; width: 100%;'")); //, "style='display:none;'")
                //DiscoverTab.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());



                DiscoverTab.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());

                DiscoverTab.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("bacnetProperties", "style='height:380px; width: 500px; float:left; margin-left: 10px;'")); //, "style='display:none;'")

                DiscoverTab.Append("<table id='bacnetPropertiesTable'></table>");

                DiscoverTab.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());


                DiscoverTab.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("bacnetButtonFooter", "style='height:70px; width: 100%; float:left; '"));

                htmlBuilder DiscoverBACNET = new htmlBuilder("discoverBACnetDevices" + Instance.ajaxName);
                DiscoverTab.Append(DiscoverBACNET.Gobutton("discoverBACnetDevices", "Discover BACnet devices on network").print());//Gobutton redirects to a new page, not a postback thing


                DiscoverTab.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());


                //var ftPath = System.Web.Hosting.HostingEnvironment.MapPath("~/jquery.fancytree-all.min.js");

                //var ftPath = HttpContext.Current.Server.MapPath("~/jquery.fancytree-all.min.js");
                //DiscoverTab.Append(String.Format("<script src='{0}' />", ftPath));
                //var 


                string basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));


                DiscoverTab.Append("<link rel='stylesheet' type='text/css' href='https://cdn.datatables.net/v/ju/dt-1.10.13/datatables.min.css' />");
                DiscoverTab.Append("<script type='text/javascript' src='https://cdn.datatables.net/v/ju/dt-1.10.13/datatables.min.js'></script>");


                String ftCss = File.ReadAllText(Path.Combine(basePath, "js", "ui.fancytree.css"));
                DiscoverTab.Append("<style>" + ftCss + "</style>");


                String ftJs = File.ReadAllText(Path.Combine(basePath, "js", "jquery.fancytree-all.min.js"));
                DiscoverTab.Append("<script>" + ftJs + "</script>");

                DiscoverTab.Append(String.Format("<script>var dataServiceUrl = '{0}';</script>", Instance.bacnetDataService.PageName)); 
                //BacNet discovery needs to know this - this is where tree gets its data from

                String bacnetDiscoveryJs = File.ReadAllText(Path.Combine(basePath, "js", "bacnetDiscovery.js"));
                DiscoverTab.Append("<script>" + bacnetDiscoveryJs + "</script>");




                htmlTable BACnetTable = BacnetBuilder.htmlTable();
                if (Instance.bacPage.DiscoveredBACnetDevices.Count > 0)
                {
                    BACnetTable.add(" BACnet Devices on network:");
                    htmlBuilder addBACnet = new htmlBuilder("addBACnetDevice" + Instance.ajaxName);
                   int count = 0;
                    foreach (string entry in Instance.bacPage.DiscoveredBACnetDevices)
                    {
                      
                        BACnetTable.addDevMain(entry, addBACnet.Qbutton("B_"+count, "Add this BACnet device to Homeseer").print());
                        count++;
                    }

                }
                DiscoverTab.Append(BACnetTable.print());
                //Here is placeholder
                //Button to run discover program
                //Do BACnet stuff, then populates the list here with the results I guess?

                //Discover button

                //Placeholder of discovered devices
                //button to add Homeseer device of selected discovered devices
                // bacPage.DiscoveredBACnetDevices is the list of all discovered devices (in some format, maybe jquery)
                tab.tabContent = DiscoverTab.ToString();

               jqtabs.postOnTabClick = false;
                jqtabs.tabs.Add(tab);

                StringBuilder ConfigTab = new StringBuilder();

                tab = new clsJQuery.Tab();
                tab.tabTitle = "Configuration";
                tab.tabDIVID = "BACnetConfTab";

                htmlTable BACnetConfigTable = BacnetBuilder.htmlTable();
                BACnetConfigTable.add(" Configuration:");
                BACnetConfigTable.add(" Need to add specific BACnet options here and also save/load these options from config file");
                ConfigTab.Append("<div id=confTab style='display:block;'>" + BACnetConfigTable.print() + "</div>");


                // string TestStuff = new numberInput().print() + loglevel.print() + logTF.print();
                // tab.tabContent = TestStuff;

                tab.tabContent = ConfigTab.ToString();
                jqtabs.postOnTabClick = false;
                jqtabs.tabs.Add(tab);

                stb.Append(jqtabs.Build());


                stb.Append(PageBuilderAndMenu.clsPageBuilder.DivEnd());
            }


            catch (Exception ex)
            {
                stb.Append("Test page error: " + ex.Message);
            }
            stb.Append("<br>");

            stb.Append("<script>");

            String Javascript = @"$('input:radio[name=""pluginSelection""]').change(
    function(){
                if (this.checked && this.value == 2) {
                        modbus.style.display = '';
                    }
                    else{
                        modbus.style.display = 'none';
                    }

 if (this.checked && this.value == 3) {
                        bacnet.style.display = '';
                    }
                    else{
                        bacnet.style.display = 'none';
                    }
                    });";


            stb.Append(Javascript);

            stb.Append("</script>");

            page.AddBody(stb.ToString());

            return page.BuildPage();

        }
    }
}
