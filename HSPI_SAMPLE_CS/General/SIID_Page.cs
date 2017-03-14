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

namespace HSPI_SIID_ModBusDemo
{
    public class SIID_Page : PageBuilderAndMenu.clsPageBuilder

    {
        public ModbusDevicePage ModPage { get; set; }

        public static Dictionary<int, System.Threading.Timer> PluginTimerDictionary = new Dictionary<int, Timer>(); //Indexed by device ID, value is timers, intended for modbus gateway polls. idea is one per gateway ID
       //Needs to instance this when the plugin initializes, needs to update when a new gateway is added or when the polling interval changes


        public SIID_Page(string pagename) : base(pagename)
        {
            ModPage = new ModbusDevicePage("SIID UTILITY PAGE");
            ItializeModbusGatewayTimers();
        }

       

        public int selectedPlugin { get; set; }


        public void LoadINISettings()
        {

            selectedPlugin= Convert.ToInt32(Util.hs.GetINISetting("CONFIG", "Selected_Plugin", "1", Util.IFACE_NAME + Util.Instance + ".INI"));
            MosbusAjaxReceivers.loadModbusConfig();


            SaveAllINISettings();
        }

        public void SaveAllINISettings()
        {
            //general config
            Util.hs.SaveINISetting("CONFIG", "Selected_Plugin", selectedPlugin.ToString(), Util.IFACE_NAME + Util.Instance + ".INI");

            MosbusAjaxReceivers.saveModbusConfig();
            //modbus specific config



        }
        

        public void SaveSpecificINISetting(string section, string key, string value)
        {
            Util.hs.SaveINISetting(section, key , value , Util.IFACE_NAME+Util.Instance+".INI");

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
                            var dv = Util.hs.NewDeviceRef("ImportingDevice");
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
                                    CodeLookup[HeaderArray[FRind]] = fieldRowCell;
                                    FRind++;
                                }
                                try
                                {
                                    Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(OldToNew[int.Parse(CodeLookup["id"])]);
                                    newDevice.set_Name(Util.hs, FetchAttribute(CodeLookup, "Name"));
                                    newDevice.set_Location2(Util.hs, FetchAttribute(CodeLookup, "Floor"));
                                    newDevice.set_Location(Util.hs, FetchAttribute(CodeLookup, "Room"));
                                    //newDevice.set_Interface(Util.hs, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
                                    newDevice.set_Interface(Util.hs, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function
                                    newDevice.set_Address(Util.hs, FetchAttribute(CodeLookup, "address"));
                                    newDevice.set_Code(Util.hs, FetchAttribute(CodeLookup, "code"));
                                    //newDevice.set_InterfaceInstance()''  SET INTERFACE INSTANCE
                                    newDevice.set_Status_Support(Util.hs, bool.Parse(FetchAttribute(CodeLookup, "statusOnly")));
                                    newDevice.set_Can_Dim(Util.hs, bool.Parse(FetchAttribute(CodeLookup, "CanDim")));
                                    newDevice.set_UserAccess(Util.hs, FetchAttribute(CodeLookup, "useraccess"));
                                    newDevice.set_UserNote(Util.hs, FetchAttribute(CodeLookup, "notes"));
                                    newDevice.set_Device_Type_String(Util.hs, FetchAttribute(CodeLookup, "deviceTypeString"));

                                    
                                    switch(FetchAttribute(CodeLookup, "RelationshipStatus"))
                                    {
                                        case ("Not_Set"):
                                            {
                                                newDevice.set_Relationship(Util.hs,Enums.eRelationship.Not_Set);
                                                break;
                                            }
                                        case ("Indeterminate"):
                                            {
                                                newDevice.set_Relationship(Util.hs, Enums.eRelationship.Indeterminate);
                                                break;
                                            }
                                        case ("Child"):
                                            {
                                                newDevice.set_Relationship(Util.hs, Enums.eRelationship.Child);
                                                break;
                                            }

                                        case ("Parent_Root"):
                                            {
                                                newDevice.set_Relationship(Util.hs, Enums.eRelationship.Parent_Root);
                                                break;
                                            }
                                        case ("Standalone"):
                                            {
                                                newDevice.set_Relationship(Util.hs, Enums.eRelationship.Standalone);
                                                break;
                                            }


                                    }


                                    if (bool.Parse(FetchAttribute(CodeLookup, "donotlog")))
                                    {
                                        newDevice.MISC_Set(Util.hs, Enums.dvMISC.NO_LOG);

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
                                            newDevice.set_DeviceType_Set(Util.hs, DevINFO);
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
                                            newDevice.AssociatedDevice_Add(Util.hs, OldToNew[T]);
                                        }
                                      
                                    }
                                    HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();
                                    switch (CodeLookup["type"])
                                    {
                                        case ("BacNet???"):
                                            {

                                                break;
                                            }
                                        case ("Modbus Gateway"):
                                            {
                                                ModPage.MakeGatewayGraphicsAndStatus(OldToNew[int.Parse(CodeLookup["id"])]);
                                                newDevice.MISC_Set(Util.hs, Enums.dvMISC.SHOW_VALUES);



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
                                                ModPage.MakeSubDeviceGraphicsAndStatus(OldToNew[int.Parse(CodeLookup["id"])]);
                                                newDevice.MISC_Set(Util.hs, Enums.dvMISC.SHOW_VALUES);
                                                var parts = HttpUtility.ParseQueryString(string.Empty);
                                                parts["Type"] = FetchAttribute(CodeLookup, "type");

                                                parts["GateID"] = OldToNew[int.Parse(FetchAttribute(CodeLookup, "GateID"))].ToString(); //Replace


                                                parts["Gateway"] = FetchAttribute(CodeLookup, "Gateway");
                                                parts["RegisterType"] = FetchAttribute(CodeLookup, "RegisterType");//MosbusAjaxReceivers.modbusDefaultPoll.ToString(); //0 is discrete input, 1 is coil, 2 is InputRegister, 3 is Holding Register
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
                                    newDevice.set_PlugExtraData_Set(Util.hs, EDO);




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
            string FileName = Util.IFACE_NAME + "_" + Util.Instance + "_" + "Export_Devices.CSV";
            StringBuilder FileContent = new StringBuilder();
  
            Scheduler.Classes.clsDeviceEnumeration DevNum = (Scheduler.Classes.clsDeviceEnumeration)Util.hs.GetDeviceEnumerator();
            var Dev = DevNum.GetNext();
            List<int> ModGateways = new List<int>();
            List<int> ModDevices = new List<int>();
            List<int> BackNetDevices = new List<int>();
            while (Dev != null)
            {
                try
                {
                    
                    //StringBuilder Row = new StringBuilder();
                 


                    var EDO = Dev.get_PlugExtraData_Get(Util.hs);
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString()); //So it is a SIID device
                   if (Dev.get_Interface(Util.hs).ToString() == Util.IFACE_NAME.ToString()) //Then it's one of ours (Need maybe an extra chack for Instance
                        {
                        switch (parts["Type"])
                        {
                            case ("BacNet???"):
                                {
                                    BackNetDevices.Add(Dev.get_Ref(Util.hs));
                                    break;
                                }
                            case ("Modbus Gateway"):
                                {
                                    ModGateways.Add(Dev.get_Ref(Util.hs));
                                    break;
                                }
                            case ("Modbus Device"):
                                {
                                    ModDevices.Add(Dev.get_Ref(Util.hs));
                                    break;
                                }

                             


                                  }
                                  


                        }


                    
                    //   if (parts["Type"] == "Modbus Device")
                    //     {
                    //        ModbusDevs.Add(Dev.get_Ref(Util.hs));
                    //    }

                }
                catch(Exception e)
                {
                    int a = 1;
                }
                Dev = DevNum.GetNext();


            }

            if (BackNetDevices.Count > 0)
            {
                FileContent.Append("Backnet Devices\r\n");
                FileContent.Append(new HSPI_SIID.SIIDDevice(BackNetDevices[0]).ReturnCSVHead());
                foreach (int ID in BackNetDevices)
                {
                    FileContent.Append(new HSPI_SIID.SIIDDevice(ID).ReturnCSVRow());
                }
            }
            if (ModGateways.Count > 0)
            {
                FileContent.Append("Modbus Gateways\r\n");
                FileContent.Append(new HSPI_SIID.SIIDDevice(ModGateways[0]).ReturnCSVHead());
                foreach (int ID in ModGateways)
                {
                    FileContent.Append(new HSPI_SIID.SIIDDevice(ID).ReturnCSVRow());
                }
            }
            if (ModDevices.Count > 0)
            {
                FileContent.Append("Modbus Devices\r\n");
                FileContent.Append(new HSPI_SIID.SIIDDevice(ModDevices[0]).ReturnCSVHead());
                foreach (int ID in ModDevices)
                {
                    FileContent.Append(new HSPI_SIID.SIIDDevice(ID).ReturnCSVRow());
                }
            }





     
         
       
            return FileName+"_)(*&^%$#@!"+ FileContent.ToString();
        }

        public string postbackSSIDConfigPage(string page, string data, string user, int userRights)
        {
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);
            string ID = parts["id"].Split('_')[0];
            switch (ID)
            {
                case "Import":
                    {
                        ImportDevices(parts["value"]);
                        return ""; 
                        break;
                    }
                case "Export":
                    {
                        return ReturnDevicesInExportForm();   
                        break;
                    }
                case "Scratchpad":
                    {
                        

                        break;
                    }
                    case "Instance":
                    {

                      Util.Instance = new Random().Next().ToString();
                        break;
                    }
                    
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

       

        public  string postBackProc(string page, string data, string user, int userRights)
        {
            //So I guess AJAX calls come back here?
            //need to register
            

            return base.postBackProc(page, data, user, userRights);
        }

        public void ItializeModbusGatewayTimers()
        {
            List<int> ModbusGates = ModbusDevicePage.getAllGateways().ToList();
            List<Scheduler.Classes.DeviceClass> ModbusDevs = new List<Scheduler.Classes.DeviceClass>();

            foreach (int GID in ModbusGates)
            {
                Scheduler.Classes.DeviceClass Dev = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(Convert.ToInt32(GID));
                var EDO = Dev.get_PlugExtraData_Get(Util.hs);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

                if (!PluginTimerDictionary.ContainsKey(Convert.ToInt32(GID)))
                {
                    System.Threading.Timer GateTimer = new System.Threading.Timer(ModPage.PollActiveFromGate, GID, 10000, Convert.ToInt32(parts["Poll"]));

                    Console.WriteLine("Starting Polling timer for gateway: " + GID);
                    PluginTimerDictionary.Add(Convert.ToInt32(GID), GateTimer);
                }
            }

            
        }


        public string AllModbusDevices()
        {//gets list of all associated devices. 
         //Get the collection of these devices which are modbus gateways or devices
         //build Gateway / Devices table with the appropriate links and the appropriate Add Device buttons
         //returns the built html string
            StringBuilder sb = new StringBuilder();
            htmlBuilder ModbusBuilder = new htmlBuilder("AddModbusDevice");

          List<int>  ModbusGates=ModbusDevicePage.getAllGateways().ToList();
            List<Scheduler.Classes.DeviceClass> ModbusDevs = new List<Scheduler.Classes.DeviceClass>();

            foreach (int GID in ModbusGates)
            {
                Scheduler.Classes.DeviceClass Dev = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(Convert.ToInt32(GID));
                 var EDO = Dev.get_PlugExtraData_Get(Util.hs);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                StringBuilder updatedList = new StringBuilder();
                foreach (var subId in parts["LinkedDevices"].Split(','))
                {
                    try
                    {
                        Scheduler.Classes.DeviceClass MDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(Convert.ToInt32(subId));
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
                Dev.set_PlugExtraData_Set(Util.hs, EDO);
            }

            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable(800);
            sb.Append("<br>");
            foreach (int GateRef in ModbusGates)
            {
                Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GateRef);
                ModbusConfHtml.addDevHeader("Gateway");
                Gateway.get_Image(Util.hs);
                Gateway.get_Name(Util.hs);
                ModbusConfHtml.addDevMain(ModbusBuilder.MakeImage(16,16, Gateway.get_Image(Util.hs)).print()+
                    ModbusBuilder.MakeLink("/deviceutility?ref="+GateRef
                    +"&edit=1", Gateway.get_Name(Util.hs)).print(), ModbusBuilder.Qbutton("G_"+GateRef,"Add Device").print());
                sb.Append(ModbusConfHtml.print());
                ModbusConfHtml = ModbusBuilder.htmlTable(800);
                ModbusConfHtml.addSubHeader("Enabled","Device Name","Address","Type","Format");
               
                
                foreach (Scheduler.Classes.DeviceClass MDevice in ModbusDevs)
                {
                 
                    if (MDevice != null)
                    {
                        var EDO = MDevice.get_PlugExtraData_Get(Util.hs);
                        var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                        if (Convert.ToInt32(parts["GateID"]) == GateRef)
                        {
                            ModbusConfHtml.addSubMain(ModbusBuilder.MakeImage(16, 16, MDevice.get_Image(Util.hs)).print(),
                               ModbusBuilder.MakeLink("/deviceutility?ref=" + MDevice.get_Ref(Util.hs) + "&edit=1", MDevice.get_Name(Util.hs)).print(),
                               parts["SlaveId"],
                               ModPage.GetReg(parts["RegisterType"]),
                               ModPage.GetRet(parts["ReturnType"]));

                        }
                    }


                }
                sb.Append(ModbusConfHtml.print());
                sb.Append("<br>");
                ModbusConfHtml = ModbusBuilder.htmlTable(800);
            }

            
            ModbusDevicePage.UpdateGateList(ModbusGates.ToArray());
            return sb.ToString();
        }


        public string GetPagePlugin(string pageName, string user, int userRights, string queryString)
        {
            StringBuilder stb = new StringBuilder();
            SIID_Page page = this;
            htmlBuilder ModbusBuilder = new htmlBuilder("ModBus");
            ItializeModbusGatewayTimers();
            try
            {
                page.reset();

                page.AddHeader(Util.hs.GetPageHeader(pageName, Util.IFACE_NAME + " main plugin page", "", "", false, true));

                htmlBuilder GeneralPageStuff = new htmlBuilder("SIIDConfPage");
                stb.Append("<hr>SIID Options<br><br>");
                stb.Append("<div>");
                stb.Append(GeneralPageStuff.Uploadbutton("Import", "Import SIID Devices from CSV File").print());
                stb.Append(GeneralPageStuff.Downloadbutton("Export", "Export SIID Devices to CSV File").print());
                stb.Append(GeneralPageStuff.button("Scratchpad", "Make new Scratchpad Rule").print());
                stb.Append(GeneralPageStuff.button("Instance", "Switch Instances").print());
                stb.Append("</div>");





                stb.Append("<hr>Select plugin API<br><br>");
               clsJQuery.jqRadioButton rb = new clsJQuery.jqRadioButton("pluginSelection", "SIIDConfPage", false);
               
                //rb.buttonset = False
                 rb.id = "SelectPlugin";
                rb.values.Add("Select an API", "1");
                rb.values.Add("ModBus", "2");
                rb.values.Add("BACnet", "3");
                rb.@checked = selectedPlugin.ToString();
                stb.Append(rb.Build());
                //

                //Modbus
                if (selectedPlugin == 2)
                {
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("modbus", "style=''"));
                }
                else
                {
                    stb.Append(PageBuilderAndMenu.clsPageBuilder.DivStart("modbus", "style='display:none;'"));
                }
               

                clsJQuery.jqTabs jqtabs = new clsJQuery.jqTabs("tab1id", this.PageName);
                clsJQuery.Tab tab = new clsJQuery.Tab();
                tab.tabTitle = "Devices";
                tab.tabDIVID = "modBusDevTab";

                //have the ModBus Add device button
                //Also list all associated modbus devices
                htmlBuilder AddModbusDevBuilder = new htmlBuilder("AddModbusGate");
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


                new HSPI_SIID_ModBusDemo.Modbus.ModbusClasses.ModbusGeneralConfigSettings();


                    htmlTable ModbusConfHtml =  ModbusBuilder.htmlTable();
                ModbusConfHtml.add(" Configuration:");
                ModbusConfHtml.add(" Default Poll Interval in miliseconds<br>(can be overridden per gateway):", ModbusBuilder.numberInput("polltime", MosbusAjaxReceivers.modbusDefaultPoll).print());
                selectorInput loglevel = ModbusBuilder.selectorInput(new string[] { "Trace", "Debug", "Info", "Warn", "Error", "Fatal" },"logL","Log Level", MosbusAjaxReceivers.modbusLogLevel);
                ModbusConfHtml.add(" Log Level:", loglevel.print());
                checkBoxInput logTF = ModbusBuilder.checkBoxInput("modlog", MosbusAjaxReceivers.modbusLogToFile);
                ModbusConfHtml.add(" Log To File:", logTF.print());

                 string ConfigTable = "<div id=confTab style='display:block;'>" + ModbusConfHtml.print() + "</div>";


               // string TestStuff = new numberInput().print() + loglevel.print() + logTF.print();
               // tab.tabContent = TestStuff;

                  tab.tabContent = ConfigTable;

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
                    });";


            stb.Append(Javascript);

            stb.Append("</script>");

            page.AddBody(stb.ToString());

            return page.BuildPage();

        }
    }
}
