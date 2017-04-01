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





        public void addSSIDExtraData(Scheduler.Classes.DeviceClass Device, string Key, string value)
        {


            var EDO = Device.get_PlugExtraData_Get(Instance.host);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            parts[Key] = value;
            EDO.RemoveNamed("SSIDKey");
            EDO.AddNamed("SSIDKey", parts.ToString());
            Device.set_PlugExtraData_Set(Instance.host, EDO);

        }




        public void updateBacnetNodeData(Scheduler.Classes.DeviceClass Device, string Key, string value)
        {


            var EDO = Device.get_PlugExtraData_Get(Instance.host);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

            var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);
            bacnetNodeData[Key] = value;
            parts["BACnetNodeData"] = bacnetNodeData.ToString();

            parts[Key] = value;
            EDO.RemoveNamed("SSIDKey");
            EDO.AddNamed("SSIDKey", parts.ToString());
            Device.set_PlugExtraData_Set(Instance.host, EDO);

        }


       public BACnetObject getBacObj(NameValueCollection bacnetNodeData)
       {

            try
            {
                BACnetObject bno = null;
                if (bacnetNodeData["node_type"] == "device")
                {
                    var d = this.Instance.bacnetDataService.GetBacnetDevice(bacnetNodeData, true);  //still bug in this, somehow...
                    bno = d.DeviceObject;
                }
                else
                {
                    bno = this.Instance.bacnetDataService.GetBacnetObject(bacnetNodeData, true);
                }

                return bno;


            }

            catch (Exception ex)
            {
                return null;

                //ModbusConfHtml.add("Error: ", "Could not connect to device to retrieve object properties.  Please make sure that the BACnet device is present on the network.");
            }

       }





        public string parseBacnetDeviceTab(string data)
        {

            Console.WriteLine("ConfigDevicePost: " + data);


            System.Collections.Specialized.NameValueCollection changed = null;
            changed = System.Web.HttpUtility.ParseQueryString(data);


            string partID = changed["id"].Split(new string[] { "__" }, StringSplitOptions.None)[1];
            int devId = Int32.Parse(changed["id"].Split(new string[] { "__" }, StringSplitOptions.None)[0]);
            Scheduler.Classes.DeviceClass device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(devId);
            var newVal = changed["value"];

            if (newVal == null)
                newVal = changed[changed["id"]];


            var EDO = device.get_PlugExtraData_Get(Instance.host);
            var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            var bacnetNodeDataString = parts["BACnetNodeData"];
            var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);


            if (bacnetNodeData[partID] != null)
            {

                //updateBacnetNodeData(device, partID, newVal);     probably don't let them edit this....


            }
            else
            {



                BACnetObject bno = getBacObj(bacnetNodeData);   //even if device, gets the device object

                if (!bno.AllPropertiesFetched)
                    bno.FetchProperties();      //expensive, but have to make sure property exists...

                //property can be identified uniquely by its ID...don't need to generate a string.
                var propId = (BacnetPropertyIds)(Int32.Parse(partID));

                var prop = bno.GetBacnetProperty(propId);


                IList<BacnetValue> vals = prop.BacnetPropertyValue.Value.value;

                BacnetApplicationTags propTag = vals[0].Tag;


                //object valToWrite;        // = vals[0].Value;
                Type t = newVal.GetType();


                String propElem;
                switch (propTag)
                {
                    case BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN:
                        var isSet = (newVal == "checked");
                        prop.WriteValue(isSet);
                        break;
                    case BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL:
                        var numVal = Single.Parse(newVal);
                        prop.WriteValue(numVal);
                        break;
                    default:
                        prop.WriteValue(newVal);
                        break;
                }

            }



            return "";

        }




        public string BACnetObjectPropertiesTable(BACnetObject bno)
        {
            var stb = new StringBuilder();

            stb.Append("<div><table id='bacnetObjProps'></table></div>");


            string basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));


            stb.Append("<link rel='stylesheet' type='text/css' href='https://cdn.datatables.net/v/ju/dt-1.10.13/datatables.min.css' />");
            stb.Append("<script type='text/javascript' src='https://cdn.datatables.net/v/ju/dt-1.10.13/datatables.min.js'></script>");



            String tableJs = File.ReadAllText(Path.Combine(basePath, "js", "bacnetPropertiesTable.js"));
            stb.Append("<script>" + tableJs + "</script>");

            var props = new JavaScriptSerializer().Serialize(bno.GetProperties());
            stb.Append(String.Format("<script>var propsList = {0};</script>", props));


            stb.Append("<script>buildHtmlTable(propsList, '#bacnetObjProps');</script>");


            return stb.ToString();
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


            StringBuilder stb = new StringBuilder();
            htmlBuilder ModbusBuilder = new htmlBuilder("BACnetDevTab" + Instance.ajaxName);
            htmlTable ModbusConfHtml = ModbusBuilder.htmlTable();


            //First just generate display fields based on node data.

            var bacNodeTyp = (bacnetNodeData["node_type"] == "device" ? "Device" : "Object");
            ModbusConfHtml.addT("Associated BACnet " + bacNodeTyp + ":");


            ModbusConfHtml.add("IP Address: ", ModbusBuilder.stringInput(dv1 + "__ip_address", bacnetNodeData["ip_address"]).print());

            //new clsJQuery.jqTextBox("TriggerWeight" + sUnique, "number", Trig1.TriggerWeight.ToString(), "Events", 8, true);



            ModbusConfHtml.add("Device Instance: ", ModbusBuilder.numberInput(dv1 + "__device_instance", Int32.Parse(bacnetNodeData["device_instance"])).print());



            if (bacnetNodeData["node_type"] == "object")
            {

                ModbusConfHtml.add("Object Type: ", ModbusBuilder.stringInput(dv1 + "__object_type", ((BacnetObjectTypes)Int32.Parse(bacnetNodeData["object_type"])).ToString()).print());


                ModbusConfHtml.add("Object Instance: ", ModbusBuilder.numberInput(dv1 + "__object_instance", Int32.Parse(bacnetNodeData["object_instance"])).print());


            }



            ModbusConfHtml.addT("BACnet Object Properties: ");


            //make non-editable...?


            BACnetObject bno = getBacObj(bacnetNodeData);
            if (bno == null)
            {
                ModbusConfHtml.add("", "Error: could not connect to device to retrieve object properties.  Please make sure that the BACnet device is present on the network.");

            }

            else
            {



                bno.FetchProperties();


                //for now, just generate HTML table still.

                foreach (var bacnetProperty in bno.BacnetProperties)
                {
                    BacnetPropertyIds propId = bacnetProperty.Key;

                    if (propId == BacnetPropertyIds.PROP_OBJECT_IDENTIFIER || propId == BacnetPropertyIds.PROP_OBJECT_TYPE)
                        continue;

                    var propIdNum = (int)propId;
                    BACnetProperty prop = bacnetProperty.Value;

                    IList<BacnetValue> vals = prop.BacnetPropertyValue.Value.value;



                    BacnetApplicationTags propTag = vals[0].Tag;


                    object prop_val = vals[0].Value;
                    Type t = prop_val.GetType();


                    String propElemId = dv1 + "__" + propIdNum;


                    String propElem;
                    switch (propTag)
                    {
                        case BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN:

						    clsJQuery.jqCheckBox CB1 = new clsJQuery.jqCheckBox(propElemId, "", "BACnetDevTab", true, false);

                            if ((Boolean)prop_val)
                                CB1.@checked = true;

                                propElem = CB1.Build();
                            break;

                        default:
                                propElem = ModbusBuilder.stringInput(propElemId, prop.ValueString()).print();
                            break;

                    }


                    ModbusConfHtml.add(prop.Name + ": ", propElem);

                    //TODO: depending on property type, just 

                }






                    //    clsJQuery.jqDropList DL = new clsJQuery.jqDropList("Action1TypeList" + sUnique, "Events", true);
                    //    DL.AddItem("(Not Set)", "0", true);
                    //    DL.AddItem("Round Tonnage", "1", false);
                    //    DL.AddItem("Unrounded Tonnage", "2", false);
                    //    st.Append("Set Weight Option Mode:" + DL.Build());
                    //} else {
                    //    clsJQuery.jqCheckBox CB1 = new clsJQuery.jqCheckBox("Action1Type" + sUnique, "", "Events", true, true);
                    //    if (Act1.SetTo == Classes.MyAction1EvenTon.eSetTo.Rounded) {
                    //        CB1.@checked = true;
                    //        st.Append("Uncheck to revert to Unrounded weights:");
                    //    } else {
                    //        CB1.@checked = false;
                    //        st.Append("Check to change to Rounded weights:");
                    //    }
                    //    st.Append(CB1.Build());






            }






            //yes, first step is always to get the physical object we're working with.  Though maybe can generate some other stuff first....

            //IP address, device instance, device 





            //somehow store snapshot of all properties into device after generation.  This way, can be reloaded if lost connection.
            //Wait, is this right?  Properties aren't persistent to homeseer device, but to bacnet device.  The homeseer device just needs a persistent way of communicating with bacnet device.
            //Should be able to change ip address and device instance accordingly.  Maybe....again, just do this later.
            //Kinda makes sense that if the device goes offline, config tab doesn't show up.


            //OK, nevermind, at least display them.


            //remember device name not stored, it's just



            //int DefGateway = ModbusGates.IndexOf(Item);
            //ModbusConfHtml.add("Modbus Gateway ID: ", ModbusBuilder.selectorInput(GatewayStringArray, dv + "_GateID", "GateID", DefGateway).print());
            //ModbusConfHtml.add("Selector Type: ", ModbusBuilder.selectorInput(RegTypeArray, dv + "_RegisterType", "RegisterType", Convert.ToInt32(parts["RegisterType"])).print());
            //ModbusConfHtml.add("Slave ID: ", ModbusBuilder.numberInput(dv + "_SlaveId", Convert.ToInt32(parts["SlaveId"])).print());
            //ModbusConfHtml.add("Register Address: ", "<div style:'display:inline;'><div style='float:left;'>" + ModbusBuilder.numberInput(dv + "_RegisterAddress", Convert.ToInt32(parts["RegisterAddress"])).print() + "</div><div style='float:left;' id='TrueAdd'>()</div></div>");


            ////0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
            ////tells us how many registers to read/write and also how to parse returns
            ////note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
            ////So coil and discrete are bool ONLY
            ////Rest are 16 bit stuff and every mutiple of 16 is number of registers to read
            //ModbusConfHtml.add("Return Type: ", ModbusBuilder.selectorInput(RetTypeArray, dv + "_ReturnType", "RegisterType", Convert.ToInt32(parts["ReturnType"])).print());
            //ModbusConfHtml.add("Signed Value: ", ModbusBuilder.checkBoxInput(dv + "_SignedValue", Boolean.Parse(parts["SignedValue"])).print());
            //ModbusConfHtml.add(" Calculator: ", "<div style: 'display:inline;'><div style = 'float:left;'> " + ModbusBuilder.stringInput(dv + "_ScratchpadString", parts["ScratchpadString"]).print() + "</div><div style='float:left;' id='HelpText'>$(DeviceID) is the raw value for the homeseer device with ID DeviceID. #(DeviceID) is the value resulting from the device's calculator. Any SIID device's value can be called here.</div>");
            //ModbusConfHtml.add("Display Format: ", ModbusBuilder.stringInput(dv + "_DisplayFormatString", parts["DisplayFormatString"]).print());
            //ModbusConfHtml.add("Read Only Device: ", ModbusBuilder.checkBoxInput(dv + "_ReadOnlyDevice", Boolean.Parse(parts["ReadOnlyDevice"])).print());



            //ModbusConfHtml.add("Device Enabled: ", ModbusBuilder.checkBoxInput(dv + "_DeviceEnabled", Boolean.Parse(parts["DeviceEnabled"])).print());
            




            ////TODO: for now, return object.GetProperties() and format it into a table.  But ultimately, need to display widgets....
            ////Look at how it's done on modbus page - string inputs, numeric, drop-downs.  Are there any other types we need?




            //TODO: get device from actual network based on 




            //if (Int32.Parse(bacnetNodeData["object_type"]) == BacnetObjectTypes.OBJECT_DEVICE.ToInt())
            //{



            //}

            stb.Append(ModbusConfHtml.print());



            foreach (var nodeDataProp in bacnetNodeData.Keys)
            {
                stb.Append(String.Format("<script>$('#{0}__{1}').prop('disabled', true);</script>", dv1, nodeDataProp));

            }


            //string dv = "" + dv1 + "";

            //StringBuilder stb = new StringBuilder();
            //stb.Append("SO HERE WE CAN PUT BACNET SPECIFIC STUFF<br>");
            //stb.Append("Generate it as HTML<br> THE CURRENT QUERY STRING IS:");




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
