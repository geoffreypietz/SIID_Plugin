using HomeSeerAPI;
using HSPI_Utilities_Plugin.General;
using Scheduler;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace HSPI_Utilities_Plugin.ScratchPad
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
            var parts = HttpUtility.ParseQueryString(Rule.Device.get_PlugExtraData_Get(Instance.host).GetNamed("SSIDKey").ToString());

            string RawNumberString = GeneralHelperFunctions.GetValues(Instance, parts["LiveUpdateID"]);
            double CalculatedString = CalculateString(RawNumberString);
            try
            {
                String Name=Rule.Device.get_Name(Instance.host).ToLower();
                //OK should apply tiered rate to the Calculated string if appropriate.
                if (Name.Contains("water") && Name.Contains("meter") && (Name.Contains("indoor") || Name.Contains("outdoor")))
                {
                    double MeterString = Double.Parse(parts["RawValue"]);
                    CalculatedString = LiveTier(MeterString,CalculatedString, Rule);
               


                }
                else
                {


                    double Rate = 1;
                    if (Boolean.Parse(parts["showTier"])){
                        Rate = Double.Parse(parts["RateTier1"]);
                    }
                    else if (!String.IsNullOrEmpty(parts["RateValue"]))
                    {
                        Rate = Double.Parse(parts["RateValue"]);

                    }

               
                    CalculatedString = CalculatedString * Rate;

                }
              

          
            }
            catch
            {
            }
            if (Math.Abs(CalculatedString) < .00001) 
            {
                CalculatedString = 0;
            }

           

            Rule.UpdateExtraData("LiveValue", "" + CalculatedString.ToString());
             


        }

        //Functions like TieredRates, but just gets the tier and applies that rate to LiveValue
        public Double LiveTier(Double MeterValue,Double LiveValue, SiidDevice Rule)
        {
        

            var parts = HttpUtility.ParseQueryString(Rule.Device.get_PlugExtraData_Get(Instance.host).GetNamed("SSIDKey").ToString());
            Double AWCorLot = Double.Parse(parts["AWCOrLot"]);
            Double Rate1 = Double.Parse(parts["RateTier1"]);
            Double Rate2 = Double.Parse(parts["RateTier2"]);
            Double Rate3 = Double.Parse(parts["RateTier3"]);

            String Name = Rule.Device.get_Name(Instance.host).ToLower();
            if (Name.Contains("indoor"))
            {
                //Indoor water meter
                //AWCOrLot is AWC
                //Final Amount = Rate1 * (Meter Value up to AWC) + Rate2 * (Meter value from AWC up to AWC*1.2) + Rate3 * (Meter value greater than AMC*1.2)
                Double Amount1 = 0;
                Double Amount2 = 0;
                Double Amount3 = 0;
                if (MeterValue > AWCorLot * 1.2)
                {
                    return Rate3 * LiveValue;


                }
                if (MeterValue > AWCorLot)
                {
                    return Rate2 * LiveValue;
                }
                return Rate1 * LiveValue;


            }
            else
            {
                //Outdoor water meter, AWC
                //AWCOrLot is LotSize
                //Use lot size to calculate outdoor allotment ALT 

                //ALT follows the table in the Sterling Ranch Outdoor Water Allotment Table
                //10/13/2017
                /*Lot Size      Gal/year        Apr         May         June         July       Aug     Set     Oct         Multiplier
                 * 0 - 3000     10000           700         1600        1900        2100        1900    1300    500         1
                 * 3001-4000    12500           875         2000        2375        2625        2375    1625    625         1.25
                 * 4001-5000    15000           1050        2400        2850        3150        2850    1950    750         1.5
                 * 5001-6000    27000           1890        4320        5130        5670        5130    3510    1350        2.7
                 * 6001-7000    32000           2240        5120        6080        6720        6080    4160    1600        3.2
                 * 7001-8000    39000           2730        6240        7410        8190        7410    5070    1950        3.9
                 * 8001-11000   49000           3430        7840        9310        10290       9310    6370    2450        4.9
                 * 11001-20000  60000           4200        9600        11400       12600       11400   7800    3000        6
                 * 20001-30000  80000           5600        12800       15200       16800       15200   10400   4000        8
                 * 30001 ++     100000          7000        1600        19000       21000       19000   13000   5000        10
              
                 */
                //Definitely a better way to do this. 
                double[] MonthRate = new double[] { 700, 1600, 1900, 2100, 1900, 1300, 500, 0, 0, 0, 0, 0 };
                String[] MonthIndex = new String[] { "4", "5", "6", "7", "8", "9", "10", "11", "12", "1", "2", "3" };
                double[] SizeCap = new double[] { 3001, 4001, 5001, 6001, 7001, 8001, 11001, 20001, 30001 };
                double[] MultPlier = new double[] { 1, 1.25, 1.5, 2.7, 3.2, 3.9, 4.9, 6, 8, 10 };
                int index = 0;
                foreach (double Cap in SizeCap)
                {
                    if (Cap > AWCorLot)
                    {
                        break;
                    }
                    index++;
                }
                double Mult = MultPlier[index];
                //  int thisMonth = int.Parse(DateTime.Now.ToString("MM")); //this is the month number


                double MonthVal = MonthRate[Array.IndexOf(MonthIndex, DateTime.Now.Month.ToString())];


                double ALT = Mult * MonthVal;
                Rule.UpdateExtraData("OutdoorWaterBudget", "" + ALT);
                //ALT is the budget. Want to return this to STEWARD also.
                //Final Amount = Rate1 * (Meter Value up to ALT) + Rate2 * (Meter value from ALT up to ALT*1.2) + Rate3 * (Meter value from ALT*1.2 to 1.4*ALT) + Rate4 * (Meter value greater than AMC*1.4)

                if (ALT > 0)
                {


                    Double Rate4 = Double.Parse(parts["RateTier4"]);
                
                    if (MeterValue > AWCorLot * 1.4)
                    {
                        return Rate4 * LiveValue;
                    }
                    if (MeterValue > AWCorLot * 1.2)
                    {
                        return Rate3 * LiveValue;
                    }
                    if (MeterValue > AWCorLot)
                    {
                        return Rate2 * LiveValue;
                    }
                    return Rate1 * LiveValue;
                }
                else
                { //If the allowence is zero, just apply Rate1 to full amount
                   return  LiveValue * Rate1;
                }
            }
          //  double Rate = Double.Parse(parts["RateValue"]);
        //   return LiveValue * Rate1;
          

          

        }

        /// <summary>
        /// Turns a raw meter read into a tiered value according to hardcoded rules
        /// </summary>
        /// <param name="MeterValue"></param>
        /// <param name="Rule"></param>
        /// <returns></returns>
        public Double TieredRate(Double MeterValue, SiidDevice Rule)
        {
            Double Out = 0;
            var parts = HttpUtility.ParseQueryString(Rule.Device.get_PlugExtraData_Get(Instance.host).GetNamed("SSIDKey").ToString());

                /*       
        "showTier",
        "RateTier1",
        "RateTier2",
        "RateTier3",
        "AWCOrLot"
        */

            Double AWCorLot = Double.Parse(parts["AWCOrLot"]);
            Double Rate1 = Double.Parse(parts["RateTier1"]);
            Double Rate2 = Double.Parse(parts["RateTier2"]);
            Double Rate3 = Double.Parse(parts["RateTier3"]);

            String Name = Rule.Device.get_Name(Instance.host).ToLower();
            if (Name.Contains("indoor"))
            {
                //Indoor water meter
                //AWCOrLot is AWC
                //Final Amount = Rate1 * (Meter Value up to AWC) + Rate2 * (Meter value from AWC up to AWC*1.2) + Rate3 * (Meter value greater than AMC*1.2)
                Double Amount1 = 0;
                Double Amount2 = 0;
                Double Amount3 = 0;
                if (MeterValue > AWCorLot * 1.2) {
                    Amount3 = MeterValue - (AWCorLot * 1.2);
                    MeterValue -= Amount3;
                }
                if (MeterValue > AWCorLot) {
                    Amount2 = MeterValue - AWCorLot;
                    MeterValue -= Amount2;
                }
                Amount1 = MeterValue;
                Out = Rate1 * Amount1 + Rate2 * Amount2 + Rate3 * Amount3;


            }
            else
            {
                //Outdoor water meter, AWC
                //AWCOrLot is LotSize
                //Use lot size to calculate outdoor allotment ALT 

                //ALT follows the table in the Sterling Ranch Outdoor Water Allotment Table
                //10/13/2017
                /*Lot Size      Gal/year        Apr         May         June         July       Aug     Set     Oct         Multiplier
                 * 0 - 3000     10000           700         1600        1900        2100        1900    1300    500         1
                 * 3001-4000    12500           875         2000        2375        2625        2375    1625    625         1.25
                 * 4001-5000    15000           1050        2400        2850        3150        2850    1950    750         1.5
                 * 5001-6000    27000           1890        4320        5130        5670        5130    3510    1350        2.7
                 * 6001-7000    32000           2240        5120        6080        6720        6080    4160    1600        3.2
                 * 7001-8000    39000           2730        6240        7410        8190        7410    5070    1950        3.9
                 * 8001-11000   49000           3430        7840        9310        10290       9310    6370    2450        4.9
                 * 11001-20000  60000           4200        9600        11400       12600       11400   7800    3000        6
                 * 20001-30000  80000           5600        12800       15200       16800       15200   10400   4000        8
                 * 30001 ++     100000          7000        1600        19000       21000       19000   13000   5000        10
              
                 */
                 //Definitely a better way to do this. 
                double[] MonthRate = new double[]{ 700, 1600, 1900, 2100, 1900, 1300, 500 ,0,0,0,0,0};
                String[] MonthIndex = new String[] { "4", "5", "6", "7", "8", "9", "10","11","12","1","2","3" };
                double[] SizeCap = new double[] { 3001, 4001, 5001, 6001, 7001, 8001, 11001, 20001, 30001 };
                double[] MultPlier = new double[] { 1, 1.25, 1.5, 2.7, 3.2, 3.9, 4.9, 6, 8,10};
                int index = 0;
                foreach (double Cap in SizeCap)
                {
                    if (Cap > AWCorLot)
                    {
                        break;
                    }
                    index++;
                }
                double Mult = MultPlier[index];
                //  int thisMonth = int.Parse(DateTime.Now.ToString("MM")); //this is the month number
              

                    double MonthVal = MonthRate[Array.IndexOf(MonthIndex, DateTime.Now.Month.ToString())];
              
              
                double ALT = Mult * MonthVal;
                Rule.UpdateExtraData("OutdoorWaterBudget", "" + ALT);
                //ALT is the budget. Want to return this to STEWARD also.
                //Final Amount = Rate1 * (Meter Value up to ALT) + Rate2 * (Meter value from ALT up to ALT*1.2) + Rate3 * (Meter value from ALT*1.2 to 1.4*ALT) + Rate4 * (Meter value greater than AMC*1.4)

                if (ALT > 0)
                {


                    Double Rate4 = Double.Parse(parts["RateTier4"]);
                    Double Amount1 = 0;
                    Double Amount2 = 0;
                    Double Amount3 = 0;
                    Double Amount4 = 0;
                    if (MeterValue > AWCorLot * 1.4)
                    {
                        Amount4 = MeterValue - (AWCorLot * 1.4);
                        MeterValue -= Amount4;
                    }
                    if (MeterValue > AWCorLot * 1.2)
                    {
                        Amount3 = MeterValue - (AWCorLot * 1.2);
                        MeterValue -= Amount3;
                    }
                    if (MeterValue > AWCorLot)
                    {
                        Amount2 = MeterValue - AWCorLot;
                        MeterValue -= Amount2;
                    }
                    Amount1 = MeterValue;
                    Out = Rate1 * Amount1 + Rate2 * Amount2 + Rate3 * Amount3 + Rate4 * Amount4;
                }
                else
                { //If the allowence is zero, just apply Rate1 to full amount
                    Out = MeterValue * Rate1;
                }
            }
            return Out;

            }



        public void UpdateDisplay(SiidDevice Rule)
        {
            try
            {
                var parts=HttpUtility.ParseQueryString(Rule.Device.get_PlugExtraData_Get(Instance.host).GetNamed("SSIDKey").ToString());
               
                //Do the calculator string parse to get the new value
                string RawNumberString = GeneralHelperFunctions.GetValues(Instance,parts["ScratchPadString"]);
                double RawCalculatedString = CalculateString(RawNumberString); //Raw meter read
                double CalculatedString = 0; //difference betweenold and new values
                double RatedString = 0; //difference multiplied by the rate
             

               
             
                if (bool.Parse(parts["IsAccumulator"]))
                {
                    CalculatedString = RawCalculatedString - Double.Parse(parts["OldValue"]);

                }

 //Raw value is before rate
                //So to get the pre-rate value from another scratchpad rule do $(ruleID)
                //To get the post rate value do #(ruleID)

                try
                {

                    //In a meter setting the Calculated String is the raw meter read. Now we multiply it by the rate (because the output should be the cost of the meter)

                    //SO Time for some odd hardcoding.

                    //Every meter uses rate Unless it's an indoor water meter or an outdoor water meter.  We will check for those and apply the tiered rates if so.
                    //In the future we may want to genealize this
                    String Name = Rule.Device.get_Name(Instance.host).ToLower();
                    if (Name.Contains("water") && Name.Contains("meter") && (Name.Contains("indoor") || Name.Contains("outdoor")))
                    {
                        RatedString = TieredRate(CalculatedString, Rule);
                            
                          
                    }
                    else
                    {
                        double Rate = 1;
                        if (Boolean.Parse(parts["showTier"])){
                            Rate = Double.Parse(parts["RateTier1"]);
                        }
                        else if (!String.IsNullOrEmpty(parts["RateValue"]))
                        {
                            Rate = Double.Parse(parts["RateValue"]);

                        }

                        //round calculated string and rated string to nearest hundreths place
                        RatedString = CalculatedString * Rate;
                       
                    }

             

                }
                catch (Exception e)
                {
                    Instance.hspi.Log("Problem rendering scratchpad rule " + e.Message, 2);
                }


                //processed value is after rate  

        

                //If the reset time was too soon, set CalculatedString to be 0


               
                if (bool.Parse(parts["IsAccumulator"])&&DateTime.Now.AddHours(-1).Subtract(DateTime.Parse(parts["DateOfLastReset"])).TotalSeconds < 45)  //If the reset was less than 45 seconds ago, display 0 as the scratchpad value
                {
                   
                    CalculatedString = 0;
                    RatedString = 0;
                }

                RatedString = Math.Round(RatedString * 1000) / 1000;
                CalculatedString = Math.Round(CalculatedString * 1000) / 1000;
                RawCalculatedString = Math.Round(RawCalculatedString * 1000) / 1000;

                string ValueString = String.Format(parts["DisplayString"], RatedString);

                Instance.host.SetDeviceString(Rule.Ref, ValueString, true);
                Instance.host.SetDeviceValueByRef(Rule.Ref, CalculatedString, true);

                Rule.LoadExtraData();
                Rule.UpdateExtraDataNoCall("NewValue", "" + RawCalculatedString.ToString());  //Newest meter read
                Rule.UpdateExtraDataNoCall("RawValue", "" + CalculatedString);  //Raw meter read for this month
                Rule.UpdateExtraDataNoCall("ProcessedValue", "" + RatedString);  //Raw meter read multiplied by rate information
                Rule.UpdateExtraDataNoCall("CurrentTime", "" + DateTime.Now.ToString());
                Rule.UpdateExtraDataNoCall("DisplayedValue", "" + ValueString);  //the ProcessedValue displayed on the meter
                Rule.SaveExtraData();

                // EDO.RemoveNamed("SSIDKey");
                //  EDO.AddNamed("SSIDKey", parts.ToString());

                 parts = HttpUtility.ParseQueryString(Rule.Device.get_PlugExtraData_Get(Instance.host).GetNamed("SSIDKey").ToString());

                string userNote = Rule.Device.get_UserNote(Instance.host);
                userNote = userNote.Split("PLUGIN EXTRA DATA:".ToCharArray())[0];
                userNote += "PLUGIN EXTRA DATA:" + parts.ToString();
                Rule.Device.set_UserNote(Instance.host, userNote);

                Rule.Device.set_PlugExtraData_Set(Instance.host, Rule.Extra);
           
            }
            catch (Exception e)
            {
                Instance.hspi.Log("Problem updating scratchpad display " + e.Message, 2);
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
            parts["DateOfLastReset"] = DateTime.Now.AddHours(-1).ToString();
           
            EDO.RemoveNamed("SSIDKey");
            EDO.AddNamed("SSIDKey", parts.ToString());
            Rule.Device.set_PlugExtraData_Set(Instance.host, EDO);
            string userNote=Rule.Device.get_UserNote(Instance.host);
            userNote = userNote.Split("PLUGIN EXTRA DATA:".ToCharArray())[0];
            userNote += "PLUGIN EXTRA DATA:"+parts.ToString();
            Rule.Device.set_UserNote(Instance.host, userNote);
            
           Rule.Extra = EDO;
            UpdateDisplay(Rule);
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
           //Instance.siidPage.InitializeBacnetDeviceTimers();      //No, can't do this here, since this method itself is called on a timer
            
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

            parts["FixedCost"] = "0";
            parts["RateTier1"] = "0";
            parts["RateTier2"] = "0";
            parts["RateTier3"] = "0";
            parts["RateTier4"] = "0";
            parts["AWCOrLot"] = "8000";
            parts["showTier"] = "False";

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
           
            var Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
            Control.PairType = VSVGPairs.VSVGPairType.Range;
            Control.RangeStart = -100000;
            Control.RangeEnd = 100000;
            Control.Render = Enums.CAPIControlType.TextBox_Number;
          var IS =  Instance.host.DeviceVSP_AddPair(deviceID, Control);

        

        }


        public List<SiidDevice> getAllRules()
        {
            List<SiidDevice> listOfDevices = new List<SiidDevice>();
            List<SiidDevice> B = new List<SiidDevice>();
            lock (Instance.Devices)
            {
                
                foreach (SiidDevice C in Instance.Devices) {
                    B.Add(C);
                }
            }
                
                SiidDevice.Update(Instance);

                foreach (SiidDevice Dev in B)
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
            ScratchTable.addHead(new string[] { "Rule Name", "Value", "Enable Rule", "Is Accumulator", "Reset Type", "Reset Interval", "Rule String", "Fixed Cost","Show Tiered Rates", "Rate ($ per unit)", "Real Time Data Rule String", "Rule Formatting", "HomeseerID" }); //0,1,2,3,4,5

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

            Row.Add(ScratchBuilder.stringInput("ScratchPadString_" + ID, parts["ScratchPadString"].Replace("(^p^)", "+")).print());
            if (parts["FixedCost"] == null)
            {
                parts["FixedCost"] = "0";
            }
            Row.Add(ScratchBuilder.stringInput("FixedCost_" + ID, parts["FixedCost"].Replace("(^p^)", "+")).print());

            if (parts["showTier"] == null)
            {
                parts["showTier"] = "False";
            }
            Row.Add(ScratchBuilder.checkBoxInput("showTier_" + ID, bool.Parse(parts["showTier"])).print());
            if (parts["RateTier1"] == null)
            {
                parts["RateTier1"] = "0";
            }
            if (parts["RateTier2"] == null)
            {
                parts["RateTier2"] = "0";
            }
            if (parts["RateTier3"] == null)
            {
                parts["RateTier3"] = "0";
            }
            if (parts["RateTier4"] == null)
            {
                parts["RateTier4"] = "0";
            }
            if (parts["AWCOrLot"] == null)
            {
                parts["AWCOrLot"] = "8000";
            }

            if (parts["showTier"] == "True")
            {
                Row.Add("<div id='tiers" + ID + "' style='display:inline'>Tier 1:" + ScratchBuilder.stringInput("RateTier1_" + ID, parts["RateTier1"]).print() + "  Tier 2:" +
ScratchBuilder.stringInput("RateTier2_" + ID, parts["RateTier2"]).print() + "  Tier 3:" +
ScratchBuilder.stringInput("RateTier3_" + ID, parts["RateTier3"]).print() + "  Tier 4:" +
ScratchBuilder.stringInput("RateTier4_" + ID, parts["RateTier4"]).print() + "  AWC or LotSize:" +
ScratchBuilder.stringInput("AWCOrLot_" + ID, parts["AWCOrLot"]).print() + "</div>" + "<div id='rate" + ID + "' style='display:none'>" + ScratchBuilder.stringInput("RateValue_" + ID, parts["RateValue"]).print() + "</div>" + @"<script>
UpdateTier=function(id){
IsChecked = $('#showTier_' + id)[0].checked;
if(IsChecked){
$('#tiers'+id)[0].style.display='inline';
$('#rate'+id)[0].style.display='none';
}
else{
$('#tiers'+id)[0].style.display='none';
$('#rate'+id)[0].style.display='inline';
}
}
TierChange=function(){
console.log('TierChange');
console.log(this);
UpdateTier(this.id.split('_')[1]);
}
UpdateTier(" + ID + @");
$('#showTier_" + ID + @"').change(TierChange); //OK HERE

</script>");



            }
            else
            {
                Row.Add("<div id='tiers" + ID + "' style='display:none'>Tier 1:" + ScratchBuilder.stringInput("RateTier1_" + ID, parts["RateTier1"]).print() + "  Tier 2:" +
ScratchBuilder.stringInput("RateTier2_" + ID, parts["RateTier2"]).print() + "  Tier 3:" +
ScratchBuilder.stringInput("RateTier3_" + ID, parts["RateTier3"]).print() + "  Tier 4:" +
ScratchBuilder.stringInput("RateTier4_" + ID, parts["RateTier4"]).print() + "  AWC or LotSize:" +
ScratchBuilder.stringInput("AWCOrLot_" + ID, parts["AWCOrLot"]).print() + "</div>" + "<div id='rate" + ID + "' style='display:none'>" + ScratchBuilder.stringInput("RateValue_" + ID, parts["RateValue"]).print() + "</div>" + @"<script>
UpdateTier=function(id){
IsChecked = $('#showTier_' + id)[0].checked;
if(IsChecked){
$('#tiers'+id)[0].style.display='inline';
$('#rate'+id)[0].style.display='none';
}
else{
$('#tiers'+id)[0].style.display='none';
$('#rate'+id)[0].style.display='inline';
}
}
TierChange=function(){
console.log('TierChange');
console.log(this);
UpdateTier(this.id.split('_')[1]);
}
UpdateTier(" + ID + @");
$('#showTier_" + ID + @"').change(TierChange); //OK HERE

</script>");
            }


            Row.Add(ScratchBuilder.stringInput("LiveUpdateID_" + ID, parts["LiveUpdateID"]).print());
            //Instance.hspi.Log(parts["ScratchPadString"],0);
            Row.Add(ScratchBuilder.stringInput("DisplayString_" + ID, parts["DisplayString"]).print());
            Row.Add("" + Dev.Ref);

            Row.Add(ScratchBuilder.button("R_" + ID.ToString(), "Reset").print());
            Row.Add(ScratchBuilder.DeleteDeviceButton(ID.ToString()).print());
            //  Row.Add(ScratchBuilder.button("S_" + ID.ToString(), "Add Associated Device").print());

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
        /// <summary>
        /// SIID set rate(units per dollar):		   float between 200 and 1000 (subtract 200 round to 4 decimal places), rate range between 0 and 800
        /// </summary>
        /// <param name="Device"></param>
        /// <param name="RatePlus200"></param>
        public void SetRate(SiidDevice Device, double RatePlus200) {
            double Rate = 0;
     //       if (RatePlus200 < 0)
      //      {
      //          Rate = RatePlus200 + 200;
      //      }
       //     else
    //        {
                Rate = RatePlus200 - 200;
      //      }
            Rate= Double.Parse(Rate.ToString("0.0000"));
            Device.UpdateExtraData("RateValue", Rate.ToString());
            UpdateDisplay(Device);
            CheckForReset(Device);

        }

        /// <summary>
        /// SIID fixed cost: Between 1000 and 2000(subtract 1000 round to 4 decmil, range from 0 to 1000)
        /// </summary>
        /// <param name="Device"></param>
        /// <param name="FixedCostPlus1000"></param>
        public void SetFixedCost(SiidDevice Device, double FixedCostPlus1000)
        {
            double FixedCost = 0;

            FixedCost = FixedCostPlus1000 - 1000;

            FixedCost = Double.Parse(FixedCost.ToString("0.00000"));
            Device.UpdateExtraData("FixedCost", FixedCost.ToString());
            UpdateDisplay(Device);
            CheckForReset(Device);
        }

        /// <summary>
        ///SIID Tiers(4 tiers with boundaries based on AWC or Lotsize), each tier needs an editable rate.
        ///        Tier 1 rate between 2000 and 3000(subtract 2000, round to 4 decmil, range 0 to 1000)
        ///        Tier 2 rate between 3000 and 4000(subtract 3000, round to 4 decmil, range 0 to 1000)
        ///        Tier 3 rate between 4000 and 5000(subtract 4000, round to 4 decmil, range 0 to 1000)
        ///        Tier 4 rate between 5000 and 6000(subtract 4000, round to 4 decmil, range 0 to 1000)
        /// </summary>
        /// <param name="Device"></param>
        /// <param name="TieredRate"></param>
        public void SetTieredRate(SiidDevice Device, double TieredRate)
        {
            String Tier = "RateTier";
            Double Rate = 0;
            //Tier 4 rate
            if (TieredRate >= 5000)
            {
                Rate = TieredRate - 5000;
                Tier += "4";
            }
            //Tier 3 rate
            if (TieredRate >= 4000) {
                Rate = TieredRate - 4000;
                Tier += "3";
            }
            //Tier 2 rate
            else if (TieredRate >= 3000) {
                Rate = TieredRate - 3000;
                Tier += "2";
            }
            //Tier 1 rate
            else {
                Rate = TieredRate - 2000;
                Tier += "1";
            }

            Rate = Double.Parse(Rate.ToString("0.0000"));
            Device.UpdateExtraData(Tier, Rate.ToString());
            Device.UpdateExtraData("showTier", "True");
            UpdateDisplay(Device);
            CheckForReset(Device);

        }

        /// <summary>
        ///SIID AWC: Negative number less than - 10(negate, subtract 10) No hardcoded upper range
        ///SIID LotSize: -Either AWC or LOTSIZE depending on indooor / outdoor, can use the same range for setting
        ///
        /// SIID stores AWX/LotSize, Steward decides what to do with it and with the tiered rates
        /// </summary>
        /// <param name="Device"></param>
        /// <param name="AWCOrLot"></param>
        public void SetAWC_LotSize(SiidDevice Device, double AWCOrLot)
        {
          
           //
      
            double AWC_Lot = 8000;
            AWC_Lot = -(AWCOrLot + 10);


            AWC_Lot = Double.Parse(AWC_Lot.ToString("0.00000"));
            Device.UpdateExtraData("AWCOrLot", AWC_Lot.ToString());
            Device.UpdateExtraData("showTier", "True");
            UpdateDisplay(Device);
            CheckForReset(Device);
        }

        /// <summary>
        /// for all accumulators:					 float/double value
        ///SIID reset accumulator: 			  -1
        ///SIID set day of monthly reset:			   integer between 0 and 28
        ///SIID set rate(units per dollar):		   float between 200 and 1000 (subtract 200 round to 4 decimal places), rate range between 0 and 800
        ///SIID fixed cost: Between 1000 and 2000(subtract 1000 round to 4 decmil, range from 0 to 1000)
        ///For indoor / outdoor water:
        ///SIID Tiers(4 tiers with boundaries based on AWC or Lotsize), each tier needs an editable rate.
        ///        Tier 1 rate between 2000 and 3000(subtract 2000, round to 4 decmil, range 0 to 1000)
        ///        Tier 2 rate between 3000 and 4000(subtract 3000, round to 4 decmil, range 0 to 1000)
        ///        Tier 3 rate between 4000 and 5000(subtract 4000, round to 4 decmil, range 0 to 1000)

        ///SIID AWC: Negative number less than - 10(negate, subtract 10) No hardcoded upper range
        ///SIID LotSize: -Either AWC or LOTSIZE depending on indooor / outdoor, can use the same range for setting

        /// </summary>
        /// <param name="ActionIn"></param>
        public void scratchpadCommandIn(CAPI.CAPIControl ActionIn)
        {
            try
            {
                var devID = ActionIn.Ref;
                var newDevice = SiidDevice.GetFromListByID(Instance.Devices, devID);
                if (ActionIn.ControlValue == -1)
                { //reset if -1
                    Reset(newDevice);
                }
                else if (ActionIn.ControlValue >= 0 && ActionIn.ControlValue < 35)
                {
                    newDevice.UpdateExtraData("IsEnabled", "True");
                    newDevice.UpdateExtraData("IsAccumulator", "True");
                    newDevice.UpdateExtraData("ResetType", "3");
                    newDevice.UpdateExtraData("DayOfMonth", "" + Math.Round(ActionIn.ControlValue).ToString() + "");
                }
                else if (ActionIn.ControlValue >= 200 && ActionIn.ControlValue < 1000)
                {
                    SetRate(newDevice, ActionIn.ControlValue);
                }
                else if (ActionIn.ControlValue >= 1000 && ActionIn.ControlValue < 2000)
                {
                    SetFixedCost(newDevice, ActionIn.ControlValue);
                }
                else if (ActionIn.ControlValue >= 2000)
                {
                    SetTieredRate(newDevice, ActionIn.ControlValue);
                }
                else if (ActionIn.ControlValue <= -10)
                {
                    SetAWC_LotSize(newDevice, ActionIn.ControlValue);
                }

                else
                { //Set to monthly, accumulator, active, with date as value
                    Instance.hspi.Log("Error when parsing scratchpad command: Command was not in a recognised range. \nDevice: " + ActionIn.Ref + " Command: " + ActionIn.ControlValue.ToString(), 2);
                }

            }
            catch(Exception E)
            {
                Instance.hspi.Log("Error when parsing scratchpad command " + E.Message+"\nDevice: "+ ActionIn.Ref+" Command: "+ ActionIn.ControlValue.ToString(), 2);
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


          //  Console.WriteLine("ConfigDevicePost: " + data);


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
                    UpdateDisplay(newDevice);
              
                }
            }

             
            return "True";
        }


    }
}
