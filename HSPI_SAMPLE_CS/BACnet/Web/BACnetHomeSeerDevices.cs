using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using HSPI_Utilities_Plugin.General;
using Scheduler;
using HomeSeerAPI;
using System.Web;
using System.IO.BACnet;

namespace HSPI_Utilities_Plugin.BACnet
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



            foreach (Scheduler.Classes.DeviceClass hsBacnetDevice in BACnetDevs)
            {
                var EDO = hsBacnetDevice.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

                var bacnetNodeData = BACnetDevices.ParseJsonString(parts["BACnetNodeData"]);
                //var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);

                var DevRef = hsBacnetDevice.get_Ref(Instance.host);


                //var bacnetDevice = this.Instance.bacnetDataService.GetBacnetDevice(bacnetNodeData);       //shouldn't need this - everything kept in nodeData


                //Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(GateRef);


                //var bn = this.Instance.bacnetDataService.GetBacnetNetwork(bacnetNodeData); //, true);


                //var bd = this.Instance.bacnetDataService.GetBacnetDevice(bacnetNodeData);


                //bacnetConfHtml.addDevHeader("BACnet Device");
                //bacnetConfHtml.addDevMain(BACnetBuilder.MakeImage(16, 16, bacnetDevice.get_Image(Instance.host)).print() +
                //    BACnetBuilder.MakeLink("/deviceutility?ref=" + DevRef
                //    + "&edit=1", bacnetDevice.get_Name(Instance.host)).print(), "");        //not sure what should go in place of "Add device" button
                ////  BACnetBuilder.Qbutton("BACnetDevice_" + GateRef, "Add Device").print()


                //bacnetConfHtml.addHead(new string[] { " ", "BACnet Device", "Instance No.", "IP Address", "UDP Port", "BACnet Name", "BACnet Object Identifier" });


                var tdString = "<td {0} {1} >{2}</td>";

                StringBuilder row = new StringBuilder();
                row.Append(String.Format(tdString, "class='columnheader'", "", ""));
                row.Append(String.Format(tdString, "class='columnheader'", "colspan='2'", "BACnet Device"));
                row.Append(String.Format(tdString, "class='columnheader'", "", "IP Address"));
                row.Append(String.Format(tdString, "class='columnheader'", "", "UDP Port"));
                row.Append(String.Format(tdString, "class='columnheader'", "", "Object Type"));
                row.Append(String.Format(tdString, "class='columnheader'", "", "Instance No."));
                row.Append(String.Format(tdString, "class='columnheader'", "", "Object Name"));
                row.Append(String.Format(tdString, "class='columnheader'", "", "Device ID"));
                row.Append(String.Format(tdString, "class='columnheader'", "", "Device Value"));
                //row.Append(String.Format(tdString, "class='columnheader'", "", "BACnet Object Identifier"));
                bacnetConfHtml.addRow(row.ToString());



            //foreach (string head in HeadArray)
            //{
            //    var classAttr = (index == 0 && head == "") ? " " : " class='columnheader' ";
            //    var colSpanAttr = (head == "BACnet Device" ? " colspan='2' " : " ");
            //    row.Append("<td " + classAttr + colSpanAttr + "> "+ head + "</td>");
            //}
            //addRow(row.ToString());



                var deviceImage = BACnetBuilder.MakeImage(16, 16, hsBacnetDevice.get_Image(Instance.host)).print();

                var deviceLink = BACnetBuilder.MakeLink("/deviceutility?ref=" + DevRef
                    + "&edit=1", hsBacnetDevice.get_Name(Instance.host)).print();


                row = new StringBuilder();
                row.Append(String.Format(tdString, "class='tableroweven'", "", deviceImage));
                row.Append(String.Format(tdString, "class='tableroweven'", "colspan='2'", deviceLink));
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["ip_address"]));
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["device_udp_port"]));
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["object_type_string"]));
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["device_instance"]));
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["object_name"]));
                row.Append(String.Format(tdString, "class='tableroweven'", "", DevRef));
                row.Append(String.Format(tdString, "class='tableroweven'", "", parts["RawValue"]));
                //row.Append(String.Format(tdString, "class='columnheader'", "", bacnetNodeData["object_identifier"]));
                bacnetConfHtml.addRow(row.ToString());


                //sb.Append(bacnetConfHtml.print());
                //bacnetConfHtml = BACnetBuilder.htmlTable(800);

                List<Scheduler.Classes.DeviceClass> BACnetObjs = getChildBacnetDevices(bacnetNodeData["device_instance"]).ToList(); ;

                if (BACnetObjs.Count == 0)
                    continue;


                //bacnetConfHtml.addHead(new string[]{"", "Enabled", "BACnet Object", "", "", "", "BACnet Name", "BACnet Object Identifier"});  //, "Type", "Format");     //maybe put in, i.e. object identifier?


                row = new StringBuilder();
                row.Append(String.Format(tdString, "", "", ""));
                row.Append(String.Format(tdString, "class='columnheader'", "", "Enabled"));
                row.Append(String.Format(tdString, "class='columnheader'", "", "BACnet Object"));
                row.Append(String.Format(tdString, "class='columnheader'", "", ""));
                row.Append(String.Format(tdString, "class='columnheader'", "", ""));
                row.Append(String.Format(tdString, "class='columnheader'", "", ""));
                row.Append(String.Format(tdString, "class='columnheader'", "", ""));
                row.Append(String.Format(tdString, "class='columnheader'", "", ""));
                row.Append(String.Format(tdString, "class='columnheader'", "", ""));
                row.Append(String.Format(tdString, "class='columnheader'", "", ""));
                bacnetConfHtml.addRow(row.ToString());





                foreach (Scheduler.Classes.DeviceClass bacnetObject in BACnetObjs)
                {

                    var EDO2 = bacnetObject.get_PlugExtraData_Get(Instance.host);
                    var parts2 = HttpUtility.ParseQueryString(EDO2.GetNamed("SSIDKey").ToString());

                    //var bacnetNodeData2 = HttpUtility.ParseQueryString(parts2["BACnetNodeData"]);
                    var bacnetNodeData2 = BACnetDevices.ParseJsonString(parts2["BACnetNodeData"]);


                    var subDeviceRef = bacnetObject.get_Ref(Instance.host);


                    var subDeviceImage = BACnetBuilder.MakeImage(16, 16, bacnetObject.get_Image(Instance.host)).print();

                    

                    var subDeviceLink = BACnetBuilder.MakeLink("/deviceutility?ref=" + subDeviceRef
                        + "&edit=1", bacnetObject.get_Name(Instance.host)).print();




                    row = new StringBuilder();
                    row.Append(String.Format(tdString, "", "", ""));
                    row.Append(String.Format(tdString, "class='tableroweven'", "", subDeviceImage));
                    row.Append(String.Format(tdString, "class='tableroweven'", "", subDeviceLink));
                    row.Append(String.Format(tdString, "class='tableroweven'", "", ""));
                    row.Append(String.Format(tdString, "class='tableroweven'", "", ""));
                    row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData2["object_type_string"]));
                    row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData2["object_instance"]));
                    row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData2["object_name"]));
                    row.Append(String.Format(tdString, "class='tableroweven'", "", subDeviceRef));
                    row.Append(String.Format(tdString, "class='tableroweven'", "", parts2["RawValue"]));
                    //row.Append(String.Format(tdString, "class='columnheader'", "", bacnetNodeData["object_identifier"]));
                    bacnetConfHtml.addRow(row.ToString());





                    //bacnetConfHtml.addSubMain(subDeviceImage, subDeviceLink, "", bacnetNodeData2["object_name"], bacnetNodeData2["object_identifier"]);


                    //bacnetConfHtml.addSubMain(BACnetBuilder.MakeImage(16, 16, bacnetObject.get_Image(Instance.host)).print(),
                    //       BACnetBuilder.MakeLink("/deviceutility?ref=" + bacnetObject.get_Ref(Instance.host) + "&edit=1", bacnetObject.get_Name(Instance.host)).print(),
                    //       "", "", "");
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
       
                    //var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);
                    var bacnetNodeData = BACnetDevices.ParseJsonString(parts["BACnetNodeData"]);
                
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
                    //var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);
                    var bacnetNodeData = BACnetDevices.ParseJsonString(parts["BACnetNodeData"]);

                    if (parts["Type"] == "BACnet Object" && belongsToThisInstance(Dev) && bacnetNodeData["node_type"] == "object" && bacnetNodeData["device_instance"] == bacnetDeviceInstance)
                        listOfDevices.Add(Dev);
                }
                catch
                {

                }
                Dev = DevNum.GetNext();


            }
            return listOfDevices;

        }



        public List<int> getChildBacnetDevices2(String bacnetDeviceInstance)
        {


            List<Scheduler.Classes.DeviceClass> listOfDevices = new List<Scheduler.Classes.DeviceClass>();
            List<int> devIds = new List<int>();

            //TODO: collection was modified, enumeration may not execute

            lock (Instance.Devices)
            {
                var Z = Instance.Devices.ToList();
                foreach (SiidDevice Dev in Z)    //collection modified; enum may not execute
                {
                    var hsBacnetDevice = Dev.Device;

                    try
                    {
                        var EDO = hsBacnetDevice.get_PlugExtraData_Get(Instance.host);
                        var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                        string s = parts["Type"];

                        //var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);
                        var bacnetNodeData = BACnetDevices.ParseJsonString(parts["BACnetNodeData"]);


                        if (parts["Type"] == "BACnet Object" && belongsToThisInstance(hsBacnetDevice) && bacnetNodeData["node_type"] == "object" && bacnetNodeData["device_instance"] == bacnetDeviceInstance)
                            //listOfDevices.Add(hsBacnetDevice);
                            devIds.Add(Dev.Ref);
                    }
                    catch
                    {

                    }

                }
            }

            //return listOfDevices;
            return devIds;
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
        public string addOrEditBacnetHomeseerDevice(string bacnetNodeDataString, bool import = false)   //remember BacnetTreeNodeData is as query string, not JSON object
        {
            // queryString contains nodeData of tree node from which to make device
            //remember BacnetTreeNodeData is as query string, not JSON object


            System.Collections.Specialized.NameValueCollection bacnetNodeData = null;
            bacnetNodeData = System.Web.HttpUtility.ParseQueryString(bacnetNodeDataString);     //No, here this actually is a query string.
            //bacnetNodeData = BACnetDevices.ParseJsonString(bacnetNodeDataString);


            var nodeType = bacnetNodeData["node_type"];


            int? dv;

            dv = getExistingHomeseerBacnetNodeDevice(bacnetNodeData, "device") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeData, "device");

            if (nodeType == "object")
                dv = getExistingHomeseerBacnetNodeDevice(bacnetNodeData, "object") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeData, "object", dv);



            //dv = getExistingBacnetHomeseerDevice(bacnetNodeDataString, "device") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "device");

            //if (nodeType == "object")
            //    dv = getExistingBacnetHomeseerDevice(bacnetNodeDataString, "object") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "object");


            return "/deviceutility?ref=" + dv + "&edit=1";


        }









        //public int? getExistingHomeseerBacnetNodeDevice(String bacnetNodeData, String nodeType)      //now using 'node' to mean a bacnet device or object
        public int? getExistingHomeseerBacnetNodeDevice(NameValueCollection bacnetNodeData, String nodeType)      //now using 'node' to mean a bacnet device or object
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
                    var bacnetTypeString = (nodeType == "device" ? "BACnet Device" : "BACnet Object");

                    var hsBacnetNodeData = BACnetDevices.ParseJsonString(parts["BACnetNodeData"]);

                    if (parts["Type"] == bacnetTypeString)   //parts["BacnetTreeNodeData"]
                    {
                        if ((Dev.get_Interface(Instance.host).ToString() == Util.IFACE_NAME.ToString()) && (Dev.get_InterfaceInstance(Instance.host) == Instance.name)) //Then it's one of ours
                        {
                            if (isMatchingHomeseerBacnetNode(bacnetNodeData, hsBacnetNodeData, nodeType))
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




        private int bacnetObjectWritePriorityDevice(int deviceDv, int objDv, NameValueCollection bacnetNodeData, String name)      //parentDv is for the device device
        {
            bacnetNodeData = new NameValueCollection(bacnetNodeData);   //don't want to change in calling function

            var dv = Instance.host.NewDeviceRef("BACnet Object (write priority)");

            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            //Scheduler.Classes.DeviceClass newDevice = SiidDevice.GetFromListByID(Instance.Devices, dv); // would need to add first

            newDevice.set_InterfaceInstance(Instance.host, Instance.name);




            //var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);
            //if (nodeType == "device")   //bacnetNodeData will come in as the node data of the child node, so we need to overwrite
            //{
            //    bacnetNodeData["node_type"] = "device";       //now done in calling function
            //    bacnetNodeData["object_type"] = "8";
            //    bacnetNodeData["object_instance"] = bacnetNodeData["device_instance"];
            //}



            //TODO: the below will fail if we are importing, and the device is not actually on the network.  Still create in HomeSeer?

            //BACnetObject bacnetObject = Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(bacnetNodeData);
            //var bacnetDevice = bacnetObject.BacnetDevice;


            //var addressParts = bacnetDevice.BacnetAddress.ToString().Split(":".ToCharArray());
            //var ipAddress = addressParts[0];
            //var udpPort = addressParts[1];

            //bacnetNodeData["device_ip_address"] = ipAddress;
            //bacnetNodeData["device_udp_port"] = udpPort;



            //var nameProp = bacnetObject.GetBacnetProperty(BacnetPropertyIds.PROP_OBJECT_NAME);
            //var objectName = nameProp.BacnetPropertyValue.Value.value[0].ToString();

            //var idProp = bacnetObject.GetBacnetProperty(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER);
            //var objectId = idProp.BacnetPropertyValue.Value.value[0].ToString();

            //bacnetNodeData["object_name"] = objectName;
            //bacnetNodeData["object_identifier"] = objectId;     //want this whether device or object
            //bacnetNodeData["object_type_string"] = objectId.Split(":".ToCharArray())[0];


            ////bacnetNodeData["write_priority"] = "0";         //not really a persistent parameter; just useful when on config page, writing to properties



            //if (nodeType == "device")   //bacnetNodeData will come in as the node data of the child node, so we need to overwrite
            //{

            //    //newDevice.set_Name(Instance.host, "BACnet Device - " + objectName); //Can include ID in the name cause why not

            //    //bacnetNodeDataString = bacnetNodeData.ToString();



            //    //TODO: set default polling interval here


            //    //leave this alone.  Store polling interval in config...

            //    MakeBACnetGraphicsAndStatus(dv);



            //    bacnetNodeData["polling_interval"] = "5000";    //default


            //    //Instance.bacnetDevices.updateDevicePollTimer(dv, 5000);



            //    //TODO: just for testing, so we can see pres. value.

            //    //var Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
            //    //Control.PairType = VSVGPairs.VSVGPairType.Range;
            //    //Control.RangeStart = 0;
            //    //Control.RangeEnd = 100000;
            //    //Control.Render = Enums.CAPIControlType.TextBox_Number;
            //    //var IS = Instance.host.DeviceVSP_AddPair(dv, Control);


            //    //TODO: so now the object HS device has to look at the present value of 



            //}
            //else
            //{
            //    //bacnetNodeData["parent_hs_device"] = parentDv.ToString();



                var parentDevId = (int)deviceDv;   //parent is the BACnet object device, not the BACnet device device
                var parentHomeseerDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(parentDevId);

                parentHomeseerDevice.AssociatedDevice_Add(Instance.host, dv);
                parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.Parent_Root);

                newDevice.AssociatedDevice_Add(Instance.host, parentDevId);
                newDevice.set_Relationship(Instance.host, Enums.eRelationship.Child);



                bacnetNodeData["bacnet_object_hs_device"] = objDv.ToString();     //on change, can then go and change nodeData in that object.


                //bacnetNodeData[""]


                //newDevice.set_Name(Instance.host, "BACnet Object - " + objectName); //Can include ID in the name cause why not



                //parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.);

                newDevice.set_Status_Support(Instance.host, true);      //not entirely sure about this yet.


                Instance.host.SetDeviceValueByRef(dv, 16, false);
                Instance.host.SetDeviceString(dv, "16", false);

                //but shouldn't it update when you change?


                //TODO: add the different write priorities here.


                //I//nstance.hspi.







                var strs = new String[] { 
                    //"0 (No Priority)",        //doesn't really exist.  16 is lowest priority
                    "1 (Manual Life Safety)",
                    "2 (Automatic Life Safety)",
                    "3",
                    "4",
                    "5 (Critical Equipment Control)",
                    "6 (Minimum On/Off)",
                    "7",
                    "8 (Manual Operator)",
                    "9",
                    "10",
                    "11",
                    "12",
                    "13",
                    "14",
                    "15",
                    "16"
                
                };

                for (int i = 0; i < strs.Length; i++)
                {
                    var Control = new VSVGPairs.VSPair(ePairStatusControl.Control);
                    Control.PairType = VSVGPairs.VSVGPairType.SingleValue;


                    Control.Render_Location.Row = 0;
                    Control.Render_Location.Column = 0;
                    Control.Render_Location.ColumnSpan = 0;



                    //

                    Control.Value = i + 1;
                    Control.Status = strs[i];

                    Control.IncludeValues = false;

                    //Control.StringListAdd = "blah";

                    //Control.StringListAdd = "blah2";


                    //Control.StringList = new String[] { "Release2" };

                    Control.Render = Enums.CAPIControlType.Single_Text_from_List;


                    var IS = Instance.host.DeviceVSP_AddPair(dv, Control);



                }







                //var Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
                //Control.PairType = VSVGPairs.VSVGPairType.Range;


                ////Control.


                ////Control.Value = 2.0;

                //Control.RangeStart = 0;
                //Control.RangeEnd = 16;


                ////won't need these once we build custom one....



                ////Control.StringListAdd = "blah";


                ////Control.

                ////Control.

                ////Control.Value = 2;



                ////Control.Render = Enums.CAPIControlType.TextBox_Number;


                //Control.Render = Enums.CAPIControlType.ValuesRange;

                ////Control.Render = Enums.CAPIControlType.List_Text_from_List;


                //var IS2 = Instance.host.DeviceVSP_AddPair(dv, Control);



            //Instance.host.

            //newDevice.


            //Instance.host.



            //}



            newDevice.set_Name(Instance.host,  name + " (write priority)");


            newDevice.set_Location2(Instance.host, "BACnet"); //Location 2 is the Floor, can say whatever we want
            newDevice.set_Location(Instance.host, "System");

            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function



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



            


            EDO.AddNamed("SSIDKey", siidDeviceData(bacnetNodeData, true)); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            //Could be BACnet device or object, but either way turns into a HomeSeer device


            newDevice.set_PlugExtraData_Set(Instance.host, EDO);


            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;  //Necessary for having the ability to control the device from the devices page using a defined action
            //may be useful later when we want to try actually writing to devices, however we decide to do it 
            newDevice.set_DeviceType_Set(Instance.host, DevINFO);





            Instance.Devices.Add(new SiidDevice(Instance, newDevice));



            return dv;




        }









        public int testDev()      //parentDv is for the device device
        {
            var dv = Instance.host.NewDeviceRef("BACnet Object (write priority)");

            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);



            //var parentDevId = (int)deviceDv;   //parent is the BACnet object device, not the BACnet device device
            //var parentHomeseerDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(parentDevId);

            //parentHomeseerDevice.AssociatedDevice_Add(Instance.host, dv);
            //parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.Parent_Root);

            //newDevice.AssociatedDevice_Add(Instance.host, parentDevId);
            //newDevice.set_Relationship(Instance.host, Enums.eRelationship.Child);



            //bacnetNodeData["bacnet_object_hs_device"] = objDv.ToString();     //on change, can then go and change nodeData in that object.


            //bacnetNodeData[""]


            //newDevice.set_Name(Instance.host, "BACnet Object - " + objectName); //Can include ID in the name cause why not



            //parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.);

            newDevice.set_Status_Support(Instance.host, true);      //not entirely sure about this yet.


            Instance.host.SetDeviceValueByRef(dv, 0, false);
            Instance.host.SetDeviceString(dv, "0", false);



            //Instance.host.DeviceScriptButton_Add(dv, "blah", "blah.js", "func", "", 1, 1, 1);


            //Instance.host.DeviceProperty_StrArray(

            //Instance.host.DeviceScriptButton_Add

            //but shouldn't it update when you change?


            //TODO: add the different write priorities here.


            //I//nstance.hspi.










            //var Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
            //Control.PairType = VSVGPairs.VSVGPairType.Range;


            //Control.RangeStart = 0;
            //Control.RangeEnd = 16;


            //Control.StringListAdd = "blah";



            //Control.Render = Enums.CAPIControlType.Single_Text_from_List;


            //var IS = Instance.host.DeviceVSP_AddPair(dv, Control);














            var Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
            Control.PairType = VSVGPairs.VSVGPairType.SingleValue;


            //Control.RangeStart = 0;
            //Control.RangeEnd = 1;


            //Control.Value = 2;


            Control.Status = "blahblah";



            //Control.

            //Control.StringListAdd = "blah";

            //Control.StringList = new String[] { "blah" };


            //

            //Control.Render = Enums.CAPIControlType.TextList;


            var IS = Instance.host.DeviceVSP_AddPair(dv, Control);





















            Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
            Control.PairType = VSVGPairs.VSVGPairType.Range;


            //Control.RangeStart = 0;
            //Control.RangeEnd = 1;


            //Control.Value = 2;


            //Control.Status = "blahblah";


            //Control.StringListAdd = "blah";

            //Control.StringList = new String[] { "blah" };


            //

            Control.Render = Enums.CAPIControlType.ValuesRange;


            IS = Instance.host.DeviceVSP_AddPair(dv, Control);



            //Instance.host.

            //newDevice.


            //Instance.host.



            //}



            newDevice.set_Name(Instance.host, "test (write priority)");


            newDevice.set_Location2(Instance.host, "BACnet"); //Location 2 is the Floor, can say whatever we want
            newDevice.set_Location(Instance.host, "System");

            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function



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
            //HomeSeerAPI.PlugExtraData.clsPlugExtraData EDO = new PlugExtraData.clsPlugExtraData();






            //EDO.AddNamed("SSIDKey", siidDeviceData(bacnetNodeData.ToString(), true)); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            //Could be BACnet device or object, but either way turns into a HomeSeer device


            //newDevice.set_PlugExtraData_Set(Instance.host, EDO);


            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;  //Necessary for having the ability to control the device from the devices page using a defined action
            //may be useful later when we want to try actually writing to devices, however we decide to do it 
            newDevice.set_DeviceType_Set(Instance.host, DevINFO);





            Instance.Devices.Add(new SiidDevice(Instance, newDevice));



            return dv;




        }













        private int bacnetObjectPriorityArrayDevice(int deviceDv, int objDv, NameValueCollection bacnetNodeData, String name)      //parentDv is for the device device
        {
            bacnetNodeData = new NameValueCollection(bacnetNodeData);   //don't want to change in calling function

            var dv = Instance.host.NewDeviceRef("BACnet Object (priority array)");

            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);




            //var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);
            //if (nodeType == "device")   //bacnetNodeData will come in as the node data of the child node, so we need to overwrite
            //{
            //    bacnetNodeData["node_type"] = "device";       //now done in calling function
            //    bacnetNodeData["object_type"] = "8";
            //    bacnetNodeData["object_instance"] = bacnetNodeData["device_instance"];
            //}



            //TODO: the below will fail if we are importing, and the device is not actually on the network.  Still create in HomeSeer?

            //BACnetObject bacnetObject = Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(bacnetNodeData);
            //var bacnetDevice = bacnetObject.BacnetDevice;


            //var addressParts = bacnetDevice.BacnetAddress.ToString().Split(":".ToCharArray());
            //var ipAddress = addressParts[0];
            //var udpPort = addressParts[1];

            //bacnetNodeData["device_ip_address"] = ipAddress;
            //bacnetNodeData["device_udp_port"] = udpPort;



            //var nameProp = bacnetObject.GetBacnetProperty(BacnetPropertyIds.PROP_OBJECT_NAME);
            //var objectName = nameProp.BacnetPropertyValue.Value.value[0].ToString();

            //var idProp = bacnetObject.GetBacnetProperty(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER);
            //var objectId = idProp.BacnetPropertyValue.Value.value[0].ToString();

            //bacnetNodeData["object_name"] = objectName;
            //bacnetNodeData["object_identifier"] = objectId;     //want this whether device or object
            //bacnetNodeData["object_type_string"] = objectId.Split(":".ToCharArray())[0];


            ////bacnetNodeData["write_priority"] = "0";         //not really a persistent parameter; just useful when on config page, writing to properties



            //if (nodeType == "device")   //bacnetNodeData will come in as the node data of the child node, so we need to overwrite
            //{

            //    //newDevice.set_Name(Instance.host, "BACnet Device - " + objectName); //Can include ID in the name cause why not

            //    //bacnetNodeDataString = bacnetNodeData.ToString();



            //    //TODO: set default polling interval here


            //    //leave this alone.  Store polling interval in config...

            //    MakeBACnetGraphicsAndStatus(dv);



            //    bacnetNodeData["polling_interval"] = "5000";    //default


            //    //Instance.bacnetDevices.updateDevicePollTimer(dv, 5000);



            //    //TODO: just for testing, so we can see pres. value.

            //    //var Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
            //    //Control.PairType = VSVGPairs.VSVGPairType.Range;
            //    //Control.RangeStart = 0;
            //    //Control.RangeEnd = 100000;
            //    //Control.Render = Enums.CAPIControlType.TextBox_Number;
            //    //var IS = Instance.host.DeviceVSP_AddPair(dv, Control);


            //    //TODO: so now the object HS device has to look at the present value of 



            //}
            //else
            //{
            //    //bacnetNodeData["parent_hs_device"] = parentDv.ToString();



            var parentDevId = (int)deviceDv;   //parent is the BACnet object device, not the BACnet device device
            var parentHomeseerDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(parentDevId);

            parentHomeseerDevice.AssociatedDevice_Add(Instance.host, dv);
            parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.Parent_Root);

            newDevice.AssociatedDevice_Add(Instance.host, parentDevId);
            newDevice.set_Relationship(Instance.host, Enums.eRelationship.Child);



            bacnetNodeData["bacnet_object_hs_device"] = objDv.ToString();     //on change, can then go and change nodeData in that object.




            //TODO: instead, store reference to present value device in parent.




            //bacnetNodeData[""]


            //newDevice.set_Name(Instance.host, "BACnet Object - " + objectName); //Can include ID in the name cause why not



            //parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.);

            newDevice.set_Status_Support(Instance.host, true);      //not entirely sure about this yet.


            //Instance.host.SetDeviceValueByRef(dv, 0, false);
            Instance.host.SetDeviceString(dv, "{}", false);

            //but shouldn't it update when you change?


            //TODO: add the different write priorities here.


            //I//nstance.hspi.








            // Maybe don't need this at all if not controllable...

            //var Control = new VSVGPairs.VSPair(ePairStatusControl.Status);
            //Control.PairType = VSVGPairs.VSVGPairType.SingleValue;

            //Control.

            //Control.


            //Control.Value = 2.0;

            //Control.RangeStart = 0;
            //Control.RangeEnd = 16;

            //Control.

            //Control.

            //Control.Value = 2;



            //Control.Render = Enums.CAPIControlType.TextBox_Number;
            //Control.Render = Enums.CAPIControlType.
            //var IS = Instance.host.DeviceVSP_AddPair(dv, Control);

            //Instance.host.

            //newDevice.


            //Instance.host.



            //}



            newDevice.set_Name(Instance.host, name + " (priority array)");


            newDevice.set_Location2(Instance.host, "BACnet"); //Location 2 is the Floor, can say whatever we want
            newDevice.set_Location(Instance.host, "System");

            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function



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






            EDO.AddNamed("SSIDKey", siidDeviceData(bacnetNodeData, true)); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            //Could be BACnet device or object, but either way turns into a HomeSeer device


            newDevice.set_PlugExtraData_Set(Instance.host, EDO);


            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;  //Necessary for having the ability to control the device from the devices page using a defined action
            //may be useful later when we want to try actually writing to devices, however we decide to do it 
            newDevice.set_DeviceType_Set(Instance.host, DevINFO);





            Instance.Devices.Add(new SiidDevice(Instance, newDevice));



            return dv;




        }









        private int? makeNewHomeseerBacnetNodeDevice(NameValueCollection bacnetNodeData, String nodeType, int? parentDv = null)    //only supplied if nodeType is object.
        {

            bacnetNodeData = new NameValueCollection(bacnetNodeData);   //create copy so this is not changed in calling function

            var bacnetTypeString = (nodeType == "device" ? "BACnet Device" : "BACnet Object");

            var dv = Instance.host.NewDeviceRef(bacnetTypeString);  
            
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);




            //var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);    //this actually should be OK; coming from query string in URL
            if (nodeType == "device")   //bacnetNodeData will come in as the node data of the child node, so we need to overwrite
            {
                bacnetNodeData["node_type"] = "device";       //now done in calling function
                bacnetNodeData["object_type"] = "8";
                bacnetNodeData["object_instance"] = bacnetNodeData["device_instance"];
            }



            //TODO: the below will fail if we are importing, and the device is not actually on the network.  Still create in HomeSeer?

            BACnetObject bacnetObject = Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(bacnetNodeData);
            var bacnetDevice = bacnetObject.BacnetDevice;


            var addressParts = bacnetDevice.BacnetAddress.ToString().Split(":".ToCharArray());
            var ipAddress = addressParts[0];
            var udpPort = addressParts[1];

            bacnetNodeData["device_ip_address"] = ipAddress;
            bacnetNodeData["device_udp_port"] = udpPort;



            var nameProp = bacnetObject.GetBacnetProperty(BacnetPropertyIds.PROP_OBJECT_NAME);
            var objectName = nameProp.BacnetPropertyValue.Value.value[0].ToString();

            var idProp = bacnetObject.GetBacnetProperty(BacnetPropertyIds.PROP_OBJECT_IDENTIFIER);
            var objectId = idProp.BacnetPropertyValue.Value.value[0].ToString();

            bacnetNodeData["object_name"] = objectName;
            bacnetNodeData["object_identifier"] = objectId;     //want this whether device or object
            bacnetNodeData["object_type_string"] = objectId.Split(":".ToCharArray())[0];


            //bacnetNodeData["write_priority"] = "0";         //not really a persistent parameter; just useful when on config page, writing to properties

            VSVGPairs.VSPair Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
            String statusText = "";

            if (nodeType == "device")   //bacnetNodeData will come in as the node data of the child node, so we need to overwrite
            {

                //newDevice.set_Name(Instance.host, "BACnet Device - " + objectName); //Can include ID in the name cause why not

                //bacnetNodeDataString = bacnetNodeData.ToString();



                //TODO: set default polling interval here


                //leave this alone.  Store polling interval in config...

                MakeBACnetGraphicsAndStatus(dv);



                bacnetNodeData["polling_interval"] = "5000";    //default


                //Instance.bacnetDevices.updateDevicePollTimer(dv, 5000);



                //TODO: just for testing, so we can see pres. value.

                //var Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
                //Control.PairType = VSVGPairs.VSVGPairType.Range;
                //Control.RangeStart = 0;
                //Control.RangeEnd = 100000;
                //Control.Render = Enums.CAPIControlType.TextBox_Number;
                //var IS = Instance.host.DeviceVSP_AddPair(dv, Control);


                //TODO: so now the object HS device has to look at the present value of 


                bacnetNodeData["bacnet_device_hs_device"] = dv.ToString();
                //bacnetNodeData["bacnet_object_hs_device"] = dv.ToString();

            }
            else
            {
                //bacnetNodeData["parent_hs_device"] = parentDv.ToString();

                bacnetNodeData["bacnet_device_hs_device"] = parentDv.ToString();
                bacnetNodeData["bacnet_object_hs_device"] = dv.ToString();

                bacnetNodeData["write_priority"] = "0";  


                var parentDevId = (int)parentDv;

                Scheduler.Classes.DeviceClass parentHomeseerDevice = null;

                while (parentHomeseerDevice == null)
                {
                    System.Threading.Thread.Sleep(1000);
                    parentHomeseerDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(parentDevId);
                }

                //var parentHomeseerDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(parentDevId);

                parentHomeseerDevice.AssociatedDevice_Add(Instance.host, dv);
                parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.Parent_Root);

                newDevice.AssociatedDevice_Add(Instance.host, parentDevId);
                newDevice.set_Relationship(Instance.host, Enums.eRelationship.Child);


                //newDevice.set_Name(Instance.host, "BACnet Object - " + objectName); //Can include ID in the name cause why not



                //parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.);

                newDevice.set_Status_Support(Instance.host, true);      //not entirely sure about this yet.



                //VSVGPairs.VSPair Control;
                bool IS;

                if (bacnetObject.BacnetObjectId.Type == BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE)
                {

                    //string list will be list of state text


                //    statusText += @"
                
                //<script>
                //$(function() {
                
                //        $('#devicecontrol_" + dv + @" select').click($(this).change()); 

                
                //});

                //</script>
                
                
                
                //";


                    var stateTextProp = bacnetObject.GetBacnetProperty(BacnetPropertyIds.PROP_STATE_TEXT);


                    var strs = stateTextProp.ValueString().TrimStart("{".ToCharArray()).TrimEnd("}".ToCharArray()).Split(",".ToCharArray());


                    for (int i = 0; i < strs.Length; i++)
                    {
                        Control = new VSVGPairs.VSPair(ePairStatusControl.Control);
                        Control.PairType = VSVGPairs.VSVGPairType.SingleValue;


                        Control.Render_Location.Row = 1;
                        Control.Render_Location.Column = 1;
                        Control.Render_Location.ColumnSpan = 1;



                        //

                        Control.Value = i + 1;      //1-based indices for state text
                        Control.Status = (i + 1).ToString() + " (" + strs[i] + ")";

                        Control.IncludeValues = false;

                        //Control.StringListAdd = "blah";

                        //Control.StringListAdd = "blah2";


                        //Control.StringList = new String[] { "Release2" };

                        Control.Render = Enums.CAPIControlType.Single_Text_from_List;


                        IS = Instance.host.DeviceVSP_AddPair(dv, Control);



                    }



                    Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
                    Control.PairType = VSVGPairs.VSVGPairType.SingleValue;
                    //Control.RangeStart = -100000;
                    //Control.RangeEnd = 100000;
                    //Control.Render = Enums.CAPIControlType.TextBox_Number;
                    //Control.StringList = new String[] { "Release" };
                    //Control.StringListAdd = "Release";


                    Control.Render_Location.Row = 1;
                    Control.Render_Location.Column = 2;
                    Control.Render_Location.ColumnSpan = 1;



                    Control.Status = "Command";

                    //Control.

                    //ntrol.ControlUse = ePairControlUse.

                    //Control.ControlStatus = ePairStatusControl.

                    //Control.Value = "Release";

                    //Control.Render_Location = Enums.CAPIControlLocation.

                    Control.Render = Enums.CAPIControlType.Button;




                    IS = Instance.host.DeviceVSP_AddPair(dv, Control);




                }
                else
                {


                    Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
                    Control.PairType = VSVGPairs.VSVGPairType.Range;
                    Control.RangeStart = -100000;
                    Control.RangeEnd = 100000;
                    //Control.Render = Enums.CAPIControlType.TextBox_Number;

                    //Control.Status = "Command";   //no, leave this alone, messes up status number/graphics



                    Control.Render_Location.Row = 1;
                    Control.Render_Location.Column = 1;
                    Control.Render_Location.ColumnSpan = 1;


                    //Control.

                    Control.Render = Enums.CAPIControlType.TextBox_String;


                    IS = Instance.host.DeviceVSP_AddPair(dv, Control);

                }



                

                //Control.


                






                Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
                Control.PairType = VSVGPairs.VSVGPairType.SingleValue;
                //Control.RangeStart = -100000;
                //Control.RangeEnd = 100000;
                //Control.Render = Enums.CAPIControlType.TextBox_Number;
                //Control.StringList = new String[] { "Release" };
                //Control.StringListAdd = "Release";


                Control.Render_Location.Row = 2;
                Control.Render_Location.Column = 1;
                Control.Render_Location.ColumnSpan = 1;



                statusText = "Release" + @"
                
                <script>
                $(function() {
                
                        $('#dv_Control" + dv + @" span.ui-button-text').filter(function(){
                            return ($(this).text() == 'Submit');
                        }).text('Command');

                        $('#dv_Control" + dv + @" span.ui-button-text').each(function(){
                            $(this).children().first().removeClass('button_selected');
                        });


                   
                
                
                });
        
                
                </script>
                
                
                
                " + statusText;


                //Control.Status = statusText;






                //Control.

                //ntrol.ControlUse = ePairControlUse.

                //Control.ControlStatus = ePairStatusControl.

                //Control.Value = "Release";

                //Control.Render_Location = Enums.CAPIControlLocation.

                Control.Render = Enums.CAPIControlType.Button;
                //Control.Render = Enums.CAPIControlType.Button_Script;



                //IS = Instance.host.DeviceVSP_AddPair(dv, Control);
                //doesn't show up for multi-state value, for some reason
























   //Dim MyVSP As New VSPair(ePairStatusControl.Both)
   //     Dim i As Integer
   //     For i = 0 To _Buttons.Length - 1
   //         MyVSP.PairType = VSVGPairType.SingleValue
   //         MyVSP.Render_Location.Row = 0
   //         MyVSP.Render_Location.Column = 0
   //         MyVSP.Render_Location.ColumnSpan = 0
   //         MyVSP.Value = i
   //         MyVSP.Status = _Buttons(i)
   //         MyVSP.IncludeValues = False
   //         MyVSP.Render = Enums.CAPIControlType.TextList
   //         hs.DeviceVSP_AddPair(ref, MyVSP)
   //     Next






        //    Control = new VSVGPairs.VSPair(ePairStatusControl.Control);
        //    Control.PairType = VSVGPairs.VSVGPairType.Range;

        //    Control.RangeStart = -1;
        //    Control.RangeEnd = strs.Length - 1;


        //Control.Render_Location.Row = 1;
        //Control.Render_Location.Column = 1;
        //Control.Render_Location.ColumnSpan = 15;


        //    //Control.Status = "<script>console.log('blah');</script>";


        //    //Control.StringListAdd = "blah";

        //    //Control.StringListAdd = "blah2";


        //    //Control.StringList = new String[] { "Release2" };

        //    Control.Render = Enums.CAPIControlType.ValuesRange;


        //    IS = Instance.host.DeviceVSP_AddPair(dv, Control);





            }



            newDevice.set_Name(Instance.host, bacnetTypeString + " - " + objectName);


            newDevice.set_Location2(Instance.host, "BACnet"); //Location 2 is the Floor, can say whatever we want
            newDevice.set_Location(Instance.host, "System");

            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function



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


            //moved below
            //EDO.AddNamed("SSIDKey", siidDeviceData(bacnetNodeData.ToString())); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            ////Could be BACnet device or object, but either way turns into a HomeSeer device


            //newDevice.set_PlugExtraData_Set(Instance.host, EDO);


            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;  //Necessary for having the ability to control the device from the devices page using a defined action
            //may be useful later when we want to try actually writing to devices, however we decide to do it 
            newDevice.set_DeviceType_Set(Instance.host, DevINFO);





            





            //else

            if (nodeType == "object")
            {



                var writePriorityHsDv = bacnetObjectWritePriorityDevice((int)parentDv, dv, bacnetNodeData, bacnetTypeString + " - " + objectName);

                //These comments break input. Don't include them in the div
                /*                
                        //$('#devicecontrol_" + writePriorityHsDv + @" select').val('16');    //default
                        //$('select[name*=""droplist_" + writePriorityHsDv + @"""]').val('16');    //default


                        //console.log($('#dv_Control" + writePriorityHsDv + @" select'));*/

                statusText += @"
                
                <script>
                $(function() {


                        var wp = $('#dv_Status" + writePriorityHsDv + @" div').text();
                        $('#dv_Control" + writePriorityHsDv + @" select').val(wp);
                
                });

                </script>
                
                
                
                ";


                var thisObjType = Int32.Parse(bacnetNodeData["object_type"]);
                var msv = (int)(BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE);

                //if (Int32.Parse(bacnetNodeData["object_type"]) == (int)(BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE))
                if (bacnetObject.GetBacnetProperty(BacnetPropertyIds.PROP_PRIORITY_ARRAY) != null)
                {
                    var priorityArrayHsDv = bacnetObjectPriorityArrayDevice((int)parentDv, dv, bacnetNodeData, bacnetTypeString + " - " + objectName);

                    bacnetNodeData["priority_array_hs_device"] = priorityArrayHsDv.ToString();

                    //Instance.bacnetDevices.updateBacnetNodeData(dv, "priority_array_hs_device", priorityArrayHsDv.ToString());

                }



                Control.Status = statusText;
                Instance.host.DeviceVSP_AddPair(dv, Control);

                //Instance.bacnetDevices.UpdateBacnetObjectHsDeviceStatus(dv);


            }


            Console.WriteLine("Creating new HS bacnet device w/ node data: ");



            Console.WriteLine(bacnetNodeData.ToString());



            //var siidData = siidDeviceData(BACnetDevices.BuildJsonString(bacnetNodeData));

            var siidData = siidDeviceData(bacnetNodeData);

            Console.WriteLine("All SIID Data for this device: " + siidData);


            EDO.AddNamed("SSIDKey", siidData); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            //Could be BACnet device or object, but either way turns into a HomeSeer device


            newDevice.set_PlugExtraData_Set(Instance.host, EDO);



            var ped = newDevice.get_PlugExtraData_Get(Instance.host);

            Console.WriteLine("Retrieved SIID Data for this device: " + ped.ToString());

            Console.WriteLine(EDO.GetNamed("SSIDKey"));



            Instance.Devices.Add(new SiidDevice(Instance, newDevice));



            if (nodeType == "object")
                Instance.bacnetDevices.UpdateBacnetObjectHsDeviceStatus(dv);


            if (nodeType == "device")
                Instance.bacnetDevices.updateDevicePollTimer(dv, Int32.Parse(bacnetNodeData["polling_interval"]));  //creates a new poll timer for this device




            return dv;

        }






        //private Boolean isMatchingHomeseerBacnetNode(String bacnetNodeData, String homeseerNodeData, String nodeType)      //we may want to fetch an existing homeseer device node based on an incoming object node
        private Boolean isMatchingHomeseerBacnetNode(NameValueCollection bacnetNodeData, NameValueCollection hsBacnetNodeData, String nodeType)
        {


            //var bacnetNode = HttpUtility.ParseQueryString(bacnetNodeData);
            //var bacnetNode = BACnetDevices.ParseJsonString(homeseerNodeData);


            //var homeseerNode = HttpUtility.ParseQueryString(homeseerNodeData);
            //var homeseerNode = BACnetDevices.ParseJsonString(homeseerNodeData);



            foreach (var nodeProp in (nodeType == "device" ? BACnetTreeNode.DeviceNodeProperties : BACnetTreeNode.ObjectNodeProperties))
                if (bacnetNodeData[nodeProp] != hsBacnetNodeData[nodeProp])
                    return false;

            return true;
        }





        //Generates the plugin extra data stuff for BACnet Devices
        //Really only thing I want these to have at minimum is type, rawvalue,processedvalue
        public string siidDeviceData(NameValueCollection bacnetNodeData, Boolean isWritePriorityDev = false)
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);


            //.var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);
            //var bacnetNodeData = BACnetDevices.ParseJsonString(bacnetNodeDataString);


            var bacnetTypeString = (bacnetNodeData["node_type"] == "device" ? "BACnet Device" : "BACnet Object");


            if (isWritePriorityDev)
                bacnetTypeString += " (write priority)";


            parts["Type"] = bacnetTypeString;
            //...


            //parts["BACnetNodeData"] = bacnetNodeDataString;   //this is string representation of nodeData from original node in tree, representing BACnet device or object

            parts["BACnetNodeData"] = BACnetDevices.BuildJsonString(bacnetNodeData);

            //ex. ip_address=192.168.1.1&device_instance=400001

            //within here, "node_type" indicates whether device or object...



            //enough information to be able to uniquely identify it in network.
            //also tells us whether this is a BACnet device or a BACnet object.


            //sure, for now...will have to tap into Present_Value property if present.  Won't apply for devices.
            try
            {
               
                parts["RawValue"] = bacnetNodeData["Present_Value"].ToString();
                parts["ProcessedValue"] = bacnetNodeData["Present_Value"].ToString();
            }
            catch
            {
                parts["RawValue"] = "0";
                parts["ProcessedValue"] = "0";

            }
         


            return parts.ToString();

        }




        ////Generates the plugin extra data stuff for BACnet Devices
        ////Really only thing I want these to have at minimum is type, rawvalue,processedvalue
        //public string siidDeviceData(String bacnetNodeDataString, Boolean isWritePriorityDev = false)
        //{
        //    var parts = HttpUtility.ParseQueryString(string.Empty);


        //    //.var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);
        //    var bacnetNodeData = BACnetDevices.ParseJsonString(bacnetNodeDataString);


        //    var bacnetTypeString = (bacnetNodeData["node_type"] == "device" ? "BACnet Device" : "BACnet Object");


        //    if (isWritePriorityDev)
        //        bacnetTypeString += " (write priority)";


        //    parts["Type"] = bacnetTypeString;
        //    //...


        //    parts["BACnetNodeData"] = bacnetNodeDataString;   //this is string representation of nodeData from original node in tree, representing BACnet device or object
        //    //ex. ip_address=192.168.1.1&device_instance=400001

        //    //within here, "node_type" indicates whether device or object...



        //    //enough information to be able to uniquely identify it in network.
        //    //also tells us whether this is a BACnet device or a BACnet object.


        //    //sure, for now...will have to tap into Present_Value property if present.  Won't apply for devices.
        //    parts["RawValue"] = "0";
        //    parts["ProcessedValue"] = "0";


        //    return parts.ToString();

        //}




        //Makes status strings and associates graphics with the enw device. So we have some bacis stuff like "Device is down" or whatever
        //Can use custom images here or just use some homeseer defauilt images, whatever we ultimately decide.
        //Code below is just copied from modbus gateway placeholders, so should change
        public void MakeBACnetGraphicsAndStatus(int dv)
        {
            // Use these to show connection status of device.  May not be necessary/compatible with storing present value in device....


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
