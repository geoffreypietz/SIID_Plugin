using HomeSeerAPI;
using HSPI_SIID_ModBusDemo;
using Scheduler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


        public string parseInstances(string data)
        {
            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);
            string partID = changed["id"].Split('_')[0];
            int devId = Int32.Parse(changed["id"].Split('_')[1]);

            //Make changes to existing device's attributes here
            //Check for if time input messes anything up.


            return "True";
        }


        public string makeNewRules()
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);


            parts["Type"] = "Scratchpad";
            parts["IsEnabled"] = "false";
            parts["IsAccumulator"] = "false";
            parts["UpdateInterval"] = "30000";
            parts["ResetType"] = "0";
            parts["ResetInterval"] = "0";
            parts["ScratchPadString"] = "";
            parts["OldValue"] = "0";
            parts["NewValue"] = "0";
            parts["DisplayedValue"] = "0";
   
            return parts.ToString();
        }
        public string MakeNewRule() {


            var dv = Instance.host.NewDeviceRef("R");

    
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);
            newDevice.set_Name(Instance.host, "Scratchpad Rule " + dv);
            newDevice.set_Location2(Instance.host, "ScratchpadRule");
            newDevice.set_Location(Instance.host, "System");
            //newDevice.set_Interface(Instance.host, "Modbus Configuration");//Put here the registered name of the page for what we want in the Modbus tab!!!  So easy!
            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function
                                                                     //newDevice.set_InterfaceInstance()''  SET INTERFACE INSTANCE
            newDevice.set_Relationship(Instance.host, Enums.eRelationship.Not_Set);

            newDevice.MISC_Set(Instance.host, Enums.dvMISC.NO_LOG);
            newDevice.MISC_Set(Instance.host, Enums.dvMISC.HIDDEN);
            HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();

            // EDO = newDevice.get_PlugExtraData_Get(Instance.host);

            EDO.AddNamed("SSIDKey", makeNewRules());
            newDevice.set_PlugExtraData_Set(Instance.host, EDO);
        
            // newDevice.set_Device_Type_String(Instance.host, makeNewModbusGateway());
            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;


            newDevice.set_DeviceType_Set(Instance.host, DevINFO);



            return "TRUE";

        }

        public List<Scheduler.Classes.DeviceClass> getAllRules()
        {
            List<Scheduler.Classes.DeviceClass> listOfDevices = new List<Scheduler.Classes.DeviceClass>();

            Scheduler.Classes.clsDeviceEnumeration DevNum = (Scheduler.Classes.clsDeviceEnumeration)Instance.host.GetDeviceEnumerator();
            var Dev = DevNum.GetNext();
            while (Dev != null)
            {
                try
                {
                    var EDO = Dev.get_PlugExtraData_Get(Instance.host);
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    string s = parts["Type"];
                    if (parts["Type"] == "Scratchpad")
                    {
                        if ((Dev.get_Interface(Instance.host).ToString() == Util.IFACE_NAME.ToString()) && (Dev.get_InterfaceInstance(Instance.host) == Instance.name)) //Then it's one of ours
                        {
                            listOfDevices.Add(Dev);
                        }


                    }


                }
                catch
                {

                }
                Dev = DevNum.GetNext();


            }
            return listOfDevices;

        }

    }
}
