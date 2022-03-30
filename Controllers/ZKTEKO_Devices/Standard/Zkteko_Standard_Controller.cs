using CardLibrary;
using KztekObject.Cards;
using KztekObject.enums;
using KztekObject.Events;
using KztekObject.Objects;
using SocketHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimezoneLibrary;
using static CardLibrary.CardFactory;
using static KztekObject.Controllers.ZKTEKO_Devices.Standard.Zkteco_Standard_SDK_Helper;
using static KztekObject.Events.ControllerEvent;

namespace KztekObject.Controllers.ZKTEKO_Devices.Standard
{
    public class Zkteko_Standard_Controller : IController
    {
        private CancellationTokenSource cts;
        ManualResetEvent ForceLoopIteration;
        public Zkteco_Standard_SDK_Helper standard_SDK_Helper = new Zkteco_Standard_SDK_Helper();
        #region: PROPERTIES
        public Controller ControllerInfor { get; set; } = new Controller();
        public bool IsAutoRestart { get; set; } = false;
        public DateTime AutoRestartTime { get; set; } = DateTime.MinValue;
        public bool isAutoEnableGetEventTime { get; set; } = false;
        public DateTime AutoStopGetEventTime { get; set; } = DateTime.MinValue;
        public DateTime AutoStartGetEventTime { get; set; } = DateTime.MinValue;
        public bool IsStopGetEvent { get; set; } = false;
        public bool IsBusy { get; set; } = false;
        #endregion: END PROPERTIES

        #region:EVENT
        public event CardEventHandler CardEvent;
        public event ErrorEventHandler ErrorEvent;
        public event InputEventHandler InputEvent;
        public event ConnectStatusChangeEventHandler ConnectStatusChangeEvent;
        #endregion: END EVENT

        #region: CONNECT--OK
        public bool TestConnection()
        {
            if (CommunicationType.IS_TCP(this.ControllerInfor.ControllerCommunicationType))
                return NetWorkTools.IsPingSuccess(this.ControllerInfor.Comport, 500);
            return false;
        }
        public bool Connect()
        {
            if (this.TestConnection())
            {
                this.ControllerInfor.Comkey = 1;
                standard_SDK_Helper.ControllerInfor = this.ControllerInfor;
                bool result = standard_SDK_Helper.Connect();
                ConnectStatusChangeEvent?.Invoke(this, new ConnectStatusCHangeEventArgs()
                {
                    ControllerID = this.ControllerInfor.ID,
                    CurrentStatus = this.ControllerInfor.IsConnect,
                });
                return result;
            }
            return false;
        }
        public bool Disconnect()
        {
            return standard_SDK_Helper.Disconnect();
        }
        #endregion: END CONNECT

        #region: THREAD --OK
        public void PollingStart()
        {
            cts = new CancellationTokenSource();
            ForceLoopIteration = new ManualResetEvent(false);
            Task.Run(() =>
                LoadEventFromLog(cts.Token), cts.Token
            );
        }

        public void PollingStop()
        {
            cts.Cancel();
            WaitHandle.WaitAny(
                        new[] { cts.Token.WaitHandle, ForceLoopIteration },
                        TimeSpan.FromMilliseconds(50));
        }

        private async Task LoadEventFromLog(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (this.IsBusy)
                    {
                        continue;
                    }
                    this.IsBusy = true;
                    standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);

                    int sdwEnrollNumber = 0;
                    int idwVerifyMode = 0;
                    int idwInOutMode = 0;
                    DateTime eventTime = DateTime.MinValue;
                    int machineID = 0;
                    int machineNumber = 0;

                    if (standard_SDK_Helper.ReadAllGLogs(this.ControllerInfor.MachineID))
                    {
                        while (standard_SDK_Helper.GetAllGLogData(this.ControllerInfor.MachineID, ref machineID, ref sdwEnrollNumber, ref machineNumber, ref idwVerifyMode, ref idwInOutMode, ref eventTime))//get records from the memory
                        {
                            CardEventArgs e = new CardEventArgs();
                            e.UserID = sdwEnrollNumber.ToString();
                            if (Convert.ToInt32(e.UserID) > 0)
                            {
                                e.EventStatus = EM_EventStatus.ACCESS_GRANT;
                            }
                            else
                            {
                                e.EventStatus = EM_EventStatus.ACCESS_DENIED;
                            }
                            e.Date = eventTime.ToString("yyyy/MM/dd");
                            e.Time = eventTime.ToString("HH:mm:ss");
                            CardEvent?.Invoke(this, e);
                        }
                        standard_SDK_Helper.ClearGLog(this.ControllerInfor.MachineID);
                    }
                    standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
                    this.IsBusy = false;
                }
                catch (Exception ex)
                {
                    ErrorEventArgs e = new ErrorEventArgs()
                    {
                        ErrorString = ex.Message,
                        ErrorFunc = GetFunctionName(),
                    };
                    ErrorEvent?.Invoke(this, e);
                }
                finally
                {
                    GC.Collect();
                    await Task.Delay(300);
                }
            }
        }
        #endregion: END THREAD

        #region: CARD
        public void SetCardFactory(Dictionary<int, EM_CardType> reader_CardTypes)
        {
            this.ControllerInfor.ReaderCardTypes = reader_CardTypes;
        }
        #endregion: END CARD

        #region: USER --OK
        public List<Employee> GetAllUserInfo(List<Employee> employees)
        {
            int userID = 2;
            bool bEnabled = true;
            string sName = "";
            string sPassword = "";
            EM_UserPrivilege iPrivilege = EM_UserPrivilege.COMMON;
            string sFPTmpData = "";
            int iFPTmpLength = 0;
            int iFpCount = 0;
            while (this.IsBusy)
            {
                Task.Delay(100);
            }
            this.IsBusy = true;
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.ReadAllUserID(this.ControllerInfor.MachineID);
            //standard_SDK_Helper.ReadAllTemplate(this.ControllerInfor.MachineID);

            while (standard_SDK_Helper.GetAllUserInfo(this.ControllerInfor.MachineID, ref userID, ref sName, ref sPassword, ref iPrivilege, ref bEnabled))//get all the users' information from the memory
            {
                Employee employee = new Employee();
                employee.MemoryID = Convert.ToInt32(userID);
                employee.Name = sName;
                employee.CardNumber = sPassword;
                standard_SDK_Helper.axCZKEM1.GetStrCardNumber(out string sCardnumber);//get the card number from the memory             
                employee.CardNumber = sCardnumber;
                for (int idwFingerIndex = 0; idwFingerIndex < 10; idwFingerIndex++)
                {
                    if (standard_SDK_Helper.GetUserTmpExStr(this.ControllerInfor.MachineID, userID.ToString(), idwFingerIndex, out int flag, out string TmpData, out int dataLength))//get the corresponding templates string and length from the memory
                    {
                        employee.fingerDatas.Add(TmpData);
                        iFpCount++;
                    }
                    TmpData = "";
                }
                employees.Add(employee);
            }
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            this.IsBusy = false;
            return employees;
        }
        public Employee GetUserByUserId(int userID)
        {
            Employee employee = new Employee();
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            string name = "";
            string password = "";
            EM_UserPrivilege privilege = EM_UserPrivilege.COMMON;
            bool isEnable = false;
            standard_SDK_Helper.GetUserInfoByID(this.ControllerInfor.MachineID, userID, ref name, ref password, ref privilege, ref isEnable);
            standard_SDK_Helper.axCZKEM1.GetStrCardNumber(out string sCardnumber);//get the card number from the memory             
            int idwFingerIndex;
            int iFpCount = 0;

            for (idwFingerIndex = 0; idwFingerIndex < 10; idwFingerIndex++)
            {
                if (standard_SDK_Helper.GetUserTmpExStr(this.ControllerInfor.MachineID, userID.ToString(), idwFingerIndex, out int flag, out string TmpData, out int dataLength))//get the corresponding templates string and length from the memory
                {
                    employee.fingerDatas.Add(TmpData);
                    iFpCount++;
                }
                TmpData = "";
            }

            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return employee;
        }
        public bool DownloadUser(Employee employee)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            int userID = employee.MemoryID;
            string sName = employee.Name;
            string sCardNumber = employee.CardNumber;
            string password = employee.Password;
            int iPrivilege = (int)EM_UserPrivilege.COMMON;
            bool result = false;
            if (sCardNumber != "" && sCardNumber != "0")
            {
                Int64 cardNumber = 0;
                ICardController cardController = CardFactory.CreateCardController(employee.CardType);
                string cardNumberHexValue = cardController.GetCardHexNumber(employee.CardNumber);
                switch (employee.CardType)
                {
                    case (int)EM_CardType.Mifare:
                        cardNumber = Convert.ToInt64(sCardNumber,16);
                        break;
                    case (int)EM_CardType.Proximity_Left:
                        cardNumber = Convert.ToInt64(cardNumberHexValue);
                        break;
                    case (int)EM_CardType.ProximityRight:
                        cardNumber = Convert.ToInt64(sCardNumber);
                        break;
                }
                result = standard_SDK_Helper.axCZKEM1.SetStrCardNumber(cardNumber.ToString());
            }

            if (standard_SDK_Helper.SetUserInfo(this.ControllerInfor.MachineID, Convert.ToInt32(userID), sName, password, EM_UserPrivilege.COMMON, true))//upload user information to the device
            {
                for (int i = 1; i <= employee.fingerDatas.Count; i++)
                {
                    if (employee.fingerDatas[i-1] != null && employee.fingerDatas[i-1] != "")
                    {
                        result = standard_SDK_Helper.SetUserTmpExStr(this.ControllerInfor.MachineID, userID.ToString(), i, true, employee.fingerDatas[i-1]);//upload templates information to the device
                    }
                }
            }
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;
        }
        public bool DownloadMultyUser(List<Employee> employees)
        {
            bool isAllSuccess = true;
            foreach (Employee employee in employees)
            {
                if (!DownloadUser(employee))
                {
                    isAllSuccess = false;
                }
            }
            return isAllSuccess;
        }
        public bool DeleteCardByUserID(int userID)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);

            bool result = standard_SDK_Helper.DeleteEnrollData(this.ControllerInfor.MachineID, userID, 12);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return result;
        }
        #endregion: END USER

        #region: FINGER --OK
        public List<string> GetFinger(string userID)
        {
            List<string> fingers = new List<string>();
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            int idwFingerIndex;
            int iFpCount = 0;

            for (idwFingerIndex = 0; idwFingerIndex < 10; idwFingerIndex++)
            {
                if (standard_SDK_Helper.GetUserTmpExStr(this.ControllerInfor.MachineID, userID.ToString(), idwFingerIndex, out int flag, out string TmpData, out int dataLength))//get the corresponding templates string and length from the memory
                {
                    fingers.Add(TmpData);
                    iFpCount++;
                }
            }
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return fingers;
        }
        #endregion: END FINGER

        #region: DATE TIME --OK
        public bool SetDateTime(DateTime DateTime)
        {
            return standard_SDK_Helper.SetDeviceTime(this.ControllerInfor.MachineID, DateTime);
        }
        public bool SyncDateTime()
        {
            return standard_SDK_Helper.SyncTime(this.ControllerInfor.MachineID);
        }
        public bool GetDateTime(ref DateTime dateTime)
        {
            return standard_SDK_Helper.GetDeviceTIme(this.ControllerInfor.MachineID, ref dateTime);
        }
        #endregion: END DATETIME

        #region: TIMEZONE
        //SET
        public bool SetTimezone(AccessTimezone timezoneData)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            bool result = standard_SDK_Helper.SetTimezone(this.ControllerInfor.MachineID, timezoneData);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return result;
        }
        //GET
        public bool GetTimeZone(ref AccessTimezone timezoneData)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID,false);
            Dictionary<string, Dictionary<string, string>> timezones = new Dictionary<string, Dictionary<string, string>>();
            bool result = standard_SDK_Helper.GetTimezone(this.ControllerInfor.MachineID, timezoneData.ID, ref timezoneData);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID,true);
            return result;
        }
        //DELETE
        public bool DeleteTimeZone(int timezoneID)
        {
            return false;
        }
        #endregion: END TIMEZONE

        #region: LOG
        public bool ClearTransactionLog()
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;
        }
        public void GetTransactionLog()
        {
        }
        #endregion: END LOG

        #region: CONTROL RELAY
        public bool PulseOnDoor(int relayIndex, int second)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;
        }
        public bool PulseOnAUX(int relayIndex, int second)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;
        }
        public bool OpenDoor(int relayIndex, int miliSecond)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;

        }
        public bool OpenMultyDoor(List<int> relayIndexs)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;
        }
        public bool OpenAUX(int relayIndex, int second)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;
        }
        public bool CloseDoor(int relayIndex)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;
        }
        public bool CloseAUX(int relayIndex)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;
        }
        public bool SetRelayDelayTiime(int delayTime)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return false;

        }
        #endregion: END CONTROL RELAY

        #region:TCP_IP
        //GET
        public bool GetIP(ref string IP)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            bool isGetIPSuccess = standard_SDK_Helper.GetDeviceIp(this.ControllerInfor.MachineID, ref IP);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return isGetIPSuccess;
        }
        public bool GetMac(ref string macAddr)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            bool isGetMacSuccess = standard_SDK_Helper.GetDeviceMac(this.ControllerInfor.MachineID, ref macAddr);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return isGetMacSuccess;
        }
        public bool GetDefaultGateway(ref string defaultGateway)
        {
            return false;
        }
        public bool GetPort(ref int port)
        {

            return false;
        }
        public bool GetComkey(ref string Comkey)
        {
            return false;
        }
        //SET
        public bool SetMac(string macAddr)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            bool isSetMacSuccess = standard_SDK_Helper.SetDeviceMac(this.ControllerInfor.MachineID, macAddr);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return isSetMacSuccess;
        }
        public bool SetNetWorkInfor(string ip, string subnetMask, string defaultGateway, string macAddr)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            bool isSetMacSuccess = SetMac(macAddr);
            bool isSetIpSuccess = standard_SDK_Helper.SetDeviceIP(this.ControllerInfor.MachineID, ip);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return isSetMacSuccess && isSetIpSuccess;
        }
        public bool SetComKey(string comKey)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);

            int _comkey = Convert.ToInt32(comKey);
            bool result = standard_SDK_Helper.SetDeviceCommPwd(this.ControllerInfor.MachineID, _comkey);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return result;
        }
        #endregion: END TCP_IP

        #region: SYSTEM
        public bool RestartDevice()
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            bool result = standard_SDK_Helper.Restart(this.ControllerInfor.MachineID);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            return result;
        }
        public bool ResetDefault()
        {
            return false;
        }
        public bool InitCardEvent()
        {
            return true;
        }
        public bool GetFirmwareVersion(ref string firmwareVersion)
        {
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, false);
            bool result = standard_SDK_Helper.GetFirmwareVersion(this.ControllerInfor.MachineID, ref firmwareVersion);
            standard_SDK_Helper.EnableDevice(this.ControllerInfor.MachineID, true);
            return result;
        }
        #endregion: END SYSTEM

        private string GetFunctionName()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame stackFrame = stackTrace.GetFrame(1);
            MethodBase methodBase = stackFrame.GetMethod();
            return methodBase.Name;
        }

        #region: ANTI PASS BACK
        public bool SetAntiPassBack(int antyPassBackIndex, int mode)
        {
            return false;
        }

        public bool GetAntiPassBack(int antiPassBackIndex, ref int mode)
        {
            return false;
        }
        #endregion
    }
}
