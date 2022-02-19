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
    using System.Text.RegularExpressions;

    [ServiceObject(DeviceType.RemoteOrderDisplay, "OpenPOS 1.16 GestureControl", "OPOS GestureControl CCO Interop", 1, 16)]
    public class OpenPOSGestureControl : RemoteOrderDisplay, IGestureControl116, ILegacyControlObject, IDisposable
    {
        private OpenPOS.Devices.OPOSGestureControl _cco = null;
        private const string _oposDeviceClass = "GestureControl";
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

        public OpenPOSGestureControl()
        {
            _cco = null;
            _oposDeviceName = "";
            _binaryConversion = 0;
        }

        ~OpenPOSGestureControl()
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
                    //_cco.DataEvent -= (_IOPOSGestureControlEvents_DataEventEventHandler)_cco_DataEvent;
                    _cco.DirectIOEvent -= (_IOPOSGestureControlEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
                    _cco.ErrorEvent -= (_IOPOSGestureControlEvents_ErrorEventEventHandler)_cco_ErrorEvent;
                    _cco.OutputCompleteEvent -= (_IOPOSGestureControlEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
                    _cco.StatusUpdateEvent -= (_IOPOSGestureControlEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
            get { return 0; }
        }

        public override bool DataEventEnabled
        {
            get
            {
                return false;
            }
            set
            {
                VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
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
            get { return _cco.OutputID; }
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
                    _cco = new OpenPOS.Devices.OPOSGestureControl();

                    // Register event handler
                    //_cco.DataEvent += new _IOPOSGestureControlEvents_DataEventEventHandler(_cco_DataEvent);
                    _cco.DirectIOEvent += new _IOPOSGestureControlEvents_DirectIOEventEventHandler(_cco_DirectIOEvent);
                    _cco.ErrorEvent += new _IOPOSGestureControlEvents_ErrorEventEventHandler(_cco_ErrorEvent);
                    _cco.OutputCompleteEvent += new _IOPOSGestureControlEvents_OutputCompleteEventEventHandler(_cco_OutputCompleteEvent);
                    _cco.StatusUpdateEvent += new _IOPOSGestureControlEvents_StatusUpdateEventEventHandler(_cco_StatusUpdateEvent);
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

            //_cco.DataEvent -= (_IOPOSGestureControlEvents_DataEventEventHandler)_cco_DataEvent;
            _cco.DirectIOEvent -= (_IOPOSGestureControlEvents_DirectIOEventEventHandler)_cco_DirectIOEvent;
            _cco.ErrorEvent -= (_IOPOSGestureControlEvents_ErrorEventEventHandler)_cco_ErrorEvent;
            _cco.OutputCompleteEvent -= (_IOPOSGestureControlEvents_OutputCompleteEventEventHandler)_cco_OutputCompleteEvent;
            _cco.StatusUpdateEvent -= (_IOPOSGestureControlEvents_StatusUpdateEventEventHandler)_cco_StatusUpdateEvent;
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
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void ClearOutput()
        {
            VerifyResult(_cco.ClearOutput());
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

        #region OPOSRemoteOrderDisplay  Specific Properties

        public override bool CapMapCharacterSet
        {
            get { return false; }
        }

        public override bool CapSelectCharacterSet
        {
            get { return false; }
        }

        public override bool CapTone
        {
            get { return false; }
        }

        public override bool CapTouch
        {
            get { return false; }
        }

        public override bool CapTransaction
        {
            get { return false; }
        }

        public override bool AsyncMode
        {
            get
            {
                return false;
            }
            set
            {
                VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
            }
        }

        public override int AutoToneDuration
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

        public override int AutoToneFrequency
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

        public override int CharacterSet
        {
            get { return 0; }
        }

        public override int[] CharacterSetList
        {
            get { return null; }
        }

        public override int Clocks
        {
            get { return 0; }
        }

        public override DeviceUnits CurrentUnitId
        {
            get
            {
                return DeviceUnits.Unit1;
            }
            set
            {
                VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
            }
        }

        public override string ErrorString
        {
            get { return string.Empty; }
        }

        public override DeviceUnits ErrorUnits
        {
            get { return DeviceUnits.Unit1; }
        }

        public override string EventString
        {
            get { return string.Empty; }
        }

        public override RemoteOrderDisplayEventTypes EventType
        {
            get
            {
                return RemoteOrderDisplayEventTypes.TouchUp;
            }
            set
            {
                VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
            }
        }

        public override DeviceUnits EventUnitId
        {
            get { return DeviceUnits.Unit1; }
        }

        public override DeviceUnits EventUnits
        {
            get { return DeviceUnits.Unit1; }
        }

        public override bool MapCharacterSet
        {
            get
            {
                return false;
            }
            set
            {
                VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
            }
        }

        public override int SystemClocks
        {
            get { return 0; }
        }

        public override int SystemVideoSaveBuffers
        {
            get { return 0; }
        }

        public override int Timeout
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

        public override DeviceUnits UnitsOnline
        {
            get { return DeviceUnits.Unit1; }
        }

        public override int VideoDataCount
        {
            get { return 0; }
        }

        public override int VideoMode
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

        private static Regex s_vm = new Regex(@"[0-9]+:[0-9]+x[0-9]+x[0-9]+[MC]", RegexOptions.Compiled);

        public override VideoMode[] VideoModesList
        {
            get
            {
                List<VideoMode> vml = new List<VideoMode>();
                return vml.ToArray();
            }
        }

        public override int VideoSaveBuffers
        {
            get { return 0; }
        }

        #endregion OPOSRemoteOrderDisplay  Specific Properties

        #region OPOSGestureControl  Specific Properties

        public string AutoMode
        {
            get
            {
                return _cco.AutoMode;
            }
            set
            {
                _cco.AutoMode = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public string[] AutoModeList
        {
            get { return _cco.AutoModeList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        public string CapAssociatedHardTotalsDevice
        {
            get { return _cco.CapAssociatedHardTotalsDevice; }
        }

        public bool CapMotion
        {
            get { return _cco.CapMotion; }
        }

        public bool CapMotionCreation
        {
            get { return _cco.CapMotionCreation; }
        }

        public bool CapPose
        {
            get { return _cco.CapPose; }
        }

        public bool CapPoseCreation
        {
            get { return _cco.CapPoseCreation; }
        }

        public CapStorageType CapStorage
        {
            get { return (CapStorageType)InteropEnum<CapStorageType>.ToEnumFromInteger(_cco.CapStorage); }
        }

        public GestureJoint[] JointList
        {
            get
            {
                string[] joints = _cco.JointList.Split(new[] { ',' });
                List<GestureJoint> result = new List<GestureJoint>();
                foreach (var item in joints)
                {
                    string[] entry = item.Split(new[] { ':' });
                    result.Add(new GestureJoint(entry[0], (entry[1] == "1")));
                }
                return result.ToArray();
            }
        }

        public string[] MotionList
        {
            get { return _cco.AutoModeList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        public bool PoseCreationMode
        {
            get
            {
                return _cco.PoseCreationMode;
            }
            set
            {
                _cco.PoseCreationMode = value;
                VerifyResult(_cco.ResultCode);
            }
        }

        public string[] PoseList
        {
            get { return _cco.AutoModeList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries); }
        }

        public StorageType Storage
        {
            get
            {
                return (StorageType)InteropEnum<StorageType>.ToEnumFromInteger(_cco.Storage);
            }
            set
            {
                _cco.Storage = (int)value;
                VerifyResult(_cco.ResultCode);
            }
        }

        #endregion OPOSGestureControl  Specific Properties

        #region OPOSRemoteOrderDisplay  Specific Methods

        public override void ClearVideo(DeviceUnits units, VideoAttributes attribute)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void ClearVideoRegion(DeviceUnits units, int row, int column, int height, int width, VideoAttributes attribute)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void ControlClock(DeviceUnits units, ClockFunction clockFunction, int clockId, int hours, int minutes, int seconds, int row, int column, VideoAttributes attribute, ClockMode mode)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void ControlCursor(DeviceUnits units, VideoCursorType cursorType)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void CopyVideoRegion(DeviceUnits units, int row, int column, int height, int width, int targetRow, int targetColumn)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void DisplayData(DeviceUnits units, int row, int column, VideoAttributes attribute, string data)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void DrawBox(DeviceUnits units, int row, int column, int height, int width, VideoAttributes attribute, BorderType borderType)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void FreeVideoRegion(DeviceUnits units, int bufferId)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void ResetVideo(DeviceUnits units)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void RestoreVideoRegion(DeviceUnits units, int targetRow, int targetColumn, int bufferId)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void SaveVideoRegion(DeviceUnits units, int row, int column, int height, int width, int bufferId)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void SelectCharacterSet(DeviceUnits units, int characterSet)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void SetCursor(DeviceUnits units, int row, int column)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void TransactionDisplay(DeviceUnits units, RemoteOrderDisplayTransaction transactionFunction)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void UpdateVideoRegionAttribute(DeviceUnits units, VideoAttributeCommand attributeFunction, int row, int column, int height, int width, VideoAttributes attribute)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        public override void VideoSound(DeviceUnits units, int frequency, int duration, int numberOfCycles, int interSoundWait)
        {
            VerifyResult((int)OPOS_Constants.OPOS_E_ILLEGAL);
        }

        #endregion OPOSRemoteOrderDisplay  Specific Methods

        #region OPOSGestureControl  Specific Methods

        public void CreateMotion(string fileName, string[] poseList)
        {
            VerifyResult(_cco.CreateMotion(fileName, string.Join(",", poseList)));
        }

        public void CreatePose(string fileName, int time)
        {
            VerifyResult(_cco.CreatePose(fileName, time));
        }

        public int GetPosition(string jointId)
        {
            VerifyResult(_cco.GetPosition(jointId, out int position));
            return position;
        }

        public void SetPosition(IEnumerable<GesturePosition> positionList, int time, bool absolute)
        {
            List<string> positions = new List<string>();
            foreach (GesturePosition pos in positionList)
            {
                List<string> group = new List<string>();
                group.Add(pos.JointId);
                group.Add(pos.Position.ToString());
                positions.Add(string.Join(":", group));
            }
            VerifyResult(_cco.SetPosition(string.Join(",", positions), time, absolute));
        }

        public void SetSpeed(IEnumerable<GestureSpeed> speedList, int time)
        {
            List<string> speeds = new List<string>();
            foreach (GestureSpeed pos in speedList)
            {
                List<string> group = new List<string>();
                group.Add(pos.JointId);
                group.Add(pos.Speed.ToString());
                speeds.Add(string.Join(":", group));
            }
            VerifyResult(_cco.SetSpeed(string.Join(",", speeds), time));
        }

        public void StartMotion(string fileName)
        {
            VerifyResult(_cco.StartMotion(fileName));
        }

        public void StartPose(string fileName)
        {
            VerifyResult(_cco.StartPose(fileName));
        }

        public void StopControl(int outputId)
        {
            VerifyResult(_cco.StopControl(outputId));
        }

        #endregion OPOSGestureControl  Specific Methods
    }
}