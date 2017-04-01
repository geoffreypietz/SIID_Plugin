using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID.Modbus
{
    class ModbusDevice
    {
        public static string[] Attributes = new string[]{
               "Type",
        "GateID",
            "Gateway",
            "RegisterType",
            "SlaveId",
            "ReturnType",
            //0=Bool,1 = Int16, 2=Int32,3=Float32,4=Int64,5=string2,6=string4,7=string6,8=string8
            //tells us how many registers to read/write and also how to parse returns
            //note that coils and descrete inputs are bits, registers are 16 bits = 2 bytes
            //So coil and discrete are bool ONLY
            //Rest are 16 bit stuff and every mutiple of 16 is number of registers to read
            "SignedValue",
            "ScratchpadString",
            "DisplayFormatString",
            "ReadOnlyDevice",
            "DeviceEnabled",
            "RegisterAddress",
            "RawValue",
            "ProcessedValue"};




    }
}
