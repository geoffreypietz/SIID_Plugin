using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID.BACnet
{
    //Mostly for use with import and export function, not yet used for any more
    class BACnetDevice
    {
        public static string[] Attributes = new string[]{
               "Type",
       "BACnetString",

       //From design doc
       "InstanceNumber",
       "NetworkNumber",
       "UDPPort",
            "IPAddress",
            "PollInterval",
            "BACnetName",

            "RawValue",
            "ProcessedValue"};

    }
}
