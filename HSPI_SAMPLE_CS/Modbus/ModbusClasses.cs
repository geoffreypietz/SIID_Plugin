using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SAMPLE_CS.Modbus
{
    class ModbusClasses
    {
        [Serializable()]
        public class ModbusGeneralConfigSettings
        {

   

            public Int32 defPollInt { get; set;}
            public int logLevel { get; set;}
            public bool logToFile { get; set; }

            
        }

    }
}
