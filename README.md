# Alternative way to call OPOS version 1.16 control from POS for.NET

This is POS for.NET service object for calling OPOS version 1.16 controls from POS for.NET.

In POS for.NET, a mechanism called Legacy COM Interop, called OPOS control, is built inside POS for.NET.  
However, this mechanism has the following issues:

- The following additions/changes added in UnifiedPOS version 1.16 are not supported.
  - Added Pattern related property/methods, multiple lights on method to Lights device.  
  - Added battery status report in seconds related properties of PosPower device.  
  - Added RCSD(Retail Communication Service Devices) related interfaces.  
- The following additions/changes added in UnifiedPOS version 1.15 are not supported.
  - Added CAT device functions to ElectronicValueReader/Writer device.  
  - Added EVRW_ST_CAT to the ServiceType property of ElectronicValueReader/Writer device.  
  - Added values to CountryCode and DateType properties of FiscalPrinter device.  
- Only 24 of 36 types of device classes defined in UnifiedPOS are supported (in case of v1.14.1)  
  Unsupported devices:  
  - Belt
  - Bill Acceptor
  - Bill Dispenser
  - Biometrics
  - Coin Acceptor
  - Electronic Journal
  - Electronic Value Reader/Writer
  - Gate
  - Image Scanner
  - Item Dispenser
  - Lights
  - RFID Scanner
- Changing the value of OPOS's BinaryConversion property requires a different conversion process than when calling OPOS directly

In order to solve these problems, i created a service object with the following features:

- An extension DLL for POS for.NET that supports the interfaces and definitions added in UnifiedPOS version 1.15/1.16 has been incorporated.  
- Supported devices added in UnifiedPOS version 1.16 with a loophole that makes them look like existing devices up to v1.14.1.
- Supported all 45 types of device classes defined in UnifiedPOS.  
- BinaryConversion processing for OPOS was divided into two kinds.  
  - In POS for.NET and OPOS, string properties/parameters are passed through without processing anything.  
  - Properties/parameters such as byte[] or Bitmap etc. in POS for.NET perform conversion processing according to the value of BinaryConversion.  
- When reading the property considered as Enum in POS for.NET, if OPOS notifies the undefined value, it raises PosControlException and notifies it by storing it in the exception's ErrorCodeExtended property.  
- Information on the corresponding OPOS device name is defined in the Configuration.xml file of POS for.NET.  


## Development/Execuion environment

To develop and execute this program you need:

- Visual Studio 2022 or Visual Studio Community 2022  version 17.1.0 (development only)  
- .NET framework 4.8  
- Microsoft Point of Service for .NET v1.14.1 (POS for.NET) : https://www.microsoft.com/en-us/download/details.aspx?id=55758  
- OpenPOS.Extension.Ver115 : https://github.com/kunif/OpenPOSExtensionVer115  
- OpenPOS.Extension.Ver116 : https://github.com/kunif/OpenPOSExtensionVer116  
- Common Control Objects 1.16.000 : https://github.com/kunif/OPOS-CCO  
- OPOS service object of target device  

To develop/execute this service object, you need the CCO of Common Control Objects 1.16 and the PIA and UnifiedPOS 1.15/1.16 support extension DLL for POS for.NET.  
If the device vendor's OPOS only installs .ocx for the target device, or if CCO 1.14.001 and earlier versions are not supported, replace them.  
Install both CCO and PIA with Install_CCOandPIA.bat of Common Control Objects 1.16.  
Please register UnifiedPOS 1.15/1.16 Support extension DLL for POS for.NET.  


## Installation on execution environment

To install on the execution environment, please follow the procedure below.

- Create an appropriate folder and copy OpenPOS.CCO116Interop.dll.  
  It is not the root of the drive, and the path name of the folder does not include the blank space and should consist only of alphanumeric characters less equal 0x7E.  
  There is less chance of that person having a problem.  

- Register with the arbitrary name with the above folder as the value in the ControlAssemblies key of the POS for.NET registry.  
  For example "AltCCOInterops"="C:\\\\POSforNET\\\\CCOInterop\\\\"  
  However, during development work, it is automatically registered as part of the processing at build time.  
  The position of the target key is as follows.  
  - 64bitOS: HKEY_LOCAL_MACHINE\\SOFTWARE\\WOW6432Node\\POSfor.NET\\ControlAssemblies  
  - 32bitOS: HKEY_LOCAL_MACHINE\\SOFTWARE\\POSfor.NET\\ControlAssemblies  


## A loophole to support v1.16 devices with v1.14.1  

The extension of Legacy COM Interop by ILegacyControlObject in v1.14.1 itself is a hint, and even if it is a new device class that does not exist in v1.14.1, it once supports the same common property / method / event. If I could create an object as a device of, I thought that it would be possible to call it as a new device of v1.16 by checking the existence of the interface of the new device.  
This mechanism can also be used to create Native v1.16 service objects for POS for.NET itself.  

The supported relationships between v1.16 and v1.14.1 devices are shown below.  
The POS for.NET object is treated as an existing v1.14.1 device, but the OPOS object uses the CCO and registry of the corresponding device.  

- VideoCapture  
  - Supports ClearInput method, ErrorEvent event  
  - Implemented as ImageScanner in this project  
  - It can also use Biometrics, BumpBar, CheckScanner, ElectronicJournal, MICR, MSR, PINPad, PointCardRW, POSKeyboard, RemoteOrderDisplay, Scale, Scanner, SignatureCapture, SmartCardRW.  

- DeviceMonitor  
- IndividualRecognition  
- SoundRecorder  
- VoiceRecognition  
  - Supports AutoDisable/DataCount/DataEventEnabled properties, ClearInput/ClearInputProperies methods, DataEvent/ErrorEvent events  
  - Implemented as RFIDScanner in this project  
  - It can also use ElectronicValueRW  

- GestureControl  
- GraphicDisplay  
- SoundPlayer  
- SpeechSynthesis  
  - Supports OutputID property, ClearOutput method, ErrorEvent/OutputCompleteEvent event  
  - In this project, GestureControl and GraphicDisplay are implemented as RemoteOrderDisplay and SoundPlayer and SpeechSynthesis are implemented as ToneIndicator.  
  - It can also use CAT, FiscalPrinter, POSPrinter, BumpBar, PointCardRW, SmartCardRW, ElectronicJournal, RFIDScanner, ElectronicValueRW.


## Configuration

Create a device entry and set its properties using the posdm.exe program of POS for.NET.

- Create a device entry with the ADDDEVICE command of posdm  
  Example usage: posdm ADDDEVICE OposDmon1 /type:RFIDScanner /soname:"OpenPOS 1.16 DeviceMonitor"  

  - The specified device name ("OposEVRW1" in the example) is stored as the value of "HardwarePath"  
    Please specify a unique name that does not overlap the name of other POS for.NET or OPOS name.  
  - Please append "OpenPOS 1.16 " to the head of /soname: and specify the device class name enclosed in double quotes.  
    For example, "OpenPOS 1.16 CashDrawer", "OpenPOS 1.16 POSPrinter", "OpenPOS 1.16 VideoCapture", "OpenPOS 1.16 GestureControl" etc.  

- Set the OPOS device name to be used with the ADDPROPERTY command of posdm  
  Example usage: posdm ADDPROPERTY OposDeviceName VenderName_ModelName /type:RFIDScanner /soname:"OpenPOS 1.16 DeviceMonitor" /path:OposDmon1  

  - The property name to be set is "OposDeviceName".  
  - Please specify the device name key or logical device name that exists in the OPOS registry for the value to be set ("VenderName_ModelName" in the example).  

Target device entry in Configuration.xml after execution example:


    <ServiceObject Name="OpenPOS 1.16 DeviceMonitor" Type="RFIDScanner">  
      <Device HardwarePath="" Enabled="yes" PnP="no" />  
      <Device HardwarePath="OposDmon1" Enabled="yes" PnP="no">  
        <Property Name="OposDeviceName" Value="VenderName_ModelName" />  
      </Device>  
    </ServiceObject>  


## How to call

Here is a procedure and an example of calling the device entry created in the usage example of the above setting.

- Add "OpenPOS.Extension.Ver115" to the project reference (found below).
  "C:\\Windows\\Microsoft.NET\\assembly\\GAC_MSIL\\OpenPOS.Extension.Ver115\\v4.0_1.15.0.0__ad2c9a67c3439201\\OpenPOS.Extension.Ver115.dll"
- Add "OpenPOS.Extension.Ver116" to the project reference (found below).
  "C:\\Windows\\Microsoft.NET\\assembly\\GAC_MSIL\\OpenPOS.Extension.Ver116\\v4.0_1.16.0.0__ad2c9a67c3439201\\OpenPOS.Extension.Ver116.dll"
- Add "using OpenPOS.Extension;" to the source code.  
- Call PoExplorer's GetDevices method with the device class name and DeviceCompatibilities specified and get the device collection of the corresponding device class  
- Search for a DeviceInfo whose ServiceObjectName and HardwarePath match in the acquired device collection and generate an object with the CreateInstance method based on it  
- Register event handler  
- Call Open method  

Example code:


    RFIDScanner dmonObj1 = null;
    PosExplorer explorer = new PosExplorer();
    DeviceCollection rfidList = explorer.GetDevices("RFIDScanner", DeviceCompatibilities.CompatibilityLevel1);
    foreach (DeviceInfo rfidInfo in rfidList)
    {
        if  (rfidInfo.ServiceObjectName == "OpenPOS 1.16 DeviceMonitor")
        {
            if (rfidInfo.HardwarePath == "OposDmon1")
            {
                dmonObj1 = (RFIDScanner)explorer.CreateInstance(rfidInfo);
                break;
            }
        }
    }
    if (dmonObj1 != null)
    {
        dmonObj1.DataEvent += dmonObj1_DataEvent;
        dmonObj1.DirestIOEvent += dmonObj1_DirectIOEvent;
        dmonObj1.ErrorEvent += dmonObj1_ErrorEvent;
        dmonObj1.StatusUpdateEvent += dmonObj1_StatusUpdateEvent;

        dmonObj1.Open();
    }


Note: The value of the Compatibility property(DeviceCompatibilities) varies from case to case.  
In the state listed in DeviceCollection/DeviceInfo, it is "CompatibilityLevel1", and for objects generated by CreateInstance it is "Opos".


- Check whether the interface of OpenPOS.Extension.Ver116 is included and then call the method of UnifoedPOS 1.16.  

Example code:


    // The object is declared in RFIDScanner dmonObj1
    
    try
    {
        if (dmonObj1 is IDeviceMonitor116)
        {
            string sDeviceID = "Device01";
            MonitoringModeType eMMode = MonitoringModeType.Update;
            int iBoundary = 180;
            int iSubBoundary = 0;
            int iIntervalTime = 500;
            try
            {
                ((IDeviceMonitor116) dmonObj1).AddMonitoringDevice(sDeviceID, eMMode, iBoundary, iSubBoundary, iIntervalTime);
            }
            catch(UPOSException ue)
            {
            }
        }
    }
    catch(Exception ae)
    {
    }


## Known issues

Currently known issues are as follows.

- Have not checked the operation using actual OPOS or device.  
- In particular, it is unknown whether the conversion of string (OPOS) and Bitmap etc (POS for.NET) of the following property/parameter/return value is correct.  
  - BiometricsInformationRecord(BIR) related property/parameter/return value of Biometrics device  
  - RawSensorData property of Biometrics device  
  - ImageData property of CheckScanner device  
  - FrameData property of ImageScanner device  
- There are no functions such as acquisition of operation record and information acquisition for troubleshooting.  

## Issues of POS for.NET  

The following issues were found in POS for.NET during the development process.  

- Information is insufficient in Biometrics' BIR defined in POS for.NET.  
  It does not contain the following information in the BIR diagram described in the UnifiedPOS specification.  
  - Quality  
  - Product ID (Owner, Type)  
  - Subtype  
  - Index Flag  
  - Index (UUID)  
  - Digital Signature  
- Also at BIR, whether it is a issue or not is not clear, but the situation is as follows.  
  - The Creation Date/Creation Time is the date and time when the BIR object was created in the POS for.NET service object, not the date and time the device read the information.  
    Although it may not be a problem with POS for.NET's own service object, information is rewritten in the case of processing relayed to/from OPOS.  

## Customize

If you want to customize for specific user/vendor specific processing etc, please do it freely.  
However, in that case, please change all the following information to make it an independent file so that it can be used concurrently with this service object at the same time.  

- File name: OpenPOS.CCO116Interop.dll  
- namespace: OpenPOS.CCO116Interop  
- GUID: [assembly: Guid("8fd7a631-348b-4f02-a0bd-eb918d9effd5")]  
- Service object name: [ServiceObject(DeviceType.Xxxx, "OpenPOS 1.16 Xxxx",  
- Class name: public class OpenPOSXxxx :  

Note) "Xxxx" in the above contains the device class name of UnifiedPOS/POS for.NET.

It is good to reduce the amount of work to extract only the device you want to customize and create a new one.  

In Addition, if it is good function with versatility, please propose it here.

## License

Licensed under the [zlib License](./LICENSE).
