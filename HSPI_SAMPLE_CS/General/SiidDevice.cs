using HomeSeerAPI;
using HSPI_SIID_ModBusDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace HSPI_SIID.General
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
        public SiidDevice(InstanceHolder I, Scheduler.Classes.DeviceClass Dev)
        {
            this.Instance = I;
            this.Ref = Dev.get_Ref(Instance.host);
            this.Device = Dev;
            this.Extra = Dev.get_PlugExtraData_Get(Instance.host);
        }

        public static SiidDevice GetFromListByID(List<SiidDevice> li, int R)
        {
            foreach (SiidDevice Dev in li)
            {
                if (Dev.Ref == R)
                {
                    return Dev;
                }
            }
            return null;
        }

        public void UpdateExtraData(string key, string value)
        {
           
            var parts = HttpUtility.ParseQueryString(Extra.GetNamed("SSIDKey").ToString());
            parts[key] = value;
            Extra.RemoveNamed("SSIDKey");
            Extra.AddNamed("SSIDKey", parts.ToString());
            Device.set_PlugExtraData_Set(Instance.host, Extra);

        }


    }
}
