using HSPI_SIID.BACnet;
using HSPI_SIID.General;
using HSPI_SIID.ScratchPad;
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
             "vsp",
           "vgp",
            //Want status graphic pairs
            //Want control pairs

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

        public static string EncodeVSPairs(HomeSeerAPI.VSVGPairs.VSPair[] AllStatus)
        {
            var properties = typeof(HomeSeerAPI.VSVGPairs.VSPair).GetProperties();


   
        
     

        StringBuilder sb = new StringBuilder();
           
            foreach (HomeSeerAPI.VSVGPairs.VSPair P in AllStatus)
            {
                sb.Append("VSPAIRTAG:");
                sb.Append("HasAdditionalData: "+P.HasAdditionalData);
                sb.Append(",HasScale: " + P.HasScale);
                sb.Append(",IncludeValues: " + P.IncludeValues);
                sb.Append(",PairButtonImage: " + P.PairButtonImage);
                sb.Append(",PairButtonImageType: " + P.PairButtonImageType);
                sb.Append(",PairType: " + P.PairType);
                sb.Append(",RangeEnd: " + P.RangeEnd);
                sb.Append(",RangeStart: " + P.RangeStart);
                sb.Append(",RangeStatusDecimals: " + P.RangeStatusDecimals);
                sb.Append(",RangeStatusDivisor: " + P.RangeStatusDivisor);
                sb.Append(",RangeStatusPrefix: " + P.RangeStatusPrefix);
                sb.Append(",Render_Location_column: " + P.Render_Location.Column);
                sb.Append(",Render_Location_columnspan: " + P.Render_Location.ColumnSpan);
                sb.Append(",Render_Location_row: " + P.Render_Location.Row);
                sb.Append(",RangeStatusSuffix: " + P.RangeStatusSuffix);
                sb.Append(",ValueOffset: " + P.ValueOffset);
                sb.Append(",ZeroPadding: " + P.ZeroPadding);
                sb.Append(",ControlStatus: " + P.ControlStatus);
                sb.Append(",ControlUse: " + P.ControlUse);
                sb.Append(",Render: " + P.Render);
                sb.Append(",StringList: " + P.StringList);
                sb.Append(",Value: " + P.Value);
                sb.Append(",status: " + P.GetPairString(P.Value, null, null)+",");






                sb.Append("");


            }

            sb.Append("");
            return sb.ToString();

        }
        public static HomeSeerAPI.VSVGPairs.VSPair[] DecodeVSPairs(String Encoded)
        {
            IList<HomeSeerAPI.VSVGPairs.VSPair> Pairs = new List<HomeSeerAPI.VSVGPairs.VSPair>();

           var PO=Encoded.Split(new[] {"VSPAIRTAG:"}, StringSplitOptions.None);
            foreach(string L in PO) {

                if (L.Count() > 5)
                {
                    string StatusControl = L.Split(new[] {",ControlStatus: "}, StringSplitOptions.None)[1].Split(',')[0];
                    int SC = 1;
                    switch (StatusControl) {
                        case ("Status"):
                            {
                                SC = 1;
                                break;
                            }
                        case ("Control"):
                            {
                                SC = 2;
                                break;
                            }
                        case ("Both"):
                            {
                                SC = 3;
                                break;
                            }
                    }

                    HomeSeerAPI.VSVGPairs.VSPair P = new HomeSeerAPI.VSVGPairs.VSPair((HomeSeerAPI.ePairStatusControl)SC);
                    P.HasAdditionalData = Boolean.Parse(L.Split(new[] {"HasAdditionalData: "}, StringSplitOptions.None)[1].Split(',')[0]);
                    P.HasScale = Boolean.Parse(L.Split(new[] {"HasScale: "}, StringSplitOptions.None)[1].Split(',')[0]);
                    P.IncludeValues = Boolean.Parse(L.Split(new[] {"IncludeValues: "}, StringSplitOptions.None)[1].Split(',')[0]);
                    P.PairButtonImage = (L.Split(new[] {"PairButtonImage: "}, StringSplitOptions.None)[1].Split(',')[0]);

                  
                     StatusControl = L.Split(new[] {"PairButtonImageType: "}, StringSplitOptions.None)[1].Split(',')[0];
                     SC = 0;
                    switch (StatusControl)
                    {
                        case ("Not_Specified"):
                            {
                                SC = 0;
                                break;
                            }
                        case ("Use_Status_Value"):
                            {
                                SC = 1;
                                break;
                            }
                        case ("Use_Custom"):
                            {
                                SC = 2;
                                break;
                            }
                    }

                    P.PairButtonImageType = ((HomeSeerAPI.Enums.CAPIControlButtonImage)SC);

                    StatusControl = L.Split(new[] {"PairType: "}, StringSplitOptions.None)[1].Split(',')[0];
                    SC = 1;
                    switch (StatusControl)
                    {
                        case ("SingleValue"):
                            {
                                SC = 1;
                                break;
                            }
                        case ("Range"):
                            {
                                SC = 2;
                                break;
                            }
                     
                    }
                    P.PairType = ((HomeSeerAPI.VSVGPairs.VSVGPairType)SC);

                    P.RangeEnd = Double.Parse((L.Split(new[] {"RangeEnd: "}, StringSplitOptions.None)[1].Split(',')[0]));
                    P.RangeStart = Double.Parse((L.Split(new[] {"RangeStart: "}, StringSplitOptions.None)[1].Split(',')[0]));

                    P.RangeStatusDecimals = Int32.Parse((L.Split(new[] {"RangeStatusDecimals: "}, StringSplitOptions.None)[1].Split(',')[0]));
                    P.RangeStatusDivisor = Double.Parse((L.Split(new[] {"RangeStatusDivisor: "}, StringSplitOptions.None)[1].Split(',')[0]));
                    P.RangeStatusPrefix = L.Split(new[] {"RangeStatusPrefix: "}, StringSplitOptions.None)[1].Split(',')[0];

               int Col= Int32.Parse((L.Split(new[] {"Render_Location_column: "}, StringSplitOptions.None)[1].Split(',')[0]));
                    int Colspan = Int32.Parse((L.Split(new[] {"Render_Location_columnspan: "}, StringSplitOptions.None)[1].Split(',')[0]));
                    int row = Int32.Parse((L.Split(new[] {"Render_Location_row: "}, StringSplitOptions.None)[1].Split(',')[0]));

                    HomeSeerAPI.Enums.CAPIControlLocation CL = new HomeSeerAPI.Enums.CAPIControlLocation();
                    CL.Column = Col;
                    CL.ColumnSpan = Colspan;
                    CL.Row = row;
                    P.Render_Location = CL;

                    P.RangeStatusSuffix = L.Split(new[] {"RangeStatusSuffix: "}, StringSplitOptions.None)[1].Split(',')[0];

                    P.ValueOffset = Double.Parse((L.Split(new[] {"ValueOffset: "}, StringSplitOptions.None)[1].Split(',')[0]));
                    P.ZeroPadding = Boolean.Parse(L.Split(new[] {"ZeroPadding: "}, StringSplitOptions.None)[1].Split(',')[0]);



                    StatusControl = L.Split(new[] {"ControlUse: "}, StringSplitOptions.None)[1].Split(',')[0];
                    SC = 0;
                    switch (StatusControl)
                    {
                        case ("_On"):
                            {
                                SC = 1;
                                break;
                            }
                        case ("_Off"):
                            {
                                SC = 2;
                                break;
                            }

                        case ("_HeatSetPoint"):
                            {
                                SC = 12;
                                break;
                            }
                        case ("_CoolSetPoint"):
                            {
                                SC = 13;
                                break;
                            }
                        case ("_ThermModeOff"):
                            {
                                SC = 14;
                                break;
                            }
                        case ("_Shuffle"):
                            {
                                SC = 11;
                                break;
                            }
                        case ("_Repeat"):
                            {
                                SC = 10;
                                break;
                            }
                        case ("_Rewind"):
                            {
                                SC = 9;
                                break;
                            }
                        case ("_Forward"):
                            {
                                SC = 8;
                                break;
                            }
                        case ("_Stop"):
                            {
                                SC = 7;
                                break;
                            }
                        case ("_Pause"):
                            {
                                SC = 6;
                                break;
                            }
                        case ("_Play"):
                            {
                                SC = 5;
                                break;
                            }
                        case ("_On_Alternate"):
                            {
                                SC = 4;
                                break;
                            }
                        case ("_Dim"):
                            {
                                SC = 3;
                                break;
                            }
                        case ("_ThermModeHeat"):
                            {
                                SC = 15;
                                break;
                            }
                        case ("_ThermModeCool"):
                            {
                                SC = 16;
                                break;
                            }
                        case ("_ThermModeAuto"):
                            {
                                SC = 17;
                                break;
                            }
                        case ("_DoorLock"):
                            {
                                SC = 18;
                                break;
                            }
                        case ("_DoorUnLock"):
                            {
                                SC = 19;
                                break;
                            }
                        case ("_ThermFanAuto"):
                            {
                                SC = 20;
                                break;
                            }
                        case ("_ThermFanOn"):
                            {
                                SC = 21;
                                break;
                            }

                    }
                    P.ControlUse = ((HomeSeerAPI.ePairControlUse)SC);

                    StatusControl = L.Split(new[] {"Render: "}, StringSplitOptions.None)[1].Split(',')[0];
                    SC = 1;
                    switch (StatusControl)
                    {
                        case ("Not_Specified"):
                            {
                                SC = 1;
                                break;
                            }
                        case ("Values"):
                            {
                                SC = 2;
                                break;
                            }
                        case ("Single_Text_from_List"):
                            {
                                SC = 3;
                                break;
                            }
                        case ("List_Text_from_List"):
                            {
                                SC = 4;
                                break;
                            }
                        case ("Button"):
                            {
                                SC = 5;
                                break;  
                            }
                        case ("ValuesRange"):
                            {
                                SC = 6;
                                break;
                            }
                        case ("ValuesRangeSlider"):
                            {
                                SC = 7;
                                break;
                            }
                        case ("TextList"):
                            {
                                SC = 8;
                                break;
                            }
                        case ("TextBox_Number"):
                            {
                                SC = 9;
                                break;
                            }
                        case ("TextBox_String"):
                            {
                                SC = 10;
                                break;
                            }
                        case ("Radio_Option"):
                            {
                                SC = 11;
                                break;
                            }
                        case ("Button_Script"):
                            {
                                SC = 12;
                                break;
                            }
                        case ("Color_Picker"):
                            {
                                SC = 13;
                                break;
                            } 

                    }
                    P.Render = ((HomeSeerAPI.Enums.CAPIControlType)SC);

                    P.Value = Double.Parse(L.Split(new[] {"Value: "}, StringSplitOptions.None)[1].Split(',')[0]);
                    P.Status = L.Split(new[] { "status: " }, StringSplitOptions.None)[1].Split(',')[0];

                   
                    //Skipping StringList
                    Pairs.Add(P);
                }

            }


            return Pairs.ToArray();

        }

        public  string EncodeVGPairs(HomeSeerAPI.VSVGPairs.VSPair[] AllStatus, int DevID)
        {
            StringBuilder sb = new StringBuilder();
            if (AllStatus.Count() == 0 || Instance.host.DeviceVGP_Count(DevID)==0) {

                return "";
            }

            foreach (var P in AllStatus)
            {
                try
                {

                    HomeSeerAPI.VSVGPairs.VGPair V= Instance.host.DeviceVGP_Get(DevID, P.Value);
                    sb.Append("VGPAIRTAG:");
                    sb.Append("PairType: " + V.PairType);
                    sb.Append(",RangeEnd: " + V.RangeEnd);
                    sb.Append(",RangeStart: " + V.RangeStart);
                    sb.Append(",ProtectionSet: " + V.Protection);
                    sb.Append(",Graphic: " + V.GetGraphic(V.Value));
                    sb.Append(",Set_Value: " + V.Value);
           

          

                }
                catch { } 

            }



            return sb.ToString();

        }

        public static HomeSeerAPI.VSVGPairs.VGPair[] DecodeVGPairs(String Encoded)
        {
            IList<HomeSeerAPI.VSVGPairs.VGPair> Pairs = new List<HomeSeerAPI.VSVGPairs.VGPair>();

            var PO = Encoded.Split(new[] { "VGPAIRTAG:" }, StringSplitOptions.None);
            foreach (string L in PO)
            {

                if (L.Count() > 5)
                {
                    var V = new HomeSeerAPI.VSVGPairs.VGPair();


                    V.RangeEnd = Double.Parse(L.Split(new[] { "RangeEnd: " }, StringSplitOptions.None)[1].Split(',')[0]);
                    V.RangeStart = Double.Parse(L.Split(new[] { "RangeEnd: " }, StringSplitOptions.None)[1].Split(',')[0]);
                    V.ProtectionSet = Int32.Parse(L.Split(new[] { "ProtectionSet: " }, StringSplitOptions.None)[1].Split(',')[0]);
                    V.Graphic = L.Split(new[] { "Graphic: " }, StringSplitOptions.None)[1].Split(',')[0];
                    V.Set_Value = Double.Parse(L.Split(new[] { "Set_Value: " }, StringSplitOptions.None)[1].Split(',')[0]);
                    string StatusControl = L.Split(new[] { "PairType: " }, StringSplitOptions.None)[1].Split(',')[0];
                    int SC = 1;
                    switch (StatusControl)
                    {
                        case ("SingleValue"):
                            {
                                SC = 1;
                                break;
                            }
                        case ("Range"):
                            {
                                SC = 2;
                                break;
                            }
                    }


                    V.PairType = (HomeSeerAPI.VSVGPairs.VSVGPairType)SC;
                    Pairs.Add(V);
                }
            }

        
            return Pairs.ToArray();
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
            Values["DeviceType"] = DeviceType.Device_API.ToString()+"?"+DeviceType.Device_API_Description.ToString()+"?"+DeviceType.Device_SubType.ToString()+"?"+DeviceType.Device_SubType_Description.ToString()+"?"+DeviceType.Device_Type.ToString()+"?"+DeviceType.Device_Type_Description.ToString();
             Values["deviceTypeString"]= Device.get_Device_Type_String(Instance.host);
             Values["RelationshipStatus"]=Device.get_Relationship(Instance.host);
             Values["associatedDevicesList"]= Device.get_AssociatedDevices_List(Instance.host);
            HomeSeerAPI.VSVGPairs.VSPair[] AllStatus= Instance.host.DeviceVSP_GetAllStatus(SiidDev.Ref);

            Values["vsp"] = EncodeVSPairs(AllStatus);

        

            Values["vgp"] = EncodeVGPairs(AllStatus,SiidDev.Ref);
            //  Values["vgp"] =;
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
                    Row.Append("\"" + "" + "\"");
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

    

