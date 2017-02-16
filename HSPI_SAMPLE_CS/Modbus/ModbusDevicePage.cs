﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler;
using System.Web;
using HomeSeerAPI;
using Modbus.Data;
using Modbus.Device;
using Modbus.Utility;

using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Data;

namespace HSPI_SAMPLE_CS.Modbus
{


    public class ModbusDevicePage : PageBuilderAndMenu.clsPageBuilder

    {

       

        public ModbusDevicePage(string pagename) : base(pagename)
        {
            UpdateGateList( getAllGateways());
        }

        htmlBuilder ModbusBuilder = new htmlBuilder("ModBusGatewayPage");
        public static List<KeyValuePair<int,string>> ModbusGates { get; set; }


        public static int[] getAllGateways()
        {

            Scheduler.Classes.clsDeviceEnumeration DevNum = (Scheduler.Classes.clsDeviceEnumeration)Util.hs.GetDeviceEnumerator();
            List<int> ModbusGates = new List<int>();
          
            //Scheduler.Classes.DeviceClass
            var Dev = DevNum.GetNext();
            while (Dev != null)
            {
                try
                {
                    var EDO = Dev.get_PlugExtraData_Get(Util.hs);
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    string s = parts["Type"];
                    if (parts["Type"] == "Modbus Gateway")
                    {
                        ModbusGates.Add(Dev.get_Ref(Util.hs));
   

                    }
                    //   if (parts["Type"] == "Modbus Device")
                    //     {
                    //        ModbusDevs.Add(Dev.get_Ref(Util.hs));
                    //    }

                }
                catch
                {

                }
                Dev = DevNum.GetNext();


            }
            return ModbusGates.ToArray();
        }

        public string makeNewModbusGateway()
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);


            parts["Type"] = "Modbus Gateway";
            parts["Gateway"] = "";
            parts["TCP"] = "502";
            parts["Poll"] = MosbusAjaxReceivers.modbusDefaultPoll.ToString();
            parts["Enabled"] = "false";
            parts["BigE"] = "false";
            parts["ZeroB"] = "true";
            parts["RWRetry"] = "2";
            parts["RWTime"] = "1000";
            parts["Delay"] = "0";
            parts["RegWrite"] = "1";
            parts["LinkedDevices"] = "";
            parts["RawValue"] = "0";
            parts["ProcessedValue"] = "0";
            return parts.ToString();

        }
        public string makeNewModbusDevice(int GatewayID, int DeviceID)
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);
            Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GatewayID); //Should keep in gateway a list of devices


            var EDO = Gateway.get_PlugExtraData_Get(Util.hs);
            var GParts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            parts["Type"] = "Modbus Device";
            parts["GateID"] = "" + GatewayID + "";
            parts["Gateway"] = Gateway.get_Name(Util.hs);
            parts["RegisterType"] = "0";//MosbusAjaxReceivers.modbusDefaultPoll.ToString(); //0 is discrete input, 1 is coil, 2 is InputRegister, 3 is Holding Register
            parts["SlaveId"] = "1"; //get number of slaves from gateway?
            parts["ReturnType"] = "0";
            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
            //tells us how many registers to read/write and also how to parse returns
            //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
            //So coil and discrete are bool ONLY
            //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read
            parts["SignedValue"] = "false";
            parts["ScratchpadString"] = "$("+ DeviceID + ")";
            parts["DisplayFormatString"] = "{0}";
            parts["ReadOnlyDevice"] = "true";
            parts["DeviceEnabled"] = "false";
            parts["RegisterAddress"] = "1";
            parts["RawValue"] = "0";
            parts["ProcessedValue"] = "0";
            return parts.ToString();
            //uint is unsigned int,
        }


        public void addSSIDExtraData(Scheduler.Classes.DeviceClass Device, string Key, string value)
        {


            var EDO = Device.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            parts[Key] = value;
            EDO.RemoveNamed("SSIDKey");
            EDO.AddNamed("SSIDKey", parts.ToString());
            Device.set_PlugExtraData_Set(Util.hs, EDO);

        }

        public void RemoveDeviceFromGateway(int DeviceId, int GatewayId)
        {
            Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GatewayId); //Should keep in gateway a list of devices
            var EDO = Gateway.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            String[] PLIST = parts["LinkedDevices"].Split(',');
            StringBuilder NL = new StringBuilder();
           
            foreach (string P in PLIST)
            {
                if ((!String.IsNullOrEmpty(P))&&(P!= ""+DeviceId + ""))
                {
                    NL.Append(P + ",");

                }
            }
            parts["LinkedDevices"] = NL.ToString();
            EDO.RemoveNamed("SSIDKey");
            EDO.AddNamed("SSIDKey", parts.ToString());
            Gateway.set_PlugExtraData_Set(Util.hs, EDO);
            Gateway.AssociatedDevice_Remove(Util.hs, DeviceId);


        }
        public void AddDeviceToGateway(int DeviceId, int GatewayId)
        {
            Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GatewayId); //Should keep in gateway a list of devices
            var EDO = Gateway.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            parts["LinkedDevices"] += DeviceId + ",";
            EDO.RemoveNamed("SSIDKey");
            EDO.AddNamed("SSIDKey", parts.ToString());
            Gateway.set_PlugExtraData_Set(Util.hs, EDO);
            Gateway.AssociatedDevice_Add(Util.hs, DeviceId);

            

        }
        public void changeGateway(Scheduler.Classes.DeviceClass Device, string newGatewayId)
        {

            var EDO = Device.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            string OldGatewayID = parts["GateID"];
            RemoveDeviceFromGateway(Device.get_Ref(Util.hs), Convert.ToInt32(OldGatewayID));
            AddDeviceToGateway(Device.get_Ref(Util.hs), Convert.ToInt32(newGatewayId));
            Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(Convert.ToInt32(newGatewayId));
            addSSIDExtraData(Device, "Gateway", Gateway.get_Name(Util.hs));


        }

        public string MakeSubDeviceRedirect(string pageName, string user, int userRights, string GatewayID)
        {

            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;
            GatewayID = GatewayID.Split("_".ToCharArray())[1];

            var dv = Util.hs.NewDeviceRef("Modbus Device");
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv);
            newDevice.set_Name(Util.hs, "Modbus Device " + dv);
            newDevice.set_Location2(Util.hs, "Modbus");
            newDevice.set_Location(Util.hs, "System");
            //newDevice.set_Interface(Util.hs, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
            newDevice.set_Interface(Util.hs, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function

            newDevice.set_Relationship(Util.hs, Enums.eRelationship.Child);
            newDevice.set_Status_Support(Util.hs, true);
            newDevice.MISC_Set(Util.hs, Enums.dvMISC.NO_LOG);
            newDevice.MISC_Set(Util.hs, Enums.dvMISC.SHOW_VALUES);
         



            HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();

            // EDO = newDevice.get_PlugExtraData_Get(Util.hs);

            EDO.AddNamed("SSIDKey", makeNewModbusDevice(Convert.ToInt32(GatewayID),dv));
            newDevice.set_PlugExtraData_Set(Util.hs, EDO);

            AddDeviceToGateway(dv, Convert.ToInt32(GatewayID));

            // newDevice.set_Device_Type_String(Util.hs, makeNewModbusGateway());

            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


            newDevice.set_DeviceType_Set(Util.hs, DevINFO);


            stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='/deviceutility?ref=" + dv + "&edit=1'\" />");
            //    stb.Append("<a id = 'LALA' href='/deviceutility?ref=" + dv + "&edit=1'/><script>LALA.click()</script> ");
            page.AddBody(stb.ToString());
            return page.BuildPage();
        }

   
    

        public void MakeGatewayGraphicsAndStatus(int dv)
        {
            VSVGPairs.VSPair Unreachable = new VSVGPairs.VSPair(ePairStatusControl.Status);
            Unreachable.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Unreachable.Render = Enums.CAPIControlType.TextBox_String;
            Unreachable.Value = 0;
            Unreachable.Status = "Unreachable";
       



            Util.hs.DeviceVSP_AddPair(dv, Unreachable);

            Unreachable = new VSVGPairs.VSPair(ePairStatusControl.Status);
            Unreachable.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Unreachable.Render = Enums.CAPIControlType.TextBox_String;
            Unreachable.Value = 1;
            Unreachable.Status = "Available";
        
          

            Util.hs.DeviceVSP_AddPair(dv, Unreachable);

            Util.hs.DeviceVSP_AddPair(dv, Unreachable);

            Unreachable = new VSVGPairs.VSPair(ePairStatusControl.Status);
            Unreachable.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Unreachable.Render = Enums.CAPIControlType.TextBox_String;
            Unreachable.Value = 2;
            Unreachable.Status = "Disabled";



            Util.hs.DeviceVSP_AddPair(dv, Unreachable);

            var Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/SIID/SIIDDisabledPlaceholder.png";
            Graphic.Set_Value=0;

            Util.hs.DeviceVGP_AddPair(dv, Graphic);

            Util.hs.DeviceVSP_AddPair(dv, Unreachable);

             Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/SIID/SIIDGoodPlaceholder.png";
            Graphic.Set_Value = 1;

            Util.hs.DeviceVGP_AddPair(dv, Graphic);
            Util.hs.DeviceVSP_AddPair(dv, Unreachable);

             Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/SIID/SIIDMedPlaceholder.png";
            Graphic.Set_Value = 2;

            Util.hs.DeviceVGP_AddPair(dv, Graphic);


        }
        public string MakeDeviceRedirect(string pageName, string user, int userRights, string queryString)
        {
            
            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;


            var dv = Util.hs.NewDeviceRef("Modbus IP Gateway");
            MakeGatewayGraphicsAndStatus(dv);
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv);
           
            newDevice.set_Name(Util.hs, "Modbus IP Gateway " + dv);
            newDevice.set_Location2(Util.hs, "Modbus");
            newDevice.set_Location(Util.hs, "System");
            //newDevice.set_Interface(Util.hs, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
           newDevice.set_Interface(Util.hs, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function
            newDevice.set_Relationship(Util.hs, Enums.eRelationship.Parent_Root);

            newDevice.MISC_Set(Util.hs, Enums.dvMISC.NO_LOG);
            newDevice.MISC_Set(Util.hs, Enums.dvMISC.SHOW_VALUES);
            HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();
            
           // EDO = newDevice.get_PlugExtraData_Get(Util.hs);

            EDO.AddNamed("SSIDKey", makeNewModbusGateway());
            newDevice.set_PlugExtraData_Set(Util.hs, EDO);
            pingGateway(dv);

            // newDevice.set_Device_Type_String(Util.hs, makeNewModbusGateway());
          var DevINFO=  new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


            newDevice.set_DeviceType_Set(Util.hs, DevINFO);



       /*     var DeviceValue = new VSVGPairs.VSPair(ePairStatusControl.Status);
            DeviceValue.PairType = VSVGPairs.VSVGPairType.SingleValue;
            DeviceValue.Render = Enums.CAPIControlType.TextBox_String;
            DeviceValue.Value = 0;
            DeviceValue.Status = "Unknown";
            
            Util.hs.DeviceVSP_AddPair(dv, DeviceValue);*/


            stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='/deviceutility?ref=" + dv + "&edit=1'\" />");
            //    stb.Append("<a id = 'LALA' href='/deviceutility?ref=" + dv + "&edit=1'/><script>LALA.click()</script> ");
            page.AddBody(stb.ToString());
            return page.BuildPage();
        }

        public string BuildModbusGatewayTab(int dv1)
        {//Need to pull from device associated modbus information. Need to create when new device is made
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv1);

            

            var EDO = newDevice.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            string dv = "" + dv1 + "";
      
            StringBuilder stb = new StringBuilder();
            htmlBuilder ModbusBuilder = new htmlBuilder("ModBusGateTab");
            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable();
            ModbusConfHtml.addT("Gateway Settings");
            ModbusConfHtml.add("Modbus Gateway hostname or IP address: ", ModbusBuilder.stringInput(dv+"_Gateway", parts["Gateway"]).print());
            ModbusConfHtml.add("TCP Port:", ModbusBuilder.numberInput(dv+"_TCP", Int32.Parse(parts["TCP"])).print());
            ModbusConfHtml.add("Poll Interval:", ModbusBuilder.numberInput(dv+"_Poll", Int32.Parse(parts["Poll"])).print());
            ModbusConfHtml.add("Gateway Enabled:", ModbusBuilder.checkBoxInput(dv+"_Enabled", Boolean.Parse(parts["Enabled"])).print());
            ModbusConfHtml.addT("Advanced Settings");
            ModbusConfHtml.add("Big-Endian Value:", ModbusBuilder.checkBoxInput(dv+"_BigE", Boolean.Parse(parts["BigE"])).print());
            ModbusConfHtml.add("Zero-based Addressing:", ModbusBuilder.checkBoxInput(dv+"_ZeroB", Boolean.Parse(parts["ZeroB"])).print());
            ModbusConfHtml.add("Read/Write Retries:", ModbusBuilder.numberInput(dv+"_RWRetry", Int32.Parse(parts["RWRetry"])).print());
            ModbusConfHtml.add("Read/Write Timeout (ms):", ModbusBuilder.numberInput(dv+"_RWTime", Int32.Parse(parts["RWTime"])).print());
            ModbusConfHtml.add("Delay between each address poll (ms):", ModbusBuilder.numberInput(dv+"_Delay", Int32.Parse(parts["Delay"])).print());
            ModbusConfHtml.add("Register Write Function:", ModbusBuilder.radioButton(dv + "_RegWrite", new string[] { "Write Single Register", "Write Multiple Registers" }, Int32.Parse(parts["RegWrite"])).print());
            stb.Append(ModbusConfHtml.print());
        //    stb.Append(ModbusBuilder.button(dv + "_Done", "Done").print());
            stb.Append("<br><br>"+ModbusBuilder.ShowMesbutton(dv + "_Test", "Test").print());
            stb.Append("<br><br><div id='conMes' style='font-size:130%;     font-weight: bold; display:none; color:red;'></div>");
            return stb.ToString();

        }

        public static void UpdateGateList(int[] ModGates)
        {
            ModbusGates = new List<KeyValuePair<int, string>>();
            foreach (int ModGateID in ModGates)
            {
                Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(ModGateID);
                ModbusGates.Add(new KeyValuePair<int, string>(ModGateID, newDevice.get_Name(Util.hs)));

            }

        }

        public string BuildModbusDeviceTab(int dv1)
        {
           
            getAllGateways();
            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv1);
            var EDO = newDevice.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            string dv = "" + dv1 + "";

            htmlBuilder ModbusBuilder = new htmlBuilder("ModBusDevTab");
            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable();
            //ModbusConfHtml.addDevHeader("Gateway: " + parts["Gateway"]);
           string[] GatewayStringArray = (from kvp in ModbusGates select kvp.Value).ToArray();
            //OK so want the index of the selected gateway in our gateway string array
             KeyValuePair<int, string> Item = ModbusGates.Where(x => x.Key == Convert.ToInt32(parts["GateID"])).First();

            int DefGateway = ModbusGates.IndexOf(Item);
            ModbusConfHtml.add("Modbus Gateway ID: ", ModbusBuilder.selectorInput(GatewayStringArray, dv + "_GateID", "GateID", DefGateway).print());
            ModbusConfHtml.add("Selector Type: ", ModbusBuilder.selectorInput(new string[] { "Discrete Input (RO)", "Coil (RW)", "Input Register (RO)", "Holding Register (RW)" }, dv + "_RegisterType", "RegisterType", Convert.ToInt32(parts["RegisterType"])).print());
            ModbusConfHtml.add("Slave ID: ", ModbusBuilder.numberInput(dv + "_SlaveId", Convert.ToInt32(parts["SlaveId"])).print());
            ModbusConfHtml.add("Register Address: ", "<div style:'display:inline;'><div style='float:left;'>"+ModbusBuilder.numberInput(dv + "_RegisterAddress", Convert.ToInt32(parts["RegisterAddress"])).print()+"</div><div style='float:left;' id='TrueAdd'>()</div></div>");

            string ScratchpadHelpText = "";



            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
            //tells us how many registers to read/write and also how to parse returns
            //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
            //So coil and discrete are bool ONLY
            //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read
            ModbusConfHtml.add("Return Type: ", ModbusBuilder.selectorInput(new string[] { "Boolean", "Int16", "Int32", "Float32", "Int64", "Double64",
            "2 character string","4 character string","6 character string","8 character string"}, dv + "_ReturnType", "RegisterType", Convert.ToInt32(parts["ReturnType"])).print());
                        ModbusConfHtml.add("Signed Value: ", ModbusBuilder.checkBoxInput(dv + "_SignedValue", Boolean.Parse(parts["SignedValue"])).print());
                        ModbusConfHtml.add("Calculator: ", ModbusBuilder.stringInput(dv + "_ScratchpadString", parts["ScratchpadString"]).print());
                        ModbusConfHtml.add("Display Format: ", ModbusBuilder.stringInput(dv + "_DisplayFormatString", parts["DisplayFormatString"]).print());
                        ModbusConfHtml.add("Read Only Device: ", ModbusBuilder.checkBoxInput(dv + "_ReadOnlyDevice", Boolean.Parse(parts["ReadOnlyDevice"])).print());

             
           
            ModbusConfHtml.add("Device Enabled: ", ModbusBuilder.checkBoxInput(dv + "_DeviceEnabled", Boolean.Parse(parts["DeviceEnabled"])).print());
            stb.Append(ModbusConfHtml.print());
  //          stb.Append(ModbusBuilder.button(dv + "_Done", "Done").print());
            stb.Append(@"<script>

OffsetArray=[10000,0,30000,40000];
UpdateTrue=function(){
CalNum=parseInt($('#" + dv + @"_RegisterAddress')[0].value)+OffsetArray[parseInt($('#" + dv + @"_RegisterType')[0].value)];
TrueAdd.textContent='('+CalNum+')';
}
UpdateDisplay=function(){
UpdateTrue();
value=$('#" + dv + @"_RegisterType')[0].value;
$('#" + dv + @"_ReturnType')[0].parentElement.parentElement.style.display='';
$('#" + dv + @"_SignedValue')[0].parentElement.parentElement.style.display='';
$('#" + dv + @"_ScratchpadString')[0].parentElement.parentElement.style.display='';
$('#" + dv + @"_DisplayFormatString')[0].parentElement.parentElement.style.display='';
$('#" + dv + @"_ReadOnlyDevice')[0].parentElement.parentElement.style.display='';


if(value==0){
$('#" + dv + @"_ReturnType')[0].parentElement.parentElement.style.display='none';
$('#" + dv + @"_SignedValue')[0].parentElement.parentElement.style.display='none';
$('#" + dv + @"_ScratchpadString')[0].parentElement.parentElement.style.display='none';
$('#" + dv + @"_DisplayFormatString')[0].parentElement.parentElement.style.display='none';
$('#" + dv + @"_ReadOnlyDevice')[0].parentElement.parentElement.style.display='none';
}
if(value==1){
$('#" + dv + @"_ReturnType')[0].parentElement.parentElement.style.display='none';
$('#" + dv + @"_SignedValue')[0].parentElement.parentElement.style.display='none';
$('#" + dv + @"_ScratchpadString')[0].parentElement.parentElement.style.display='none';
$('#" + dv + @"_DisplayFormatString')[0].parentElement.parentElement.style.display='none';
}
if(value==2){
$('#" + dv + @"_ReadOnlyDevice')[0].parentElement.parentElement.style.display='none';
}
if(value==3){

}
};

$('#" + dv + @"_RegisterType').change(UpdateDisplay);

UpdateDisplay();

$('#" + dv + @"_RegisterAddress').change(UpdateTrue);

                
</script>");



            /*            parts["GateID"] = ""+GatewayID+"";
            parts["Gateway"] = Gateway.get_Name(Util.hs) ;
            parts["RegisterType"] = MosbusAjaxReceivers.modbusDefaultPoll.ToString();
            parts["SlaveId"] = "1"; //get number of slaves from gateway?
            parts["ReturnType"] = "Int16";
            parts["SignedValue"] = "false";
            parts["ScratchpadString"] = "";
            parts["DisplayFormatString"] = "{0}";
            parts["ReadOnlyDevice"] = "true";
            parts["DeviceEnabled"] = "false";
            parts["RegisterAddress"] = "22";*/


            return stb.ToString();

        }




        public string pingGateway(int deviceId)
        {
            Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(deviceId);
            var EDO = Gateway.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            string ip = parts["Gateway"];
          //  int port = Convert.ToInt32(parts["TCP"]);
            //Do check, if good, set to 1, if bad set to 0, if good and disabled set to 2
            if (!Boolean.Parse(parts["Enabled"]))
            {
                Util.hs.SetDeviceValueByRef(deviceId, 2, true);
                return "Gateway is disabled";
            }
            else
            {
                System.Net.NetworkInformation.Ping Ping = new System.Net.NetworkInformation.Ping();
                System.Net.Sockets.TcpClient Socket = new System.Net.Sockets.TcpClient();
                try
                {
                   var Rep= Ping.Send(ip);
                    //  Socket.Connect(ip, port);
                    //  Socket.Close();
                    if (Rep.Status.ToString() == "Success") {
                        Util.hs.SetDeviceValueByRef(deviceId, 1, true);
                        return "Connection Successfull.";
                    }
                    else{
                        Util.hs.SetDeviceValueByRef(deviceId, 0, true);
                        return "Failed connectivty test: "+ Rep.Status.ToString();
                    }
                }
                catch(Exception e)
                {
                    Util.hs.SetDeviceValueByRef(deviceId, 0, true);
                    return "Failed connectivty test: " + e.Message;

                }
      
            }
            return "Gateway cannot be reached";

        }

        public string parseModbusGatewayTab( string data)
        {
            Console.WriteLine("ConfigDevicePost: " + data);

 
            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);
            string partID = changed["id"].Split('_')[1];
            int devId = Int32.Parse(changed["id"].Split('_')[0]);
            if(partID == "Test")
            {
                return(pingGateway(devId).ToString());
            }
            else
            {
                Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devId);
                addSSIDExtraData(newDevice, partID, changed["value"]);
            }
            if(partID== "Poll")
            {
                //Update timer dictionary
                SIID_Page.PluginTimerDictionary[devId].Change(0, Convert.ToInt32(changed["value"]));


            }
            return "True";
         
 

        }


        public void parseModbusDeviceTab(string data)
        {


            Console.WriteLine("ConfigDevicePost: " + data);


            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);
            string partID = changed["id"].Split('_')[1];
            int devId = Int32.Parse(changed["id"].Split('_')[0]);

            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devId);
            //check for gateway change, do something special
            if(partID == "GateID")
            {
                changed["value"] = ""+ModbusGates[Convert.ToInt32(changed["value"])].Key+"";
                changeGateway(newDevice, changed["value"]);
                
            }
            
                addSSIDExtraData(newDevice, partID, changed["value"]);

            //  ModbusTcpMasterReadInputs();


        }

    public  void PollActiveFromGate(object RawID)
        {
            //First, check if device even exits

            int GateID=Convert.ToInt32((int)RawID);
            //Check if gate is active
            Scheduler.Classes.DeviceClass Gateway = null;
            try
            {
                Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GateID);
            }
            catch
            {//If gateway doesn't exist, we need to stop this timer and remove it from our timer dictionary.
                SIID_Page.PluginTimerDictionary[GateID].Dispose();
                SIID_Page.PluginTimerDictionary.Remove(GateID);


            }
            if (Gateway != null)
            {


                var EDO = Gateway.get_PlugExtraData_Get(Util.hs);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                if (bool.Parse(parts["Enabled"]))
                {
                    //Get list of devices
                    //If they're enabled, poll them
                    //Use the time-between and retry things
                    foreach (var subId in parts["LinkedDevices"].Split(','))
                    {
                        try
                        {
                            Scheduler.Classes.DeviceClass MDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(Convert.ToInt32(subId));
                            if (MDevice != null)
                            {
                                var EDO2 = MDevice.get_PlugExtraData_Get(Util.hs);
                                var parts2 = HttpUtility.ParseQueryString(EDO2.GetNamed("SSIDKey").ToString());
                                if (bool.Parse(parts2["DeviceEnabled"])) //Device exists and is enabled
                                {
                                    //Add read write retries here
                                    //Add read/write timeout here
                                    ReadFromModbusDevice(Convert.ToInt32(subId));
                                    ProcessCalculator(Convert.ToInt32(subId));
                                    //add delay between each address poll here

                                }
                            }
                        }
                        catch
                        {

                        }


                    }


                }
            }

        }

        public void FlipBits(ushort[] Input)
        {
           int  index = 0;
            foreach(ushort Item in Input){
                byte[] temp = BitConverter.GetBytes(Item);
                Array.Reverse(temp);
                Input[index] = BitConverter.ToUInt16(temp,0);
                index++;
            }



        }

        public void ProcessCalculator(int devID)
        {
            Scheduler.Classes.DeviceClass ModDev = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devID);
            var EDO = ModDev.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            string ScratchPadString = parts["ScratchpadString"];

            //OK scratchpad rules
            //All variables are numbers
            //The result will be a number, output formatting can happen from the format string stuff and there we can add $ for money or Kw/H or whatever

            //$(DevId) is the Raw value from the device DevId, and can pull from other devices in this way
            //#(DevId) is the Processed value (the result of their scratchpad calculation)
            //Maybe need a special destination thing.  i.e. 'LOL' <- $(141) + #(222) means add the raw for device 141 to the processed 222 and put this as the value of device named LOL
            //I guess it's time to learn RegEx
            //Note that can do a double.Parse(ScratchPadString) once I convert all the number (will not do complicated things like power or squareroots 
            //(Requirements only call for addition,subtraction,multiplication,division,accumulator)
            //accumulator will be special, rest is already built into the double.parse

            //Get set of Devices Raw and Devices processed in the string
            //retrive the raw/processed device values
            //replace the call with the values (Or NAN of whatever if not)
            //parse the expression as a double and save that result as the processed (or save the error string as the processed value)
            List<int> Raws = new List<int>();
            List<int> Processed = new List<int>();
            Match m = Regex.Match(ScratchPadString, @"(\$\()+(\d+)(\))+");
            while (m.Success)
            {
                if (!Raws.Contains(int.Parse(m.Groups[2].ToString())))
                {

                    Raws.Add(int.Parse(m.Groups[2].ToString()));
                }
                m = m.NextMatch();
            }
            m = Regex.Match(ScratchPadString, @"(\#\()+(\d+)(\))+");
            while (m.Success)
            {
                if (!Processed.Contains(int.Parse(m.Groups[2].ToString())))
                {
                    Processed.Add(int.Parse(m.Groups[2].ToString()));
                }
                m = m.NextMatch();
            }
            StringBuilder FinalString = new StringBuilder(ScratchPadString);
            foreach (int dv in Raws)
            {
                Scheduler.Classes.DeviceClass TempDev = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv);
                var TempEDO = ModDev.get_PlugExtraData_Get(Util.hs);
                var Tempparts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                string Rep = Tempparts["RawValue"];
                FinalString.Replace("$(" + dv + ")", Rep);

            }
            foreach (int dv in Processed)
            {
                Scheduler.Classes.DeviceClass TempDev = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv);
                var TempEDO = ModDev.get_PlugExtraData_Get(Util.hs);
                var Tempparts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                string Rep = Tempparts["ProcessedValue"];
                FinalString.Replace("$(" + dv + ")", Rep);
            }
            string OutValue = "NAN";
            try
            {
                if (int.Parse(parts["ReturnType"]) > 5 || int.Parse(parts["ReturnType"]) ==0) //return is a string or a bool
                {
                    OutValue = FinalString.ToString();
                }
                else
                {
                    OutValue = Convert.ToDouble(new DataTable().Compute(FinalString.ToString(), null)).ToString();
                }

            }
            catch { }
        

            addSSIDExtraData(ModDev, "ProcessedValue", OutValue);


            string ValueString = String.Format(parts["DisplayFormatString"], OutValue);
            Util.hs.SetDeviceString(devID,ValueString,true);

     


        }


            public void ReadFromModbusDevice(int devID) //Takes device ID, does a read action on it and puts it in the RawValue component of the extra data
        {
            Scheduler.Classes.DeviceClass ModDev = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devID);
            var EDO = ModDev.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5 = double64, 6=string2,7=string4,8=string6,9=string8
            bool Signed = bool.Parse(parts["SignedValue"]);
            var Returned = OpenModDeviceConnection(devID);
            string RawString = "";
            switch (parts["ReturnType"])
            {
                case ("0"):
                    {
                        RawString = "true";
                        if (Returned[0] == 0){
                            RawString = "false";
                        }

                        break;
                    }
                case ("1"):
                case ("2"):
                case ("4")://Ints
                    {
                        byte[] Bytes = new byte[Returned.Count()*2];
                        int index = 0;
                        foreach (ushort Item in Returned)
                        {
                            byte[] temp = BitConverter.GetBytes(Item);
                            Bytes[index] = temp[0];
                            index++;
                            Bytes[index] = temp[1];
                            index++;

                        }
                        if (Signed)
                        {
                            switch (Returned.Count())
                            {
                                case (1):
                                    {
                                        RawString = BitConverter.ToInt16(Bytes,0).ToString();
                                        break;
                                    }
                                case (2):
                                    {
                                        RawString = BitConverter.ToInt32(Bytes, 0).ToString();
                                        break;
                                    }
                                default:
                                    {
                                        RawString = BitConverter.ToInt64(Bytes, 0).ToString();
                                        break;
                                    }


                            }

                        }
                        else
                        {
                            switch (Returned.Count())
                            {
                                case (1):
                                    {
                                        RawString = BitConverter.ToUInt16(Bytes, 0).ToString();
                                        break;
                                    }
                                case (2):
                                    {
                                        RawString = BitConverter.ToUInt32(Bytes, 0).ToString();
                                        break;
                                    }
                                default:
                                    {
                                        RawString = BitConverter.ToUInt64(Bytes, 0).ToString();
                                        break;
                                    }


                            }
                        }
                    
                        break;
                    }
           
                case ("3"): //float
                    {
                        byte[] Bytes = new byte[Returned.Count() * 2];
                        int index = 0;
                        foreach (ushort Item in Returned)
                        {
                            byte[] temp = BitConverter.GetBytes(Item);
                            Bytes[index] = temp[0];
                            index++;
                            Bytes[index] = temp[1];
                            index++;

                        }
                        RawString = BitConverter.ToSingle(Bytes, 0).ToString();
                        break;
                    }
                case ("5")://double
                    {
                        byte[] Bytes = new byte[Returned.Count() * 2];
                        int index = 0;
                        foreach (ushort Item in Returned)
                        {
                            byte[] temp = BitConverter.GetBytes(Item);
                            Bytes[index] = temp[0];
                            index++;
                            Bytes[index] = temp[1];
                            index++;

                        }
                        RawString = BitConverter.ToDouble(Bytes, 0).ToString();
                        break;
                        break;
                    }
                default:
                    {//string
                        StringBuilder OUT = new StringBuilder();

                        foreach (ushort Item in Returned)
                        {
                            byte[] temp = BitConverter.GetBytes(Item);
                            Array.Reverse(temp);
                            OUT.Append(System.Text.Encoding.Default.GetString(temp));


                        }
                        RawString = OUT.ToString();
                        break;
                    }
                  


            }

           // parts["RawValue"] = RawString;
            //  parts["ProcessedValue"] = "0";
            addSSIDExtraData(ModDev, "RawValue", RawString);
           

        }

        public ushort[] OpenModDeviceConnection(int devID, ushort[] Data=null) //General modbus read/write function.
        {
            Scheduler.Classes.DeviceClass ModDev = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devID);
            var EDO = ModDev.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            int GatewayID = Int32.Parse(parts["GateID"]);
            Scheduler.Classes.DeviceClass Gatrway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GatewayID);
            var EDOGate = Gatrway.get_PlugExtraData_Get(Util.hs);
            var partsGate = HttpUtility.ParseQueryString(EDOGate.GetNamed("SSIDKey").ToString());

            bool flipbits = bool.Parse(partsGate["BigE"]);

            int Offset = 0;
            if(bool.Parse(partsGate["ZeroB"])){
                Offset = -1;
            }
            ushort[] OffsetArray =new ushort[] { 10000, 0, 30000, 40000 };

            // int Taddress = Int32.Parse(parts["RegisterAddress"]) + Offset + OffsetArray[Int32.Parse(parts["RegisterType"])];
            // ushort startAddress = (ushort)Math.Max(0, Taddress);
            int Taddress = Int32.Parse(parts["RegisterAddress"]) + Offset ;
            ushort startAddress = (ushort)Math.Max(0, Taddress);

            //Use bigE to reverse or not the retuned bytes before passing them up to wherever they're going, (or down in case of write)

            ushort[] NumRegArrray =new ushort[] { 1, 1, 2, 2, 4, 4,1, 2, 3, 4 };
            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=double64, 6=string2,7=string4,8=string6,9=string8
            //tells us how many registers to read/write and also how to parse returns
            //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
            //So coil and discrete are bool ONLY
            //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read

            ushort[] Return = null;
            ushort numInputs = NumRegArrray[Int32.Parse(parts["ReturnType"])]; //Should hava a check if Data is not null to see that it's appropriate to write.
            try
            {
                using (TcpClient client = new TcpClient(partsGate["Gateway"], Int32.Parse(partsGate["TCP"])))
                {
                    ModbusIpMaster master = ModbusIpMaster.CreateIp(client);
                    master.Transport.ReadTimeout = Int32.Parse(partsGate["RWTime"]);
                    master.Transport.Retries = Int32.Parse(partsGate["RWRetry"]);
                    master.Transport.WaitToRetryMilliseconds = Int32.Parse(partsGate["Delay"]);
                    if (Data==null)
                    {
                        if (Int32.Parse(parts["RegisterType"]) < 3) //It's a coil or Discrete Input
                        {
                            bool Returned = master.ReadCoils(startAddress, 1)[0];
                            if (Returned)
                                Return = new ushort[] { 1 };
                            else
                                Return =new ushort[] { 0 };

                        }
                        else
                        {
                            Return = master.ReadHoldingRegisters(startAddress, numInputs);
                            if (flipbits)
                            { //Note according to blog here: https://ctlsys.com/common_modbus_protocol_misconceptions/
                              //Each register is in Big Endian and the little endienness is the order we read the registers
                              //so::
                                Return.Reverse();
                                // FlipBits(Return);//may not just be flip bits, may be flip the array also
                                //IDEA is we have a returned array
                                /*
                                 * [B1B2]
                                 * [B3B4]
                                 * [B5B6] 
                                 * [B7B8]
                                 * 
                                 * So each register is 2 bytes is 16 bits
                                 * 
                                 * Depenging on our datatype, we want to parse these 4 registers as a single thing
                                 */
                            }
                        }
                        return Return;
    

                }
                    else
                    {

                        if (Int32.Parse(parts["RegisterType"]) < 3) //It's a coil or Discrete Input
                        {
                            bool Send = true;
                            if (Data[0] == 0)
                            {
                                Send = false;
                            }
                            master.WriteSingleCoil(startAddress, Send);
                        }
                        else
                        {
                            if (flipbits)
                            {
                                Data.Reverse();
                                //FlipBits(Data);
                            }
                            master.WriteMultipleRegisters(startAddress, Data);

                        }
                        return new ushort[] { 1 };

                    }

                }
            }
            catch(Exception e)
            {
                return new ushort[] { 0 };

            }
                

            


        
        }
        /* var parts = HttpUtility.ParseQueryString(string.Empty);


            parts["Type"] = "Modbus Gateway";
            parts["Gateway"] = "";
            parts["TCP"] = "502";
            parts["Poll"] = MosbusAjaxReceivers.modbusDefaultPoll.ToString();
            parts["Enabled"] = "false";
            parts["BigE"] = "false";
            parts["ZeroB"] = "true";
            parts["RWRetry"] = "2";
            parts["RWTime"] = "1000";
            parts["Delay"] = "0";
            parts["RegWrite"] = "1";
            parts["LinkedDevices"] = "";
            return parts.ToString();

        }
        public string makeNewModbusDevice(int GatewayID)
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);
            Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GatewayID); //Should keep in gateway a list of devices


            var EDO = Gateway.get_PlugExtraData_Get(Util.hs);
            var GParts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            parts["Type"] = "Modbus Device";
            parts["GateID"] = "" + GatewayID + "";
            parts["Gateway"] = Gateway.get_Name(Util.hs);
            parts["RegisterType"] = "0";//MosbusAjaxReceivers.modbusDefaultPoll.ToString(); //0 is discrete input, 1 is coil, 2 is InputRegister, 3 is Holding Register
            parts["SlaveId"] = "1"; //get number of slaves from gateway?
            parts["ReturnType"] = "0";//0 = Int16, 1=Int32,2=Float32,3=Int64,4=Bool
            parts["SignedValue"] = "false";
            parts["ScratchpadString"] = "";
            parts["DisplayFormatString"] = "{0}";
            parts["ReadOnlyDevice"] = "true";
            parts["DeviceEnabled"] = "false";
            parts["RegisterAddress"] = "1";
            return parts.ToString();*/

        public static void ModbusTcpMasterReadInputs()
        {
           /* StringBuilder Printout = new StringBuilder();
            using (TcpClient client = new TcpClient("129.82.36.221", 502))
            {
                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);
                
                // read five input values
                ushort startAddress = 20021;
                ushort numInputs = 32;
                ushort[] inputs = master.ReadHoldingRegisters(startAddress, numInputs);
         
              
                for (int i = 0; i < numInputs; i++)
                {
                   Printout.Append("Input " + inputs[i]);
                    inputs[i] = (ushort)i;
                }

                master.WriteMultipleRegisters(startAddress-1, inputs);
             //   master.ReadWriteMultipleRegisters(startAddress, numInputs, startAddress, inputs);
                 inputs = master.ReadHoldingRegisters(startAddress, numInputs);


                for (int i = 0; i < numInputs; i++)
                {
                    Printout.Append("Input " + inputs[i]);
                  
                }
            }
            string a = Printout.ToString();
            int b = 1;
            // output: 
            // Input 100=0
            // Input 101=0
            // Input 102=0
            // Input 103=0
            // Input 104=0*/
        }




    }
}
