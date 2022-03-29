using KztekObject.Cards;
using KztekObject.Controllers.Abstract_KZTEK_Controllers;
using KztekObject.enums;
using KztekObject.Objects;
using SocketHelper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static KztekObject.Events.ControllerEvent;

namespace KztekObject.Controllers.KZE02NET_v3._0_Controller
{
    public class KZE02_NET_v3_0 : abstract_KZTEK_Controller
    {
        #region: PROPERTIES
        #endregion: END PROPERTIES

        #region:EVENT
        #endregion: END EVENT

        #region: CONNECT
        #endregion: END CONNECT

        #region: THREAD
        public override void PollingStart()
        {
            Task.Run(new Action(() =>
            {
                PollingGetEventFunc();
            }));
        }

        public override void PollingStop()
        {
            this.IsStopGetEvent = true;
        }

        private void PollingGetEventFunc()
        {
            while (true)
            {
                if (!IsStopGetEvent)
                {
                    try
                    {
                        string viewraw = "";
                        string[] message = null;
                        if (CommunicationType.IS_TCP(this.ControllerInfor.ControllerCommunicationType))
                        {
                            // Thuc hien lenh den thiet bi (pc <-> host)
                            string cmd = KZTEK_CMD.GetEventCMD();
                            string response = SocketTools.ExecuteCommand(this.ControllerInfor.Comport, this.ControllerInfor.Baudrate, null, cmd, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
                            // Trang thai thiet bi
                            if (this.ControllerInfor.IsConnect != (response != ""))
                            {
                                this.ControllerInfor.IsConnect = response != "";
                                ConnectStatusCHangeEventArgs e = new ConnectStatusCHangeEventArgs();
                                e.CurrentStatus = response != "";
                                e.ControllerID = this.ControllerInfor.ID;
                                this.OnConnectStatusChangeEvent(e);
                            }
                            //AccessCardGrant: Char(2) + GetEvent?/Style=Card/UserID=100/LenCard=4/Card=7C19F640/Reader=01/DateTime=YYYYMMDDhhmmss/CardState=U/AccessState=1/Door=00/StateMSG=00 + char(3)
                            //AccessCardDenie: Char(2) + GetEvent?/Style=Card/UserID=Null/LenCard=4/Card=7C19F640/Reader=01/DateTime=YYYYMMDDhhmmss/CardState=U/AccessState=1/Door=00/StateMSG=00 + char(3)
                            //InputEvent     : Char(2) + GetEvent?/Style=input/Input=INPUT1/DateTime=YYYYMMDDhhmmss + char(3)
                            //NoEvent        : Char(2) + GetEvent?/NotEvent + char(3)
                            if (response != "" && (response.Contains("GetEvent?/")) && !response.Contains("NotEvent") && message[0] == "02")
                            {
                                string[] data = response.Split('/');
                                bool isCardEvent = response.Contains("Card");
                                if (isCardEvent)
                                {
                                    CallCardEvent(ControllerInfor, data);
                                }
                                else
                                {
                                    CallInputEvent(ControllerInfor, data);
                                }
                                DeleteCardEvent();
                            }
                        }
                    }
                    catch
                    {
                    }
                }
                else
                {
                    bool isPingSuccess = NetWorkTools.IsPingSuccess(ControllerInfor.Comport, ControllerInfor.Baudrate);
                    if (this.ControllerInfor.IsConnect != isPingSuccess)
                    {
                        this.ControllerInfor.IsConnect = isPingSuccess;
                        ConnectStatusCHangeEventArgs e = new ConnectStatusCHangeEventArgs();
                        e.CurrentStatus = isPingSuccess;
                        e.ControllerID = this.ControllerInfor.ID;
                        this.OnConnectStatusChangeEvent(e);
                    }
                }
                Thread.Sleep(300);
            }
        }

        #region:Delete
        public override void DeleteCardEvent()
        {
            string viewraw = "";
            string[] message = null;
            string cmd = KZTEK_CMD.DeleteEventCMD();
            SocketTools.ExecuteCommand(this.ControllerInfor.Comport, this.ControllerInfor.Baudrate, null, cmd, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
        }
        #endregion

        private void CallCardEvent(Controller controller, string[] cardEventDatas)
        {
            //AccessCardGrant: Char(2) + GetEvent?/Style=Card/UserID=100/LenCard=4/Card=7C19F640/Reader=01/DateTime=YYYYMMDDhhmmss/CardState=U/AccessState=1/Door=00/StateMSG=00 + char(3)
            //AccessCardDenie: Char(2) + GetEvent?/Style=Card/UserID=Null/LenCard=4/Card=7C19F640/Reader=01/DateTime=YYYYMMDDhhmmss/CardState=U/AccessState=1/Door=00/StateMSG=00 + char(3)
            CardEventArgs e = new CardEventArgs();
            e.ControllerID = controller.ID;
            e.CardNumber = cardEventDatas[4].Substring(cardEventDatas[4].IndexOf("=") + 1);
            while (e.CardNumber.Length < 8)
            {
                e.CardNumber = "0" + e.CardNumber;
            }
            if (controller.DisplayMode == EM_DisplayMode.DEC)
                e.CardNumber = GetCardProXiDecNum(e.CardNumber);

            e.ReaderIndex = Convert.ToInt32(cardEventDatas[5].Substring(cardEventDatas[5].IndexOf("=") + 2));
            string time = cardEventDatas[6].Substring(cardEventDatas[6].IndexOf("=") + 1);
            e.Date = time.Substring(0, 4) + "/" + time.Substring(4, 2) + "/" + time.Substring(6, 2);
            e.Time = time.Substring(8, 2) + ":" + time.Substring(10, 2) + ":" + time.Substring(12, 2);
            bool isAccessGrantEvent = cardEventDatas[2].Contains("Null");
            e.EventType = EM_EventType.CARD;

            if (isAccessGrantEvent)
            {
                e.UserID = cardEventDatas[2].Substring(cardEventDatas[2].IndexOf("=") + 1);
                e.EventStatus = EM_EventStatus.ACCESS_GRANT;
            }
            else
            {
                e.UserID = " ";
                e.EventStatus = EM_EventStatus.ACCESS_DENIED;
            }
            OnCardEvent(e);
        }

        private void CallInputEvent(Controller controller, string[] inputEventDatas)
        {
            //InputEvent : Char(2) + GetEvent?/Style=input/Input=INPUT1/DateTime=YYYYMMDDhhmmss + char(3)
            InputEventArgs e = new InputEventArgs()
            {
                InputName = inputEventDatas[2].Substring(inputEventDatas[2].IndexOf("=") + 1),
                Time = DateTime.ParseExact(inputEventDatas[3].Substring(inputEventDatas[3].IndexOf("=") + 1).Trim(), "YYYYMMDDhhmmss", CultureInfo.CurrentCulture),
            };
            OnInputEvent(e);
        }
        private string GetCardProXiDecNum(string hexValue)
        {
            string hexValue1 = hexValue.Substring(0, 4);
            string hexValue2 = hexValue.Substring(4, 4);
            string decValue1 = Convert.ToInt32(hexValue1, 16) + "";
            string decValue2 = Convert.ToInt32(hexValue2, 16) + "";
            while (decValue1.Length < 3)
            {
                decValue1 = "0" + decValue1;
            }
            while (decValue2.Length < 5)
            {
                decValue2 = "0" + decValue2;
            }
            return decValue1 + "" + decValue2;
        }
        #endregion: END THREAD

        #region: CARD
        #endregion: END CARD

        #region: USER
        public override bool DownloadUser(Employee employee)
        {
            if (employee == null)
            {
                return false;
            }
            ICardController cardController = CardFactory.CreateCardController(employee.CardType);
            string _cardNumber = cardController.GetCardHexNumber(employee.CardNumber);
            int cardlen = cardController.CardLen();
            string door = KzHelper.GetOpenDoor(employee.Doors, KzHelper.EM_ByteLength._1BYTE);
            if (door == "")
            {
                return false;
            }
            string viewraw = "";
            string[] message = null;
            string downloadCMD = KZTEK_CMD.DownloadUserCMD(employee.MemoryID, _cardNumber, cardlen, employee.TimezoneID, door);
            string response = SocketTools.ExecuteCommand(ControllerInfor.Comport, ControllerInfor.Baudrate, null, downloadCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            bool result = SocketTools.isSuccess(response, "OK");
            return result;
        }
        #endregion: END USER

        #region: FINGER
        #endregion: END FINGER

        #region: DATE TIME
        #endregion: END DATETIME

        #region: TIMEZONE
        //SET
        public override bool SetTimezone(int timezoneID, string[] timezoneString)
        {
            //
            return false;
        }
        //GET
        public override bool GetTimeZone(ref string timeZoneString, int TimeZoneID)
        {
            return false;
        }
        //DELETE
        public override bool DeleteTimeZone(int timezoneID)
        {
            return false;
        }
        #endregion: END TIMEZONE

        #region: LOG
        public override bool ClearTransactionLog()
        {
            return false;
        }
        public override void GetTransactionLog()
        {
            return;
        }

        #endregion: END LOG

        #region: CONTROL RELAY
        public override bool OpenMultyDoor(List<int> relayIndexs)
        {
            return false;
        }
        #endregion: END CONTROL RELAY

        #region:TCP_IP
        public override bool SetComKey(string comKey)
        {
            return false;
        }
        #endregion: END TCP_IP

        #region: SYSTEM
        #endregion: END SYSTEM

        #region: ANTI PASS BACK
        public override bool SetAntiPassBack(int antyPassBackIndex, int mode)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string SetAntiPassBackCMD = KZTEK_CMD.SetAntiPassBackCMD(antyPassBackIndex,mode);
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, SetAntiPassBackCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            //Char(2) + SetAntiPassBack?/OK + char(3)
            //Char(2) + SetAntiPassBack?/ERR + char(3)
            if (SocketTools.isSuccess(response, "OK"))
            {
                return true;
            }
            else if (SocketTools.isSuccess(response, "ERR"))
            {
                return false;
            }
            OnErrorEvent(new ErrorEventArgs()
            {
                ErrorString = response,
                ErrorFunc = GetFunctionName(),
                CMD = SetAntiPassBackCMD
            });
            return false;
        }

        public override bool GetAntiPassBack(int antiPassBackIndex, ref int mode)
        {
            string viewraw = "";
            string[] message = null;
            string comport = this.ControllerInfor.Comport;
            int baudrate = this.ControllerInfor.Baudrate;
            string GetAntiPassbackCMD = KZTEK_CMD.GetDateTimeCMD();
            string response = SocketTools.ExecuteCommand(comport, baudrate, null, GetAntiPassbackCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            //Char(2) + GetAntiPassBack?/AntiPassBackLock1=0 + char(3)
            if (SocketTools.isSuccess(response, "GetAntiPassBack?/"))
            {
                mode = Convert.ToInt32(response.Substring(response.IndexOf("=") + 1));
                return true;
            }
            mode = -1;
            OnErrorEvent(new ErrorEventArgs()
            {
                ErrorString = response,
                ErrorFunc = GetFunctionName(),
                CMD = GetAntiPassbackCMD
            });
            return false;
        }
        #endregion: END ANTI PASS BACK

        private string GetFunctionName()
        {
            StackTrace stackTrace = new StackTrace();
            StackFrame stackFrame = stackTrace.GetFrame(1);
            MethodBase methodBase = stackFrame.GetMethod();
            return methodBase.Name;
        }
    }
}