using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID.ScratchPad
{
    class ScratchpadDevice
    {
        //Hidden homeseer device, only visible on the SIID page (and accessed through whatever we want to read these)
        //Type = Scratchpad
        //IsAccumulator = bool
        //Update interval (how often to call the scratchpad string rule)
        //IF is accumulator, reset type is Interval or Monthly or Daily, or Weekly
        //ResetInterval is number of seconds if Interval, Numbered day of month if monthly, time of day if daily, day of wee if weekly
        //if accumulator, the displayed is the difference between the old value and the new value, and when reset is called, write new value to old value
        public static string[] Attributes = new string[]{
        "Type",
        "IsEnabled",
       "IsAccumulator",
       
       "UpdateInterval",
       "ResetType", //reset type is an int (0=interval
       "ResetInterval",
       "ScratchPadString",
       "OldValue",
       "NewValue",
       "DisplayedValue"
        };

        public static string[] ResetType = new string[] {
            "Periodically",
            "Daily",
            "Weekly",
            "Monthly"
        };

    }
}

