using HomeSeerAPI;
using HSPI_SIID.General;
using HSPI_SIID;
using Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace HSPI_SIID.ScratchPad
{
  public  class ScratchpadDevicePage : PageBuilderAndMenu.clsPageBuilder
    {
        public InstanceHolder Instance { get; set; }
        public htmlBuilder ScratchpadBuilder { get; set; }
        public ScratchpadDevicePage(string pagename, InstanceHolder instance) : base(pagename)
        {

            Instance = instance;
            ScratchpadBuilder = new htmlBuilder("Scratchpad" + Instance.ajaxName);
          
        }



       
        private double CalculateString(string FinalString)
        {
            double OutValue = 0;
            try
            {
               

                    OutValue = GeneralHelperFunctions.Evaluate(FinalString.ToString());



            }
            catch (Exception e)
            {
                OutValue = 0;
            }
            return OutValue;
        }
        public void doLiveRule(SiidDevice Rule)
        {
            var parts = HttpUtility.ParseQueryString(Rule.Extra.GetNamed("SSIDKey").ToString());

            string RawNumberString = GeneralHelperFunctions.GetValues(Instance, parts["LiveUpdateID"]);
            double CalculatedString = CalculateString(RawNumberString);
            try
            {
                double Rate = Double.Parse(parts["RateValue"]);
                CalculatedString = CalculatedString * Rate;
            }
            catch
            {
            }
            Rule.UpdateExtraData("LiveValue", "" + CalculatedString.ToString());

   

        }
        public void UpdateDisplay(SiidDevice Rule)
        {
            try
            {
                var parts=HttpUtility.ParseQueryString(Rule.Extra.GetNamed("SSIDKey").ToString());
               
                //Do the calculator string parse to get the new value
                string RawNumberString = GeneralHelperFunctions.GetValues(Instance,parts["ScratchPadString"]);
                double CalculatedString = CalculateString(RawNumberString);
                Rule.UpdateExtraData("NewValue", "" + CalculatedString.ToString());
             
                if (bool.Parse(parts["IsAccumulator"]))
                {
                    CalculatedString = CalculatedString - Double.Parse(parts["OldValue"]);

                }


                try
                {
                    double Rate = Double.Parse(parts["RateValue"]);
                    CalculatedString = CalculatedString * Rate;
                }
                catch
                {
                }
               
              
                Rule.UpdateExtraData("RawValue", ""+CalculatedString);
                Rule.UpdateExtraData("ProcessedValue", ""+CalculatedString);
                Rule.UpdateExtraData("CurrentTime", "" + DateTime.Now.ToString());

                string ValueString = String.Format(parts["DisplayString"], CalculatedString);

                Instance.host.SetDeviceString(Rule.Ref, ValueString, true);
                Instance.host.SetDeviceValueByRef(Rule.Ref, CalculatedString, true);
                Rule.UpdateExtraData("DisplayedValue", "" + ValueString);


                // EDO.RemoveNamed("SSIDKey");
                //  EDO.AddNamed("SSIDKey", parts.ToString());

                 parts = HttpUtility.ParseQueryString(Rule.Extra.GetNamed("SSIDKey").ToString());

                string userNote = Rule.Device.get_UserNote(Instance.host);
                userNote = userNote.Split("PLUGIN EXTRA DATA:".ToCharArray())[0];
                userNote += "PLUGIN EXTRA DATA:" + parts.ToString();
                Rule.Device.set_UserNote(Instance.host, userNote);

                Rule.Device.set_PlugExtraData_Set(Instance.host, Rule.Extra);
           
            }
            catch (Exception e)
            {

            }

            //Check if rule is an accumulator, if so do NewValue - OldValue

            //String Format the value, put results in display
            doLiveRule(Rule);
        }
        public void Reset(SiidDevice Rule)
        {
            var EDO = Rule.Extra;
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            parts["OldValue"] = parts["NewValue"];
            parts["DateOfLastReset"] = DateTime.Now.ToString();
           
            EDO.RemoveNamed("SSIDKey");
            EDO.AddNamed("SSIDKey", parts.ToString());
            Rule.Device.set_PlugExtraData_Set(Instance.host, EDO);
            string userNote=Rule.Device.get_UserNote(Instance.host);
            userNote = userNote.Split("PLUGIN EXTRA DATA:".ToCharArray())[0];
            userNote += "PLUGIN EXTRA DATA:"+parts.ToString();
            Rule.Device.set_UserNote(Instance.host, userNote);
            
           Rule.Extra = EDO;
        }
        public void CheckForReset(SiidDevice Rule)
        {
            var EDO = Rule.Extra;
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            if (bool.Parse(parts["IsAccumulator"]))
            {           //Check if accumulator
                DateTime OldDate = DateTime.Parse(parts["DateOfLastReset"]);
                switch (parts["ResetType"]) {
                    case "0": //interval in minutes
                        {
                            if ((DateTime.Now - OldDate).Minutes > Convert.ToInt64(parts["ResetInterval"]))
                            {
                                Reset(Rule);
                            }
                            break;
                        }
                    case "1"://time of day
                        {
                            DateTime Saved = Convert.ToDateTime(parts["ResetTime"]);
                            int hour = Saved.Hour;
                            int min = Saved.Minute;
                          
                           if (((DateTime.Now - OldDate).Days > 0) && (DateTime.Now.Hour>=hour)&&(DateTime.Now.Minute >= min)) 
                            {
                                Reset(Rule);
                            }
                            break;
                        }
                    case "2"://day of week
                        {
                            if (((DateTime.Now - OldDate).Days > 0) && ((int)DateTime.Now.DayOfWeek == Convert.ToInt32(parts["DayOfWeek"])))
                            {
                                Reset(Rule);
                            }
                            break;
                        }
                    case "3": //day of month
                        {
                            if (((DateTime.Now - OldDate).Days > 0) && ((int)DateTime.Now.Day == Convert.ToInt32(parts["DayOfMonth"])))
                            {
                                Reset(Rule);
                            }
                            break;
                        }

                }

            }
 
            //Check if reset conditions were met between now and last update (Make sure we dont keep resetting until we leave the reset behind for instance only reset once on the day where reset is defined)
            //set the date of last reset to DateTime.Now

        }

        public void DoScratchRules(object stuff)
        {
       
            var Rules = getAllRules();
            foreach (SiidDevice Rule in Rules)
            {
                try
                {
                    var EDO = Rule.Extra;
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    if (bool.Parse(parts["IsEnabled"]))
                    {
                        UpdateDisplay(Rule);
                        CheckForReset(Rule);
                    }
                }
                catch
                {
                }

            }
            //Make sure all modbus gateway's have timer. Had problem where gateways would be added but somehow not appear in the timer.
           Instance.siidPage.InitializeModbusGatewayTimers();
           Instance.siidPage.InitializeBacnetDeviceTimers();
            
        }


        public string makeNewRules()
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);


            parts["Type"] = "Scratchpad";
            parts["IsEnabled"] = "false";
            parts["IsAccumulator"] = "false";
        //    parts["UpdateInterval"] = "30000"; //Global every 30 seconds for all rules
            parts["ResetType"] = "0";
            parts["ResetInterval"] = "0";
        
               parts["ResetTime"] = "12:00:00 AM";
            parts["DayOfWeek"] = "0";
            parts["DayOfMonth"] = "0"; 
  
            parts["ScratchPadString"] = "";
            parts["DisplayString"] = "{0}";
            parts["OldValue"] = "0";
            parts["NewValue"] = "0";
            parts["DisplayedValue"] = "0";
            parts["DateOfLastReset"] = DateTime.Now.ToString() ;
            parts["LiveUpdateID"] = "";
            parts["RateValue"] = "";
            parts["LiveValue"] ="0";




            return parts.ToString();
        }
        public string MakeNewRule() {


            var dv = Instance.host.NewDeviceRef("R");

    
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);
            newDevice.set_Name(Instance.host, "Type Meter Calc " + dv);
            //newDevice.set_Location2(Instance.host, "ScratchpadRule");
            newDevice.set_Location(Instance.host, "Utilities");
            newDevice.set_Location2(Instance.host, "Calcs");
            //newDevice.set_Interface(Instance.host, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function
                                                                     //newDevice.set_InterfaceInstance()''  SET INTERFACE INSTANCE
            newDevice.set_Relationship(Instance.host, Enums.eRelationship.Not_Set);

            newDevice.MISC_Set(Instance.host, Enums.dvMISC.NO_LOG);
            newDevice.MISC_Set(Instance.host, Enums.dvMISC.SHOW_VALUES);
            // newDevice.MISC_Set(Instance.host, Enums.dvMISC.HIDDEN);
            HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();

            // EDO = newDevice.get_PlugExtraData_Get(Instance.host);

            string ruleString = makeNewRules();


            string userNote = newDevice.get_UserNote(Instance.host);
            userNote = userNote.Split("PLUGIN EXTRA DATA:".ToCharArray())[0];
            userNote += "PLUGIN EXTRA DATA:"+ruleString.ToString();
            newDevice.set_UserNote(Instance.host, userNote);

            EDO.AddNamed("SSIDKey", ruleString);
            newDevice.set_PlugExtraData_Set(Instance.host, EDO);
        
            // newDevice.set_Device_Type_String(Instance.host, makeNewModbusGateway());
            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


            newDevice.set_DeviceType_Set(Instance.host, DevINFO);
            Instance.Devices.Add(new SiidDevice(Instance,newDevice));

            MakeStewardVSP(dv);

            return "refresh";

        }


       public void MakeStewardVSP(int deviceID) {
        

            var Control = new VSVGPairs.VSPair(ePairStatusControl.Control);
            Control.PairType = VSVGPairs.VSVGPairType.Range;
            Control.RangeStart = -100;
            Control.RangeEnd = 1000;
            Control.Render = Enums.CAPIControlType.TextBox_Number;
          var IS =  Instance.host.DeviceVSP_AddPair(deviceID, Control);
          

        }


        public List<SiidDevice> getAllRules()
        {
            List<SiidDevice> listOfDevices = new List<SiidDevice>();
            SiidDevice.Update(Instance);

            foreach (SiidDevice Dev in Instance.Devices)
            {
                try
                {
                    var EDO = Dev.Extra;
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    string s = parts["Type"];
                    if (parts["Type"] == "Scratchpad")
                    {
                        if (Dev.Device.get_Location2(Instance.host) != "Rates")
                        {
                            listOfDevices.Add(Dev);
                        }
                
                        


                    }


                }
                catch
                {

                }
               


            }
            return listOfDevices;

        }

        public string BuildScratchDeviceTab(int id)
        {
            StringBuilder sb = new StringBuilder();
            SiidDevice Dev = SiidDevice.GetFromListByID(Instance.Devices, id);


                htmlBuilder ScratchBuilder = new htmlBuilder("Scratch" + Instance.ajaxName);
                sb.Append("<div><h2>ScratchPad Rules:<h2><hl>");
                htmlTable ScratchTable = ScratchBuilder.htmlTable();

                ScratchTable.addHead(new string[] { "Rule Name", "Value", "Enable Rule", "Is Accumulator", "Reset Type", "Reset Interval", "Rule String", "Rule Formatting" }); //0,1,2,3,4,5

              
                    int ID = Dev.Ref;
                    List<string> Row = new List<string>();
                    var EDO = Dev.Extra;
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    Row.Add(ScratchBuilder.stringInput("Name_" + ID, Dev.Device.get_Name(Instance.host)).print());
                    Row.Add(parts["DisplayedValue"]);
                    Row.Add(ScratchBuilder.checkBoxInput("IsEnabled_" + ID, bool.Parse(parts["IsEnabled"])).print());
                    Row.Add(ScratchBuilder.checkBoxInput("IsAccumulator_" + ID, bool.Parse(parts["IsAccumulator"])).print());

                    //Reset type is 0=periodically, 1=daily,2=weekly,3=monthly

                    Row.Add(ScratchBuilder.selectorInput(ScratchpadDevice.ResetType, "ResetType_" + ID, "ResetType_" + ID, Convert.ToInt32(parts["ResetType"])).print());
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
UpdateDisplay(" + ID + @");
$('#ResetType_" + ID + @"').change(DoChange); //OK HERE

</script>");
                    Row.Add(ComplexCell.ToString());

                    Row.Add(ScratchBuilder.stringInput("ScratchPadString_" + ID, parts["ScratchPadString"]).print());
                    Row.Add(ScratchBuilder.stringInput("DisplayString_" + ID, parts["DisplayString"]).print());


                    ScratchTable.addArrayRow(Row.ToArray());
                

                sb.Append(ScratchTable.print());

                sb.Append("</div>");
            

            return sb.ToString();


        }


        /*   public void addSSIDExtraData(Scheduler.Classes.DeviceClass Device, string Key, string value)
           {


               var EDO = Device.get_PlugExtraData_Get(Instance.host);
               var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
               parts[Key] = value;
               EDO.RemoveNamed("SSIDKey");
               EDO.AddNamed("SSIDKey", parts.ToString());
               Device.set_PlugExtraData_Set(Instance.host, EDO);

           }*/

            public void setValue(CAPI.CAPIControl ActionIn)
        {
            var devID = ActionIn.Ref;
            var newDevice = SiidDevice.GetFromListByID(Instance.Devices, devID);
            Instance.host.SetDeviceValueByRef(newDevice.Ref, ActionIn.ControlValue, true);
            Instance.host.SetDeviceString(newDevice.Ref, ""+ActionIn.ControlValue, true);
            newDevice.UpdateExtraData("ScratchPadString", ""+ActionIn.ControlValue);


        }
        public void SetRate(SiidDevice Device, double RatePlus200) {
            double Rate = 0;
            if (RatePlus200 < 0)
            {
                Rate = RatePlus200 + 200;
            }
            else
            {
                Rate = RatePlus200 - 200;
            }
            Device.UpdateExtraData("RateValue", Rate.ToString());
            UpdateDisplay(Device);
            CheckForReset(Device);

        }
        public void resetOrSetDateOrRate(CAPI.CAPIControl ActionIn)
        {
            var devID = ActionIn.Ref;
            var newDevice = SiidDevice.GetFromListByID(Instance.Devices, devID);

            if (Math.Abs(ActionIn.ControlValue) >= 200)
            {
                SetRate(newDevice, ActionIn.ControlValue);
                return;
            }

            if (ActionIn.ControlValue < 0){
                Reset(newDevice);

            }
            else { //Set to monthly, accumulator, active, with date as value
                newDevice.UpdateExtraData("IsEnabled", "True");
                newDevice.UpdateExtraData("IsAccumulator", "True");
                newDevice.UpdateExtraData("ResetType", "3");
                newDevice.UpdateExtraData("DayOfMonth", ""+ActionIn.ControlValue+"");
            }

        }

        public string addSubrule(string data)
        {


            //Make a new rule, but make it in service to an existing rule:
            var dv = Instance.host.NewDeviceRef("R");


            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);
            newDevice.set_Name(Instance.host, "Type Meter Rate" + dv);
            //   newDevice.set_Location2(Instance.host, "ScratchpadSubRule");
            newDevice.set_Location(Instance.host, "Utilities");
            newDevice.set_Location2(Instance.host, "Rates");

            //newDevice.set_Interface(Instance.host, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function
                                                                     //newDevice.set_InterfaceInstance()''  SET INTERFACE INSTANCE
            newDevice.set_Relationship(Instance.host, Enums.eRelationship.Not_Set);

            newDevice.MISC_Set(Instance.host, Enums.dvMISC.NO_LOG);
            newDevice.MISC_Set(Instance.host, Enums.dvMISC.SHOW_VALUES);
            // newDevice.MISC_Set(Instance.host, Enums.dvMISC.HIDDEN);
            HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();

            // EDO = newDevice.get_PlugExtraData_Get(Instance.host);


            string ruleString = makeNewRules();


            string userNote = newDevice.get_UserNote(Instance.host);
            userNote = userNote.Split("PLUGIN EXTRA DATA:".ToCharArray())[0];
            userNote += "PLUGIN EXTRA DATA:"+ruleString.ToString();
            newDevice.set_UserNote(Instance.host, userNote);

            EDO.AddNamed("SSIDKey", ruleString);
            newDevice.set_PlugExtraData_Set(Instance.host, EDO);

            // newDevice.set_Device_Type_String(Instance.host, makeNewModbusGateway());
            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


            newDevice.set_DeviceType_Set(Instance.host, DevINFO);
            Instance.Devices.Add(new SiidDevice(Instance, newDevice));

            MakeStewardVSP(dv);





            StringBuilder stb = new StringBuilder();
            var page = this;
           string GatewayID = data.Split("_".ToCharArray())[1];

           
            SiidDevice GateWay = SiidDevice.GetFromListByID(Instance.Devices, Convert.ToInt32(GatewayID));
            Scheduler.Classes.DeviceClass Gateway = GateWay.Device; //Should keep in gateway a list of devices
     
            Gateway.AssociatedDevice_Add(Instance.host, dv); //This is totally working actually

            return "refresh";

            stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='/deviceutility?ref=" + dv + "&edit=1'\" />");
            //    stb.Append("<a id = 'LALA' href='/deviceutility?ref=" + dv + "&edit=1'/><script>LALA.click()</script> ");
            page.AddBody(stb.ToString());
            return page.BuildPage();

        }

        public string parseInstances(string data)
        {


            Console.WriteLine("ConfigDevicePost: " + data);


            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);
            string partID = changed["id"].Split('_')[0];
            int devId = Int32.Parse(changed["id"].Split('_')[1]);

            if (partID == "S")
            {
                addSubrule(changed["id"]);
            }
            else if (partID == "R")
            {
                Reset(SiidDevice.GetFromListByID(Instance.Devices, devId));
            }

            else
            {

                if (partID == "ScratchPadString")
                {
                    changed["value"] = changed["value"].Replace(" ", "%2B");

                }
                SiidDevice newDevice = SiidDevice.GetFromListByID(Instance.Devices, devId);
                //check for gateway change, do something special
                if (partID == "Name")
                {
                    newDevice.Device.set_Name(Instance.host, changed["value"]);
                }
                else if (partID == "devdelete")
                {
                    Instance.host.DeleteDevice(devId);
                    SiidDevice.removeDev(Instance.Devices, devId);
                }

                else
                {
                    newDevice.UpdateExtraData(partID, changed["value"]);
                    if (partID == "ScratchPadString")
                    {
                        UpdateDisplay(newDevice);
                    }
                }
            }

            
            return "True";
        }


    }
}
