using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using HSPI_SIID.General;
using Scheduler;
using HSPI_SIID;
using HomeSeerAPI;
using System.Web;
using System.IO.BACnet;

namespace HSPI_SIID.BACnet
{
   public class BACnetHomeSeerDevices : PageBuilderAndMenu.clsPageBuilder
    {
        public InstanceHolder Instance { get; set; }


        //public htmlBuilder BACnetBuilder { get; set; }

        //public List<string> DiscoveredBACnetDevices { get; set; }


       public static String BaseUrl = "BACnetHomeSeerDevicesView";


       //public String PageName { get; set; }

        public BACnetHomeSeerDevices(string pagename, InstanceHolder instance)
            : base(pagename)
        {

            Instance = instance;    //hmm.  Aren't we still accessing same network data, even if multiple instances?  Is a lock necessary?
            //BACnetBuilder = new htmlBuilder("BACnetObjectDataService" + Instance.ajaxName);
            //DiscoveredBACnetDevices = new List<string>();

            this.PageName = pagename + instance.ajaxName;

            //PageName = BaseUrl +instance.ajaxName.Replace(":", "_"); ;

        }

         


        public string AllBacnetDevices()
        {//gets list of all associated devices. 
            //Get the collection of these devices which are bacnet
            //build Devices table with the appropriate data displays

            StringBuilder sb = new StringBuilder();
            htmlBuilder BACnetBuilder = new htmlBuilder("BacnetDev" + Instance.ajaxName);

            htmlTable bacnetConfHtml = BACnetBuilder.htmlTable(800);

            List<Scheduler.Classes.DeviceClass> BACnetDevs = getParentBacnetDevices().ToList();
            //HERE



            foreach (Scheduler.Classes.DeviceClass bacnetDevice in BACnetDevs)
            {
                var EDO = bacnetDevice.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);

                var DevRef = bacnetDevice.get_Ref(Instance.host);


                //Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(GateRef);



                bacnetConfHtml.addDevHeader("BACnet Device");
                bacnetConfHtml.addDevMain(BACnetBuilder.MakeImage(16, 16, bacnetDevice.get_Image(Instance.host)).print() +
                    BACnetBuilder.MakeLink("/deviceutility?ref=" + DevRef
                    + "&edit=1", bacnetDevice.get_Name(Instance.host)).print(), "");        //not sure what should go in place of "Add device" button
                //  BACnetBuilder.Qbutton("BACnetDevice_" + GateRef, "Add Device").print()
                sb.Append(bacnetConfHtml.print());

                bacnetConfHtml = BACnetBuilder.htmlTable(800);

                List<Scheduler.Classes.DeviceClass> BACnetObjs = getChildBacnetDevices(bacnetNodeData["device_instance"]).ToList(); ;

                if (BACnetObjs.Count == 0)
                    continue;

                
                bacnetConfHtml.addSubHeader("Enabled", "BACnet Object", "", "", "");  //, "Type", "Format");     //maybe put in, i.e. object identifier?


                foreach (Scheduler.Classes.DeviceClass bacnetObject in BACnetObjs)
                {

                    var EDO2 = bacnetObject.get_PlugExtraData_Get(Instance.host);
                    var parts2 = HttpUtility.ParseQueryString(EDO2.GetNamed("SSIDKey").ToString());

                    bacnetConfHtml.addSubMain(BACnetBuilder.MakeImage(16, 16, bacnetObject.get_Image(Instance.host)).print(),
                           BACnetBuilder.MakeLink("/deviceutility?ref=" + bacnetObject.get_Ref(Instance.host) + "&edit=1", bacnetObject.get_Name(Instance.host)).print(),
                           "", "", "");
                           //Instance.modPage.GetReg(parts["RegisterType"]),
                           //Instance.modPage.GetRet(parts["ReturnType"])

                }





                //bacnetConfHtml.addDevMain(BACnetBuilder.MakeImage(16, 16, device.get_Image(Instance.host)).print() +
                //    BACnetBuilder.MakeLink("/deviceutility?ref=" + device.get_Ref(Instance.host)
                //    + "&edit=1", device.get_Name(Instance.host)).print(), EDO.GetNamed("SSIDKey").ToString());

            }
            sb.Append(bacnetConfHtml.print());



            return sb.ToString();
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





        private bool belongsToThisInstance(Scheduler.Classes.DeviceClass Dev)
        {
            var devInterface = Dev.get_Interface(Instance.host).ToString();
            var utilInterface = Util.IFACE_NAME.ToString();

            var interfaceInstance = Dev.get_InterfaceInstance(Instance.host);

            return ((devInterface == utilInterface) && (interfaceInstance == Instance.name));


        }







        //Modeling off the the way I did it for Modbus, A button on SIID page goes to an empty webpage as a trigger that we want to make a BACnet device
        //Makes the device here, then redirects the viewer to the new device's configuration page
        //Probably a better way to do this somewhere -mark
        public string addOrEditBacnetHomeseerDevice(string bacnetNodeDataString)   //remember BacnetTreeNodeData is as query string, not JSON object
        {
            // queryString contains nodeData of tree node from which to make device
            //remember BacnetTreeNodeData is as query string, not JSON object


            System.Collections.Specialized.NameValueCollection bacnetNodeData = null;
            bacnetNodeData = System.Web.HttpUtility.ParseQueryString(bacnetNodeDataString);

            var nodeType = bacnetNodeData["node_type"];


            int? dv;

            dv = getExistingHomeseerBacnetNodeDevice(bacnetNodeDataString, "device") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "device");

            if (nodeType == "object")
                dv = getExistingHomeseerBacnetNodeDevice(bacnetNodeDataString, "object") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "object", dv);



            //dv = getExistingBacnetHomeseerDevice(bacnetNodeDataString, "device") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "device");

            //if (nodeType == "object")
            //    dv = getExistingBacnetHomeseerDevice(bacnetNodeDataString, "object") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "object");


            return "/deviceutility?ref=" + dv + "&edit=1";


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











        private int? makeNewHomeseerBacnetNodeDevice(String bacnetNodeDataString, String nodeType, int? parentDv = null)    //only supplied if nodeType is object.
        {

            var dv = Instance.host.NewDeviceRef("BACnet Device");   //TODO: do I need to name it something else?  Maybe at least differentiate between device and object?
            MakeBACnetGraphicsAndStatus(dv);
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);




            newDevice.set_Location2(Instance.host, "BACnet"); //Location 2 is the Floor, can say whatever we want
            newDevice.set_Location(Instance.host, "System");

            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function



            if (nodeType == "device")   //bacnetNodeData will come in as the node data of the child node, so we need to overwrite
            {

                var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);
                bacnetNodeData["node_type"] = "device";
                bacnetNodeData["object_type"] = "8";
                bacnetNodeData["object_instance"] = bacnetNodeData["device_instance"];
                bacnetNodeDataString = bacnetNodeData.ToString();

                newDevice.set_Name(Instance.host, "BACnet Device " + dv); //Can include ID in the name cause why not
                newDevice.set_Relationship(Instance.host, Enums.eRelationship.Parent_Root);
            }
            else
            {
                newDevice.set_Name(Instance.host, "BACnet Object " + dv); //Can include ID in the name cause why not
                newDevice.set_Relationship(Instance.host, Enums.eRelationship.Child);
                var parentHomeseerDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef((int)parentDv);
                parentHomeseerDevice.AssociatedDevice_Add(Instance.host, dv);

                newDevice.set_Status_Support(Instance.host, true);      //not entirely sure about this yet.
            }

            //newDevice.set_Relationship(Instance.host, Enums.eRelationship.Standalone); //So this part here is if we want the device to have a grouping relationship with anything else
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



            EDO.AddNamed("SSIDKey", siidDeviceData(bacnetNodeDataString)); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            //Could be BACnet device or object, but either way turns into a HomeSeer device


            newDevice.set_PlugExtraData_Set(Instance.host, EDO);


            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;  //Necessary for having the ability to control the device from the devices page using a defined action
            //may be useful later when we want to try actually writing to devices, however we decide to do it 
            newDevice.set_DeviceType_Set(Instance.host, DevINFO);

            return dv;

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



        //Generates the plugin extra data stuff for BACnet Devices
        //Really only thing I want these to have at minimum is type, rawvalue,processedvalue
        public string siidDeviceData(String bacnetNodeDataString)
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);

            parts["Type"] = "BACnet Device";
            //...


            parts["BACnetNodeData"] = bacnetNodeDataString;   //this is string representation of nodeData from original node in tree, representing BACnet device or object
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







    }
}
