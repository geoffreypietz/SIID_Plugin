using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Scheduler;
using System.Web;

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
            return parts.ToString();
       
        }

        public string MakeDeviceRedirect(string pageName, string user, int userRights, string queryString)
        {
            
            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;

            var dv = Util.hs.NewDeviceRef("Modbus IP Gateway");
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv);
            newDevice.set_Location2(Util.hs, "Modbus");
            newDevice.set_Location(Util.hs, "System");
            //newDevice.set_Interface(Util.hs, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
           newDevice.set_Interface(Util.hs, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function

            newDevice.set_Device_Type_String(Util.hs, makeNewModbusGateway());

           


            stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='/deviceutility?ref=" + dv + "&edit=1'\" />");
            //    stb.Append("<a id = 'LALA' href='/deviceutility?ref=" + dv + "&edit=1'/><script>LALA.click()</script> ");
            page.AddBody(stb.ToString());
            return page.BuildPage();
        }

        public string BuildModbusGatewayTab(int dv1)
        {//Need to pull from device associated modbus information. Need to create when new device is made
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(dv1);
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = System.Web.HttpUtility.ParseQueryString(newDevice.get_Device_Type_String(Util.hs));

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
            stb.Append(ModbusBuilder.button("Done", "Done").print());
            stb.Append(ModbusBuilder.button("Test", "Test").print());
            return stb.ToString();

        }

        public string BuildModbusDeviceTab(int dv)
        {
            StringBuilder stb = new StringBuilder();
            ModbusDevicePage page = this;
            stb.Append("CHECK IT OUT WOO A DEVICE");
            page.AddBody(stb.ToString());
            return page.BuildPage();

        }

        public void parseModbusGatewayTab( string data)
        {
            Console.WriteLine("ConfigDevicePost: " + data);

            // handle form items:
            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);
            string partID = changed["id"].Split('_')[1];
            int devId = Int32.Parse(changed["id"].Split('_')[0]);

            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devId);
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = System.Web.HttpUtility.ParseQueryString(newDevice.get_Device_Type_String(Util.hs));

            parts[partID] = changed["value"];
            newDevice.set_Device_Type_String(Util.hs, parts.ToString());

        }
        public void parseModbusDeviceTab(string data)
        {
        

            Console.WriteLine("ConfigDevicePost: " + data);

            // handle form items:
            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);
            string partID = changed["id"].Split('_')[1];
            int devId = Int32.Parse(changed["id"].Split('_')[0]);

            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Util.hs.GetDeviceByRef(devId);
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = System.Web.HttpUtility.ParseQueryString(newDevice.get_Device_Type_String(Util.hs));

            parts[partID] = changed["value"];
            newDevice.set_Device_Type_String(Util.hs, parts.ToString());

            // handle items like:
            // if parts("id")="mybutton" then
            //Util.callback.ConfigPageCommandsAdd("newpage", "status")
            Util.callback.ConfigDivToUpdateAdd("sample_div", "The div has been updated with this content");

        }



    }
}
