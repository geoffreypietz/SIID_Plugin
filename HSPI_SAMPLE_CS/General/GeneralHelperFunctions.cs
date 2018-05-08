using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using SoftCircuits;

namespace HSPI_Utilities_Plugin.General
{
    class GeneralHelperFunctions
    {


        public static string GetValues(InstanceHolder Instance, string ScratchPadString)
        {
            ScratchPadString= ScratchPadString.Replace("(^p^)", "+");
            List<int> Raws = new List<int>();
            List<int> Processed = new List<int>();
            Match m = Regex.Match(ScratchPadString, @"(\$\()+(\d+)(\))+");
            while (m.Success)
            {
                if (!Raws.Contains(int.Parse(m.Groups[2].ToString())))
                {

                    Raws.Add(int.Parse(m.Groups[2].ToString()));
                }
                m = m.NextMatch();
            }
            m = Regex.Match(ScratchPadString, @"(\#\()+(\d+)(\))+");
            while (m.Success)
            {
                if (!Processed.Contains(int.Parse(m.Groups[2].ToString())))
                {
                    Processed.Add(int.Parse(m.Groups[2].ToString()));
                }
                m = m.NextMatch();
            }
            StringBuilder FinalString = new StringBuilder(ScratchPadString);
            foreach (int dv in Raws)
            {
        
                SiidDevice TempDev = SiidDevice.GetFromListByID(Instance.Devices, dv);
            



                if (TempDev == null) 
                {
                    try
                    {
                        Scheduler.Classes.DeviceClass newDevice = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv); //If the device exists and is a SIID device, adds it into the list
      
                        var E = newDevice.get_PlugExtraData_Get(Instance.host);
                        if (E.GetNamed("SSIDKey") != null)
                        {
                            TempDev = new SiidDevice(Instance, newDevice);
                            Instance.Devices.Add(TempDev);
                        }

                    }
                    catch
                    {
                        //Device reference either incorrect or not a SIID device.
                    }
                    

                }

                if (TempDev != null) 
                {
                    //OK the extradata store in the SIID device list is outdated. grab from the device...
                    //var TempEDO = TempDev.Extra;
                    var TempEDO = TempDev.Device.get_PlugExtraData_Get(Instance.host);

                    var Tempparts = HttpUtility.ParseQueryString(TempEDO.GetNamed("SSIDKey").ToString());
                    try
                    {
                        string Rep = Tempparts["RawValue"];
                        if (Rep == null)
                            throw new Exception();
                        FinalString.Replace("$(" + dv + ")", Rep);
                    }
                    catch
                    {
                        try
                        {
                            string Rep = Instance.host.DeviceValueEx(dv).ToString(); //Problem, device values return as int
                            if (Rep == null)
                                throw new Exception();
                            FinalString.Replace("$(" + dv + ")", Rep);
                        }
                        catch
                        {

                        }
                    }
                }
                else //Not an SIID device, use the device value
                {

                    try
                    {
                        string Rep = Instance.host.DeviceValueEx(dv).ToString(); //Fails for BACnet devices because the status does not reflect the displayed value
                        if (Rep == null)
                            throw new Exception();
                        FinalString.Replace("$(" + dv + ")", Rep); 
                    }
                    catch(Exception E)
                    {
                        Instance.hspi.Log("Error parsing scratchpad string: Could not retrive device value for device: "+dv, 2);
                    }

                }

            }
            foreach (int dv in Processed)
            {
                SiidDevice TempDev = SiidDevice.GetFromListByID(Instance.Devices, dv);// (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(dv);
                if (TempDev != null)
                {
                    var TempEDO = TempDev.Extra;
                    var Tempparts = HttpUtility.ParseQueryString(TempEDO.GetNamed("SSIDKey").ToString());
                    try
                    {
                        string Rep = Tempparts["ProcessedValue"];
                        if (Rep == null)
                            throw new Exception();
                        FinalString.Replace("#(" + dv + ")", Rep);
                    }
                    catch
                    {
                        try
                        {
                            string Rep = Instance.host.DeviceValueEx(dv).ToString();
                            if (Rep == null)
                                throw new Exception();
                            FinalString.Replace("#(" + dv + ")", Rep);
                        }
                        catch
                        {

                        }
                    }
                }
                else
                {
                    try
                    {
                        string Rep = Instance.host.DeviceValueEx(dv).ToString();
                        if (Rep != null)
                            FinalString.Replace("#(" + dv + ")", Rep);
                    }
                    catch
                    {

                    }

                }
            }

            return FinalString.ToString();

        }



        public static string[] DaysOfWeek = new string[] { "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday" };

        public static double Evaluate(String input)       {


            var E = new Eval();
           return E.Execute(input);

            //http://stackoverflow.com/questions/333737/evaluating-string-342-yield-int-18?noredirect=1&lq=1  Modified for numbers of more than one character
/*
            String expr = "(" + input + ")";
            Stack<String> ops = new Stack<String>();
            Stack<Double> vals = new Stack<Double>();
            int SSLength = 1;
      
           
            for (int i = 0; i < expr.Length; i=i+0)
            {
                SSLength = 1;
          
                if (char.IsDigit(expr[i]) || expr[i].Equals('.'))
                {
                    int j = i+1;
                    while (j < expr.Length && (char.IsDigit(expr[j]) || expr[j].Equals('.'))){
                        SSLength++;
                        j++;
                    }

                }

                String s = expr.Substring(i, SSLength);
                i = i + SSLength;

                if (s.Equals("(")) { }
                else if (s.Equals("+")|| s.Equals("-")|| s.Equals("*")||s.Equals("/"))
                 ops.Push(s);


                else if (s.Equals(")"))
                {
                    int count = ops.Count;
                    while (count > 0)
                    {
                        String op = ops.Pop();
                        double v = vals.Pop();
                        if (op.Equals("+")) v = vals.Pop() + v;
                        else if (op.Equals("-")) v = vals.Pop() - v;
                        else if (op.Equals("*")) v = vals.Pop() * v;
                        else if (op.Equals("/")) v = vals.Pop() / v;
                        //else if (op.Equals("sqrt")) v = Math.Sqrt(v);
                        vals.Push(v);

                        count--;
                    }
                }
                else vals.Push(Double.Parse(s));
            }
            return vals.Pop();
            */
        }


    }
}
