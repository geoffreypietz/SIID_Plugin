using HomeSeerAPI;
using HSPI_SIID_ModBusDemo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HSPI_SIID.General
{
    public class SiidDevice
    {
        public Scheduler.Classes.DeviceClass Device { get; set; }
        public PlugExtraData.clsPlugExtraData Extra { get; set; }
        public int Ref { get; set; }
        public SiidDevice(InstanceHolder Instance,int R)
        {
            this.Ref = R;
            this.Device = (Scheduler.Classes.DeviceClass)Instance.host.GetDeviceByRef(R);
            this.Extra = Device.get_PlugExtraData_Get(Instance.host);
        }
        public SiidDevice(InstanceHolder Instance, Scheduler.Classes.DeviceClass Dev)
        {
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


    }
}
