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



            foreach (Scheduler.Classes.DeviceClass hsBacnetDevice in BACnetDevs)
            {
                var EDO = hsBacnetDevice.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);

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
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["device_ip_address"]));
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["device_udp_port"]));
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["object_type_string"]));
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["device_instance"]));
                row.Append(String.Format(tdString, "class='tableroweven'", "", bacnetNodeData["object_name"]));
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
                bacnetConfHtml.addRow(row.ToString());





                foreach (Scheduler.Classes.DeviceClass bacnetObject in BACnetObjs)
                {

                    var EDO2 = bacnetObject.get_PlugExtraData_Get(Instance.host);
                    var parts2 = HttpUtility.ParseQueryString(EDO2.GetNamed("SSIDKey").ToString());
                    var bacnetNodeData2 = HttpUtility.ParseQueryString(parts2["BACnetNodeData"]);
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


            foreach (SiidDevice Dev in Instance.Devices)
            {
                var hsBacnetDevice = Dev.Device;

                try
                {
                    var EDO = hsBacnetDevice.get_PlugExtraData_Get(Instance.host);
                    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                    string s = parts["Type"];
                    var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);
                    if (parts["Type"] == "BACnet Object" && belongsToThisInstance(hsBacnetDevice) && bacnetNodeData["node_type"] == "object" && bacnetNodeData["device_instance"] == bacnetDeviceInstance)
                        //listOfDevices.Add(hsBacnetDevice);
                        devIds.Add(Dev.Ref);
                }
                catch
                {

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









        public int? getExistingHomeseerBacnetNodeDevice(String bacnetNodeData, String nodeType)      //now using 'node' to mean a bacnet device or object
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

                    if (parts["Type"] == bacnetTypeString)   //parts["BacnetTreeNodeData"]
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




        private int bacnetObjectWritePriorityDevice(int deviceDv, int objDv, NameValueCollection bacnetNodeData, String name)      //parentDv is for the device device
        {
            var dv = Instance.host.NewDeviceRef("BACnet Object (write priority)");

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


                //bacnetNodeData[""]


                //newDevice.set_Name(Instance.host, "BACnet Object - " + objectName); //Can include ID in the name cause why not



                //parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.);

                newDevice.set_Status_Support(Instance.host, true);      //not entirely sure about this yet.


                Instance.host.SetDeviceValueByRef(dv, 0, false);
                Instance.host.SetDeviceString(dv, "0", false);

                //but shouldn't it update when you change?


                //TODO: add the different write priorities here.


                //I//nstance.hspi.










                var Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
                Control.PairType = VSVGPairs.VSVGPairType.Range;


                //Control.


                //Control.Value = 2.0;

                Control.RangeStart = 0;
                Control.RangeEnd = 16;

                //Control.

                //Control.

                //Control.Value = 2;



                //Control.Render = Enums.CAPIControlType.TextBox_Number;
                Control.Render = Enums.CAPIControlType.ValuesRange;
                var IS = Instance.host.DeviceVSP_AddPair(dv, Control);

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



            


            EDO.AddNamed("SSIDKey", siidDeviceData(bacnetNodeData.ToString(), true)); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            //Could be BACnet device or object, but either way turns into a HomeSeer device


            newDevice.set_PlugExtraData_Set(Instance.host, EDO);


            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;  //Necessary for having the ability to control the device from the devices page using a defined action
            //may be useful later when we want to try actually writing to devices, however we decide to do it 
            newDevice.set_DeviceType_Set(Instance.host, DevINFO);





            Instance.Devices.Add(new SiidDevice(Instance, newDevice));



            return dv;




        }






        private int? makeNewHomeseerBacnetNodeDevice(String bacnetNodeDataString, String nodeType, int? parentDv = null)    //only supplied if nodeType is object.
        {

            var bacnetTypeString = (nodeType == "device" ? "BACnet Device" : "BACnet Object");

            var dv = Instance.host.NewDeviceRef(bacnetTypeString);  
            
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);




            var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);
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



            }
            else
            {
                //bacnetNodeData["parent_hs_device"] = parentDv.ToString();



                bacnetNodeData["write_priority"] = "0";  


                var parentDevId = (int)parentDv;
                var parentHomeseerDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(parentDevId);

                parentHomeseerDevice.AssociatedDevice_Add(Instance.host, dv);
                parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.Parent_Root);

                newDevice.AssociatedDevice_Add(Instance.host, parentDevId);
                newDevice.set_Relationship(Instance.host, Enums.eRelationship.Child);


                //newDevice.set_Name(Instance.host, "BACnet Object - " + objectName); //Can include ID in the name cause why not



                //parentHomeseerDevice.set_Relationship(Instance.host, Enums.eRelationship.);

                newDevice.set_Status_Support(Instance.host, true);      //not entirely sure about this yet.





                var Control = new VSVGPairs.VSPair(ePairStatusControl.Both);
                Control.PairType = VSVGPairs.VSVGPairType.Range;
                Control.RangeStart = -100000;
                Control.RangeEnd = 100000;
                //Control.Render = Enums.CAPIControlType.TextBox_Number;
                Control.Render = Enums.CAPIControlType.TextBox_String;
                var IS = Instance.host.DeviceVSP_AddPair(dv, Control);








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



            EDO.AddNamed("SSIDKey", siidDeviceData(bacnetNodeData.ToString())); //Made it so all SIID devices have all their device data in the extra data store under the key SIIDKey
            //Could be BACnet device or object, but either way turns into a HomeSeer device


            newDevice.set_PlugExtraData_Set(Instance.host, EDO);


            var DevINFO = new DeviceTypeInfo_m.DeviceTypeInfo();
            DevINFO.Device_API = DeviceTypeInfo_m.DeviceTypeInfo.eDeviceAPI.Plug_In;  //Necessary for having the ability to control the device from the devices page using a defined action
            //may be useful later when we want to try actually writing to devices, however we decide to do it 
            newDevice.set_DeviceType_Set(Instance.host, DevINFO);





            Instance.Devices.Add(new SiidDevice(Instance, newDevice));




            if (nodeType == "device")
                Instance.bacnetDevices.updateDevicePollTimer(dv, Int32.Parse(bacnetNodeData["polling_interval"]));  //creates a new poll timer for this device
            else
            {



                bacnetObjectWritePriorityDevice((int)parentDv, dv, bacnetNodeData, bacnetTypeString + " - " + objectName);

            }


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
        public string siidDeviceData(String bacnetNodeDataString, Boolean isWritePriorityDev = false)
        {
            var parts = HttpUtility.ParseQueryString(string.Empty);

            var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);

            var bacnetTypeString = (bacnetNodeData["node_type"] == "device" ? "BACnet Device" : "BACnet Object");


            if (isWritePriorityDev)
                bacnetTypeString += " (write priority)";


            parts["Type"] = bacnetTypeString;
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
