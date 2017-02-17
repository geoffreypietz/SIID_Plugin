using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler;
using System.Web;
using HomeSeerAPI;

namespace HSPI_SAMPLE_CS.Modbus
{
 

    public class ModbusDevicePage : PageBuilderAndMenu.clsPageBuilder

    {
        

        public ModbusDevicePage(string pagename) : base(pagename)
        {
        }

        htmlBuilder ModbusBuilder = new htmlBuilder("ModBusGatewayPage");

      
        public string makeNewModbusGateway()
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);


            parts["Type"]= "Modbus Gateway";
            parts["Gateway"]= "";
            parts["TCP"]= "502";
            parts["Poll"]= MosbusAjaxReceivers.modbusDefaultPoll.ToString();
            parts["Enabled"]= "false";
            parts["BigE"]= "false";
            parts["ZeroB"]= "true";
            parts["RWRetry"]= "2";
            parts["RWTime"]= "1000";
            parts["Delay"]= "0";
            parts["RegWrite"]= "1";
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
            parts["GateID"] = ""+GatewayID+"";
            parts["Gateway"] = Gateway.get_Name(Util.hs) ;
            parts["RegisterType"] = "0";//MosbusAjaxReceivers.modbusDefaultPoll.ToString(); //0 is discrete input, 1 is coil, 2 is InputRegister, 3 is Holding Register
            parts["SlaveId"] = ""+ GParts["LinkedDevices"].Split(',').Count()+""; //get number of slaves from gateway?
            parts["ReturnType"] = "0";//0 = Int16, 1=Int32,2=Float32,3=Int64,4=Bool
            parts["SignedValue"] = "false";
            parts["ScratchpadString"] = "";
            parts["DisplayFormatString"] = "{0}";
            parts["ReadOnlyDevice"] = "true";
            parts["DeviceEnabled"] = "false";
            parts["RegisterAddress"] = "22";
            return parts.ToString();

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

    
        public void AddDeviceToGateway(int DeviceId, int GatewayId)
        {
            Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(GatewayId); //Should keep in gateway a list of devices
            var EDO = Gateway.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            parts["LinkedDevices"] += DeviceId+",";
            EDO.RemoveNamed("SSIDKey");
            EDO.AddNamed("SSIDKey", parts.ToString());
            Gateway.set_PlugExtraData_Set(Util.hs, EDO);


        }

        public string MakeSubDeviceRedirect(string pageName, string user, int userRights, string GatewayID)
        {

            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;
             GatewayID = GatewayID.Split("_".ToCharArray())[1];

            var dv = Util.hs.NewDeviceRef("Modbus Device");
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv);
            newDevice.set_Name(Util.hs,"Modbus Device " + dv);
            newDevice.set_Location2(Util.hs, "Modbus");
            newDevice.set_Location(Util.hs, "System");
            //newDevice.set_Interface(Util.hs, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
            newDevice.set_Interface(Util.hs, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function


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

        public string MakeDeviceRedirect(string pageName, string user, int userRights, string queryString)
        {
            
            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;

            var dv = Util.hs.NewDeviceRef("Modbus IP Gateway");
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv);
            newDevice.set_Name(Util.hs, "Modbus IP Gateway " + dv);
            newDevice.set_Location2(Util.hs, "Modbus");
            newDevice.set_Location(Util.hs, "System");
            //newDevice.set_Interface(Util.hs, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
           newDevice.set_Interface(Util.hs, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function


            HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();
            
           // EDO = newDevice.get_PlugExtraData_Get(Util.hs);

            EDO.AddNamed("SSIDKey", makeNewModbusGateway());
            newDevice.set_PlugExtraData_Set(Util.hs, EDO);

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
            stb.Append(ModbusBuilder.button(dv + "_Done", "Done").print());
            stb.Append(ModbusBuilder.button(dv + "_Test", "Test").print());
            return stb.ToString();

        }

        public string BuildModbusDeviceTab(int dv1)
        {
            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv1);
            var EDO = newDevice.get_PlugExtraData_Get(Util.hs);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            string dv = "" + dv1 + "";

            htmlBuilder ModbusBuilder = new htmlBuilder("ModBusDevTab");
            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable();
            ModbusConfHtml.addDevHeader("Gateway: " + parts["Gateway"]);
            ModbusConfHtml.add("Modbus Gateway ID: ", ModbusBuilder.numberInput(dv + "_GateID", Convert.ToInt32(parts["GateID"])).print());
            ModbusConfHtml.add("Selector Type: ", ModbusBuilder.selectorInput(new string[] { "Discrete Input (RO)", "Coil (RW)", "Input Register (RO)", "Holding Register (RW)" }, dv + "_RegisterType", "RegisterType", Convert.ToInt32(parts["RegisterType"])).print());
            ModbusConfHtml.add("Slave ID: ", ModbusBuilder.numberInput(dv + "_SlaveId", Convert.ToInt32(parts["SlaveId"])).print());
            ModbusConfHtml.add("Register Address: ", ModbusBuilder.numberInput(dv + "_RegisterAddress", Convert.ToInt32(parts["RegisterAddress"])).print());

            switch (parts["RegisterType"])
            {
                case ("0")://Discrete input
                    {
              
                        break;
                    }
                case ("1")://Coil
                    {
                        ModbusConfHtml.add("Read Only Device: ", ModbusBuilder.checkBoxInput(dv + "_ReadOnlyDevice", Boolean.Parse(parts["ReadOnlyDevice"])).print());

                        break;
                    }
                case ("2")://Input Register 
                    {//0 = Int16, 1=Int32,2=Float32,3=Int64,4=Bool
                        ModbusConfHtml.add("Return Type: ", ModbusBuilder.selectorInput(new string[] { "Int16", "Int32", "Float32", "Int64", "Bool" }, dv + "_ReturnType", "RegisterType", Convert.ToInt32(parts["ReturnType"])).print());
                        ModbusConfHtml.add("Signed Value: ", ModbusBuilder.checkBoxInput(dv + "_SignedValue", Boolean.Parse(parts["SignedValue"])).print());
                        ModbusConfHtml.add("Scratch Pad: ", ModbusBuilder.stringInput(dv + "_ScratchpadString", parts["ScratchpadString"]).print());
                        ModbusConfHtml.add("Display Format: ", ModbusBuilder.stringInput(dv + "_DisplayFormatString", parts["DisplayFormatString"]).print());
                        break;
                    }
                case ("3")://Holding Register
                    {
                        ModbusConfHtml.add("Return Type: ", ModbusBuilder.selectorInput(new string[] { "Int16", "Int32", "Float32", "Int64", "Bool" }, dv + "_ReturnType", "RegisterType", Convert.ToInt32(parts["ReturnType"])).print());
                        ModbusConfHtml.add("Signed Value: ", ModbusBuilder.checkBoxInput(dv + "_SignedValue", Boolean.Parse(parts["SignedValue"])).print());
                        ModbusConfHtml.add("Scratch Pad: ", ModbusBuilder.stringInput(dv + "_ScratchpadString", parts["ScratchpadString"]).print());
                        ModbusConfHtml.add("Display Format: ", ModbusBuilder.stringInput(dv + "_DisplayFormatString", parts["DisplayFormatString"]).print());
                        ModbusConfHtml.add("Read Only Device: ", ModbusBuilder.checkBoxInput(dv + "_ReadOnlyDevice", Boolean.Parse(parts["ReadOnlyDevice"])).print());
                        break;
                    }

            }
            ModbusConfHtml.add("Device Enabled: ", ModbusBuilder.checkBoxInput(dv + "_DeviceEnabled", Boolean.Parse(parts["DeviceEnabled"])).print());
            stb.Append(ModbusConfHtml.print());
            stb.Append(ModbusBuilder.button(dv + "_Done", "Done").print());




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

        public void parseModbusGatewayTab( string data)
        {
            Console.WriteLine("ConfigDevicePost: " + data);

 
            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);
            string partID = changed["id"].Split('_')[1];
            int devId = Int32.Parse(changed["id"].Split('_')[0]);

            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devId);
            addSSIDExtraData(newDevice, partID, changed["value"]);
 

        }
        public void parseModbusDeviceTab(string data)
        {


            Console.WriteLine("ConfigDevicePost: " + data);


            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);
            string partID = changed["id"].Split('_')[1];
            int devId = Int32.Parse(changed["id"].Split('_')[0]);

            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devId);
            addSSIDExtraData(newDevice, partID, changed["value"]);


        }



    }
}
