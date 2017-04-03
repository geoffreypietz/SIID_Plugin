﻿using System;
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

        public BACnetDevices(string pagename, InstanceHolder instance) : base(pagename)
        {

            Instance = instance;
            //BACnetBuilder = new htmlBuilder("BACnetPage" + Instance.ajaxName);
            //DiscoveredBACnetDevices = new List<string>();

            PageName = BaseUrl + Instance.ajaxName;
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
                htmlBuilder builder = new htmlBuilder("BACnetDevTab" + Instance.ajaxName);
                htmlTable confHtml = builder.htmlTable();


                //First just generate display fields based on node data.

                var bacNodeTyp = (bacnetNodeData["node_type"] == "device" ? "Device" : "Object");
                confHtml.addT("Associated BACnet " + bacNodeTyp + ":");
                confHtml.add("IP Address: ", builder.stringInput(dv1 + "__ip_address", bacnetNodeData["ip_address"]).print());
                confHtml.add("Device Instance: ", builder.numberInput(dv1 + "__device_instance", Int32.Parse(bacnetNodeData["device_instance"])).print());

                if (bacnetNodeData["node_type"] == "object")
                {
                    confHtml.add("Object Type: ", builder.stringInput(dv1 + "__object_type", ((BacnetObjectTypes)Int32.Parse(bacnetNodeData["object_type"])).ToString()).print());
                    confHtml.add("Object Instance: ", builder.numberInput(dv1 + "__object_instance", Int32.Parse(bacnetNodeData["object_instance"])).print());
                }


                confHtml.addT("BACnet Object Properties: ");



                BACnetObject bno = Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(bacnetNodeData);
                if (bno == null)
                {
                    confHtml.add("", "Error: could not connect to device to retrieve object properties.  Please make sure that the BACnet device is present on the network.");
                }

                else
                {
                    bno.FetchProperties();


                    foreach (var bacnetProperty in bno.BacnetProperties)
                    {
                        BacnetPropertyIds propId = bacnetProperty.Key;

                        if (propId == BacnetPropertyIds.PROP_OBJECT_IDENTIFIER || propId == BacnetPropertyIds.PROP_OBJECT_TYPE)     //don't list these; they are displayed in device/object details above
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

                                    clsJQuery.jqCheckBox CB = new clsJQuery.jqCheckBox(propElemId + "_" + i, flagName, Instance.bacnetDevices.PageName, true, false);
                                    if (isFlagSet)
                                        CB.@checked = true;

                                    flagsDiv += CB.Build() + "<br />";
                                }

                                flagsDiv += "</div>";


                                propElem = flagsDiv;

                                break;

                            default:
                                propElem = builder.stringInput(propElemId, prop.ValueString()).print();
                                break;

                        }


                        confHtml.add(prop.Name + ": ", propElem);


                    }

                }

                stb.Append(confHtml.print());


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












            public string parseBacnetDeviceTab(string data)
            {

                Console.WriteLine("ConfigDevicePost: " + data);


                System.Collections.Specialized.NameValueCollection changed = null;
                changed = System.Web.HttpUtility.ParseQueryString(data);



                var idKeys = changed["id"].Split(new string[] { "__" }, StringSplitOptions.None); //[1];

                string propIdKeys = idKeys[1];

                string[] propIdSubKeys = propIdKeys.Split(new string[] { "_" }, StringSplitOptions.None);
                string propIdString = propIdSubKeys[0];


                int devId = Int32.Parse(idKeys[0]);

                Scheduler.Classes.DeviceClass device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(devId);
                var newVal = changed["value"];

                if (newVal == null)                     //in most cases the value is put into the query string with the key corresponding to the HTML element ID
                    newVal = changed[changed["id"]];


                var EDO = device.get_PlugExtraData_Get(Instance.host);
                var parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                var bacnetNodeDataString = parts["BACnetNodeData"];
                var bacnetNodeData = HttpUtility.ParseQueryString(bacnetNodeDataString);


                if (bacnetNodeData[propIdKeys] != null)
                {

                    //updateBacnetNodeData(device, partID, newVal);     probably don't let them edit this....


                }
                else
                {



                    BACnetObject bno = this.Instance.bacnetDataService.GetBacnetObjectOrDeviceObject(bacnetNodeData);   //even if device, gets the device object

                    if (!bno.AllPropertiesFetched)
                        bno.FetchProperties();      //expensive, but have to make sure property exists...

                    //property can be identified uniquely by its ID...don't need to generate a string.


                    var propId = (BacnetPropertyIds)(Int32.Parse(propIdString));

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
                            var singleVal = Single.Parse(newVal);
                            prop.WriteValue(singleVal);
                            break;
                        case BacnetApplicationTags.BACNET_APPLICATION_TAG_ENUMERATED:
                            //var intVal = Int32.Parse(newVal);
                            var uintVal = Convert.ToUInt32(newVal);
                            prop.WriteValue(uintVal);
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
                            prop.WriteValue(bitString);
                            break;


                        default:
                            prop.WriteValue(newVal);
                            break;
                    }

                }



                return "";

            }




            //this would be used if we eventually let them edit device or object instance #'s
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










    }
}