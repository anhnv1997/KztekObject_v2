using CardLibrary;
using KztekObject.Cards;
using KztekObject.Controllers.Abstract_KZTEK_Controllers;
using KztekObject.enums;
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
using static KztekObject.Events.ControllerEvent;

namespace KztekObject.Controllers.KZE05NET_Controller
{
    public class KZE05_NET_v3_4 : abstract_KZTEK_Controller
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
                            string cmd = KZTEK_CMD.GetCardEventCMD();
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
                            //AccessGrant:"Char(2) + GetCardEvent?/UserID=100/LenCard=4/Card=7C19F640/Reader=01/DateTime=YYYYMMDDhhmmss/CardState=U/AccessState=ERR/Door=00.00.00.01 + char(3)
                            //AccessDenie: Char(2) + GetCardEvent?/UserID= NULL/LenCard=4/Card=7C19F640/ Reader=01/DateTime=YYYYMMDDhhmmss + char(3)
                            //NoEvent:  "Char(2) + GetCardEvent ?/ NotEvent + char(3)
                            if (response != "" && (response.Contains("GetCardEvent?/") || response.Contains("GetEvent?/")) && !response.Contains("NotEvent") && message[0] == "02")
                            {
                                string[] data = response.Split('/');
                                bool isAccessGrantEvent = data.Length == 9;
                                bool isAccessDeniEvent = data.Length < 9 && data.Length > 3;
                                if (data.Length > 3)
                                {
                                    CallCardEvent(this.ControllerInfor, data, isAccessGrantEvent, isAccessDeniEvent);
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
            string cmd = KZTEK_CMD.DeleteCardEventCMD();
            SocketTools.ExecuteCommand(this.ControllerInfor.Comport, this.ControllerInfor.Baudrate, null, cmd, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
        }
        #endregion

        private void CallCardEvent(Controller controller, string[] data, bool isAccessGrantEvent, bool isAccessDeniEvent)
        {
            // 0: GetCardEvent?
            // 1: UserID=xxx
            // 2: LenCard=x
            // 3: Card=xxxxx
            // 4: Reader=xx
            // 5: DateTime=YYYYMMDDhhmmss
            // 6: CardState=U // U: Unregisterd   R: Register
            // 7: AccessState=ERR // OK - ERR
            // 8: Door = 00.00.00.01  // [4]Byte[3]Byte[2]Byte[1]Byte[0]  --> 8byte x 8bit == 64 Door
            // Byte = 8bit : bit7 bit6 ... bit0
            // Door 9 : bit0 of byte1
            CardEventArgs e = new CardEventArgs();
            e.ControllerID = controller.ID;
            e.CardNumber = data[3].Substring(data[3].IndexOf("=") + 1);
            while (e.CardNumber.Length < 8)
            {
                e.CardNumber = "0" + e.CardNumber;
            }
            if (controller.DisplayMode == EM_DisplayMode.DEC)
                e.CardNumber = GetCardProXiDecNum(e.CardNumber);
            e.ReaderIndex = Convert.ToInt32(data[4].Substring(data[4].IndexOf("=") + 2));
            string time = data[5].Substring(data[5].IndexOf("=") + 1);
            e.Date = time.Substring(0, 4) + "/" + time.Substring(4, 2) + "/" + time.Substring(6, 2);
            e.Time = time.Substring(8, 2) + ":" + time.Substring(10, 2) + ":" + time.Substring(12, 2);
            if (isAccessGrantEvent)
            {
                e.UserID = data[1].Substring(data[1].IndexOf("=") + 1);
                e.EventStatus = EM_EventStatus.ACCESS_GRANT;
                string doorIndexTemp = data[8].Substring(data[8].IndexOf("=") + 1);
                e.EventType = EM_EventType.CARD;
            }
            else if (isAccessDeniEvent)
            {
                e.UserID = " ";
                e.EventType = EM_EventType.CARD;
                e.EventStatus = EM_EventStatus.ACCESS_DENIED;
            }
            OnCardEvent(e);
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
            string door = KzHelper.GetOpenDoor(employee.Doors, KzHelper.EM_ByteLength._4BYTE);
            if (door == "")
            {
                return false;
            }
            string viewraw = "";
            string[] message = null;
            string downloadCMD = KZTEK_CMD.DownloadUserCMD(employee.MemoryID, _cardNumber, cardlen, employee.TimezoneID, door);
            this.IsBusy = true;
            string response = SocketTools.ExecuteCommand(ControllerInfor.Comport, ControllerInfor.Baudrate, null, downloadCMD, ref viewraw, ref message, 500, SocketTools.STX, Encoding.ASCII);
            this.IsBusy = false;
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
        public override bool SetTimezone(AccessTimezone timezoneData)
        {
            return false;
        }
        //GET
        public override bool GetTimeZone(ref AccessTimezone timezoneData)
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
            return false;
        }

        public override bool GetAntiPassBack(int antiPassBackIndex, ref int mode)
        {
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
