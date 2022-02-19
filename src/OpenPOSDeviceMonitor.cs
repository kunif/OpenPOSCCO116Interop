/*

  Copyright (C) 2022 Kunio Fukuchi

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Kunio Fukuchi

*/

namespace OpenPOS.CCO116Interop
{
    using Microsoft.PointOfService;
    using OpenPOS.Devices;
    using OpenPOS.Extension;
    using System;
    using System.Collections.Generic;

    [ServiceObject(DeviceType.RFIDScanner, "OpenPOS 1.16 DeviceMonitor", "OPOS DeviceMonitor CCO Interop", 1, 16)]
    public class OpenPOSDeviceMonitor : RFIDScanner, IDeviceMonitor116, ILegacyControlObject, IDisposable
    {
        private OpenPOS.Devices.OPOSDeviceMonitor _cco = null;
        private const string _oposDeviceClass = "DeviceMonitor";
        private string _oposDeviceName = "";
        private int _binaryConversion = 0;

        #region Event handler management variable

        public override event DataEventHandler DataEvent;

        public override event DirectIOEventHandler DirectIOEvent;

        public override event DeviceErrorEventHandler ErrorEvent;

        public override event OutputCompleteEventHandler OutputCompleteEvent;

        public override event StatusUpdateEventHandler StatusUpdateEvent;

        #endregion Event handler management variable

        #region Constructor, Destructor

        public OpenPOSDeviceMonitor()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSDeviceMonitor()
        {
            Dispose(false);
        }

        #region IDisposable Support

        private bool _disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // TODO: Discard the managed state (managed object).
                }

                if (_cco != null)
                {
                    _cco.DataEvent -= (_IOPOSDeviceMonitorEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSDeviceMonitorEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSDeviceMonitorEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    //_cco.OutputCompleteEvent -= (_IOPOSDeviceMonitorEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSDeviceMonitorEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
                    _cco = null;
                }

                _disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Support

        #endregion Constructor, Destructor

        #region Utility subroutine

        /// <summary>
        /// Check the processing result value of OPOS and generate a PosControlException exception if it is an error.
        /// </summary>
        /// <param name="value">OPOS method return value or ResultCode property value</param>
        private void VerifyResult(int value)
        {
            if (value != (int)ErrorCode.Success)
            {
                ErrorCode eValue = (ErrorCode)InteropEnum<ErrorCode>.ToEnumFromInteger(value);
                throw new Microsoft.PointOfService.PosControlException((_oposDeviceClass + ":" + _oposDeviceName), eValue, _cco.ResultCodeExtended);
            }
        }

        #endregion Utility subroutine

        #region Process of relaying OPOS event and generating POS for.NET event

        private void _cco_DataEvent(int Status)
        {
            if (this.DataEvent != null)
            {
                DataEvent(this, new DataEventArgs(Status));
            }
        }

        private void _cco_DirectIOEvent(int EventNumber, ref int pData, ref string pString)
        {
            if (this.DirectIOEvent != null)
            {
                DirectIOEventArgs eDE = new DirectIOEventArgs(EventNumber, pData, pString);
                DirectIOEvent(this, eDE);
                pData = eDE.Data;
                pString = Convert.ToString(eDE.Object);
            }
        }

        private void _cco_ErrorEvent(int ResultCode, int ResultCodeExtended, int ErrorLocus, ref int pErrorResponse)
        {
            if (this.ErrorEvent != null)
            {
                ErrorCode eCode = (ErrorCode)InteropEnum<ErrorCode>.ToEnumFromInteger(ResultCode);
                ErrorLocus eLocus = (ErrorLocus)InteropEnum<ErrorLocus>.ToEnumFromInteger(ErrorLocus);
                ErrorResponse eResponse = (ErrorResponse)InteropEnum<ErrorResponse>.ToEnumFromInteger(pErrorResponse);
                DeviceErrorEventArgs eEE = new DeviceErrorEventArgs(eCode, ResultCodeExtended, eLocus, eResponse);
                ErrorEvent(this, eEE);
                pErrorResponse = (int)eEE.ErrorResponse;
            }
        }

        private void _cco_OutputCompleteEvent(int OutputID)
        {
            if (this.OutputCompleteEvent != null)
            {
                OutputCompleteEvent(this, new OutputCompleteEventArgs(OutputID));
            }
        }

        private void _cco_StatusUpdateEvent(int Data)
        {
            if (this.StatusUpdateEvent != null)
            {
                StatusUpdateEvent(this, new StatusUpdateEventArgs(Data));
            }
        }

        #endregion Process of relaying OPOS event and generating POS for.NET event

        #region ILegacyControlObject member

        public BinaryConversion BinaryConversion
        {
            get
            {
                return (BinaryConversion)InteropEnum<BinaryConversion>.ToEnumFromInteger(_cco.BinaryConversion);
            }
            set
            {
                _cco.BinaryConversion = (int)value;
                VerifyResult(_cco.ResultCode);
                _binaryConversion = _cco.BinaryConversion;
            }
        }

        public string ControlObjectDescription
        {
            get { return _cco.ControlObjectDescription; }
        }

        public Version ControlObjectVersion
        {
            get { return InteropCommon.ToVersion(_cco.ControlObjectVersion); }
        }

        #endregion ILegacyControlObject member

        #region Device common properties

        public override bool CapCompareFirmwareVersion
        {
            get { return _cco.CapCompareFirmwareVersion; }
        }

        public override PowerReporting CapPowerReporting
        {
            get { return (PowerReporting)InteropEnum<PowerReporting>.ToEnumFromInteger(_cco.CapPowerReporting); }
        }

        public override bool CapStatisticsReporting
        {
            get { return _cco.CapStatisticsReporting; }
        }

        public override bool CapUpdateFirmware
        {
            get { return _cco.CapUpdateFirmware; }
        }

        public override bool CapUpdateStatistics
        {
            get { return _cco.CapUpdateStatistics; }
        }

        public override bool AutoDisable
        {
            get
            {
                return _cco.AutoDisable;
            }
            set
            {
                _cco.AutoDisable = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override string CheckHealthText
        {
            get { return _cco.CheckHealthText; }
        }

        public override bool Claimed
        {
            get { return _cco.Claimed; }
        }

        public override int DataCount
        {
            get { return _cco.DataCount; }
        }

        public override bool DataEventEnabled
        {
            get
            {
                return _cco.DataEventEnabled;
            }
            set
            {
                _cco.DataEventEnabled = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool DeviceEnabled
        {
            get
            {
                return _cco.DeviceEnabled;
            }
            set
            {
                _cco.DeviceEnabled = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override bool FreezeEvents
        {
            get
            {
                return _cco.FreezeEvents;
            }
            set
            {
                _cco.FreezeEvents = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override int OutputId
        {
            get { return 0; }
        }

        public override PowerNotification PowerNotify
        {
            get
            {
                return (PowerNotification)InteropEnum<PowerNotification>.ToEnumFromInteger(_cco.PowerNotify);
            }
            set
            {
                _cco.PowerNotify = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public override PowerState PowerState
        {
            get { return (PowerState)InteropEnum<PowerState>.ToEnumFromInteger(_cco.PowerState); }
        }

        public override ControlState State
        {
            get { return (ControlState)InteropEnum<ControlState>.ToEnumFromInteger(_cco.State); }
        }

        public override string ServiceObjectDescription
        {
            get { return _cco.ServiceObjectDescription; }
        }

        public override Version ServiceObjectVersion
        {
            get { return InteropCommon.ToVersion(_cco.ControlObjectVersion); }
        }

        public override string DeviceDescription
        {
            get { return _cco.DeviceDescription; }
        }

        public override string DeviceName
        {
            get { return _cco.DeviceName; }
        }

        #endregion Device common properties

        #region Device common method

        public override void Open()
        {
            if (string.IsNullOrWhiteSpace(_oposDeviceName))
            {
                try
                {
                    _oposDeviceName = GetConfigurationProperty("OposDeviceName");
                    _oposDeviceName.Trim();
                }
                catch
                {
                    _oposDeviceName = "";
                }
            }

            if (string.IsNullOrWhiteSpace(_oposDeviceName))
            {
                string strMessage = "OposDeviceName is not configured on " + DevicePath + ".";
                throw new Microsoft.PointOfService.PosControlException(strMessage, ErrorCode.NoExist);
            }

            if (_cco == null)
            {
                try
                {
                    // CCO object CreateInstance
                    _cco = new OpenPOS.Devices.OPOSDeviceMonitor();

                    // Register event handler
                    _cco.DataEvent += new _IOPOSDeviceMonitorEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSDeviceMonitorEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSDeviceMonitorEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    //_cco.OutputCompleteEvent += new _IOPOSDeviceMonitorEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSDeviceMonitorEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
                }
                catch
                {
                    string strMessage = "Can not create Common ControlObject on " + DevicePath + ".";
                    throw new Microsoft.PointOfService.PosControlException(strMessage, ErrorCode.Failure);
                }
            }

            VerifyResult(_cco.Open(_oposDeviceName));
        }

        public override void Close()
        {
            VerifyResult(_cco.Close());

            _cco.DataEvent -= (_IOPOSDeviceMonitorEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSDeviceMonitorEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSDeviceMonitorEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            //_cco.OutputCompleteEvent -= (_IOPOSDeviceMonitorEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSDeviceMonitorEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
            _cco = null;
        }

        public override void Claim(int timeout)
        {
            VerifyResult(_cco.ClaimDevice(timeout));
        }

        public override void Release()
        {
            VerifyResult(_cco.ReleaseDevice());
        }

        public override string CheckHealth(HealthCheckLevel level)
        {
            VerifyResult(_cco.CheckHealth((int)level));
            return _cco.CheckHealthText;
        }

        public override void ClearInput()
        {
            VerifyResult(_cco.ClearInput());
        }

        public override void ClearInputProperties()
        {
            VerifyResult(_cco.ClearInputProperties());
        }

        public override void ClearOutput()
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override DirectIOData DirectIO(int command, int data, object obj)
        {
            var intValue = data;
            var stringValue = Convert.ToString(obj);
            VerifyResult(_cco.DirectIO(command, ref intValue, ref stringValue));
            return new DirectIOData(intValue, stringValue);
        }

        public override CompareFirmwareResult CompareFirmwareVersion(string firmwareFileName)
        {
            int result;
            VerifyResult(_cco.CompareFirmwareVersion(firmwareFileName, out result));
            return (CompareFirmwareResult)InteropEnum<CompareFirmwareResult>.ToEnumFromInteger(result);
        }

        public override void UpdateFirmware(string firmwareFileName)
        {
            VerifyResult(_cco.UpdateFirmware(firmwareFileName));
        }

        public override void ResetStatistic(string statistic)
        {
            VerifyResult(_cco.ResetStatistics(statistic));
        }

        public override void ResetStatistics(string[] statistics)
        {
            VerifyResult(_cco.ResetStatistics(string.Join(",", statistics)));
        }

        public override void ResetStatistics(StatisticCategories statistics)
        {
            VerifyResult(_cco.ResetStatistics(Enum.GetName(typeof(StatisticCategories), statistics)));
        }

        public override void ResetStatistics()
        {
            VerifyResult(_cco.ResetStatistics(""));
        }

        public override string RetrieveStatistic(string statistic)
        {
            var result = statistic;
            VerifyResult(_cco.RetrieveStatistics(ref result));
            return result;
        }

        public override string RetrieveStatistics(string[] statistics)
        {
            var result = string.Join(",", statistics);
            VerifyResult(_cco.RetrieveStatistics(ref result));
            return result;
        }

        public override string RetrieveStatistics(StatisticCategories statistics)
        {
            var result = Enum.GetName(typeof(StatisticCategories), statistics);
            VerifyResult(_cco.RetrieveStatistics(ref result));
            return result;
        }

        public override string RetrieveStatistics()
        {
            var result = "";
            VerifyResult(_cco.RetrieveStatistics(ref result));
            return result;
        }

        public override void UpdateStatistic(string name, object value)
        {
            VerifyResult(_cco.UpdateStatistics(name + "=" + value));
        }

        public override void UpdateStatistics(StatisticCategories statistics, object value)
        {
            VerifyResult(_cco.UpdateStatistics(Enum.GetName(typeof(StatisticCategories), statistics) + "=" + value));
        }

        public override void UpdateStatistics(Statistic[] statistics)
        {
            VerifyResult(_cco.UpdateStatistics(InteropCommon.ToStatisticsString(statistics)));
        }

        #endregion Device common method

        #region OPOSRFIDScanner  Specific Properties

        public override bool CapContinuousRead
        {
            get { return false; }
        }

        public override bool CapDisableTag
        {
            get { return false; }
        }

        public override bool CapLockTag
        {
            get { return false; }
        }

        public override RFIDProtocols CapMultipleProtocols
        {
            get { return RFIDProtocols.Other; }
        }

        public override bool CapReadTimer
        {
            get { return false; }
        }

        public override WriteTagSections CapWriteTag
        {
            get { return WriteTagSections.None; }
        }

        public override bool ContinuousReadMode
        {
            get { return false; }
        }

        public override byte[] CurrentTagId
        {
            get { return null; }
        }

        public override RFIDProtocols CurrentTagProtocol
        {
            get { return RFIDProtocols.Other; }
        }

        public override byte[] CurrentTagUserData
        {
            get { return null; }
        }

        public override RFIDProtocols ProtocolMask
        {
            get
            {
                return RFIDProtocols.Other;
            }
            set
            {
                VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
            }
        }

        public override int ReadTimerInterval
        {
            get
            {
                return 0;
            }
            set
            {
                VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
            }
        }

        public override int TagCount
        {
            get { return 0; }
        }

        #endregion OPOSRFIDScanner  Specific Properties

        #region OPOSDeviceMonitor  Specific Properties

        public DeviceDataValue DeviceData
        {
            get
            {
                string[] result = _cco.DeviceData.Split(new[] { ':' });
                int.TryParse(result[1], out int value);
                return new DeviceDataValue(result[0], value);
            }
        }

        public DeviceInfoValue[] DeviceList
        {
            get
            {
                string[] devices = _cco.DeviceList.Split(new[] { ',' });
                List<DeviceInfoValue> result = new List<DeviceInfoValue>();
                foreach (var item in devices)
                {
                    string[] entry = item.Split(new[] { ':' });
                    result.Add(new DeviceInfoValue(entry[0], entry[1], entry[2], entry[3]));
                }
                return result.ToArray();
            }
        }

        public MonitoringDeviceValue[] MonitoringDeviceList
        {
            get
            {
                string[] devices = _cco.DeviceList.Split(new[] { ',' });
                List<MonitoringDeviceValue> result = new List<MonitoringDeviceValue>();
                foreach (var item in devices)
                {
                    string[] entry = item.Split(new[] { ':' });
                    int.TryParse((string)entry[1], out int mode);
                    int.TryParse((string)entry[2], out int boundary);
                    int.TryParse((string)entry[3], out int subboundary);
                    int.TryParse((string)entry[4], out int intervaltime);
                    result.Add(new MonitoringDeviceValue(entry[0], (MonitoringModeType)InteropEnum<MonitoringModeType>.ToEnumFromInteger(mode), boundary, subboundary, intervaltime));
                }
                return result.ToArray();
            }
        }

        #endregion OPOSDeviceMonitor  Specific Properties

        #region OPOSRFIDScanner  Specific Methods

        public override void FirstTag()
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void NextTag()
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void PreviousTag()
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void DisableTag(byte[] tagId, int timeout, byte[] password)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void LockTag(byte[] tagId, int timeout, byte[] password)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void ReadTags(RFIDReadOptions cmd, byte[] filterId, byte[] filterMask, int start, int length, int timeout, byte[] password)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void StartReadTags(RFIDReadOptions cmd, byte[] filterId, byte[] filterMask, int start, int length, byte[] password)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void StopReadTags(byte[] password)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void WriteTagData(byte[] tagId, byte[] userData, int start, int timeout, byte[] password)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void WriteTagId(byte[] sourceTagId, byte[] destinationTagId, int timeout, byte[] password)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        #endregion OPOSRFIDScanner  Specific Methods

        #region OPOSDeviceMonitor  Specific Methods

        public void AddMonitoringDevice(string deviceId, MonitoringModeType monitoringMode, int boundary, int subBoundary, int intervalTime)
        {
            VerifyResult(_cco.AddMonitoringDevice(deviceId, (int)monitoringMode, boundary, subBoundary, intervalTime));
        }

        public void ClearMonitoringDevice()
        {
            VerifyResult(_cco.ClearMonitoringDevice());
        }

        public void DeleteMonitoringDevice(string deviceId)
        {
            VerifyResult(_cco.DeleteMonitoringDevice(deviceId));
        }

        public int GetDeviceValue(string deviceId)
        {
            int result;
            VerifyResult(_cco.GetDeviceValue(deviceId, out result));
            return result;
        }

        #endregion OPOSDeviceMonitor  Specific Methods
    }
}