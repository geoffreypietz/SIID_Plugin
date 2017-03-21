using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HSPI_SIID.General;
using Scheduler;
using HSPI_SIID_ModBusDemo;
using HomeSeerAPI;
using System.Web;

namespace HSPI_SIID.BACnet
{
   public class BACnetDevicePage : PageBuilderAndMenu.clsPageBuilder
    {
        public InstanceHolder Instance { get; set; }
        public htmlBuilder BACnetBuilder { get; set; }

        public List<string> DiscoveredBACnetDevices { get; set; }

        public BACnetDevicePage(string pagename, InstanceHolder instance) : base(pagename)
        {

            Instance = instance;
            BACnetBuilder = new htmlBuilder("BACnetPage" + Instance.ajaxName);
            DiscoveredBACnetDevices = new List<string>();
        }

 
            

        public List<Scheduler.Classes.DeviceClass> getAllBacnetDevices()
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
                    if (parts["Type"] == "BACnet Device")
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


        public string DiscoverDevicesRedirect(string pageName, string user, int userRights, string GatewayID)
        {
            StringBuilder stb = new StringBuilder();
            BACnetDevicePage page = this;

            //Is a placeholder now, so currently will populate DiscoveredBACnetDevices with a dumb stringlist of not much

            DiscoveredBACnetDevices.Add("String 1");
            DiscoveredBACnetDevices.Add("String 2");
            DiscoveredBACnetDevices.Add("String 3");

           // stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='history.back()'\" />");
            stb.Append("<head><script type = 'text/javascript'>location.href = document.referrer;</script></head>>"); //Reloads previous page?
            page.AddBody(stb.ToString());
            return page.BuildPage();


        }

        //Modeling off the the way I did it for Modbus, A button on SIID page goes to an empty webpage as a trigger that we want to make a BACnet device
        //Makes the device here, then redirects the viewer to the new device's configuration page
        //Probably a better way to do this somewhere -mark
        public string MakeBACnetRedirect(string pageName, string user, int userRights, string queryString)
        {
            //OK so the queryString is B_ListIndex of the BACnet Device to add

            string BACNETSTRING = DiscoveredBACnetDevices.ElementAt(Int32.Parse(queryString.Split('_')[1]));


            StringBuilder stb = new StringBuilder();
            BACnetDevicePage page = this;


            var dv = Instance.host.NewDeviceRef("BACnet Device");

            MakeBACnetGraphicsAndStatus(dv);
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);
            newDevice.set_Name(Instance.host, "BACnet Device " + dv); //Can include ID in the name cause why not
            newDevice.set_Location2(Instance.host, "BACnet"); //Location 2 is the Floor, can say whatever we want
            newDevice.set_Location(Instance.host, "System");
          
            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function
                                                                   
            newDevice.set_Relationship(Instance.host, Enums.eRelationship.Standalone); //So this part here is if we want the device to have a grouping relationship with anything else
            //Can do:
            /* Not_Set = 0,
            Indeterminate = 1,
            Parent_Root = 2,
            Standalone = 3,
            Child = 4
            */

            newDevice.MISC_Set(Instance.host, Enums.dvMISC.NO_LOG); //Basically do we want this to log or not log somewhere, I haven't done any plugin specific logging stuff yet'
            //but this may be homeseer log stuff
            newDevice.MISC_Set(Instance.host, Enums.dvMISC.SHOW_VALUES);
            HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();

     

            EDO.AddNamed("SSIDKey", makeNewBACnetDevice(BACNETSTRING)); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            newDevice.set_PlugExtraData_Set(Instance.host, EDO);
     

 
            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;  //Necessary for having the ability to control the device from the devices page using a defined action
            //may be useful later when we want to try actually writing to devices, however we decide to do it 
            newDevice.set_DeviceType_Set(Instance.host, DevINFO);





            stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='/deviceutility?ref=" + dv + "&edit=1'\" />"); 
            //This will refresh the page and take the browser to the new device's config page
            //May not work on NotChrome
            page.AddBody(stb.ToString());
            return page.BuildPage();

        }

       //Generates the plugin extra data stuff for BACnet Devices
       //Really only thing I want these to have at minimum is type, rawvalue,processedvalue
        public string makeNewBACnetDevice(string BACNetString)
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);
 
            parts["Type"] = "BACnet Device";
            //...
            parts["BACnetString"] = BACNetString;
            parts["RawValue"] = "0";
            parts["ProcessedValue"] = "0";
            return parts.ToString();

        }

        //Makes status strings and associates graphics with the enw device. So we have some bacis stuff like "Device is down" or whatever
        //Can use custom images here or just use some homeseer defauilt images, whatever we ultimately decide.
        //Code below is just copied from modbus gateway placeholders, so should change
        public void MakeBACnetGraphicsAndStatus(int dv)
        {
            VSVGPairs.VSPair StatusPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
            StatusPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
            StatusPair.Render = Enums.CAPIControlType.TextBox_String;
            StatusPair.Value = 0;
            StatusPair.Status = "Unreachable";
            Instance.host.DeviceVSP_AddPair(dv, StatusPair); 

            StatusPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
            StatusPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
            StatusPair.Render = Enums.CAPIControlType.TextBox_String;
            StatusPair.Value = 1;
            StatusPair.Status = "Available";
            Instance.host.DeviceVSP_AddPair(dv, StatusPair);

            StatusPair = new VSVGPairs.VSPair(ePairStatusControl.Status);
            StatusPair.PairType = VSVGPairs.VSVGPairType.SingleValue;
            StatusPair.Render = Enums.CAPIControlType.TextBox_String;
            StatusPair.Value = 2;
            StatusPair.Status = "Disabled";
            Instance.host.DeviceVSP_AddPair(dv, StatusPair);

            var Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/SIID/SIIDDisabledPlaceholder.png"; //Just point these images to somewhere else maybe. starting at HomeSeer HS3/html/
            Graphic.Set_Value = 0;
            Instance.host.DeviceVGP_AddPair(dv, Graphic);


            Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/SIID/SIIDGoodPlaceholder.png";
            Graphic.Set_Value = 1;
            Instance.host.DeviceVGP_AddPair(dv, Graphic);


            Graphic = new VSVGPairs.VGPair();
            Graphic.PairType = VSVGPairs.VSVGPairType.SingleValue;
            Graphic.Graphic = "/images/SIID/SIIDMedPlaceholder.png";
            Graphic.Set_Value = 2;
            Instance.host.DeviceVGP_AddPair(dv, Graphic);

        }

        

               public string BuildBACnetDeviceTab(int dv1)
        {//Need to pull from device associated modbus information. Need to create when new device is made
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv1);



            var EDO = newDevice.get_PlugExtraData_Get(Instance.host);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            string dv = "" + dv1 + "";

            StringBuilder stb = new StringBuilder();
            stb.Append("SO HERE WE CAN PUT BACNET SPECIFIC STUFF<br>");
            stb.Append("Generate it as HTML<br> THE CURRENT QUERY STRING IS:");
            stb.Append(parts.ToString());
            return stb.ToString();

        }


    }
}
