using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID_ModBusDemo.Modbus
{
    class ModbusDevice
    {
        #region Instance attributes
        //Config tab, advanced tab, and status graphic built in to homeseer
        //just need to specifically do modbus stuff

   
        private string gateWay = "";
        private int registerTypeIndex = 0;

        private static string[] registerTypes = new string[] { "Discrete Input (RO)", "Coil (RW)", "Input Register (RO)", "Holding Register (RW)" };

     

        private int slaveId = 1;
        private int registerAddress; //Each register type has a given range of addresses
        //the registerAddress field is just the sub-address.
        //so total address in type-address + register address; i.e. 40000+1
        private bool deviceEnabled = false;
    
        private bool readOnlyDevice = false;
    
        private int returnTypeIndex = 0;
        private static string[] returnTypes = new string[] { "Int16", "Int32", "Float32", "Int64", "Bool" };
        private bool signedValue = false;
        private double multiplier = 1;//replace with scratchpad
        private string displayFormat = "";
        #endregion



    }
}
