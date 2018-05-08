namespace HSPI_Utilities_Plugin.ScratchPad
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
       
       //"UpdateInterval",  //Going to be default and global.
       "ResetType", //reset type is an int (0=interval
       "ResetInterval", //interval in muntes just for type=0
       "ResetTime",//Time for time of day type=1
       "DayOfWeek",//for type=2
       "DayOfMonth",//for type=3
       "ScratchPadString",
       "DisplayString",

       "OldValue",
       "NewValue",
       "DisplayedValue",
       "DateOfLastReset",
        "LiveUpdateID",
        "RateValue",
        "LiveValue",
        "FixedCost",
        "showTier",
        "RateTier1",
        "RateTier2",
        "RateTier3",
          "RateTier4",
        "AWCOrLot"
        };

        public static string[] ResetType = new string[] {
            "Periodically",
            "Daily",
            "Weekly",
            "Monthly"
        };

    }
}

