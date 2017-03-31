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
using System.IO.BACnet;

namespace HSPI_SIID.BACnet
{
   public class BACnetHomeSeerDevicePage : PageBuilderAndMenu.clsPageBuilder
    {
        public InstanceHolder Instance { get; set; }


        //public htmlBuilder BACnetBuilder { get; set; }

        //public List<string> DiscoveredBACnetDevices { get; set; }


        public String PageName { get; set; }


        public BACnetHomeSeerDevicePage(string pagename, InstanceHolder instance)
            : base(pagename)
        {

            Instance = instance;    //hmm.  Aren't we still accessing same network data, even if multiple instances?  Is a lock necessary?
            //BACnetBuilder = new htmlBuilder("BACnetObjectDataService" + Instance.ajaxName);
            //DiscoveredBACnetDevices = new List<string>();

            this.PageName = pagename + instance.ajaxName;

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
        public string addOrEditBacnetHomeseerDevice(string bacnetNodeDataString)   //remember BacnetTreeNodeData is as query string, not JSON object
        {
            // queryString contains nodeData of tree node from which to make device
            //remember BacnetTreeNodeData is as query string, not JSON object


            System.Collections.Specialized.NameValueCollection bacnetNodeData = null;
            bacnetNodeData = System.Web.HttpUtility.ParseQueryString(bacnetNodeDataString);

            var nodeType = bacnetNodeData["node_type"];


            int? dv;

            dv = getExistingBacnetHomeseerDevice(bacnetNodeDataString, "device") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "device");

            if (nodeType == "object")
                dv = getExistingBacnetHomeseerDevice(bacnetNodeDataString, "object") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "object", dv);



            //dv = getExistingBacnetHomeseerDevice(bacnetNodeDataString, "device") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "device");

            //if (nodeType == "object")
            //    dv = getExistingBacnetHomeseerDevice(bacnetNodeDataString, "object") ?? makeNewHomeseerBacnetNodeDevice(bacnetNodeDataString, "object");


            return "/deviceutility?ref=" + dv + "&edit=1";


            //they can also be routed here if they are trying to access an existing device.

            StringBuilder stb = new StringBuilder();
            //BACnetDevicePage page = this;
            stb.Append("<meta http-equiv=\"refresh\" content = \"0; URL='/deviceutility?ref=" + dv + "&edit=1'\" />"); 
            //This will refresh the page and take the browser to the new device's config page
            //May not work on NotChrome
            this.AddBody(stb.ToString());
            return this.BuildPage();

        }







        private int? getExistingBacnetHomeseerDevice(String bacnetNodeData, String nodeType)      //now using 'node' to mean a bacnet device or object
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



        private int? makeNewHomeseerBacnetNodeDevice(String bacnetNodeData, String nodeType, int? parentDv = null)    //only supplied if nodeType is object.
        {

            var dv = Instance.host.NewDeviceRef("BACnet Device");   //TODO: do I need to name it something else?  Maybe at least differentiate between device and object?
            MakeBACnetGraphicsAndStatus(dv);
            Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
            newDevice.set_InterfaceInstance(Instance.host, Instance.name);


            

            newDevice.set_Location2(Instance.host, "BACnet"); //Location 2 is the Floor, can say whatever we want
            newDevice.set_Location(Instance.host, "System");

            newDevice.set_Interface(Instance.host, Util.IFACE_NAME); //Needed to link device to plugin, so the tab calls back to the correct hardcoded homeseer function



            if (nodeType == "device")
            {
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

        


        public string AllBacnetDevices()
        {//gets list of all associated devices. 
            //Get the collection of these devices which are bacnet
            //build Devices table with the appropriate data displays

            StringBuilder sb = new StringBuilder();
            htmlBuilder BACnetBuilder = new htmlBuilder("BacnetDev" + Instance.ajaxName);

            htmlTable bacnetConfHtml = BACnetBuilder.htmlTable(800);

            List<Scheduler.Classes.DeviceClass> BACnetDevs = Instance.bacPage.getParentBacnetDevices().ToList();
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
                bacnetConfHtml.addSubHeader("Enabled", "BACnet Object", "", "", "");  //, "Type", "Format");     //maybe put in, i.e. object identifier?



                List<Scheduler.Classes.DeviceClass> BACnetObjs = Instance.bacPage.getChildBacnetDevices(bacnetNodeData["device_instance"]).ToList(); ;



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





       
       //maybe not
        public string AllModbusDevices()
        {//gets list of all associated devices. 
            //Get the collection of these devices which are modbus gateways or devices
            //build Gateway / Devices table with the appropriate links and the appropriate Add Device buttons
            //returns the built html string
            StringBuilder sb = new StringBuilder();
            htmlBuilder ModbusBuilder = new htmlBuilder("AddModbusDevice" + Instance.ajaxName);

            List<int> ModbusGates = Instance.modPage.getAllGateways().ToList();
            List<Scheduler.Classes.DeviceClass> ModbusDevs = new List<Scheduler.Classes.DeviceClass>();

            //foreach (int GID in ModbusGates)
            //{
            //    Scheduler.Classes.DeviceClass Dev = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(Convert.ToInt32(GID));
            //    var EDO = Dev.get_PlugExtraData_Get(Instance.host);
            //    var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            //    StringBuilder updatedList = new StringBuilder();
            //    foreach (var subId in parts["LinkedDevices"].Split(','))
            //    {
            //        try
            //        {
            //            Scheduler.Classes.DeviceClass MDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(Convert.ToInt32(subId));
            //            if (MDevice != null)
            //            {
            //                ModbusDevs.Add(MDevice);
            //                updatedList.Append(subId + ",");
            //            }
            //        }
            //        catch
            //        {

            //        }


            //    }
            //    parts["LinkedDevices"] = updatedList.ToString();
            //    EDO.RemoveNamed("SSIDKey");
            //    EDO.AddNamed("SSIDKey", parts.ToString());
            //    Dev.set_PlugExtraData_Set(Instance.host, EDO);
            //}

            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable(800);
            sb.Append("<br>");
            foreach (int GateRef in ModbusGates)
            {
                Scheduler.Classes.DeviceClass Gateway = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(GateRef);
                ModbusConfHtml.addDevHeader("Gateway");
                Gateway.get_Image(Instance.host);
                Gateway.get_Name(Instance.host);
                ModbusConfHtml.addDevMain(ModbusBuilder.MakeImage(16, 16, Gateway.get_Image(Instance.host)).print() +
                    ModbusBuilder.MakeLink("/deviceutility?ref=" + GateRef
                    + "&edit=1", Gateway.get_Name(Instance.host)).print(), ModbusBuilder.Qbutton("G_" + GateRef, "Add Device").print());
                sb.Append(ModbusConfHtml.print());
                ModbusConfHtml = ModbusBuilder.htmlTable(800);
                ModbusConfHtml.addSubHeader("Enabled", "Device Name", "Address", "Type", "Format");


                foreach (Scheduler.Classes.DeviceClass MDevice in ModbusDevs)
                {

                    if (MDevice != null)
                    {
                        var EDO = MDevice.get_PlugExtraData_Get(Instance.host);
                        var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                        if (Convert.ToInt32(parts["GateID"]) == GateRef)
                        {
                            ModbusConfHtml.addSubMain(ModbusBuilder.MakeImage(16, 16, MDevice.get_Image(Instance.host)).print(),
                               ModbusBuilder.MakeLink("/deviceutility?ref=" + MDevice.get_Ref(Instance.host) + "&edit=1", MDevice.get_Name(Instance.host)).print(),
                               parts["SlaveId"],
                               Instance.modPage.GetReg(parts["RegisterType"]),
                               Instance.modPage.GetRet(parts["ReturnType"]));

                        }
                    }


                }
                sb.Append(ModbusConfHtml.print());
                sb.Append("<br>");
                ModbusConfHtml = ModbusBuilder.htmlTable(800);
            }


            Instance.modPage.UpdateGateList(ModbusGates.ToArray());
            return sb.ToString();
        }






            //public List<Scheduler.Classes.DeviceClass> getAllBacnetDevices()
            //{
            //    List<Scheduler.Classes.DeviceClass> listOfDevices = new List<Scheduler.Classes.DeviceClass>();

            //    Scheduler.Classes.clsDeviceEnumeration DevNum = (Scheduler.Classes.clsDeviceEnumeration)Instance.host.GetDeviceEnumerator();
            //    var Dev = DevNum.GetNext();
            //    while (Dev != null)
            //    {
            //        try
            //        {
            //            var EDO = Dev.get_PlugExtraData_Get(Instance.host);
            //            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            //            string s = parts["Type"];
            //            if (parts["Type"] == "BACnet Device")
            //            {
            //                if ((Dev.get_Interface(Instance.host).ToString() == Util.IFACE_NAME.ToString()) && (Dev.get_InterfaceInstance(Instance.host) == Instance.name)) //Then it's one of ours
            //                {
            //                    listOfDevices.Add(Dev);
            //                }
            //            }
            //        }
            //        catch
            //        {

            //        }
            //        Dev = DevNum.GetNext();


            //    }
            //    return listOfDevices;


            //}



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
