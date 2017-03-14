using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Scheduler;

namespace HSPI_SIID_ModBusDemo.Modbus
{
    class MosbusAjaxReceivers
    {
        public static Int32 modbusDefaultPoll { get; set; }
        public static int modbusLogLevel { get; set; }
        public static bool modbusLogToFile { get; set; }


        public static void loadModbusConfig()
        {
            MosbusAjaxReceivers.modbusDefaultPoll = Convert.ToInt32(Util.hs.GetINISetting("MODBUS_CONFIG", "DefaultPoll", "300000", Util.IFACE_NAME+Util.Instance+".INI"));
            MosbusAjaxReceivers.modbusLogLevel = Convert.ToInt32(Util.hs.GetINISetting("MODBUS_CONFIG", "LogLevel", "2", Util.IFACE_NAME+Util.Instance+".INI"));
            MosbusAjaxReceivers.modbusLogToFile = bool.Parse(Util.hs.GetINISetting("MODBUS_CONFIG", "LogToFile", "false", Util.IFACE_NAME+Util.Instance+".INI"));

        }

        public static void saveModbusConfig()
        {

            Util.hs.SaveINISetting("MODBUS_CONFIG", "DefaultPoll", MosbusAjaxReceivers.modbusDefaultPoll.ToString(), Util.IFACE_NAME + Util.Instance + ".INI");
            Util.hs.SaveINISetting("MODBUS_CONFIG", "LogLevel", MosbusAjaxReceivers.modbusLogLevel.ToString(), Util.IFACE_NAME+Util.Instance+".INI");
            Util.hs.SaveINISetting("MODBUS_CONFIG", "LogToFile", MosbusAjaxReceivers.modbusLogToFile.ToString(), Util.IFACE_NAME+Util.Instance+".INI");
        }

        /*public static string addModbusDev(string page, string data, string user, int userRights)
        {
            System.Collections.Specialized.NameValueCollection parts = null;
            ModbusDevicePage modPage = new ModbusDevicePage("ModbusDevicePage");
            parts = HttpUtility.ParseQueryString(data);
            string ID = parts["id"];
            string value = parts["value"];
            switch (ID)
            {
                case "addModGateway":
                    {
                        //Add a new device which is a modbus gateway
                        //Looks like the way that old MODBUS plugin does it is takes user to a custom deviceutility page modeled on the homeseer one
                        //then somehow makes the device// Guess I will need to make a modbus specific device page 
                        //return modPage.GetPagePlugin("", user, userRights, ""); //need to load a page
                        //HSPI.GetPagePlugin("ModbusGatewayDev",user,userRights,"");
                        String ZZ = modPage.GetPagePlugin("", user, userRights, "");
                        break;
                    }
                case "addModBusDevice":
                    {
                        //value is the gateway we are adding the device to
                        //add a new device to that gateway
                        break;
                    }

            }



            return "true"; //PageBuilderAndMenu.clsPageBuilder.postBackProc(page, data, user, userRights);
        }*/

     

        public static string postBackProcModBus(string page, string data, string user, int userRights)
        {
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);
            string ID = parts["id"];
            string value = parts["value"];

            switch (ID)
            {
                case "polltime":
                    {
                        modbusDefaultPoll = Convert.ToInt32(value);

                        break;
                    }
                case "logL":
                    {
                        modbusLogLevel = Convert.ToInt32(value);

                        break;
                    }
                case "modlog":
                    {
                        modbusLogToFile = bool.Parse(value);

                        break;
                    }


            }
            //So in the main plugin stuff
            //need to add
            //Util.hs.RegisterPage("ModBus", Util.IFACE_NAME, Util.Instance);
            //where ModBus is the name of our ajax callback 

            //then in PostBackProc in the main plugin stuff, the pagename that comes back will be our ajax call
            //n
           

            saveModbusConfig();
            return "true";// base.postBackProc(page, data, user, userRights);
        }

    }
}
