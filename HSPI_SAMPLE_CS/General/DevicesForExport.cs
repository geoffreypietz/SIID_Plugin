using HSPI_SIID_ModBusDemo;
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
             "associatedDevices"
        };

         public    HomeSeerDevice(int DeviceID)
            {
                GenericDeviceStuff(DeviceID);
            }
        public HomeSeerDevice()//To get general information about length of attributes list
        {
        }

           public void GenericDeviceStuff(int DeviceID)
            {

                Scheduler.Classes.DeviceClass Device = (Scheduler.Classes.DeviceClass)HSPI_SIID_ModBusDemo.Util.hs.GetDeviceByRef(DeviceID);

             Values["ID"]=DeviceID;
             Values["Name"]=Device.get_Name(Util.hs);
             Values["Floor"]=Device.get_Location2(Util.hs);
             Values["Room"]=Device.get_Location(Util.hs);
             Values["code"]=Device.get_Code(Util.hs);
             Values["address"]=Device.get_Address(Util.hs);
             Values["statusOnly"]=Device.get_Status_Support(Util.hs);
             Values["CanDim"]=Device.get_Can_Dim(Util.hs);
             //hideDev "]= Device.;
             Values["doNotLog"]= Util.hs.DeviceNoLog(DeviceID);
                //    voiceC=Util.hs.DeviceProperty_Boolean(DeviceID", HomeSeerAPI.Enums.eDeviceProperty.Prop;
                //   includeInPower=Util.hs.;
                // usePopUp=Device.use;
                //doNotUp = HomeSeerAPI.Enums.eCapabilities;
             Values["userAccess"]=Device.get_UserAccess(Util.hs);
             Values["notes"]=Device.get_UserNote(Util.hs);
            var DeviceType= Device.get_DeviceType_Get(Util.hs);
            Values["DeviceType"] = DeviceType.Device_API.ToString()+"_"+DeviceType.Device_API_Description.ToString()+"_"+DeviceType.Device_SubType.ToString()+"_"+DeviceType.Device_SubType_Description.ToString()+"_"+DeviceType.Device_Type.ToString()+"_"+DeviceType.Device_Type_Description.ToString();
             Values["deviceTypeString"]= Device.get_Device_Type_String(Util.hs);
             Values["RelationshipStatus"]=Device.get_Relationship(Util.hs);
             Values["associatedDevicesList"]= Device.get_AssociatedDevices_List(Util.hs);
           //  Values["associatedDevices"]=Device.get_AssociatedDevices(Util.hs);

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

            public SIIDDevice(int DeviceID):base(DeviceID)
            {
              
                Scheduler.Classes.DeviceClass Device = (Scheduler.Classes.DeviceClass)HSPI_SIID_ModBusDemo.Util.hs.GetDeviceByRef(DeviceID);
                var EDO = Device.get_PlugExtraData_Get(Util.hs);
            System.Collections.Specialized.NameValueCollection parts = HttpUtility.ParseQueryString(EDO.GetNamed("SSIDKey").ToString());
                foreach (var Ent in parts.AllKeys)
                {
                
                    Values[Ent] = parts[Ent];
                    listOfAttributes.Add(Ent);
                }
               



            }


        }

    }

    

