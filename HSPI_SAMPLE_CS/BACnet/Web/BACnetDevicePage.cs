using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using HSPI_SIID.General;
using Scheduler;
using HSPI_SIID_ModBusDemo;
using HomeSeerAPI;
using System.Web;
using System.Web.Script.Serialization;
using System.IO;
using System.IO.BACnet;
using System.Reflection;

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

 



        //public Scheduler.Classes.DeviceClass getExistingHomeseerBacnetNode(String bacnetNodeData)      //now using 'node' to mean a bacnet device or object
        //{
        //    //List<Scheduler.Classes.DeviceClass> listOfDevices = new List<Scheduler.Classes.DeviceClass>();

        //    Scheduler.Classes.clsDeviceEnumeration DevNum = (Scheduler.Classes.clsDeviceEnumeration)Instance.host.GetDeviceEnumerator();
        //    var Dev = DevNum.GetNext();
        //    while (Dev != null)
        //    {
        //        try
        //        {
        //            var EDO = Dev.get_PlugExtraData_Get(Instance.host);
        //            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

        //            //TODO: strip out weird numerical thing that gets added on...

        //            if ((parts["Type"] == "BACnet Device") && (parts["BacnetTreeNodeData"] == bacnetNodeData))
        //            {
        //                if ((Dev.get_Interface(Instance.host).ToString() == Util.IFACE_NAME.ToString()) && (Dev.get_InterfaceInstance(Instance.host) == Instance.name)) //Then it's one of ours
        //                {
        //                    return Dev;
        //                    //could probably store instance stuff in the tree as well...
        //                    //listOfDevices.Add(Dev);
        //                }


        //            }


        //        }
        //        catch
        //        {

        //        }
        //        Dev = DevNum.GetNext();


        //    }
        //    //return Dev;
        //    return null;    //same thing


        //}







        //public Boolean isMatchingBacnetDevice(String bacnetNodeData, String homeseerNodeData)
        //{

        //    var bacnetNode = HttpUtility.ParseQueryString(bacnetNodeData);
        //    var homeseerNode = HttpUtility.ParseQueryString(homeseerNodeData);

        //    foreach (var nodeProp in BACnetTreeNode.DeviceNodeProperties)
        //        if (bacnetNode[nodeProp] != homeseerNode[nodeProp])
        //            return false;

        //    return true;
        //}


        //public Boolean isMatchingBacnetObject(String bacnetNodeData, Scheduler.Classes.DeviceClass homeseerDevice)
        //{
        //    var propsToCompare = new String[] { "ip_address", "device_instance", "object_type", "object_instance" };

        //    return true;
        //}












        //Modeling off the the way I did it for Modbus, A button on SIID page goes to an empty webpage as a trigger that we want to make a BACnet device
        //Makes the device here, then redirects the viewer to the new device's configuration page
        //Probably a better way to do this somewhere -mark
        public string MakeBACnetRedirect(string pageName, string user, int userRights, string bacnetNodeData)   //remember BacnetTreeNodeData is as query string, not JSON object
        {
            // queryString contains nodeData of tree node from which to make device
            //remember BacnetTreeNodeData is as query string, not JSON object

            int? dv;

            dv = getExistingHomeseerBacnetNodeDevice(bacnetNodeData, "device") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeData, "device");

            if (pageName == "addBACnetObject")
                dv = getExistingHomeseerBacnetNodeDevice(bacnetNodeData, "object") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeData, "object");


            StringBuilder stb = new StringBuilder();
            BACnetDevicePage page = this;
            stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='/deviceutility?ref=" + dv + "&edit=1'\" />"); 
            //This will refresh the page and take the browser to the new device's config page
            //May not work on NotChrome
            page.AddBody(stb.ToString());
            return page.BuildPage();

        }







        private int? getExistingHomeseerBacnetNodeDevice(String bacnetNodeData, String nodeType)      //now using 'node' to mean a bacnet device or object
        {
            //List<Scheduler.Classes.DeviceClass> listOfDevices = new List<Scheduler.Classes.DeviceClass>();

            Scheduler.Classes.clsDeviceEnumeration DevNum = (Scheduler.Classes.clsDeviceEnumeration)Instance.host.GetDeviceEnumerator();
            var Dev = DevNum.GetNext();
            while (Dev != null)
            {
                try
                {
                    var EDO = Dev.get_PlugExtraData_Get(Instance.host);
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

                    //TODO: strip out weird numerical thing that gets added on...

                    if (parts["Type"] == "BACnet Device")   //parts["BacnetTreeNodeData"]
                    {
                        if ((Dev.get_Interface(Instance.host).ToString() == Util.IFACE_NAME.ToString()) && (Dev.get_InterfaceInstance(Instance.host) == Instance.name)) //Then it's one of ours
                        {
                            if (isMatchingHomeseerBacnetNode(bacnetNodeData, parts["BACnetNodeData"], nodeType))
                                return Dev.get_Ref(Instance.host);
                            //could probably store instance stuff in the tree as well...
                            //listOfDevices.Add(Dev);
                        }


                    }


                }
                catch
                {

                }
                Dev = DevNum.GetNext();



                //Dev.get_Ref(Instance.hspi);
            }
            //return Dev;
            return null;    //same thing


        }




        private Boolean isMatchingHomeseerBacnetNode(String bacnetNodeData, String homeseerNodeData, String nodeType)      //we may want to fetch an existing homeseer device node based on an incoming object node
        {
            var bacnetNode = HttpUtility.ParseQueryString(bacnetNodeData);
            var homeseerNode = HttpUtility.ParseQueryString(homeseerNodeData);

            foreach (var nodeProp in (nodeType == "device" ? BACnetTreeNode.DeviceNodeProperties : BACnetTreeNode.ObjectNodeProperties))
                if (bacnetNode[nodeProp] != homeseerNode[nodeProp])
                    return false;

            return true;
        }



        private int? makeNewHomeseerBacnetNodeDevice(String bacnetNodeData, String nodeType)
        {

            var dv = Instance.host.NewDeviceRef("BACnet Device");   //TODO: do I need to name it something else?  Maybe at least differentiate between device and object?
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



            EDO.AddNamed("SSIDKey", siidDeviceData(bacnetNodeData)); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            //Could be BACnet device or object, but either way turns into a HomeSeer device





            newDevice.set_PlugExtraData_Set(Instance.host, EDO);



            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;  //Necessary for having the ability to control the device from the devices page using a defined action
            //may be useful later when we want to try actually writing to devices, however we decide to do it 
            newDevice.set_DeviceType_Set(Instance.host, DevINFO);

            return dv;

        }






       //TODO: function to actually make a new device.  return dv or the device itself.



        //Generates the plugin extra data stuff for BACnet Devices
        //Really only thing I want these to have at minimum is type, rawvalue,processedvalue
        public string siidDeviceData(string bacnetNodeData)
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);

            parts["Type"] = "BACnet Device";
            //...


            parts["BACnetNodeData"] = bacnetNodeData;   //this is string representation of nodeData from original node in tree, representing BACnet device or object
                //ex. ip_address=192.168.1.1&device_instance=400001

            //within here, "node_type" indicates whether device or object...



            //enough information to be able to uniquely identify it in network.
            //also tells us whether this is a BACnet device or a BACnet object.


            //sure, for now...will have to tap into Present_Value property if present.  Won't apply for devices.
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

            //return "";



            //or just return a JS initialization for dataTable which goes and grabs data from service based on node....


            //******************************
            //OK, so homeseer device will store BACnetNodeData even when network not on...


            //also, is it worth using homeseer device to get data directly instead of nodeData?  NO, it's probably fine...



            //TODO: can check if this is a master device or object device, and display accordingly...though it really shouldn't be much different...


            //TODO: can't let this get triggered for root node


            Scheduler.Classes.DeviceClass device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv1);



            


                   //TODO: handle differently depending on if this represents BACnet device or object...


            var EDO = device.get_PlugExtraData_Get(Instance.host);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            var bacnetNodeDataString = parts["BACnetNodeData"];
            var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);


            if (bacnetNodeData["node_type"] == "root")
                return "";     //not sure how this is happening, but...


            BACnetObject bno;

            if (bacnetNodeData["node_type"] == "device")
            {
                var d = this.Instance.bacnetDataService.GetBacnetDevice(bacnetNodeData, true);
                bno = d.DeviceObject;
            }
            else
            {
                bno = this.Instance.bacnetDataService.GetBacnetObject(bacnetNodeData, true);
            }

            


            //TODO: for now, return object.GetProperties() and format it into a table.  But ultimately, need to display widgets....
            //Look at how it's done on modbus page - string inputs, numeric, drop-downs.  Are there any other types we need?




            //TODO: get device from actual network based on 




            //if (Int32.Parse(bacnetNodeData["object_type"]) == BacnetObjectTypes.OBJECT_DEVICE.ToInt())
            //{



            //}





            //string dv = "" + dv1 + "";

            StringBuilder stb = new StringBuilder();
            //stb.Append("SO HERE WE CAN PUT BACNET SPECIFIC STUFF<br>");
            //stb.Append("Generate it as HTML<br> THE CURRENT QUERY STRING IS:");


            stb.Append("<div><table id='bacnetObjProps'></table></div>");


            string basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));


            stb.Append("<link rel='stylesheet' type='text/css' href='https://cdn.datatables.net/v/ju/dt-1.10.13/datatables.min.css' />");
            stb.Append("<script type='text/javascript' src='https://cdn.datatables.net/v/ju/dt-1.10.13/datatables.min.js'></script>");



            String tableJs = File.ReadAllText(Path.Combine(basePath, "js", "bacnetPropertiesTable.js"));
            stb.Append("<script>" + tableJs + "</script>");

            var props = new JavaScriptSerializer().Serialize(bno.GetProperties());
            stb.Append(String.Format("<script>var propsList = {0};</script>", props));


            stb.Append("<script>buildHtmlTable(propsList, '#bacnetObjProps');</script>");

                   //TODO: Go and find the object in the network...then connect to it and get its properties.  Should live connection be necessary?  Should it retain knowledge of properties from last time?

            //stb.Append(parts.ToString());
            return stb.ToString();

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





            private bool belongsToThisInstance(Scheduler.Classes.DeviceClass Dev)
            {
                return ((Dev.get_Interface(Instance.host).ToString() == Util.IFACE_NAME.ToString()) && (Dev.get_InterfaceInstance(Instance.host) == Instance.name));


            }



            public List<Scheduler.Classes.DeviceClass> getParentBacnetDevices()
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
                        var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);
                        if (parts["Type"] == "BACnet Device" && belongsToThisInstance(Dev) && bacnetNodeData["node_type"] == "device")
                            listOfDevices.Add(Dev);
                    }
                    catch
                    {

                    }
                    Dev = DevNum.GetNext();


                }
                return listOfDevices;

            }




            public List<Scheduler.Classes.DeviceClass> getChildBacnetDevices(String bacnetDeviceInstance)
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
                        var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);
                        if (parts["Type"] == "BACnet Device" && belongsToThisInstance(Dev) && bacnetNodeData["node_type"] == "object" && bacnetNodeData["device_instance"] == bacnetDeviceInstance)
                            listOfDevices.Add(Dev);
                    }
                    catch
                    {

                    }
                    Dev = DevNum.GetNext();


                }
                return listOfDevices;

            }













            ////Not using this since tree gets its own data from dataService...However, maybe we can put that stuff in here
            //public string DiscoverDevicesRedirect(string pageName, string user, int userRights, string GatewayID)
            //{
            //    StringBuilder stb = new StringBuilder();
            //    BACnetDevicePage page = this;

            //    //Is a placeholder now, so currently will populate DiscoveredBACnetDevices with a dumb stringlist of not much

            //    DiscoveredBACnetDevices.Add("String 1");
            //    DiscoveredBACnetDevices.Add("String 2");
            //    DiscoveredBACnetDevices.Add("String 3");

            //    // stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='history.back()'\" />");
            //    stb.Append("<head><script type = 'text/javascript'>location.href = document.referrer;</script></head>>"); //Reloads previous page?
            //    page.AddBody(stb.ToString());
            //    return page.BuildPage();


            //}



    }
}
