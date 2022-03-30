using KztekObject.Cards;
using KztekObject.Controllers.KZE05NET_Controller;
using KztekObject.enums;
using KztekObject.Objects;
using SocketHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TimezoneLibrary;
using static CardLibrary.CardFactory;
using static KztekObject.Events.ControllerEvent;

namespace KztekObject.Controllers.Abstract_KZTEK_Controllers
{
    public abstract class abstract_KZTEK_Controller : IController
    {
        #region: PROPERTIES
        public Controller ControllerInfor { get; set; } = new Controller();
        public bool IsAutoRestart { get; set; } = false;
        public DateTime AutoRestartTime { get; set; } = DateTime.MinValue;
        public bool isAutoEnableGetEventTime { get; set; } = false;
        public DateTime AutoStopGetEventTime { get; set; } = DateTime.MinValue;
        public DateTime AutoStartGetEventTime { get; set; } = DateTime.MinValue;
        public bool IsStopGetEvent { get; set; } = false;
        public bool IsBusy { get; set; }
        #endregion: END PROPERTIES

        #region:EVENT
        public event CardEventHandler CardEvent;
        public event ErrorEventHandler ErrorEvent;
        public event InputEventHandler InputEvent;
        public event ConnectStatusChangeEventHandler ConnectStatusChangeEvent;
        #endregion: END EVENT

        #region: CONNECT
        public bool TestConnection()
        {
            if (CommunicationType.IS_TCP(this.ControllerInfor.ControllerCommunicationType))
                return NetWorkTools.IsPingSuccess(this.ControllerInfor.Comport, 500);
            return false;
        }
        public bool Connect()
        {
            return TestConnection();
        }
        public bool Disconnect()
        {
            this.IsStopGetEvent = false;
            return true;
        }
        #endregion: END CONNECT

        #region: THREAD
        public abstract void PollingStart();
        public abstract void PollingStop();
        #region:Delete
        public abstract void DeleteCardEvent();
        #endregion
        #endregion: END THREAD

        #region: CARD
        public void SetCardFactory(Dictionary<int, EM_CardType> reader_CardTypes)
        {
            this.ControllerInfor.ReaderCardTypes = reader_CardTypes;
        }
        #endregion: END CARD

        #region: USER
        //GET
        public List<Employee> GetAllUserInfo(List<Employee> users)
        {
            users = new List<Employee>();
            string viewraw = "";
            string[] message = null;
            string response = "";
            string cmd = KZTEK_CMD.GetAllUserCMD();
            this.IsBusy = true;
            response = SocketTools.ExecuteCommand(ControllerInfor.Comport, ControllerInfor.Baudrate, null, cmd, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            //GetAllUser?/UserID=1/LenCard=3/Card=8DCF3B/Pin=12345678/Mode=0/TimeZone=0/Door=00.00.00.00.00.00.00.01
            while (!response.Contains("NULL"))
            {
                response = SocketTools.ExecuteCommand(ControllerInfor.Comport, ControllerInfor.Baudrate, null, cmd, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
                if (response != "" && !response.Contains("NULL") && !response.Contains("LenCard=0/"))
                {
                    Employee employee = new Employee();
                    string[] result = response.Split('/');
                    string UserID = result[1].Substring(result[1].IndexOf("=") + 1);
                    string cardNumber = result[3].Substring(result[3].IndexOf("=") + 1);
                    string[] doors = result[7].Substring(result[7].IndexOf("=") + 1).Split('.');
                    employee.MemoryID = Convert.ToInt32(UserID);
                    employee.CardNumber = cardNumber;
                    employee.Doors = KzHelper.GetDoorIndexs(doors);
                    if (employee.CardNumber.Length == 6)
                    {
                        employee.CardType = (int)EM_CardType.Proximity_Left;
                        int integer_value = Convert.ToInt32(employee.CardNumber, 16);
                        string cardNum = integer_value.ToString();
                        while (cardNum.Length < 10)
                        {
                            cardNum = "0" + cardNum;
                        }
                        employee.CardNumber = cardNum;
                    }
                    else
                    {
                        employee.CardType = (int)EM_CardType.Mifare;
                    }
                    users.Add(employee);
                }
            }
            this.IsBusy = false;
            return users;
        }
        public Employee GetUserByUserId(int userID)
        {
            string viewraw = "";
            string[] message = null;
            string cmd = KZTEK_CMD.GetUserByIdCMD(userID.ToString());
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(ControllerInfor.Comport, ControllerInfor.Baudrate, null, cmd, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            //GetAllUser?/UserID=1/LenCard=3/Card=8DCF3B/Pin=12345678/Mode=0/TimeZone=0/Door=00.00.00.00.00.00.00.01
            this.IsBusy = false;
            if (response.Contains("NULL"))
            {
                return null;
            }
            Employee employee = new Employee();
            string[] result = response.Split('/');
            string UserID = result[1].Substring(result[1].IndexOf("=") + 1);
            string cardNumber = result[3].Substring(result[3].IndexOf("=") + 1);
            string[] doors = result[7].Substring(result[7].IndexOf("=") + 1).Split('.');
            employee.MemoryID = Convert.ToInt32(UserID);
            employee.CardNumber = cardNumber;
            employee.Doors = KzHelper.GetDoorIndexs(doors);
            if (employee.CardNumber.Length == 6)
            {
                employee.CardType = (int)EM_CardType.Proximity_Left;
                int integer_value = Convert.ToInt32(employee.CardNumber, 16);
                string cardNum = integer_value.ToString();
                while (cardNum.Length < 10)
                {
                    cardNum = "0" + cardNum;
                }
                employee.CardNumber = cardNum;
            }
            else
            {
                employee.CardType = (int)EM_CardType.Mifare;
            }

            return employee;
        }
        //ADD
        public abstract bool DownloadUser(Employee employee);

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
        //DELETE
        public bool DeleteCardByUserID(int userID)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string deleteCMD = KZTEK_CMD.DeleteUserCMD(userID);
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, deleteCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2) + ChangeMacAddress?/OK + char(3)
            //Char(2) + ChangeMacAddress ?/ ERR + char(3)
            if (SocketTools.isSuccess(response, "OK"))
            {
                return true;
            }
            else if (SocketTools.isSuccess(response, "ERR"))
            {
                return false;
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = deleteCMD
                });
            }
            return false;
        }
        #endregion: END USER

        #region: FINGER
        public List<string> GetFinger( string userID)
        {
            return null;
        }
        #endregion: END FINGER

        #region: DATE TIME
        public bool SetDateTime(DateTime DateTime)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string SetDateTimeCMD = KZTEK_CMD.SetDateTimeCMD(DateTime.ToString("yyyyMMddHHmmss"));
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, SetDateTimeCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2) + SetDateTime?/OK + char(3)
            //Char(2) + SetDateTime?/ERR + char(3)
            if (SocketTools.isSuccess(response, "OK"))
            {
                return true;
            }
            else if (SocketTools.isSuccess(response, "ERR"))
            {
                return false;
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = SetDateTimeCMD
                });

            }
            return false;
        }
        public bool SyncDateTime()
        {
            return SetDateTime(DateTime.Now);
        }
        public bool GetDateTime(ref DateTime dateTime)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string GetDateTimeCMD = KZTEK_CMD.GetDateTimeCMD();
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, GetDateTimeCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2) + GetDateTime?/YYYYMMDDhhmmss + char(3)
            if (SocketTools.isSuccess(response, "GetDateTime?/"))
            {
                string _datetime = response.Split('/')[1];
                dateTime = DateTime.ParseExact(_datetime, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
                return true;
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = GetDateTimeCMD
                });

            }
            return false;
        }
        #endregion: END DATETIME

        #region: TIMEZONE
        //SET
        public abstract bool SetTimezone(AccessTimezone timezoneData);
        //GET
        public abstract bool GetTimeZone(ref AccessTimezone timezoneData);
        //DELETE
        public abstract bool DeleteTimeZone(int timezoneID);
        #endregion: END TIMEZONE

        #region: LOG
        public abstract bool ClearTransactionLog();
        public abstract void GetTransactionLog();
        #endregion: END LOG

        #region: CONTROL RELAY
        public bool PulseOnDoor(int relayIndex, int second)
        {
            return false;
        }
        public bool PulseOnAUX(int relayIndex, int second)
        {
            return false;
        }
        public bool OpenDoor(int relayIndex, int miliSecond)
        {
            if (SetRelayDelayTiime(miliSecond))
            {
                string viewraw = "";
                string[] message = null;
                string comport = this.ControllerInfor.Comport;
                int baudrate = this.ControllerInfor.Baudrate;
                string OpenRelayCMD = KZTEK_CMD.OpenRelayCMD(relayIndex);
                this.IsBusy = true;
                string response = SocketTools.ExecuteCommand(comport, baudrate, null, OpenRelayCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
                this.IsBusy = false;
                //Char(2) + SetRelay?/Relay=01/OK + char(3)
                //Char(2) + SetRelay?/Relay=01/ERR + char(3)
                if (SocketTools.isSuccess(response, "OK"))
                {
                    return true;
                }
                else if (SocketTools.isSuccess(response, "ERR"))
                {
                    return false;
                }
                if (ErrorEvent != null)
                {
                    ErrorEvent(this, new ErrorEventArgs()
                    {
                        ErrorString = response,
                        ErrorFunc = GetFunctionName(),
                        CMD = OpenRelayCMD
                    });

                }
                return false;
            }
            return false;

        }
        public abstract bool OpenMultyDoor(List<int> relayIndexs);
        public bool OpenAUX(int relayIndex, int second)
        {
            return false;
        }
        public bool CloseDoor(int relayIndex)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string OpenRelay = KZTEK_CMD.CloseRelayCMD(relayIndex);
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, OpenRelay, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2) + SetDateTime?/OK + char(3)
            //Char(2) + SetDateTime?/ERR + char(3)
            if (SocketTools.isSuccess(response, "OK"))
            {
                return true;
            }
            else if (SocketTools.isSuccess(response, "ERR"))
            {
                return false;
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = OpenRelay
                });

            }
            return false;
        }
        public bool CloseAUX(int relayIndex)
        {
            return false;
        }
        public bool SetRelayDelayTiime(int delayTime)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string SetRelayDelayTime = KZTEK_CMD.SetRelayDelayTimeCMD(delayTime);
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, SetRelayDelayTime, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2) + SetRelayDelayTime?/OK + char(3)
            //Char(2) + SetRelayDelayTime?/ERR + char(3)
            if (SocketTools.isSuccess(response, "OK"))
            {
                return true;
            }
            else if (SocketTools.isSuccess(response, "ERR"))
            {
                return false;
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = SetRelayDelayTime
                });

            }
            return false;

        }
        #endregion: END CONTROL RELAY

        #region:TCP_IP
        //GET
        public bool GetIP(ref string IP)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string autoDetectCMD = KZTEK_CMD.AutoDetectCMD();
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, autoDetectCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2)  + version  + “/”  + IPAdress + “/”  Port + “/” + subNetMask + “/”  + DefaultGateway + “/”+ MacAddress + char(3)
            if (response != null && response.Length > 0)
            {
                string[] _responses = response.Split('/');
                if (_responses.Length > 1)
                {
                    IP = _responses[1];
                    return true;
                }
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = autoDetectCMD
                });
            }
            return false;
        }
        public bool GetMac(ref string macAddr)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string autoDetectCMD = KZTEK_CMD.AutoDetectCMD();
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, autoDetectCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2)  + version  + “/”  + IPAdress + “/”  Port + “/” + subNetMask + “/”  + DefaultGateway + “/”+ MacAddress + char(3)
            if (response != null && response.Length > 0)
            {
                string[] _responses = response.Split('/');
                if (_responses.Length > 5)
                {
                    macAddr = _responses[5];
                    return true;
                }
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = autoDetectCMD
                });
            }
            return false;
        }
        public bool GetDefaultGateway(ref string defaultGateway)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string autoDetectCMD = KZTEK_CMD.AutoDetectCMD();
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, autoDetectCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2)  + version  + “/”  + IPAdress + “/”  Port + “/” + subNetMask + “/”  + DefaultGateway + “/”+ MacAddress + char(3)
            if (response != null && response.Length > 0)
            {
                string[] _responses = response.Split('/');
                if (_responses.Length > 4)
                {
                    defaultGateway = _responses[4];
                    return true;
                }
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = autoDetectCMD
                });
            }
            return false;
        }
        public bool GetPort(ref int port)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string autoDetectCMD = KZTEK_CMD.AutoDetectCMD();
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, autoDetectCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2)  + version  + “/”  + IPAdress + “/”  Port + “/” + subNetMask + “/”  + DefaultGateway + “/”+ MacAddress + char(3)
            if (response != null && response.Length > 0)
            {
                string[] _responses = response.Split('/');
                if (_responses.Length > 5)
                {
                    port = Convert.ToInt32(_responses[2]);
                    return true;
                }
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = autoDetectCMD
                });
            }
            return false;
        }
        public bool GetComkey(ref string Comkey)
        {
            return false;
        }
        //SET
        public bool SetMac(string macAddr)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string changeMacCMD = KZTEK_CMD.Get_ChangeMacAddressCmd(macAddr);
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, changeMacCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2) + ChangeMacAddress?/OK + char(3)
            //Char(2) + ChangeMacAddress ?/ ERR + char(3)
            if (SocketTools.isSuccess(response, "OK"))
            {
                return true;
            }
            else if (SocketTools.isSuccess(response, "ERR"))
            {
                return false;
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = changeMacCMD
                });
            }
            return false;
        }
        public bool SetNetWorkInfor(string ip, string subnetMask, string defaultGateway, string macAddr)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string setNetworkInfor = KZTEK_CMD.ChangeIPCMD(ip, subnetMask, defaultGateway, macAddr);
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, setNetworkInfor, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
            //Char(2) + ChangeIP?/OK + char(3)	
            //Char(2) + ChangeIP?/ERR + char(3)
            if (SocketTools.isSuccess(response, "OK"))
            {
                return true;
            }
            else if (SocketTools.isSuccess(response, "ERR"))
            {
                return false;
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                    CMD = setNetworkInfor
                });
            }
            return false;
        }
        public abstract bool SetComKey(string comKey);
        #endregion: END TCP_IP

        #region: SYSTEM
        public bool RestartDevice()
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string ResetCMD = "ResetDevice?/";
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, ResetCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            return true;
        }
        public bool ResetDefault()
        {
            var serverIP = IPAddress.Parse(this.ControllerInfor.Comport);
            var serverPort = this.ControllerInfor.Baudrate;
            var serverEndpoint = new IPEndPoint(serverIP, serverPort);
            var size = 500;
            var receiveBuffer = new byte[size];
            string text = KZTEK_CMD.ResetDefaultCmd();
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            var sendBuffer = Encoding.UTF8.GetBytes(text);

            socket.SendTo(sendBuffer, serverEndpoint);

            EndPoint dummyEndpoint = new IPEndPoint(IPAddress.Any, 0);
            string response = "";
            while (!response.Contains("ResetComplete"))
            {
                var length = socket.ReceiveFrom(receiveBuffer, ref dummyEndpoint);
                response = Encoding.UTF8.GetString(receiveBuffer);
                response = response.Substring(1, length - 2);
                Array.Clear(receiveBuffer, 0, size);
                Thread.Sleep(1000);
            }
            socket.Close();
            return true;
        }
        public bool InitCardEvent()
        {
            var serverIP = IPAddress.Parse(this.ControllerInfor.Comport);
            var serverPort = this.ControllerInfor.Baudrate;
            var serverEndpoint = new IPEndPoint(serverIP, serverPort);
            var size = 500;
            var receiveBuffer = new byte[size];
            string text = KZTEK_CMD.Get_InitCardEventCmd();
            var socket = new Socket(SocketType.Dgram, ProtocolType.Udp);

            var sendBuffer = Encoding.UTF8.GetBytes(text);

            socket.SendTo(sendBuffer, serverEndpoint);

            EndPoint dummyEndpoint = new IPEndPoint(IPAddress.Any, 0);
            string response = "";
            while (!response.Contains("InitComplete"))
            {
                var length = socket.ReceiveFrom(receiveBuffer, ref dummyEndpoint);
                response = Encoding.UTF8.GetString(receiveBuffer);
                response = response.Substring(1, length - 2);
                Array.Clear(receiveBuffer, 0, size);
                Thread.Sleep(1000);
            }
            socket.Close();
            return true;
        }
        public bool GetFirmwareVersion(ref string firmwareVersion)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, KZTEK_CMD.GetFirmwareCMD(), ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            //Char(2) + GetFirmwareVersion?/Version=” ” + char(3)
            if (response.Contains("Version="))
            {
                firmwareVersion = response.Substring(response.IndexOf("=") + 1);
                return true;
            }
            if (ErrorEvent != null)
            {
                ErrorEvent(this, new ErrorEventArgs()
                {
                    ErrorString = response,
                    ErrorFunc = GetFunctionName(),
                });

            }
            return SocketTools.isSuccess(response, "Version");
        }
        #endregion: END SYSTEM

        private string GetFunctionName()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame stackFrame = stackTrace.GetFrame(1);
            MethodBase methodBase = stackFrame.GetMethod();
            return methodBase.Name;
        }

        protected void OnCardEvent(CardEventArgs e)
        {
            if (this.CardEvent != null)
            {
                this.CardEvent(this, e);
            }
        }

        protected void OnConnectStatusChangeEvent(ConnectStatusCHangeEventArgs e)
        {
            if (this.ConnectStatusChangeEvent != null)
            {
                this.ConnectStatusChangeEvent(this, e);
            }
        }
        protected void OnErrorEvent(ErrorEventArgs e)
        {
            if (this.ErrorEvent != null)
            {
                this.ErrorEvent(this, e);
            }
        }

        protected void OnInputEvent(InputEventArgs e)
        {
            if (this.InputEvent != null)
            {
                this.InputEvent(this, e);
            }
        }

        #region: ANTI PASS BACK
        public abstract bool SetAntiPassBack(int antyPassBackIndex, int mode);

        public abstract bool GetAntiPassBack(int antiPassBackIndex, ref int mode);
        #endregion
    }
}
