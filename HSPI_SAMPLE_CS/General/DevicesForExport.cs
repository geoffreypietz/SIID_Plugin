using HSPI_SIID.BACnet;
using HSPI_SIID.General;
using HSPI_SIID.ScratchPad;
using HSPI_SIID;
using HSPI_SIID.Modbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace HSPI_SIID
{
   
        public class HomeSeerDevice
        {
            public Dictionary<string, object> Values = new Dictionary<string, object>();

        /*    public int ID;
            public string Name;
            public string Floor;
            public string Room;
            public string code;
            public string address;
            public bool statusO;
            public bool isDim;
         //   public bool hideDev;
            public bool doNotLog;
          //  public bool voiceC;
           // public bool includeInPower;
            //public bool usePopUp;
           // public bool doNotUp;
            public string userAccess;
            public string notes;
            public HomeSeerAPI.DeviceTypeInfo_m.DeviceTypeInfo DeviceType;
            public string deviceTypeString;
            public HomeSeerAPI.Enums.eRelationship RelationshipStatus;
            public string associatedDevicesList;
            public int[] associatedDevices;
            */
        public InstanceHolder Instance { get; set; }

        public  List<string> listOfAttributes = new List<string> {             "ID", 
             "Name",
             "Floor",
             "Room",
             "code",
             "address",
             "statusOnly",
             "CanDim",
            //    hideDev",
             "doNotLog",
            //   voiceC",
            //  includeInPower",
            // usePopUp",
            //  doNotUp",
             "userAccess",
             "notes",
             "DeviceType",
             "deviceTypeString",
             "RelationshipStatus",
             "associatedDevicesList",
            // "associatedDevices"
        };

         public    HomeSeerDevice(SiidDevice SiidDev, InstanceHolder instance)
            {
            Instance = instance;
                GenericDeviceStuff(SiidDev);
            }
        public HomeSeerDevice()//To get general information about length of attributes list
        {
        }

           public void GenericDeviceStuff(SiidDevice SiidDev)
            {

                Scheduler.Classes.DeviceClass Device = SiidDev.Device;

             Values["ID"]= SiidDev.Ref;
             Values["Name"]=Device.get_Name(Instance.host);
             Values["Floor"]=Device.get_Location2(Instance.host);
             Values["Room"]=Device.get_Location(Instance.host);
             Values["code"]=Device.get_Code(Instance.host);
             Values["address"]=Device.get_Address(Instance.host);
             Values["statusOnly"]=Device.get_Status_Support(Instance.host);
             Values["CanDim"]=Device.get_Can_Dim(Instance.host);

             Values["doNotLog"]= Instance.host.DeviceNoLog(SiidDev.Ref);
                //    voiceC=AllInstances[InstanceFriendlyName].host.DeviceProperty_Boolean(DeviceID", HomeSeerAPI.Enums.eDeviceProperty.Prop;
                //   includeInPower=AllInstances[InstanceFriendlyName].host.;
                // usePopUp=Device.use;
                //doNotUp = HomeSeerAPI.Enums.eCapabilities;
             Values["userAccess"]=Device.get_UserAccess(Instance.host);
             Values["notes"]=Device.get_UserNote(Instance.host);
            var DeviceType= Device.get_DeviceType_Get(Instance.host);
            Values["DeviceType"] = DeviceType.Device_API.ToString()+"_"+DeviceType.Device_API_Description.ToString()+"_"+DeviceType.Device_SubType.ToString()+"_"+DeviceType.Device_SubType_Description.ToString()+"_"+DeviceType.Device_Type.ToString()+"_"+DeviceType.Device_Type_Description.ToString();
             Values["deviceTypeString"]= Device.get_Device_Type_String(Instance.host);
             Values["RelationshipStatus"]=Device.get_Relationship(Instance.host);
             Values["associatedDevicesList"]= Device.get_AssociatedDevices_List(Instance.host);
           //  Values["associatedDevices"]=Device.get_AssociatedDevices(Instance.host);

        }

            public string ReturnCSVRow()
            {
                StringBuilder Row = new StringBuilder();
                bool NotFirst = false;
                foreach (string head in listOfAttributes)
                {
                    if (NotFirst)
                    {
                        Row.Append(",");
                    }
                try { Row.Append("\""+Values[head].ToString()+"\""); }
                catch
                {
                    int a = 1;
                }
                    NotFirst = true;

                }
                return Row.Append("\r\n").ToString();


            }
            public string ReturnCSVHead()
            {
                StringBuilder Row = new StringBuilder();
                bool NotFirst = false;
                foreach (string head in listOfAttributes)
                {
                    if (NotFirst)
                    {
                        Row.Append(",");
                    }
                    Row.Append(head);
                    NotFirst = true;

                }
                return Row.Append("\r\n").ToString();


            }




        }

        class SIIDDevice : HomeSeerDevice
        {

            public SIIDDevice(SiidDevice SiidDev, InstanceHolder instance):base(SiidDev, instance)
            {
            
            
                Instance = instance;


                Scheduler.Classes.DeviceClass Device = SiidDev.Device;
                var EDO = SiidDev.Extra;
            System.Collections.Specialized.NameValueCollection parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
            //Need to grab the key order from somewhere 
           string[] Orderlist = null;
            switch (parts["Type"]) {
                case ("Modbus Gateway"): {
                        Orderlist = ModbusMaster.Attributes;
                        break;
                    }
                case ("Modbus Device"):
                    {
                        Orderlist = ModbusDevice.Attributes;
                        break;
                    }
                case ("BACnet Device") : case ("BACnet Object"):
                    {

                        //Need to export everything that we would need in order to import.  All of bacnetNodeData, essentially.
                        //To this end, maybe it would be easiest to also store device-level stuff in object nodes - i.e. device IP address



                        Orderlist = BACnetObject.Attributes;


                        System.Collections.Specialized.NameValueCollection bacnetNodeData = HttpUtility.ParseQueryString(parts["BACnetNodeData"]);



                        foreach (var Ent in Orderlist)
                        {
                            listOfAttributes.Add(Ent);



                            switch (Ent)
                            {
                                case "Type":
                                    Values[Ent] = parts[Ent];
                                    break;


                                case "NetworkIPAddress":
                                    Values[Ent] = bacnetNodeData["ip_address"];
                                    break;

                                case "DeviceIPAddress":
                                    Values[Ent] = bacnetNodeData["device_ip_address"];
                                    break;

                                case "DeviceUDPPort":
                                    Values[Ent] = bacnetNodeData["device_udp_port"];
                                    break;


                                case "DeviceInstance":
                                    Values[Ent] = bacnetNodeData["device_instance"];
                                    break;


                                case "ObjectType":
                                    Values[Ent] = bacnetNodeData["object_type_string"];
                                    break;


                                case "ObjectInstance":
                                    Values[Ent] = bacnetNodeData["object_instance"];
                                    break;


                                case "ObjectName":
                                    Values[Ent] = bacnetNodeData["object_name"];
                                    break;


                                case "PollInterval":
                                    Values[Ent] = bacnetNodeData["polling_interval"];
                                    break;


                                case "RawValue":
                                case "ProcessedValue":
                                    Values[Ent] = parts[Ent];
                                    break;


                            }
                        }


                        return;
                        //Orderlist = BACnetDevice.Attributes;
                        //break;

                    }
                case ("Scratchpad"):
                    {
                        Orderlist = ScratchpadDevice.Attributes;
                        break;
                    }


            }

            foreach (var Ent in Orderlist)
                {
                
                    Values[Ent] = parts[Ent];
                    listOfAttributes.Add(Ent);
                }
               



            }


        }

    }

    

