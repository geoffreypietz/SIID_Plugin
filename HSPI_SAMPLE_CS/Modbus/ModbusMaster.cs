using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID.Modbus
{
    class ModbusMaster
    {
        #region Instance attributes
        //Config tab, advanced tab, and status graphic built in to homeseer
        //just need to specifically do modbus stuff

        #region gateway settings
        private string modbusIporHostname = "";
        private int TCPport = 502;
        private Int32 pollInterval = 300000; //store as milliseconds, display as date-time + milliseconds
        private bool gatewayEnabled = false;
        #endregion
        #region advanced settings
        private bool bigEndianValue = false;
        private bool zeroBasedAddressing = true;
        private int readWriteRetries = 2;
        private Int32 readWriteTimeout = 1000;//in milliseconds
        private Int32 delayBetweenEachAddressPoll = 0;//in milliseconds
        private bool registerWriteFunction = true; //A radio button with 2 choices
                                                   //true is Write Single Register, false is WriteMultipleRegisters
        #endregion
        #endregion

        #region Class functions

        #endregion


        public static string[] Attributes = new string[]{

            "Type",
        "Gateway",
           "TCP",
              "Poll",
        "Enabled",
            "BigE",
            "ZeroB",
            "RWRetry",
            "RWTime",
            "Delay",
            "RegWrite",
            "LinkedDevices",
            "RawValue",
            "ProcessedValue"};

      



        /*
                #region Configuration tab
                private string deviceName = "Modbus IP Gateway";
                private int floorIndex = 0;//selected floor in list
                private int roomIndex = 0;//selected room
                private int codeRightIndex = 0;
                private int codeLeftIndex = 0;
                private string address = "";
                private bool statusOnlyDevice = false;
                private bool isDimmable = false;
                private bool hideDeviceFromViews = false;
                private bool doNotLogCommandsFromThis = false;
                private bool voiceCommand = false;
                private bool confirmVoiceCommand = false;
                private bool includeInPowerFailRecovery = false;
                private bool usePopUpDialogForControl = false;
                private bool doNotUpdateDeviceLastChangeIfValueNotCHange = false;
                private string[] usersWithAccess = new string[] { "Any User" };
                private string notes = "";
                private string base64deviceImage = "";
                #endregion
                #region Advanced Tab

                #endregion

                #endregion

                #region class attributes
                private static string[] floors { get; set; }
                private static string[] rooms { get; set; }
                private static char[] codeLeft = "ABCDEFGHIJKLMNOPqrstuvwxyz".ToCharArray();
                private static int[] codeRight = Enumerable.Range(1, 1000).ToArray();
                #endregion
                #region class functions

                */



    }
}
