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
using System.Web.Script.Serialization;
using System.IO;
using System.IO.BACnet;
using System.Reflection;

namespace HSPI_SIID.BACnet
{
   public class BACnetDevices : PageBuilderAndMenu.clsPageBuilder
    {
        public InstanceHolder Instance { get; set; }
        //public htmlBuilder BACnetBuilder { get; set; }

        //public List<string> DiscoveredBACnetDevices { get; set; }

        public static string BaseUrl = "BACnetDevices";


        public static HashSet<string> WhoIsBeingPolled = new HashSet<string>();
        public static HashSet<string> WhoIsBeingWritten = new HashSet<string>();


        //public String PageName { get; set; }

        private static int tableWidth = 980;

        private static string tableClass = "hsBacnetDevice";

        public BACnetDevices(string pagename, InstanceHolder instance) : base(pagename)
        {

            Instance = instance;
            //BACnetBuilder = new htmlBuilder("BACnetPage" + Instance.ajaxName);
            //DiscoveredBACnetDevices = new List<string>();

            this.PageName = pagename + instance.ajaxName;

            //PageName = BaseUrl + Instance.ajaxName.Replace(":", "_");
        }

 


            //public string BACnetObjectPropertiesTable(BACnetObject bno)
            //{
            //    var stb = new StringBuilder();

            //    stb.Append("<div><table id='bacnetObjProps'></table></div>");


            //    string basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));


            //    stb.Append("<link rel='stylesheet' type='text/css' href='https://cdn.datatables.net/v/ju/dt-1.10.13/datatables.min.css' />");
            //    stb.Append("<script type='text/javascript' src='https://cdn.datatables.net/v/ju/dt-1.10.13/datatables.min.js'></script>");



            //    String tableJs = File.ReadAllText(Path.Combine(basePath, "js", "bacnetPropertiesTable.js"));
            //    stb.Append("<script>" + tableJs + "</script>");

            //    var props = new JavaScriptSerializer().Serialize(bno.GetProperties());
            //    stb.Append(String.Format("<script>var propsList = {0};</script>", props));


            //    stb.Append("<script>buildHtmlTable(propsList, '#bacnetObjProps');</script>");


            //    return stb.ToString();
            //}







        public void ReadWriteBacnet(Scheduler.Classes.DeviceClass hsBacnetDev, String controlValue = null)
        {
            try
            {
                //var devID = ActionIn.Ref;

                var bacnetNodeData = getBacnetNodeData(hsBacnetDev);


                BACnetObject bno = this.Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(bacnetNodeData);   //even if device, gets the device object
                //can we make changes to the object, but keep this same reference?


                if (!bno.AllPropertiesFetched)
                    bno.FetchProperties();      //expensive, but have to make sure property exists...

                //property can be identified uniquely by its ID...don't need to generate a string.


                //var propId = (BacnetPropertyIds)(Int32.Parse(propIdString));

                var prop = bno.GetBacnetProperty(BacnetPropertyIds.PROP_PRESENT_VALUE);



                    var writePriority = Int32.Parse(bacnetNodeData["write_priority"] ?? "0");   //is this a good default?



                    if (controlValue == null)
                        prop.WriteValue(null, writePriority);
                    else
                    {



                        //var writePriority = 16;


                        var propTag = prop.BacnetPropertyValue.Value.value[0].Tag;


                        switch (propTag)
                        {
                            case (BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL):
                                {
                                    var singleVal = Single.Parse(controlValue.ToString());
                                    prop.WriteValue(singleVal, writePriority);
                                    break;
                                }
                            case (BacnetApplicationTags.BACNET_APPLICATION_TAG_UNSIGNED_INT):
                                {
                                    var uintVal = UInt32.Parse(controlValue.ToString());
                                    prop.WriteValue(uintVal, writePriority);
                                    break;
                                }


                        }

                    }





                //after writing



                bno.FetchProperties();

                prop = bno.GetBacnetProperty(BacnetPropertyIds.PROP_PRESENT_VALUE);

                //if (propId == BacnetPropertyIds.PROP_PRESENT_VALUE)
                //{
                    UpdateExtraData(hsBacnetDev, "RawValue", prop.ValueString());
                    UpdateExtraData(hsBacnetDev, "ProcessedValue", prop.ValueString());
                //}


            }
            catch
            {

            }

        }




        public void ChangeWritePriority(SiidDevice siidDev, Double controlValue)
        {
            try
            {
                //var devID = ActionIn.Ref;


                Scheduler.Classes.DeviceClass hsBacnetWritePriorityDev = siidDev.Device;

                int dvRef = siidDev.Ref;

                var bacnetNodeData = getBacnetNodeData(hsBacnetWritePriorityDev);


                var hsBacnetObjectDevRef = Convert.ToInt32(bacnetNodeData["bacnet_object_hs_device"]);


                var hsBacnetObjectDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(hsBacnetObjectDevRef);



                //var hsBacnetObjectNodeData = getBacnetNodeData(hsBacnetObjectDevice);


                //hsBacnetObjectNodeData["write_priority"] = controlValue;





                var writePriority = Int32.Parse(controlValue.ToString());


                Instance.host.SetDeviceValueByRef(dvRef, writePriority, true);
                Instance.host.SetDeviceString(dvRef, writePriority.ToString(), true);





                updateBacnetNodeData(hsBacnetObjectDevice, "write_priority", writePriority.ToString());



            }
            catch
            {

            }

        }










        public string BuildHomeseerDeviceProperties(int dv1, NameValueCollection bacnetNodeData)
        {

            StringBuilder stb = new StringBuilder();
            htmlBuilder builder = new htmlBuilder(this.PageName);
            htmlTable confHtml = builder.htmlTable();
            confHtml.tableClass = tableClass;
            confHtml.header = "<table border='0' cellpadding='0' cellspacing='0' width='" + tableWidth + "' class='" + tableClass + "' style='margin-bottom: 10px;'><tbody>";

            //First just generate display fields based on node data.

            var bacNodeTyp = (bacnetNodeData["node_type"] == "device" ? "Device" : "Object");
            confHtml.addT("Associated BACnet " + bacNodeTyp + ":");
            confHtml.add("IP Address: ", builder.stringInput(dv1 + "__ip_address", bacnetNodeData["ip_address"]).print());
            confHtml.add("Device Instance: ", builder.numberInput(dv1 + "__device_instance", Int32.Parse(bacnetNodeData["device_instance"])).print());



            if (bacnetNodeData["node_type"] == "device")
            {
                //confHtml.add("Polling interval (ms): ", builder.numberInput(dv1 + "__polling_interval", Int32.Parse(bacnetNodeData["polling_interval"] ?? "5000")).print());
                var pollingInterval = bacnetNodeData["polling_interval"] ?? "5000";
                confHtml.add("Polling interval (ms): ", new clsJQuery.jqTextBox(dv1 + "__polling_interval", "number", pollingInterval,
                    this.PageName, 16, false).Build());
            } 
            else
            {
                confHtml.add("Object Type: ", builder.stringInput(dv1 + "__object_type", ((BacnetObjectTypes)Int32.Parse(bacnetNodeData["object_type"])).ToString()).print());
                confHtml.add("Object Instance: ", builder.numberInput(dv1 + "__object_instance", Int32.Parse(bacnetNodeData["object_instance"])).print());
            }






            //clsJQuery.jqDropList priorityList = new clsJQuery.jqDropList(dv1 + "__write_priority", this.PageName, false);
            //var selectedPriority = Int32.Parse(bacnetNodeData["write_priority"] ?? "0");
            ////foreach (var priorityNum in new List<int>() { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 })
            ////    priorityList.AddItem(priorityNum.ToString(), priorityNum.ToString(), priorityNum == selectedPriority);
            //foreach (var item in new Utilities.BacnetEnumValueDisplay(new BacnetWritePriority()).GetValues())
            //    priorityList.AddItem(item.Value, item.Key.ToString(), item.Key == selectedPriority);
            //confHtml.add("Write Priority: ", priorityList.Build());

            // now making a separate HS device for this...



            stb.Append(confHtml.print());


            var readOnlyProperties = new String[] { "ip_address", "device_instance", "object_instance", "object_type" };      //should probably make this constant somewhere
            foreach (var nodeDataProp in readOnlyProperties)
            {
                stb.Append(String.Format("<script>$('#{0}__{1}').prop('disabled', true);</script>", dv1, nodeDataProp));
            }



            return "<div id='homeseerDeviceProperties'>" + stb.ToString() + "</div>";
        }



        private void setDeviceStatus(int devId, int status, int? parentDevId = null)
        {

            //if (device.get_devValue(Instance.host) == 1)    //if was previously in successful communication, but now isn't, turn yellow, otherwise red
            //{

            var statusString = "";
            switch(status)
            {
                case 1:
                    statusString = "Connected";
                    break;
                case 2:
                    statusString = "Connection interrupted";
                    break;
                case 3:
                    statusString = "Not connected";
                    break;
            }


            Instance.host.SetDeviceValueByRef(devId, status, true);
            Instance.host.SetDeviceString(devId, statusString, true);


            if (parentDevId != null)
            {
                Instance.host.SetDeviceValueByRef((int)parentDevId, status, true);
                Instance.host.SetDeviceString((int)parentDevId, statusString, true);
            }


        }


        public string BuildBACnetObjectProperties(int dv1, NameValueCollection bacnetNodeData)
        {

            StringBuilder stb = new StringBuilder();
            htmlBuilder builder = new htmlBuilder(this.PageName);
            htmlTable confHtml = builder.htmlTable();
            confHtml.tableClass = tableClass;
            confHtml.header = "<table border='0' cellpadding='0' cellspacing='0' width='" + tableWidth + "' class='" + tableClass + "'><tbody>";
            //confHtml.labelColumnWidth = 100;

            confHtml.addT("BACnet Object Properties: ");


            Scheduler.Classes.DeviceClass device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv1);


            var nodeType = bacnetNodeData["node_type"];
            int? parentDeviceRef = null;

            if (nodeType == "object")
                parentDeviceRef = Instance.bacnetHomeSeerDevices.getExistingHomeseerBacnetNodeDevice(bacnetNodeData.ToString(), "device");


            BACnetObject bno = Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(bacnetNodeData);
            if (bno == null)
            {
                confHtml.add("", "Error: could not connect to device to retrieve object properties.  Please make sure that the BACnet device is present on the network.");
                

                if (device.get_devValue(Instance.host) == 1)    //if was previously in successful communication, but now isn't, turn yellow, otherwise red
                    setDeviceStatus(dv1, 2, parentDeviceRef);
                else
                    setDeviceStatus(dv1, 3, parentDeviceRef);

            }

            else
            {
                setDeviceStatus(dv1, 1, parentDeviceRef);

                bno.FetchProperties();  //force refresh, even if already fetched


                foreach (var bacnetProperty in bno.BacnetProperties)
                {
                    BacnetPropertyIds propId = bacnetProperty.Key;

                    if (propId == BacnetPropertyIds.PROP_OBJECT_IDENTIFIER || propId == BacnetPropertyIds.PROP_OBJECT_TYPE)     //don't list these; they are displayed in device/object details above
                        continue;



                    var propIdNum = (int)propId;
                    BACnetProperty prop = bacnetProperty.Value;
                    IList<BacnetValue> vals = prop.BacnetPropertyValue.Value.value;
                    BacnetApplicationTags propTag = vals[0].Tag;



                    //if (propId == BacnetPropertyIds.PROP_OBJECT_NAME)
                    //{
                    //    var devName = device.get_Name(Instance.host);
                    //    if (devName.EndsWith(dv1.ToString()))   //replace with object name...
                    //    {
                    //        devName = devName.Replace(dv1.ToString(), vals[0].ToString());
                    //        device.set_Name(Instance.host, devName);
                    //    }

                    //}



                    object prop_val = vals[0].Value;
                    //Type t = prop_val.GetType();      //doesn't work if null, but pretty sure this was just debugging anyway

                    String propElemId = dv1 + "__" + propIdNum;

                    String propElem;
                    switch (propTag)
                    {
                        case BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN:

                            clsJQuery.jqCheckBox CB1 = new clsJQuery.jqCheckBox(propElemId, "", Instance.bacnetDevices.PageName, true, false);

                            if ((Boolean)prop_val)
                                CB1.@checked = true;

                            propElem = CB1.Build();
                            break;


                        case BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED:

                            clsJQuery.jqDropList DL = new clsJQuery.jqDropList(propElemId, Instance.bacnetDevices.PageName, false);

                            var editor = (Utilities.BacnetEnumValueDisplay)(prop.PropertyDescriptor.GetEditor());
                            var valsList = editor.GetValues();

                            foreach (var item in valsList)
                            {
                                var value = item.Key;
                                var displayString = item.Value;
                                var isSelected = (value == Convert.ToInt32(prop_val));

                                DL.AddItem(displayString, value.ToString(), isSelected);
                            }

                            propElem = DL.Build();

                            break;




                        case BacnetApplicationTags.BACNET_APPLICATION_TAG_BIT_STRING:       //this does work if you just let it default to string...but not very user-friendly


                            var flagsDiv = "<div>";


                            var editor2 = (Utilities.BacnetBitStringToEnumListDisplay)(prop.PropertyDescriptor.GetEditor());
                            var flagsList = editor2.GetFlagNames();



                            String bitString = ((BacnetBitString)prop_val).ToString();  //TODO: check that this is the type it comes in as...or can be parsed to.


                            for (int i = 0; i < flagsList.Count; i++)
                            {
                                var flagName = flagsList[i];
                                var bitStringChar = bitString.ToCharArray()[i];
                                var isFlagSet = (bitStringChar == '1');

                                clsJQuery.jqCheckBox CB = new clsJQuery.jqCheckBox(propElemId + "_" + i, flagName, this.PageName, true, false);
                                if (isFlagSet)
                                    CB.@checked = true;

                                flagsDiv += CB.Build() + "<br />";
                            }

                            flagsDiv += "</div>";


                            propElem = flagsDiv;

                            break;

                        default:


                            clsJQuery.jqTextBox TB = new clsJQuery.jqTextBox(propElemId, "string", prop.ValueString(), this.PageName, 16, false);
                            propElem = TB.Build();

                            break;

                    }


                    confHtml.add(prop.Name + ": ", propElem);


                }

            }

            stb.Append(confHtml.print());



            var pollingInterval = bacnetNodeData["polling_interval"] ?? "5000";


            var refreshScript = @"<script>
                setTimeout(function() {
                    $.ajax({
                        type: 'POST',
                        url: location.protocol + '//' + location.host + '/' + '" + this.PageName + @"',
                        data: {hs_device_id: " + dv1 + @"},
                        success: function (objectPropertiesHtml) {
                            //redirect to HomeSeer device edit page (whether new or existing device)
                            //console.log(objectPropertiesHtml);
                            $('#bacnetObjectProperties').replaceWith(objectPropertiesHtml);
                        }//,
                    });
                }, " + pollingInterval + @"); </script>";


            stb.Append(refreshScript);


            return "<div id='bacnetObjectProperties'>" + stb.ToString() + "</div>";
        }




            public string BuildBACnetDeviceTab(int dv1)
            {

                Scheduler.Classes.DeviceClass device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv1);


                var EDO = device.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                var bacnetNodeDataString = parts["BACnetNodeData"];
                var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);


                if (bacnetNodeData["node_type"] == "root")
                    return "";     //not sure how this is happening, but...


                StringBuilder stb = new StringBuilder();
                stb.Append(BuildHomeseerDeviceProperties(dv1, bacnetNodeData));
                stb.Append(BuildBACnetObjectProperties(dv1, bacnetNodeData));



                



                stb.Append(@"<style>
                
/* table.hsBacnetDevice {table-layout:fixed; width:90px;}Setting the table width is important! */
table." + tableClass + @" td {overflow:hidden;}/*Hide text outside the cell.*/
table." + tableClass + @" td:nth-of-type(1) {width:200px;}/*Setting the width of column 1.*/
table." + tableClass + @" td:nth-of-type(2) {width:780px;}/*Setting the width of column 2.*/
                
                    </style>");


//                stb.Append(@"<style>
//
//                                #homeseerDeviceProperties td:first-child { 
//                                    width: 230px;
//                                }
//
//                                #bacnetObjectProperties td:first-child { 
//                                    width: 230px;
//                                }
//
//                            </style>");


                return stb.ToString();

            }





            public NameValueCollection getBacnetNodeData(Scheduler.Classes.DeviceClass device)
            {
                var EDO = device.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                var bacnetNodeDataString = parts["BACnetNodeData"];
                var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);

                return bacnetNodeData;
            }



            public List<SiidDevice> getAllDevices()
            {
                SiidDevice.Update(Instance);
                var devs = new List<SiidDevice>();
                foreach (var Siid in Instance.Devices)
                {
                    var EDO = Siid.Extra;
                    try
                    {
                        var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                        string s = parts["Type"];
                        if (parts["Type"] == "BACnet Device")
                        {
                            devs.Add(Siid);

                        }
                    }
                    catch { }
                }
                return devs;


            }




            public void updateDevicePollTimer(int devId, int newVal = 5000)
            {

                //Update timer dictionary
                //int PollVal = Math.Max(newVal, 10000);

                int PollVal = newVal;


                if (SIID_Page.PluginTimerDictionary.ContainsKey(devId))
                {
                    SIID_Page.PluginTimerDictionary[devId].Change(0, PollVal);
                }
                else
                {
                    var NewDevice = SiidDevice.GetFromListByID(Instance.Devices, devId);
                    System.Threading.Timer GateTimer = new System.Threading.Timer(PollBacnetDevice, NewDevice, 0, PollVal);     //or start right away?
                    SIID_Page.PluginTimerDictionary[devId] = GateTimer;
                }



            }







            //TODO: this was copied over from modbus
            public void PollBacnetDevice(object RawID)
            {
                //First, check if device even exits

                SiidDevice SiidDev = (SiidDevice)RawID;


                //var hsParentDev = SiidDev.Device;


                NameValueCollection hsParentBacnetNodeData;

                //Check if gate is active
                Scheduler.Classes.DeviceClass hsParentDev = null;
                try
                {
                    hsParentDev = SiidDev.Device;
                    hsParentBacnetNodeData = getBacnetNodeData(hsParentDev);
                }
                catch
                {//If gateway doesn't exist, we need to stop this timer and remove it from our timer dictionary.
                    SIID_Page.PluginTimerDictionary[SiidDev.Ref].Dispose();
                    SIID_Page.PluginTimerDictionary.Remove(SiidDev.Ref);

                    SiidDevice.Update(Instance);

                    return;
                }


                //if (BacDev == null)
                //    return;


                //if (WhoIsBeingPolled.Contains(SiidDev.Ref.ToString()))      //wait; this device is already being polled
                //    return;

                //// Currently polling parent device, so don't let controls update
                //// This shouldn't really matter...more important for the child devices which are actually tied to object properties, but...
                //lock (WhoIsBeingPolled)
                //{
                //    WhoIsBeingPolled.Add(SiidDev.Ref.ToString());
                //}


                Console.WriteLine("Polling BACnet Device: " + SiidDev.Ref);

                var EDO = SiidDev.Extra;
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                //if (bool.Parse(parts["Enabled"]))
                //{

                    //Get list of devices
                    //If they're enabled, poll them
                    //Use the time-between and retry things


                //var childDevs = getChildBacnetDevices(bacnetNodeData["device_instance"]).ToList();


                //getChildBacnetDevices(String bacnetDeviceInstance)

                var childDevs = Instance.bacnetHomeSeerDevices.getChildBacnetDevices2(hsParentBacnetNodeData["device_instance"]);

                foreach (int hsBacnetDevRef in childDevs)
                {


                    //Scheduler.Classes.DeviceClass hsBacnetDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(hsBacnetDevRef);

                    Scheduler.Classes.DeviceClass hsBacnetDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(hsBacnetDevRef);

                    //TODO: should we just get from SiidDevices list, but update this on delete?

                    if (hsBacnetDevice == null)
                    {
                        Instance.Devices.Remove(SiidDevice.GetFromListByID(Instance.Devices, hsBacnetDevRef));

                        continue;

                    }

                    //var hsBacnetDevice = Dev.Device;

                    var hsBacnetNodeData = getBacnetNodeData(hsBacnetDevice);


                    ////TODO: make sure from this instance

                    //if (hsBacnetNodeData["node_type"] == "object" && hsBacnetNodeData["device_instance"] == hsParentBacnetNodeData["device_instance"])
                    //{
                    //    // we have found a child BACnet object



                        BACnetObject bno = Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(hsBacnetNodeData);

                        if (bno == null)
                        {

                            Instance.host.SetDeviceString(hsBacnetDevRef, "Could not read object property", true);
                            continue;
                        }


                        bno.FetchProperties();



                        lock (bno.BacnetProperties)
                        {
                            var hasPresentVal = false;
                            foreach (var kvp in bno.BacnetProperties)       //TODO: delay in between tasks, not just constant refresh.
                            //TODO: collection was modified; enumeration operation may not execute
                            {
                                BacnetPropertyIds propId = kvp.Key;
                                BACnetProperty prop = kvp.Value;


                                if (propId == BacnetPropertyIds.PROP_PRESENT_VALUE)
                                {
                                    hasPresentVal = true;
                                    Instance.host.SetDeviceValueByRef(hsBacnetDevRef, Double.Parse(prop.ValueString()), true);
                                    Instance.host.SetDeviceString(hsBacnetDevRef, prop.ValueString(), true);
                                    //break;
                                }


                                if (propId == BacnetPropertyIds.PROP_PRIORITY_ARRAY && (Int32.Parse(hsBacnetNodeData["object_type"]) == (int)BacnetObjectTypes.OBJECT_MULTI_STATE_VALUE))
                                {

                                    try
                                    {


                                    }
                                    catch
                                    {


                                    }


                                    if (hsBacnetNodeData["priority_array_hs_device"] == null)
                                        continue;

                                    var hsPriorityArrayDv = Int32.Parse(hsBacnetNodeData["priority_array_hs_device"]);

                                    Scheduler.Classes.DeviceClass hsPriorityArrayDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(hsPriorityArrayDv);


                                    if (hsPriorityArrayDevice == null)
                                    {
                                        Instance.Devices.Remove(SiidDevice.GetFromListByID(Instance.Devices, hsPriorityArrayDv));

                                        continue;

                                    }


                                    Instance.host.SetDeviceString(hsPriorityArrayDv, prop.PriorityArrayTable(), false);


                                }


                            }

                            if (!hasPresentVal)
                                Instance.host.SetDeviceString(hsBacnetDevRef, "Could not retrieve present value", true);


                            //so update its displayed value by reading the present value from it.

                            //try
                            //{
                            //    lock (WhoIsBeingPolled)
                            //    {
                            //        WhoIsBeingPolled.Add(Dev.Ref.ToString());   // so GUI can't update during.
                            //    }
                            //}



                            //finally
                            //{
                            //    //OneAtATime.ReleaseMutex();
                            //    lock (WhoIsBeingPolled)
                            //    {
                            //        WhoIsBeingPolled.Remove(Dev.Ref.ToString());
                            //    }
                            //}


                        }
                    }



            }






            public void pollBacnetDevice(object someth)
            {






            }




            public string parseBacnetDeviceTab(string data)
            {

                Console.WriteLine("ConfigDevicePost: " + data);


                System.Collections.Specialized.NameValueCollection changed = null;
                changed = System.Web.HttpUtility.ParseQueryString(data);


                if (changed["hs_device_id"] != null)       //when this key is present, we are not editing any data but instead refreshing the displayed properties...
                {
                    var deviceId = Int32.Parse(changed["hs_device_id"]);
                    Scheduler.Classes.DeviceClass hsDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(deviceId);
                    var hsBacnetNodeData = getBacnetNodeData(hsDevice);

                    var html = BuildBACnetObjectProperties(deviceId, hsBacnetNodeData);

                    return html;
                }



                String idKey = changed["id"] ?? changed.Keys[0];    //for jqTextBox the id is just that of the HTML element
                String[] idKeys = idKey.Split(new string[] { "__" }, StringSplitOptions.None);

                string propIdKeys = idKeys[1];

                string[] propIdSubKeys = propIdKeys.Split(new string[] { "_" }, StringSplitOptions.None);
                string propIdString = propIdSubKeys[0];


                int devId = Int32.Parse(idKeys[0]);

                Scheduler.Classes.DeviceClass device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(devId);
                var newVal = changed["value"] ?? changed[idKey];

                //if (newVal == null)                     //in most cases the value is put into the query string with the key corresponding to the HTML element ID
                //    newVal = changed[changed["id"]];


                //var EDO = device.get_PlugExtraData_Get(Instance.host);
                //var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                //var bacnetNodeDataString = parts["BACnetNodeData"];
                //var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);
                var bacnetNodeData = getBacnetNodeData(device);



                //polling interval should only be stored/accessible in parent device.  But when the poll happens, it needs to update present value of all child objectDevices in HS.



                if (propIdKeys == "polling_interval")   //this should be the only editable one at this point...
                {
                    //int PollVal = Math.Max(Convert.ToInt32(newVal), 10000);     //min. 10 second poll time, apparently...

                    int PollVal = Int32.Parse(newVal);

                    updateBacnetNodeData(device, propIdKeys, PollVal.ToString());           //not sure this is used anymore, but...


                    updateDevicePollTimer(devId, PollVal);



                    var t = SIID_Page.PluginTimerDictionary;

                    var d = 2;


                    //Update timer dictionary
                    
                    //if (SIID_Page.PluginTimerDictionary.ContainsKey(devId))
                    //{
                    //    SIID_Page.PluginTimerDictionary[devId].Change(0, PollVal);
                    //}
                    //else
                    //{
                    //    var NewDevice = SiidDevice.GetFromListByID(Instance.Devices, devId);
                    //    System.Threading.Timer GateTimer = new System.Threading.Timer(pollBacnetDevice, NewDevice, 100000, PollVal);
                    //    SIID_Page.PluginTimerDictionary[devId] = GateTimer;
                    //}

                }

                //return "";







                var writePriority = Int32.Parse(bacnetNodeData["write_priority"] ?? "0");   //is this a good default?

                var bacnetNodeDataProps = new String[] { "ip_address", "device_instance", "object_type", "object_instance", "polling_interval" };   // , "write_priority"


                //BACnetObject bno = this.Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(bacnetNodeData);

                if (bacnetNodeDataProps.Contains(propIdKeys))
                {
                    //var readOnlyProperties = new String[]{"ip_address", "device_instance", "object_type", "object_instance"};
                    //if (!readOnlyProperties.Contains(propIdKeys))
                        updateBacnetNodeData(device, propIdKeys, newVal);

                        //if (propIdKeys == "write_priority")
                        //    bno.WritePriority = Int32.Parse(newVal);
                }
                else
                {



                    BACnetObject bno = this.Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(bacnetNodeData);   //even if device, gets the device object
                    //can we make changes to the object, but keep this same reference?


                    if (!bno.AllPropertiesFetched)
                        bno.FetchProperties();      //expensive, but have to make sure property exists...

                    //property can be identified uniquely by its ID...don't need to generate a string.


                    var propId = (BacnetPropertyIds)(Int32.Parse(propIdString));

                    var prop = bno.GetBacnetProperty(propId);





                    IList<BacnetValue> vals = prop.BacnetPropertyValue.Value.value;

                    BacnetApplicationTags propTag = vals[0].Tag;


                    if (propId == BacnetPropertyIds.PROP_PRESENT_VALUE)
                    {
                        UpdateExtraData(device, "RawValue", vals[0].ToString());
                        UpdateExtraData(device, "ProcessedValue", vals[0].ToString());
                    }



                    //object valToWrite;        // = vals[0].Value;
                    Type t = newVal.GetType();


                    String propElem;
                    switch (propTag)
                    {
                        case BacnetApplicationTags.BACNET_APPLICATION_TAG_BOOLEAN:
                            var isSet = (newVal == "checked");
                            prop.WriteValue(isSet, writePriority);
                            break;
                        case BacnetApplicationTags.BACNET_APPLICATION_TAG_REAL:
                            var singleVal = Single.Parse(newVal);
                            prop.WriteValue(singleVal, writePriority);
                            break;
                        case BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED:
                            //var intVal = Int32.Parse(newVal);
                            var uintVal = Convert.ToUInt32(newVal);
                            prop.WriteValue(uintVal, writePriority);
                            break;
                        case BacnetApplicationTags.BACNET_APPLICATION_TAG_BIT_STRING:

                            var changedFlagIndex = Int32.Parse(propIdSubKeys[1]);

                            var existingBitChars = ((BacnetBitString)vals[0].Value).ToString().ToCharArray();


                            var newBitString = "";

                            for (int i = 0; i < existingBitChars.Length; i++)
                            {
                                var bitChar = existingBitChars[i];


                                if (i == changedFlagIndex)
                                    bitChar = (newVal == "checked") ? '1' : '0';


                                newBitString += bitChar.ToString();
                            }




                            var bitString = BacnetBitString.Parse(newBitString);
                            prop.WriteValue(bitString, writePriority);
                            break;


                        default:
                            prop.WriteValue(newVal, writePriority);
                            break;
                    }

                }



                return "";

            }



            public void UpdateExtraData(Scheduler.Classes.DeviceClass Device, string key, string value)
            {
                var EDO = Device.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

                parts[key] = value;
                EDO.RemoveNamed("SSIDKey");
                EDO.AddNamed("SSIDKey", parts.ToString());
                Device.set_PlugExtraData_Set(Instance.host, EDO);

            }



            //this would be used if we eventually let them edit device or object instance #'s
            public void updateBacnetNodeData(Scheduler.Classes.DeviceClass Device, string Key, string value)
            {


                var EDO = Device.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());

                var bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);
                bacnetNodeData[Key] = value;
                parts["BACnetNodeData"] = bacnetNodeData.ToString();

                //parts[Key] = value;

                //if (Key == "write_priority")




                EDO.RemoveNamed("SSIDKey");
                EDO.AddNamed("SSIDKey", parts.ToString());
                Device.set_PlugExtraData_Set(Instance.host, EDO);

            }










    }
}
