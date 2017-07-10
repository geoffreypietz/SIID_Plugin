using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler;
using System.Web;
using HomeSeerAPI;
using Modbus.Device;
using System.Net.Sockets;
//using System.Data;
using HSPI_SIID.General;

namespace HSPI_SIID.Modbus
{


    public class ModbusDevicePage : PageBuilderAndMenu.clsPageBuilder

    {
        public InstanceHolder Instance { get; set; }
       

        public static HashSet<string> WhoIsBeingPolled = new HashSet<string>();
        public static HashSet<string> WhoIsBeingWritten = new HashSet<string>();


        public ModbusDevicePage(string pagename, InstanceHolder instance) : base(pagename)
        {
         
            Instance = instance;
            ModbusBuilder = new htmlBuilder("ModBusGatewayPage" + Instance.ajaxName);

          //  UpdateGateList(getAllGateways());

        }

        htmlBuilder ModbusBuilder { get; set; }
        public static List<SiidDevice> ModbusGates { get; set; }


        public Tuple<List<SiidDevice>, List<SiidDevice>> getAllGateways()
        {
           
            SiidDevice.Update(Instance);
           ModbusGates = new List<SiidDevice>();
            List<SiidDevice>  Orphaned = new List<SiidDevice>();
            foreach (var Siid in Instance.Devices)
            {
                var EDO = Siid.Extra;
                try
                {
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    string s = parts["Type"];
                    if (parts["Type"] == "Modbus Gateway")
                    {
                        ModbusGates.Add(Siid);

                    }
                    if ((parts["Type"] == "Modbus Device") && (parts["GateID"] == "0"))
                    {
                        Orphaned.Add(Siid);
                    }
                }
                catch { }
            }
            return new Tuple<List<SiidDevice>, List<SiidDevice>>(ModbusGates,Orphaned);
             

            
        }

        public string[] RegTypeArray = new string[] { "Discrete Input (RO)", "Coil (RW)", "Input Register (RO)", "Holding Register (RW)" };
        public string[] RetTypeArray = new string[] { "Boolean", "Int16", "Int32", "Float32", "Int64", "Double64",
            "2 character string","4 character string","6 character string","8 character string"};
public string GetReg(string instring)
        {
            try
            {
                return RegTypeArray[Convert.ToInt32(instring)];
            }
            catch
            {
                return instring;
            }

        }
        public string GetRet(string instring)
        {
            try
            {
                return RetTypeArray[Convert.ToInt32(instring)];

            }
            catch
            {
                return instring;
            }

        }


        public string makeNewModbusGateway()
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);


            parts["Type"] = "Modbus Gateway";
            parts["Gateway"] = "";
            parts["TCP"] = "502";
            parts["Poll"] = Instance.modbusDefaultPoll.ToString();
            parts["Enabled"] = "false";
            parts["BigE"] = "false";
            parts["RevByte"] = "false";
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
            SiidDevice GateWay = SiidDevice.GetFromListByID(Instance.Devices, GatewayID);
            if (GateWay != null)
            {
                Scheduler.Classes.DeviceClass Gateway = GateWay.Device;



                var EDO = GateWay.Extra;
                var GParts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

                parts["Type"] = "Modbus Device";
                parts["GateID"] = "" + GatewayID + "";
                parts["Gateway"] = Gateway.get_Name(Instance.host);
                parts["RegisterType"] = "0";//MosbusAjaxReceivers.modbusDefaultPoll.ToString(); //0 is discrete input, 1 is coil, 2 is InputRegister, 3 is Holding Register
                parts["SlaveId"] = "1"; //get number of slaves from gateway?
                parts["ReturnType"] = "0";
                //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
                //tells us how many registers to read/write and also how to parse returns
                //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
                //So coil and discrete are bool ONLY
                //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read
                parts["SignedValue"] = "false";
                parts["ScratchpadString"] = "$(" + DeviceID + ")";
                parts["DisplayFormatString"] = "{0}";
                parts["ReadOnlyDevice"] = "true";
                parts["DeviceEnabled"] = "false";
                parts["RegisterAddress"] = "1";
                parts["RawValue"] = "0";
                parts["ProcessedValue"] = "0";
                return parts.ToString();
            }
            return "";
            //uint is unsigned int,
        }


      /*  public void addSSIDExtraData(Scheduler.Classes.DeviceClass Device, string Key, string value)
        {


            var EDO = Device.get_PlugExtraData_Get(Instance.host);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            parts[Key] = value;
            EDO.RemoveNamed("SSIDKey");
            EDO.AddNamed("SSIDKey", parts.ToString());
            Device.set_PlugExtraData_Set(Instance.host, EDO);

        }*/

        public void RemoveDeviceFromGateway(int DeviceId, int GatewayId)
        {
            SiidDevice GateWay = SiidDevice.GetFromListByID(Instance.Devices, GatewayId);
            if (GateWay != null)
            {

                Scheduler.Classes.DeviceClass Gateway = GateWay.Device; //Should keep in gateway a list of devices
                var EDO = GateWay.Extra;
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                String[] PLIST = parts["LinkedDevices"].Split(',');
                StringBuilder NL = new StringBuilder();

                foreach (string P in PLIST)
                {
                    if ((!String.IsNullOrEmpty(P)) && (P != "" + DeviceId + ""))
                    {
                        NL.Append(P + ",");

                    }
                }
                GateWay.UpdateExtraData("LinkedDevices", NL.ToString());

                Gateway.AssociatedDevice_Remove(Instance.host, DeviceId);

            }
        }
        public void AddDeviceToGateway(int DeviceId, int GatewayId)
        {
            SiidDevice GateWay = SiidDevice.GetFromListByID(Instance.Devices, GatewayId);
            if (GateWay != null)
            {
                Scheduler.Classes.DeviceClass Gateway = GateWay.Device; //Should keep in gateway a list of devices
                var EDO = GateWay.Extra;
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                parts["LinkedDevices"] += DeviceId + ",";
                GateWay.UpdateExtraData("LinkedDevices", parts["LinkedDevices"]);
                Gateway.AssociatedDevice_Add(Instance.host, DeviceId);
            }

            

        }
        public void changeGateway(SiidDevice Device, string newGatewayId)
        {

            var EDO = Device.Extra;
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            string OldGatewayID = parts["GateID"];
            if (OldGatewayID != "0")
            {
                RemoveDeviceFromGateway(Device.Ref, Convert.ToInt32(OldGatewayID));
            }
            if (newGatewayId != "0")
            {
                AddDeviceToGateway(Device.Ref, Convert.ToInt32(newGatewayId));
                Scheduler.Classes.DeviceClass Gateway = SiidDevice.GetFromListByID(Instance.Devices, Convert.ToInt32(newGatewayId)).Device;
                Device.UpdateExtraData("Gateway", Gateway.get_Name(Instance.host));
            }
            else
            {
                
                Device.UpdateExtraData("Gateway", "none");
            }
           



        }

        public string MakeSubDeviceRedirect(string pageName, string user, int userRights, string GatewayID)
        {

            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;
            GatewayID = GatewayID.Split("_".ToCharArray())[1];

            var dv = Instance.host.NewDeviceRef("Modbus Device");
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);
            newDevice.set_Name(Instance.host, "Modbus Device " + dv);
            newDevice.set_Location2(Instance.host, "Modbus");
            newDevice.set_Location(Instance.host, "System");
            //newDevice.set_Interface(Instance.host, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function
            //newDevice.set_InterfaceInstance()''  SET INTERFACE INSTANCE
            newDevice.set_Relationship(Instance.host, Enums.eRelationship.Child);
            newDevice.set_Status_Support(Instance.host, true);
            newDevice.MISC_Set(Instance.host, Enums.dvMISC.NO_LOG);
            newDevice.MISC_Set(Instance.host, Enums.dvMISC.SHOW_VALUES);
         



            HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();

            // EDO = newDevice.get_PlugExtraData_Get(Instance.host);

            EDO.AddNamed("SSIDKey", makeNewModbusDevice(Convert.ToInt32(GatewayID),dv));
            newDevice.set_PlugExtraData_Set(Instance.host, EDO);

            AddDeviceToGateway(dv, Convert.ToInt32(GatewayID));

            // newDevice.set_Device_Type_String(Instance.host, makeNewModbusGateway());

            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


            newDevice.set_DeviceType_Set(Instance.host, DevINFO);

            MakeSubDeviceGraphicsAndStatus(dv);
            Instance.Devices.Add(new SiidDevice(Instance, newDevice));

            stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='/deviceutility?ref=" + dv + "&edit=1'\" />");
            //    stb.Append("<a id = 'LALA' href='/deviceutility?ref=" + dv + "&edit=1'/><script>LALA.click()</script> ");
            page.AddBody(stb.ToString());
            return page.BuildPage();
        }


        public void MakeSubDeviceGraphicsAndStatus(int dv)
        {
            var Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/HomeSeer/status/off.gif";
            Graphic.Set_Value = 0;
            Instance.host.DeviceVGP_AddPair(dv, Graphic);

            Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/HomeSeer/status/green.png";
            Graphic.Set_Value = 1;
            Instance.host.DeviceVGP_AddPair(dv, Graphic);

            Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/HomeSeer/status/yellow.png";
            Graphic.Set_Value = 2;
            Instance.host.DeviceVGP_AddPair(dv, Graphic);

            Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/HomeSeer/status/red.png";
            Graphic.Set_Value = 3;
            Instance.host.DeviceVGP_AddPair(dv, Graphic);
        }

        public void MakeGatewayGraphicsAndStatus(int dv)
        {
            VSVGPairs.VSPair Unreachable = new VSVGPairs.VSPair(ePairStatusControl.Status);
            Unreachable.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Unreachable.Render = Enums.CAPIControlType.TextBox_String;
            Unreachable.Value = 0;
            Unreachable.Status = "Unreachable";
       



            Instance.host.DeviceVSP_AddPair(dv, Unreachable);

            Unreachable = new VSVGPairs.VSPair(ePairStatusControl.Status);
            Unreachable.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Unreachable.Render = Enums.CAPIControlType.TextBox_String;
            Unreachable.Value = 1;
            Unreachable.Status = "Available";
        
          

            Instance.host.DeviceVSP_AddPair(dv, Unreachable);

      

            Unreachable = new VSVGPairs.VSPair(ePairStatusControl.Status);
            Unreachable.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Unreachable.Render = Enums.CAPIControlType.TextBox_String;
            Unreachable.Value = 2;
            Unreachable.Status = "Disabled";



            Instance.host.DeviceVSP_AddPair(dv, Unreachable);

            var Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/HomeSeer/status/red.png";
            Graphic.Set_Value=0;

            Instance.host.DeviceVGP_AddPair(dv, Graphic);
            

             Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/HomeSeer/status/green.png";
            Graphic.Set_Value = 1;

            Instance.host.DeviceVGP_AddPair(dv, Graphic);
       

             Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/HomeSeer/status/yellow.png";
            Graphic.Set_Value = 2;

            Instance.host.DeviceVGP_AddPair(dv, Graphic);


        }
        public string MakeGatewayRedirect(string pageName, string user, int userRights, string queryString)
        {
            
            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;


            var dv = Instance.host.NewDeviceRef("Modbus IP Gateway");
        
            MakeGatewayGraphicsAndStatus(dv);
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);
            newDevice.set_Name(Instance.host, "Modbus IP Gateway " + dv);
            newDevice.set_Location2(Instance.host, "Modbus");
            newDevice.set_Location(Instance.host, "System");
            //newDevice.set_Interface(Instance.host, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
           newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function
  //newDevice.set_InterfaceInstance()''  SET INTERFACE INSTANCE
            newDevice.set_Relationship(Instance.host, Enums.eRelationship.Parent_Root);

            newDevice.MISC_Set(Instance.host, Enums.dvMISC.NO_LOG);
            newDevice.MISC_Set(Instance.host, Enums.dvMISC.SHOW_VALUES);
            HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();
            
           // EDO = newDevice.get_PlugExtraData_Get(Instance.host);

            EDO.AddNamed("SSIDKey", makeNewModbusGateway());
            newDevice.set_PlugExtraData_Set(Instance.host, EDO);
         

            // newDevice.set_Device_Type_String(Instance.host, makeNewModbusGateway());
          var DevINFO=  new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


            newDevice.set_DeviceType_Set(Instance.host, DevINFO);



       /*     var DeviceValue = new VSVGPairs.VSPair(ePairStatusControl.Status);
            DeviceValue.PairType = VSVGPairs.VSVGPairType.SingleValue;
            DeviceValue.Render = Enums.CAPIControlType.TextBox_String;
            DeviceValue.Value = 0;
            DeviceValue.Status = "Unknown";
            
            Instance.host.DeviceVSP_AddPair(dv, DeviceValue);*/


            stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='/deviceutility?ref=" + dv + "&edit=1'\" />");
            //    stb.Append("<a id = 'LALA' href='/deviceutility?ref=" + dv + "&edit=1'/><script>LALA.click()</script> ");

            Instance.Devices.Add(new SiidDevice(Instance, newDevice));
         
            page.AddBody(stb.ToString());
            return page.BuildPage();
        }

        public string BuildModbusGatewayTab(int dv1)
        {//Need to pull from device associated modbus information. Need to create when new device is made
           var NewDevice = SiidDevice.GetFromListByID(Instance.Devices, dv1);
            Scheduler.Classes.DeviceClass newDevice = NewDevice.Device;
           var EDO = NewDevice.Extra;
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            foreach (String GateAttribute in ModbusMaster.Attributes)
            {
                if (parts[GateAttribute] == null)
                {
                    parts[GateAttribute] = "False";
                }

            }

            string dv = "" + dv1 + "";

            bool Enabled = false;
            bool RevOrd = false;
            bool RevByt = false;
            bool ZeroByt = false;
            try
            {
                 Enabled = Boolean.Parse(parts["Enabled"]);
            }
            catch (Exception)
            {

                
            }

            try
            {
                 RevOrd = Boolean.Parse(parts["BigE"]);
            }
            catch (Exception)
            {

               
            }
            try
            {
                 RevByt = Boolean.Parse(parts["RevByte"]);
            }
            catch (Exception)
            {

             
            }
            try
            {
                 ZeroByt = Boolean.Parse(parts["ZeroB"]);
            }
            catch (Exception)
            {

            }

          
           

            StringBuilder stb = new StringBuilder();
            htmlBuilder ModbusBuilder = new htmlBuilder("ModBusGateTab" + Instance.ajaxName);
            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable();
            ModbusConfHtml.addT("Gateway Settings");
            ModbusConfHtml.add("Modbus Gateway hostname or IP address: ", ModbusBuilder.stringInput(dv+"_Gateway", parts["Gateway"]).print());
            ModbusConfHtml.add("TCP Port:", ModbusBuilder.numberInput(dv+"_TCP", Int32.Parse(parts["TCP"])).print());
            ModbusConfHtml.add("Poll Interval:", ModbusBuilder.numberInput(dv+"_Poll", Int32.Parse(parts["Poll"])).print());
            ModbusConfHtml.add("Gateway Enabled:", ModbusBuilder.checkBoxInput(dv+"_Enabled", Enabled).print());
            ModbusConfHtml.addT("Advanced Settings");
            ModbusConfHtml.add("Reverse Register word order:", ModbusBuilder.checkBoxInput(dv+"_BigE",RevOrd).print());
            ModbusConfHtml.add("Reverse word byte order:", ModbusBuilder.checkBoxInput(dv + "_RevByte", RevByt).print());
            ModbusConfHtml.add("Zero-based Addressing:", ModbusBuilder.checkBoxInput(dv+"_ZeroB", ZeroByt).print());
            ModbusConfHtml.add("Read/Write Retries:", ModbusBuilder.numberInput(dv+"_RWRetry", Int32.Parse(parts["RWRetry"])).print());
            ModbusConfHtml.add("Read/Write Timeout (ms):", ModbusBuilder.numberInput(dv+"_RWTime", Int32.Parse(parts["RWTime"])).print());
            ModbusConfHtml.add("Delay between each address poll (ms):", ModbusBuilder.numberInput(dv+"_Delay", Int32.Parse(parts["Delay"])).print());
        //    ModbusConfHtml.add("Register Write Function:", ModbusBuilder.radioButton(dv + "_RegWrite", new string[] { "Write Single Register", "Write Multiple Registers" }, Int32.Parse(parts["RegWrite"])).print());
            stb.Append(ModbusConfHtml.print());
        //    stb.Append(ModbusBuilder.button(dv + "_Done", "Done").print());
            stb.Append("<br><br>"+ModbusBuilder.ShowMesbutton(dv + "_Test", "Test").print());
            stb.Append("<br><br><div id='conMes' style='font-size:130%;     font-weight: bold; display:none; color:red;'></div>");
            return stb.ToString();

        }


        public string BuildModbusDeviceTab(int dv1)
        {
           
            getAllGateways();
            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;
            var NewDevice = SiidDevice.GetFromListByID(Instance.Devices, dv1);
            Scheduler.Classes.DeviceClass newDevice = NewDevice.Device;
            var EDO = NewDevice.Extra;
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            string dv = "" + dv1 + "";

            htmlBuilder ModbusBuilder = new htmlBuilder("ModBusDevTab" + Instance.ajaxName);
            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable();
            //ModbusConfHtml.addDevHeader("Gateway: " + parts["Gateway"]);

           string[] Arrs = (from kvp in ModbusGates select kvp.Device.get_Name(Instance.host)).ToArray();
            string[] GatewayStringArray = new string[Arrs.Length + 1];
            GatewayStringArray[0] = "None";                                // set the prepended value
            Array.Copy(Arrs, 0, GatewayStringArray, 1, Arrs.Length);

            //OK so want the index of the selected gateway in our gateway string array
            int DefGateway = 0;
            if (parts["GateID"] != "0")
            {
                SiidDevice Item = ModbusGates.Where(x => x.Ref == Convert.ToInt32(parts["GateID"])).First();
                DefGateway = ModbusGates.IndexOf(Item)+1;
            }

     
            ModbusConfHtml.add("Modbus Gateway ID: ", ModbusBuilder.selectorInput(GatewayStringArray, dv + "_GateID", "GateID", DefGateway).print());
            ModbusConfHtml.add("Selector Type: ", ModbusBuilder.selectorInput(RegTypeArray, dv + "_RegisterType", "RegisterType", Convert.ToInt32(parts["RegisterType"])).print());
            ModbusConfHtml.add("Slave ID: ", ModbusBuilder.numberInput(dv + "_SlaveId", Convert.ToInt32(parts["SlaveId"])).print());
            ModbusConfHtml.add("Register Address: ", "<div style:'display:inline;'><div style='float:left;'>"+ModbusBuilder.numberInput(dv + "_RegisterAddress", Convert.ToInt32(parts["RegisterAddress"])).print()+"</div><div style='float:left;' id='TrueAdd'>()</div></div>");

           



            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
            //tells us how many registers to read/write and also how to parse returns
            //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
            //So coil and discrete are bool ONLY
            //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read
            ModbusConfHtml.add("Return Type: ", ModbusBuilder.selectorInput(RetTypeArray, dv + "_ReturnType", "RegisterType", Convert.ToInt32(parts["ReturnType"])).print());
                        ModbusConfHtml.add("Signed Value: ", ModbusBuilder.checkBoxInput(dv + "_SignedValue", Boolean.Parse(parts["SignedValue"])).print());
                        ModbusConfHtml.add(" Calculator: ", "<div style: 'display:inline;'><div style = 'float:left;'> "+ModbusBuilder.stringInput(dv + "_ScratchpadString", parts["ScratchpadString"]).print() + "</div><div style='float:left;' id='HelpText'>$(DeviceID) is the raw value for the homeseer device with ID DeviceID. #(DeviceID) is the value resulting from the device's calculator. Any SIID device's value can be called here.</div>");
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
            parts["Gateway"] = Gateway.get_Name(Instance.host) ;
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
     
          
           
            //Do check, if good, set to 1, if bad set to 0, if good and disabled set to 2
       
              //  System.Net.NetworkInformation.Ping Ping = new System.Net.NetworkInformation.Ping();
               
                try
                {
                    var NewDevice = SiidDevice.GetFromListByID(Instance.Devices, deviceId);
                    Scheduler.Classes.DeviceClass Gateway = NewDevice.Device;
                    var EDO = NewDevice.Extra;
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    string ip = parts["Gateway"];
                    int port = Convert.ToInt32(parts["TCP"]);
                    System.Net.Sockets.TcpClient Socket = new System.Net.Sockets.TcpClient(ip, port);
                    /*
                   var Rep= Ping.Send(ip);
                    //  Socket.Connect(ip, port);
                    //  Socket.Close();
                    if (Rep.Status.ToString() == "Success") {
                        Instance.host.SetDeviceValueByRef(deviceId, 1, true);
                        return "Connection Successfull.";
                    }
                    else{
                        Instance.host.SetDeviceValueByRef(deviceId, 0, true);
                        return "Failed connectivty test: "+ Rep.Status.ToString();
                    }*/
                    if (Socket.Connected)
                    {
                        Instance.host.SetDeviceValueByRef(deviceId, 1, true);
                        return "Connection Successfull.";
                    }
                    else
                    {
                        Instance.host.SetDeviceValueByRef(deviceId, 0, true);
                        return "Failed connectivty test: ";
                    }
                }
                catch(Exception e)
                {
                    Instance.host.SetDeviceValueByRef(deviceId, 0, true);
                    return "Failed connectivty test: " + e.Message;

                }
      
            
          

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
                var NewDevice = SiidDevice.GetFromListByID(Instance.Devices, devId);
                NewDevice.UpdateExtraData(partID, changed["value"]);
        
            }
            if(partID== "Poll")
            {
                //Update timer dictionary
                int PollVal = Math.Max(Convert.ToInt32(changed["value"]), 10000);
                if (SIID_Page.PluginTimerDictionary.ContainsKey(devId)){
                    SIID_Page.PluginTimerDictionary[devId].Change(0, PollVal);
                }
                else
                {
                    var NewDevice = SiidDevice.GetFromListByID(Instance.Devices, devId);
                    System.Threading.Timer GateTimer = new System.Threading.Timer(PollActiveFromGate, NewDevice, 100000, Convert.ToInt32(changed["value"]));
                    SIID_Page.PluginTimerDictionary[devId] = GateTimer;
                }


            }
            return "True";
         
 

        }


        public string parseModbusDeviceTab(string data)
        {


            Console.WriteLine("ConfigDevicePost: " + data);


            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);
            string partID = changed["id"].Split('_')[1];
            int devId = Int32.Parse(changed["id"].Split('_')[0]);

            var NewDevice = SiidDevice.GetFromListByID(Instance.Devices, devId);
        
            Scheduler.Classes.DeviceClass newDevice = NewDevice.Device;
            //check for gateway change, do something special
            if(partID == "GateID")
            {
                if (Convert.ToInt32(changed["value"]) > 0)
                {
                    changed["value"] = "" + ModbusGates[(Convert.ToInt32(changed["value"]) - 1)].Ref + "";
                }
                else
                {
                    changed["value"] = "0";
                }
                changeGateway(NewDevice, changed["value"]);
                
            }
            if (partID == "ScratchpadString")
            {
                changed["value"] = changed["value"].Replace(" ", "%2B");

            }
            NewDevice.UpdateExtraData(partID, changed["value"]);
            Instance.host.SetDeviceValueByRef(devId, 0, false);
            //  ModbusTcpMasterReadInputs();

            return "True";
        }
      public static System.Threading.Mutex OneAtATime = new System.Threading.Mutex();


       



        public  void PollActiveFromGate(object RawID)
        {
            //First, check if device even exits
        
            SiidDevice Gate =(SiidDevice)RawID;
            if (WhoIsBeingPolled.Contains(Gate.Ref.ToString())){
                return;

            }
            else {
                lock (WhoIsBeingPolled) //keep subdevice level locks
                {
                    WhoIsBeingPolled.Add(Gate.Ref.ToString());
                }
                //Check if gate is active
                try {
                Scheduler.Classes.DeviceClass Gateway = null;
                try
                {
                    Gate.Device.get_Ref(Instance.host);
                    Gateway = Gate.Device;

                }
                catch
                {//If gateway doesn't exist, we need to stop this timer and remove it from our timer dictionary.
                    SIID_Page.PluginTimerDictionary[Gate.Ref].Dispose();
                    SIID_Page.PluginTimerDictionary.Remove(Gate.Ref);


                }
                if (Gateway != null)
                {


                    var EDO = Gate.Extra;
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    if (bool.Parse(parts["Enabled"]))
                    {
                        Console.WriteLine("Polling Gateway: " + Gate.Ref);

                        //Get list of devices
                        //If they're enabled, poll them
                        //Use the time-between and retry things

                        List<string> GatewaysStatus = new List<string>();
                        foreach (var subId in parts["LinkedDevices"].Split(','))
                        {
                            try
                            {
                                SiidDevice Subset = SiidDevice.GetFromListByID(Instance.Devices, Convert.ToInt32(subId));
                                //IDEA, store in a list all the registers we want to poll
                                //then grab all of them from a single modbus connection
                                var EDO2 = Subset.Extra;
                                var parts2 = HttpUtility.ParseQueryString(EDO2.GetNamed("SSIDKey").ToString());
                                if (bool.Parse(parts2["DeviceEnabled"])) //Device exists and is enabled
                                {
                                    GatewaysStatus.Add(subId);
                                }
                            }
                            catch
                            {
                            }

                        }
                            if (GatewaysStatus.Count > 0)
                            {
                                PollAllDevices(GatewaysStatus); //Takes each ID, figures out everything, does all the polling in a single modbus network call
                            }  //returns the statuses.



                        /*

                            if (!WhoIsBeingPolled.Contains(subId))
                            {
                                try
                                {
                                    lock (WhoIsBeingPolled) //keep subdevice level locks
                                    {
                                        WhoIsBeingPolled.Add(subId);
                                    }
                                    SiidDevice Subset = SiidDevice.GetFromListByID(Instance.Devices, Convert.ToInt32(subId));
                                    if (Subset != null)
                                    {
                                        var EDO2 = Subset.Extra;
                                        var parts2 = HttpUtility.ParseQueryString(EDO2.GetNamed("SSIDKey").ToString());
                                        if (bool.Parse(parts2["DeviceEnabled"])) //Device exists and is enabled
                                        {
                                              //OneAtATime.WaitOne();

                                            try
                                            {
                                                Console.WriteLine("         Polling Device: " + subId);
                                                ReadFromModbusDevice(Subset);
                                                ProcessCalculator(Subset);

                                            }
                                            catch (Exception e)
                                            {
                                                Console.WriteLine("Exception: " + e.StackTrace);

                                                if (Subset.Device.get_devValue(Instance.host) == 2 || Subset.Device.get_devValue(Instance.host) == 1)
                                                {
                                                    Instance.host.SetDeviceValueByRef(Convert.ToInt32(subId), 2, true);
                                                }
                                                else
                                                {
                                                    Instance.host.SetDeviceValueByRef(Convert.ToInt32(subId), 3, true);
                                                    Instance.host.SetDeviceString(Convert.ToInt32(subId), e.Message, true);
                                                }


                                            }
                                            finally
                                            {
                                                //OneAtATime.ReleaseMutex();
                                                lock (WhoIsBeingPolled)
                                                {
                                                    WhoIsBeingPolled.Remove(subId);
                                                }
                                            }
                                            System.Threading.Thread.Sleep(Convert.ToInt32(parts["Delay"]));
                                            //add delay between each address poll here

                                        }
                                        else
                                        {
                                            Instance.host.SetDeviceValueByRef(Convert.ToInt32(subId), 0, true);
                                        }
                                    }
                                }

                                catch
                                {

                                }
                            }
                                }
                            */





                    }
                    else
                    {

                        SIID_Page.PluginTimerDictionary[Gate.Ref].Dispose();
                        SIID_Page.PluginTimerDictionary.Remove(Gate.Ref);

                    }
                }
            }
                 finally
                {
                    lock (WhoIsBeingPolled) //keep subdevice level locks
                    {
                        WhoIsBeingPolled.Remove(Gate.Ref.ToString());
                    }

                }
            }
           
        }

        public ushort[] FlipByts(ushort[] Input)
        {


           int  index = 0;
            foreach(ushort Item in Input){
                byte[] temp = BitConverter.GetBytes(Item);
                Array.Reverse(temp);
                Input[index] = BitConverter.ToUInt16(temp,0);
                index++;
            }
            return Input;



        }


        public void ReadWriteIfMod(CAPI.CAPIControl ActionIn)
        {
            try
            {
                var devID = ActionIn.Ref;
                if (!WhoIsBeingWritten.Contains(devID.ToString())){
                    lock (WhoIsBeingWritten)
                    {
                        WhoIsBeingWritten.Add(devID.ToString());
                    }
                    String Old=Instance.host.DeviceString(Convert.ToInt32(devID));
                    Old = Old.Replace("Sending command to device. Old value: ","");
                    Instance.host.SetDeviceString(Convert.ToInt32(devID), "Sending command to device. Old value: "+Old, true);


                    var NewDevice = SiidDevice.GetFromListByID(Instance.Devices, devID);
                    Scheduler.Classes.DeviceClass ModDev = NewDevice.Device;

                    var EDO = NewDevice.Extra;
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    if (parts != null)
                    {
                        if (parts["Type"].ToLower() == "Modbus Device".ToLower())
                        {
                            var Send = ActionIn.ControlValue;
                            //Send is a double = 32 bits
                            //Need to convert to whatever we write, depending on the thing we're writing to



                            try
                            {
                                if (bool.Parse(parts["ReadOnlyDevice"]) || Convert.ToInt32(parts["RegisterType"]) == 0 || Convert.ToInt32(parts["RegisterType"]) == 2)//Read only or a read only type
                                {
                                    throw new Exception("Device is set to be read only. Write commands disabled");
                                }
                                Console.WriteLine("         Writing to Device: " + devID);
                                while (WhoIsBeingPolled.Contains(devID.ToString()))
                                {
                                    Console.WriteLine("Device currently being polled, waiting for it to be free");
                                    System.Threading.Thread.Sleep(1000); //Dont try to write until we aren't reading from the register
                                }
                                lock (WhoIsBeingPolled)
                                {
                                    WhoIsBeingPolled.Add(devID.ToString());
                                }
                                Console.WriteLine("         Writing " + Send + " To device " + devID);
                                WriteToModbusDevice(NewDevice, Send);

                                Console.WriteLine("         Reading from Device: " + devID);
                                ReadFromModbusDevice(NewDevice);
                                ProcessCalculator(NewDevice);

                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Exception: " + e.StackTrace);
                                String OldM = Instance.host.DeviceString(Convert.ToInt32(devID));
                                OldM = OldM.Replace("Sending command to device. Old value: ", "");

                                Instance.host.SetDeviceString(Convert.ToInt32(devID), OldM, true);
                                if (ModDev.get_devValue(Instance.host) == 2 || ModDev.get_devValue(Instance.host) == 1)
                                {
                                    Instance.host.SetDeviceValueByRef(Convert.ToInt32(devID), 2, true);
                                }
                                else
                                {

                                    Instance.host.SetDeviceValueByRef(Convert.ToInt32(devID), 3, true);
                                }


                            }
                            finally
                            {
                                lock (WhoIsBeingPolled)
                                {

                                    WhoIsBeingPolled.Remove(devID.ToString());
                                    lock (WhoIsBeingWritten) {
                                        WhoIsBeingWritten.Remove(devID.ToString());
                                    }
                                    
                                }
                            }
                        }


                    }
                }
            }
            catch
            {

            }

        }



        public void ProcessCalculator(SiidDevice NewDevice)
        {
            int devID = NewDevice.Ref;
            Scheduler.Classes.DeviceClass ModDev = NewDevice.Device;
            var EDO = NewDevice.Extra;
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            string ScratchPadString = parts["ScratchpadString"];
           StringBuilder FinalString= new StringBuilder(GeneralHelperFunctions.GetValues(Instance, parts["ScratchPadString"]));
            /*
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
                var TEMPDev = SiidDevice.GetFromListByID(Instance.Devices, dv);
                Scheduler.Classes.DeviceClass TempDev = TEMPDev.Device;
                var TempEDO = TEMPDev.Extra;
                var Tempparts = HttpUtility.ParseQueryString(TempEDO.GetNamed("SSIDKey").ToString());
                string Rep = Tempparts["RawValue"];
                FinalString.Replace("$(" + dv + ")", Rep);

            }
            foreach (int dv in Processed)
            {
                var TEMPDev = SiidDevice.GetFromListByID(Instance.Devices, dv);
                Scheduler.Classes.DeviceClass TempDev = TEMPDev.Device;
                var TempEDO = TEMPDev.Extra;
                var Tempparts = HttpUtility.ParseQueryString(TempEDO.GetNamed("SSIDKey").ToString());
                string Rep = Tempparts["ProcessedValue"];
                FinalString.Replace("$(" + dv + ")", Rep);
            }*/
            string OutValue = "NAN";
            try
            {
                if (int.Parse(parts["ReturnType"]) > 5 || int.Parse(parts["ReturnType"]) ==0) //return is a string or a bool
                {
                    if (int.Parse(parts["ReturnType"]) == 0)//is bool check for bool controlls, make status that status
                    {
                        double val = 0;
                        if (FinalString.ToString() == "true") {
                            val = 1;
                        }
                        var status = "";
                        try
                        {

                            CAPI.CAPIControl[] controlls = Instance.host.CAPIGetControl(NewDevice.Ref);
                            foreach (CAPI.CAPIControl cont in controlls)
                            {
                                if (cont.ControlValue == val) {
                                    status = cont.Label;
                                    break;
                                }
                            }
                            if (status != "")
                            {
                                OutValue = status;
                            }
                            else
                            {
                                OutValue = FinalString.ToString();
                            }

                           
                        }
                        catch { }

                    }
                    else
                    {
                        OutValue = FinalString.ToString();
                    }
                }
                else
                {

                    OutValue = GeneralHelperFunctions.Evaluate(FinalString.ToString()).ToString();

                    
                }

            }
            catch(Exception e) {
                OutValue = "Calculator parse error: " + e.Message+" \nFinal string:"+ FinalString.ToString();
            }

            NewDevice.UpdateExtraData( "ProcessedValue", OutValue);
         
            string ValueString = String.Format(parts["DisplayFormatString"], OutValue);
            Instance.host.SetDeviceString(devID,ValueString,true);
            Instance.host.SetDeviceValueByRef(devID, 1, true);
            Console.WriteLine(devID+ " : " + ValueString);

            Instance.host.SetDeviceValueByRef(Convert.ToInt32(parts["GateID"]), 1, true);

        }


        public void WriteToModbusDevice(SiidDevice NewDevice, double InData)
        {//InData will be cast by whatever we're expecting from the return type, but is a double
            int devID = NewDevice.Ref;
            Scheduler.Classes.DeviceClass ModDev = NewDevice.Device;
            var EDO = NewDevice.Extra;
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5 = double64, 6=string2,7=string4,8=string6,9=string8
            ushort[] Data = null;
            //CURRENTLY WORKIN HERE
            string RetType = parts["ReturnType"];
            if (Convert.ToInt32(parts["RegisterType"]) < 3) // Then its a bool
            {
                RetType = "0";
            }

            byte[] ZEROS = new byte[2] { 0, 0 };
            ushort[] NumRegArrray = new ushort[] { 1, 1, 2, 2, 4, 4, 1, 2, 3, 4 };
            //OK, if the type is supposed to be a bool or int we cast as an int, if it's a double we put the bytes uncase, and if a string... 
            switch (RetType)
            {
               
                case ("0")://bool//0 is false, 1 is true
                case ("1")://int 16
                case ("2")://int 32
                case ("4")://ints 64
                    {
                     

                        byte[] temp = BitConverter.GetBytes((int)InData); //OK doubles are always big endian, so temp[0] is the most significant digit
                        ushort NumRegs = NumRegArrray[Convert.ToInt32(RetType)];
                        Data = new ushort[NumRegs];
                        for (int i = 0; i < NumRegs; i++)
                        {
                            if (2 * i > temp.Count())
                            {
                                Data[i] = BitConverter.ToUInt16(ZEROS, 0);
                            }
                            else
                            {
                                Data[i] = BitConverter.ToUInt16(temp, 2 * i);
                            }
                        }


                        break;
                    }

                case ("3"): //float
                    {
                        byte[] temp = BitConverter.GetBytes((float)InData); //OK doubles are always big endian, so temp[0] is the most significant digit
                        ushort NumRegs = NumRegArrray[Convert.ToInt32(RetType)];
                         Data = new ushort[NumRegs];
                        for (int i = 0; i < NumRegs; i++)
                        {
                            if (2 * i > temp.Count())
                            {
                                Data[i] = BitConverter.ToUInt16(ZEROS, 0);
                            }
                            else
                            {
                                Data[i] = BitConverter.ToUInt16(temp, 2 * i);
                            }
                        }

                        break;
                    }
                case ("5")://double
                    {
                        byte[] temp = BitConverter.GetBytes(InData); //OK doubles are always big endian, so temp[0] is the most significant digit
                        ushort NumRegs = NumRegArrray[Convert.ToInt32(RetType)];
                         Data = new ushort[NumRegs];
                        for (int i = 0; i < NumRegs; i++)
                        {
                            if (2 * i > temp.Count())
                            {
                                Data[i] = BitConverter.ToUInt16(ZEROS, 0);
                            }
                            else
                            {
                                Data[i] = BitConverter.ToUInt16(temp, 2 * i);
                            }
                        }
                        break;

                    }
                default:
                    {//string
                        byte[] temp = BitConverter.GetBytes(InData); //OK doubles are always big endian, so temp[0] is the most significant digit
                        //OK doubles are always big endian, so temp[0] is the most significant digit
                        ushort NumRegs = NumRegArrray[Convert.ToInt32(RetType)];
                        Data = new ushort[NumRegs];
                        for (int i = 0; i < NumRegs; i++)
                        {
                            if (2 * i > temp.Count())
                            {
                                Data[i] = BitConverter.ToUInt16(ZEROS,0);
                            }
                            else
                            {
                                Data[i] = BitConverter.ToUInt16(temp, 2 * i);
                            }
                         

                        }
                        break;
                    }


            }



            if (Data != null)
            {
                
                OpenModDeviceConnection(NewDevice, Data);

            }


        }



        public void PollAllDevices(List<string> GatewaysStatus)
        {
            

            ushort[] OffsetArray = new ushort[] { 10000, 0, 30000, 40000 };

            //GatewaysStatus key is device ID, value is what we return from the modbus devices
            //Need to make Modbus calls
            List<Tuple<bool, ushort, ushort,string>> GatewayInputs = new List<Tuple<bool, ushort, ushort,string>>();
            //is coil, startaddress,numInputs, hs device id

            
                SiidDevice device = SiidDevice.GetFromListByID(Instance.Devices, Convert.ToInt32(GatewaysStatus[0]));
          

            var parts = HttpUtility.ParseQueryString(device.Extra.GetNamed("SSIDKey").ToString());
            int GatewayID = Int32.Parse(parts["GateID"]);
            SiidDevice Gateway = SiidDevice.GetFromListByID(Instance.Devices, GatewayID);
            var EDOGate = Gateway.Extra;
            var partsGate = HttpUtility.ParseQueryString(EDOGate.GetNamed("SSIDKey").ToString());
            bool flipwords = false;
            bool flipbits = false;
            try
            {
                var A = partsGate["BigE"];
                var B = partsGate["RevByte"];
                flipwords = bool.Parse(partsGate["BigE"]); //reverse word order 
                flipbits = bool.Parse(partsGate["RevByte"]);
            }
            catch { }

            foreach (String key in GatewaysStatus)
            {

                device = SiidDevice.GetFromListByID(Instance.Devices, Convert.ToInt32(key));
                parts = HttpUtility.ParseQueryString(device.Extra.GetNamed("SSIDKey").ToString());



                int Offset = 0;
                if (bool.Parse(partsGate["ZeroB"]))
                {
                    Offset = -1;
                }
                if (Int32.Parse(parts["RegisterType"]) < 3)
                {
                    Offset -= 1; //Seems to be a bug where coil are offset by 1 (i.e. coil 1 tries to write to register 2)
                }


                // int Taddress = Int32.Parse(parts["RegisterAddress"]) + Offset + OffsetArray[Int32.Parse(parts["RegisterType"])];
                // ushort startAddress = (ushort)Math.Max(0, Taddress);
                int Taddress = Int32.Parse(parts["RegisterAddress"]) + Offset;
                ushort startAddress = (ushort)Math.Max(0, Taddress);

                //Use bigE to reverse or not the retuned bytes before passing them up to wherever they're going, (or down in case of write)

                ushort[] NumRegArrray = new ushort[] { 1, 1, 2, 2, 4, 4, 1, 2, 3, 4 };
                //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=double64, 6=string2,7=string4,8=string6,9=string8
                //tells us how many registers to read/write and also how to parse returns
                //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
                //So coil and discrete are bool ONLY
                //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read

          
                ushort numInputs = NumRegArrray[Int32.Parse(parts["ReturnType"])];


                if (Int32.Parse(parts["RegisterType"]) < 3)
                {
                    GatewayInputs.Add(new Tuple<bool, ushort, ushort, string>(true, startAddress, 1,key));


                }
                else
                {
                    GatewayInputs.Add(new Tuple<bool, ushort, ushort, string>(false, startAddress, numInputs,key));

                }

            }
            //OK now GatewayInputs is a list of our connections to make. So open the modbus tcp connection and make all the calls:
            Dictionary<string, ushort[]> Return = new Dictionary<string, ushort[]>();
            Dictionary<string, string> Errors = new Dictionary<string, string>();
            try
            {
                using (TcpClient client = new TcpClient(partsGate["Gateway"], Int32.Parse(partsGate["TCP"])))
                {

                    ModbusIpMaster master = ModbusIpMaster.CreateIp(client);
                    master.Transport.ReadTimeout = Int32.Parse(partsGate["RWTime"]);
                    master.Transport.Retries = Int32.Parse(partsGate["RWRetry"]);
                    master.Transport.WaitToRetryMilliseconds = Int32.Parse(partsGate["Delay"]);

                    foreach (var Call in GatewayInputs)
                    {
                        try
                        {
                            if (Call.Item1)
                            {
                                bool Returned = master.ReadCoils(Call.Item2, Call.Item3)[0];
                                if (Returned)
                                    Return[Call.Item4] = new ushort[] { 1 };
                                else
                                    Return[Call.Item4] = new ushort[] { 0 };
                            }
                            else
                            {
                                Return[Call.Item4] = master.ReadHoldingRegisters(Call.Item2, Call.Item3);
                            }
                        }
                        catch (Exception e)
                        {
                            Errors[Call.Item4] = e.Message;
                        }

                    }

                    client.Close();
                }
            }
            catch(Exception e) //The gateway connection is bad, so set gateway to red status
            {
                pingGateway(Gateway.Ref);

            }
            //Now parse our returns back to value strings in GatewaysStatus

            foreach (string key in GatewaysStatus) {
                if(Return.Keys.Contains(key)){


                    device = SiidDevice.GetFromListByID(Instance.Devices, Convert.ToInt32(key));
                    parts = HttpUtility.ParseQueryString(device.Extra.GetNamed("SSIDKey").ToString());
                    bool Signed = bool.Parse(parts["SignedValue"]);

                    var TheReturned = Return[key];
                    if (flipwords)
                    { //Note according to blog here: https://ctlsys.com/common_modbus_protocol_misconceptions/
                      //Each register is in Big Endian and the little endienness is the order we read the registers
                      //so::
                        TheReturned = TheReturned.Reverse().ToArray();
                   
                    }
                    if (flipbits)
                    {
                        TheReturned = FlipByts(TheReturned);
                    }

                    string RawString = "";

                    string RetType = parts["ReturnType"];
                    if (Convert.ToInt32(parts["RegisterType"]) < 3) // Then its a bool
                    {
                        RetType = "0";
                    }
                    switch (RetType)
                    {
                        case ("0"):
                            {
                                RawString = "true";
                                if (TheReturned[0] == 0)
                                {
                                    RawString = "false";
                                }

                                break;
                            }
                        case ("1"):
                        case ("2"):
                        case ("4")://Ints
                            {
                                byte[] Bytes = new byte[TheReturned.Count() * 2];
                                int index = 0;
                                foreach (ushort Item in TheReturned)
                                {
                                    byte[] temp = BitConverter.GetBytes(Item);
                                    Bytes[index] = temp[0];
                                    index++;
                                    Bytes[index] = temp[1];
                                    index++;

                                }
                                if (Signed) //bits aren't any different for signed or unsigned,. so 
                                {
                                    switch (TheReturned.Count())
                                    {
                                        case (1):
                                            {
                                                RawString = BitConverter.ToInt16(Bytes, 0).ToString();
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
                                    switch (TheReturned.Count())
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
                                byte[] Bytes = new byte[TheReturned.Count() * 2];
                                int index = 0;
                                foreach (ushort Item in TheReturned)
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
                                byte[] Bytes = new byte[TheReturned.Count() * 2];
                                int index = 0;
                                foreach (ushort Item in TheReturned)
                                {
                                    byte[] temp = BitConverter.GetBytes(Item);
                                    Bytes[index] = temp[0];
                                    index++;
                                    Bytes[index] = temp[1];
                                    index++;

                                }
                                RawString = BitConverter.ToDouble(Bytes, 0).ToString();
                                break;

                            }
                        default:
                            {//string
                                StringBuilder OUT = new StringBuilder();

                                foreach (ushort Item in TheReturned)
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
                    device.UpdateExtraData("RawValue", RawString);
                    ProcessCalculator(device);
                }
                else{


                    string Exception = "";
                    if(Errors.Keys.Contains(key)){
                        Exception = "EXCEPTION: "+Errors[key];
                    }
                    else{
                        Exception = "EXCEPTION: Uncaught error when reading from modbus device." ;
                    }


                    if (device.Device.get_devValue(Instance.host) == 2 || device.Device.get_devValue(Instance.host) == 1)
                    {
                        Instance.host.SetDeviceValueByRef(Convert.ToInt32(key), 2, true);
                    }
                    else
                    {
                        Instance.host.SetDeviceValueByRef(Convert.ToInt32(key), 3, true);
                        Instance.host.SetDeviceString(Convert.ToInt32(key), Exception, true);
                    }
                }

            }


        }

       

        public void ReadFromModbusDevice(SiidDevice SIIDDev) //Takes device ID, does a read action on it and puts it in the RawValue component of the extra data
        {
            int devID = SIIDDev.Ref;
            Scheduler.Classes.DeviceClass ModDev = SIIDDev.Device;
            var EDO = SIIDDev.Extra;
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5 = double64, 6=string2,7=string4,8=string6,9=string8
            bool Signed = bool.Parse(parts["SignedValue"]);
            var Returned = OpenModDeviceConnection(SIIDDev);
            string RawString = "";

            string RetType = parts["ReturnType"];
            if (Convert.ToInt32(parts["RegisterType"]) < 3) // Then its a bool
            {
                RetType = "0";
            } 
            switch (RetType)
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
                        if (Signed) //bits aren't any different for signed or unsigned,. so 
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
            SIIDDev.UpdateExtraData("RawValue", RawString);
          //  addSSIDExtraData(ModDev, "RawValue", RawString);
           

        }

        public ushort[] OpenModDeviceConnection(SiidDevice SIIDDev, ushort[] Data=null) //General modbus read/write function.
        {
            int devID = SIIDDev.Ref;
            Scheduler.Classes.DeviceClass ModDev = SIIDDev.Device;
            var EDO = SIIDDev.Extra;
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
      
            int GatewayID = Int32.Parse(parts["GateID"]);
            SiidDevice Gateway = SiidDevice.GetFromListByID(Instance.Devices,GatewayID);
            var EDOGate = Gateway.Extra;
            var partsGate = HttpUtility.ParseQueryString(EDOGate.GetNamed("SSIDKey").ToString());
            bool flipwords =false;
            bool flipbits = false;
            try
            {
                var A = partsGate["BigE"];
                var B = partsGate["RevByte"];
                flipwords = bool.Parse(partsGate["BigE"]); //reverse word order 
                flipbits = bool.Parse(partsGate["RevByte"]);
            }
            catch { }
            
            int Offset = 0;
            if(bool.Parse(partsGate["ZeroB"])){
                Offset = -1;
            }
            if (Int32.Parse(parts["RegisterType"]) < 3)
            {
                Offset-=1; //Seems to be a bug where coil are offset by 1 (i.e. coil 1 tries to write to register 2)
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
                    if (Data==null)//read
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
                     

                            if (flipwords)
                            { //Note according to blog here: https://ctlsys.com/common_modbus_protocol_misconceptions/
                              //Each register is in Big Endian and the little endienness is the order we read the registers
                              //so::
                                Return= Return.Reverse().ToArray();
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
                            if (flipbits)
                            {
                                Return=FlipByts(Return);
                                
                            }
                        }
                     //   return Return;
    

                }
                    else //write
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
                            if (flipwords)
                            {
                                Data= Data.Reverse().ToArray();
                                //FlipBits(Data);
                            }
                            if (flipbits)
                            {
                                Data= FlipByts(Data);
                            }
                            master.WriteMultipleRegisters(startAddress, Data);

                        }
                        Return = new ushort[] { 1 };
                        // return new ushort[] { 1 };

                    }
                   
                    client.Close();

                }
            }
            catch(Exception e)
            {
           
                throw e;
               // return new ushort[] { 0 };

            }
            
                return Return;
            
                

            


        
        }
       




    }
}
