using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Scheduler;

namespace HSPI_SIID.Modbus
{
    public class MosbusAjaxReceivers
    {


        private HSPI hspi { get; set; }
        public InstanceHolder Instance { get; set; }

        public MosbusAjaxReceivers(InstanceHolder instance)
        {
           
            Instance = instance;
            hspi = Instance.hspi;
        }

        public  void loadModbusConfig()
        {
            Instance.modbusDefaultPoll = Convert.ToInt32(Instance.host.GetINISetting("MODBUS_CONFIG", "DefaultPoll", "300000", hspi.InstanceFriendlyName() + ".INI"));
            Instance.modbusLogLevel = Convert.ToInt32(Instance.host.GetINISetting("MODBUS_CONFIG", "LogLevel", "2", hspi.InstanceFriendlyName()+".INI"));
            Instance.modbusLogToFile = bool.Parse(Instance.host.GetINISetting("MODBUS_CONFIG", "LogToFile", "false", hspi.InstanceFriendlyName()+".INI"));

        }

        public  void saveModbusConfig()
        {
            //Console.WriteLine(Util.IFACE_NAME + Util.Instance);
            Instance.host.SaveINISetting("MODBUS_CONFIG", "DefaultPoll", Instance.modbusDefaultPoll.ToString(), hspi.InstanceFriendlyName() + ".INI");
            Instance.host.SaveINISetting("MODBUS_CONFIG", "LogLevel", Instance.modbusLogLevel.ToString(), hspi.InstanceFriendlyName()+".INI");
            Instance.host.SaveINISetting("MODBUS_CONFIG", "LogToFile", Instance.modbusLogToFile.ToString(), hspi.InstanceFriendlyName()+".INI");
        }

       
     

        public string postBackProcModBus(string page, string data, string user, int userRights)
        {
            System.Collections.Specialized.NameValueCollection parts = null;
            parts = HttpUtility.ParseQueryString(data);
            string ID = parts["id"];
            string value = parts["value"];

            switch (ID)
            {
                case "polltime":
                    {
                        Instance.modbusDefaultPoll = Convert.ToInt32(value);

                        break;
                    }
                case "logL":
                    {
                        Instance.modbusLogLevel = Convert.ToInt32(value);

                        break;
                    }
                case "modlog":
                    {
                        Instance.modbusLogToFile = bool.Parse(value);

                        break;
                    }


            }
 
           

            saveModbusConfig();
            return "true";
        }

    }
}
