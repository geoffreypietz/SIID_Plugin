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
            parts["ReturnType"] = "0";
            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
            //tells us how many registers to read/write and also how to parse returns
            //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
            //So coil and discrete are bool ONLY
            //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read
            parts["SignedValue"] = "false";
            parts["ScratchpadString"] = "";
            parts["DisplayFormatString"] = "{0}";
            parts["ReadOnlyDevice"] = "true";
            parts["DeviceEnabled"] = "false";
            parts["RegisterAddress"] = "1";
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

            EDO.AddNamed("SSIDKey", makeNewModbusDevice(Convert.ToInt32(GatewayID)));
            newDevice.set_PlugExtraData_Set(Util.hs, EDO);

            AddDeviceToGateway(dv, Convert.ToInt32(GatewayID));

            // newDevice.set_Device_Type_String(Util.hs, makeNewModbusGateway());




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





            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
            //tells us how many registers to read/write and also how to parse returns
            //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
            //So coil and discrete are bool ONLY
            //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read
            ModbusConfHtml.add("Return Type: ", ModbusBuilder.selectorInput(new string[] { "Boolean", "Int16", "Int32", "Float32", "Int64",
            "2 character string","4 character string","6 character string","8 character string"}, dv + "_ReturnType", "RegisterType", Convert.ToInt32(parts["ReturnType"])).print());
                        ModbusConfHtml.add("Signed Value: ", ModbusBuilder.checkBoxInput(dv + "_SignedValue", Boolean.Parse(parts["SignedValue"])).print());
                        ModbusConfHtml.add("Scratch Pad: ", ModbusBuilder.stringInput(dv + "_ScratchpadString", parts["ScratchpadString"]).print());
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

            ModbusTcpMasterReadInputs();


        }


  //need to make a read, and a write function

        public uint[] OpenModDeviceConnection(int devID, bool Read=true, uint[] Data=null)
        {
            Scheduler.Classes.DeviceClass ModDev = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devID);
            var EDO = ModDev.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            int GatewayID = Int32.Parse(parts["GateID"]);
            Scheduler.Classes.DeviceClass Gatrway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GatewayID);
            var EDOGate = Gatrway.get_PlugExtraData_Get(Util.hs);
            var partsGate = HttpUtility.ParseQueryString(EDOGate.GetNamed("SSIDKey").ToString());

            
            int Offset = 0;
            if(bool.Parse(partsGate["ZeroB"])){
                Offset = -1;
            }
            OffsetArray =[10000, 0, 30000, 40000];

            int StartAddress = Int32.Parse(parts["RegisterAddress"]) + Offset + OffsetArray[Int32.Parse(parts["RegisterType"])];
            //Use bigE to reverse or not the retuned bytes before passing them up to wherever they're going, (or down in case of write)

            NumRegArrray =[1, 1, 2, 2, 4, 1, 2, 3, 4];
            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
            //tells us how many registers to read/write and also how to parse returns
            //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
            //So coil and discrete are bool ONLY
            //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read

            uint[] Return = null;
            ushort numInputs = NumRegArrray[Int32.Parse(parts["ReturnType"])]; //Should hava a check if Data is not null to see that it's appropriate to write.
            using (TcpClient client = new TcpClient(partsGate["Gateway"], Int32.Parse(partsGate["TCP"])))
            {
                ModbusIpMaster master = ModbusIpMaster.CreateIp(client);
                master.Transport.ReadTimeout = Int32.Parse(parts["RWTime"]);
                master.Transport.Retries = Int32.Parse(parts["RWRetry"]);
                master.Transport.WaitToRetryMilliseconds = Int32.Parse(parts["Delay"]);
                if (Read)
                {
                    if (Int32.Parse(parts["RegisterType"]) > 3) //It's a coil or Discrete Input
                    {
                        Return = master.ReadCoils(startAddress, numInputs);
                    }
                    else
                    {
                        Return = master.ReadHoldingRegisters(startAddress, numInputs);
                    }
                    
                }
                else
                {
                    if (Int32.Parse(parts["RegisterType"]) > 3) //It's a coil or Discrete Input
                    {

                        Return = master.WriteSingleCoil(startAddress, (bool)Data[0]);
                    }
                    else
                    {
                        Return = master.WriteMultipleRegisters(startAddress, Data);
                       
                    }

                }

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
            StringBuilder Printout = new StringBuilder();
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
            // Input 104=0
        }




    }
}