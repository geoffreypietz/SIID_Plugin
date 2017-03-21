using HSPI_SIID.BACnet;
using HSPI_SIID_ModBusDemo;
using HSPI_SIID_ModBusDemo.Modbus;
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

         public    HomeSeerDevice(int DeviceID, InstanceHolder instance)
            {
            Instance = instance;
                GenericDeviceStuff(DeviceID);
            }
        public HomeSeerDevice()//To get general information about length of attributes list
        {
        }

           public void GenericDeviceStuff(int DeviceID)
            {

                Scheduler.Classes.DeviceClass Device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(DeviceID);

             Values["ID"]=DeviceID;
             Values["Name"]=Device.get_Name(Instance.host);
             Values["Floor"]=Device.get_Location2(Instance.host);
             Values["Room"]=Device.get_Location(Instance.host);
             Values["code"]=Device.get_Code(Instance.host);
             Values["address"]=Device.get_Address(Instance.host);
             Values["statusOnly"]=Device.get_Status_Support(Instance.host);
             Values["CanDim"]=Device.get_Can_Dim(Instance.host);

             Values["doNotLog"]= Instance.host.DeviceNoLog(DeviceID);
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

            public SIIDDevice(int DeviceID, InstanceHolder instance):base(DeviceID,instance)
            {
            
            
                Instance = instance;


                Scheduler.Classes.DeviceClass Device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(DeviceID);
                var EDO = Device.get_PlugExtraData_Get(Instance.host);
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
                case ("BACnet Device"):
                    {
                        Orderlist = BACnetDevice.Attributes;
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

    

