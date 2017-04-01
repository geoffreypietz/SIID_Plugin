using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID.BACnet
{

    //each property contains one of these.

    public abstract class BACnetPropertyDataType
    {

        private string _mConformanceCode = "";
        protected bool isReadable = false;
        protected bool isWritable = false;

        public string ConformanceCode {
         
            get {return _mConformanceCode;}
             
            set {
                switch (value)
                {
                    case "R":
                        isReadable = true;
                        isWritable = false;
                        break;
                    case "W":
                        isReadable = false;
                        isWritable = true;
                        break;
                    case "O":
                        isReadable = true;
                        isWritable = true;
                        break;
                }
                _mConformanceCode = value;
            }
        }


        //public abstract String GetHtml();



        public BACnetPropertyDataType()
        {


        }

    }



    //Should these be in separate files?  Oh well...

    public class BACnetObjectIdentifier : BACnetPropertyDataType
    {

        public BACnetObjectIdentifier()
        {
            ConformanceCode = "R";
        }

    }

    public class CharacterString : BACnetPropertyDataType
    {

        public CharacterString()
        {
            ConformanceCode = "R";
        }

    }

    public class BACnetObjectType : BACnetPropertyDataType
    {

        public BACnetObjectType()
        {
            ConformanceCode = "R";
        }

    }

}


                                                                     //{BacnetPropertyIds.PROP_OBJECT_IDENTIFIER,    
                                                                     //     BacnetPropertyIds.PROP_OBJECT_NAME,
                                                                     //     BacnetPropertyIds.PROP_OBJECT_TYPE, 
                                                                     //     BacnetPropertyIds.PROP_PRESENT_VALUE, 
                                                                     //     BacnetPropertyIds.PROP_DESCRIPTION, 
                                                                     //     BacnetPropertyIds.PROP_DEVICE_TYPE, 
                                                                     //     BacnetPropertyIds.PROP_STATUS_FLAGS, 
                                                                     //     BacnetPropertyIds.PROP_EVENT_STATE, 
                                                                     //     BacnetPropertyIds.PROP_OUT_OF_SERVICE, 
                                                                     //     BacnetPropertyIds.PROP_UPDATE_INTERVAL, 
                                                                     //     BacnetPropertyIds.PROP_UNITS, 
                                                                     //     BacnetPropertyIds.PROP_MIN_PRES_VALUE, 
                                                                     //     BacnetPropertyIds.PROP_MAX_PRES_VALUE, 
                                                                     //     BacnetPropertyIds.PROP_RESOLUTION, 
                                                                     //     BacnetPropertyIds.PROP_COV_INCREMENT, 
                                                                     //     BacnetPropertyIds.PROP_NOTIFICATION_CLASS,
                                                                     //     BacnetPropertyIds.PROP_HIGH_LIMIT,
                                                                     //     BacnetPropertyIds.PROP_LOW_LIMIT,
                                                                     //     BacnetPropertyIds.PROP_DEADBAND,
                                                                     //     BacnetPropertyIds.PROP_LIMIT_ENABLE,
                                                                     //     BacnetPropertyIds.PROP_EVENT_ENABLE,
                                                                     //     BacnetPropertyIds.PROP_NOTIFY_TYPE,





