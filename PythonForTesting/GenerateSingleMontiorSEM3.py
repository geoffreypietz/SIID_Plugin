GateHead='ID,Name,Floor,Room,code,address,statusOnly,CanDim,doNotLog,userAccess,notes,DeviceType,deviceTypeString,RelationshipStatus,associatedDevicesList,Type,Gateway,TCP,Poll,Enabled,BigE,ZeroB,RWRetry,RWTime,Delay,RegWrite,LinkedDevices,RawValue,ProcessedValue'
DevHead='ID,Name,Floor,Room,code,address,statusOnly,CanDim,doNotLog,userAccess,notes,DeviceType,deviceTypeString,RelationshipStatus,associatedDevicesList,Type,GateID,Gateway,RegisterType,SlaveId,ReturnType,SignedValue,ScratchpadString,DisplayFormatString,ReadOnlyDevice,DeviceEnabled,RegisterAddress,RawValue,ProcessedValue'

Devs=list()
base=2385
DD=list()
for i in range(1,39,2):
    line='"'+str(i)+'","Outdoor Meter'+str(i-1)+'","Modbus","Meters","","","True","False","True","Any","","Plug_In_Plug-In API_0__0_Plug-In Type 0","","Child","","Modbus Device","100","Scadametrics","3","4","1","false","$('+str(i)+')*.1","{0}","false","true","3","'+str(2385+i-1)+'","484.5"'
    Devs.append(line)
    DD.append(str(i))



DEVLIST=','.join(DD)

Gateway='"100","Scadametrics","Modbus","Meters","","","False","False","True","Any","","Plug_In_Plug-In API_0__0_Plug-In Type 0","","Parent_Root","'+DEVLIST+'","Modbus Gateway","192.168.3.3","502","1000","true","false","false","2","1000","0","1","'+DEVLIST+'","0","0"'

printlines=list()
printlines.append(GateHead)
printlines.append(Gateway)
printlines.append(DevHead)
for line in Devs:
    printlines.append(line)

open('Imp.csv','w').write('\n'.join(printlines))
