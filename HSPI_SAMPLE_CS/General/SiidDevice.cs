using HomeSeerAPI;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HSPI_Utilities_Plugin.General
{
    public class SiidDevice
    {
        public Scheduler.Classes.DeviceClass Device { get; set; }
        public PlugExtraData.clsPlugExtraData Extra { get; set; }
        public int Ref { get; set; }
        public InstanceHolder Instance { get; set; }
        public SiidDevice(InstanceHolder I,int R)
        {
            this.Instance = I;
            this.Ref = R;
            this.Device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(R);
            this.Extra = Device.get_PlugExtraData_Get(Instance.host);
        }
        public SiidDevice(InstanceHolder I)
        {
            this.Instance = I;
            this.Ref = 0;
            this.Device = null;
            this.Extra = null;
        }
        public SiidDevice(InstanceHolder I, Scheduler.Classes.DeviceClass Dev)
        {
            this.Instance = I;
            this.Ref = Dev.get_Ref(Instance.host);
            this.Device = Dev;
            this.Extra = Dev.get_PlugExtraData_Get(Instance.host);
        }

        public static SiidDevice GetFromListByID(List<SiidDevice> li, int R)
        {
            lock (li)
            {
                foreach (SiidDevice Dev in li)
                {
                    if (Dev.Ref == R)
                    {
                        return Dev;
                    }
                }
            }
            //check to see if it is a SIID device that just didn't make it into the list

            return null;
        }
        public static void removeDev(List<SiidDevice> li, int R)
        {
            li.Remove(GetFromListByID(li, R));
            
        }

        public static void Update(InstanceHolder I)
        {
            List<SiidDevice> UpdatedDevs = new List<SiidDevice>();
            lock (I.Devices)
            {
                foreach (SiidDevice D in I.Devices.ToList())
                {
                    if (I.host.DeviceExistsRef(D.Ref))
                    {
                        UpdatedDevs.Add(D);
                    }


                }
            }
            I.Devices = UpdatedDevs;
        }

        public void UpdateExtraData(string key, string value)
        {
           
            var parts = HttpUtility.ParseQueryString(Device.get_PlugExtraData_Get(Instance.host).GetNamed("SSIDKey").ToString());
            value = value.Replace("+", "(^p^)"); //OK clearly + and 2B are all sorts of messed up
            
            //I think the parts.ToString() is setting + and %2B to %20 which is a white space, which is really obnoxious
            //(Only on homeseer boxes)
            //My workaround is to replace all "+" with "(^p^)", and replace those back later
            parts[key] = value;
            Extra.RemoveNamed("SSIDKey");
            Extra.AddNamed("SSIDKey", parts.ToString());
           // Instance.hspi.Log("Set " + key + " + " + value,0);
            Device.set_PlugExtraData_Set(Instance.host, Extra);
            

        }
    


    }
}
