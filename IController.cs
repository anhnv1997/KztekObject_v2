using KztekObject.enums;
using KztekObject.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using static KztekObject.Cards.CardFactory;
using static KztekObject.Events.ControllerEvent;

namespace KztekObject
{
    public interface IController
    {
        #region: PROPERTIES
        Controller ControllerInfor { get; set; }
        bool IsAutoRestart { get; set; }
        DateTime AutoRestartTime { get; set; }
        bool isAutoEnableGetEventTime { get; set; }
        DateTime AutoStopGetEventTime { get; set; }
        DateTime AutoStartGetEventTime { get; set; }
        bool IsStopGetEvent { get; set; }
        bool IsBusy { get; set; }
        #endregion: END PROPERTIES

        #region: CONNECT
        bool TestConnection();
        bool Connect();
        bool Disconnect();

        #endregion: END CONNECT

        #region: THREAD
        void PollingStart();
        void PollingStop();
        #endregion: END THREAD

        #region: CARD
        void SetCardFactory(Dictionary<int, EM_CardType> reader_CardTypes);
        #endregion: END CARD

        #region: USER
        //GET
        List<Employee> GetAllUserInfo(List<Employee> users);
        Employee GetUserByUserId(int userID);
        //ADD
        bool DownloadUser(Employee employee);
        bool DownloadMultyUser(List<Employee> employees);
        //DELETE
        bool DeleteCardByUserID(int userID);
        #endregion: END USER

        #region: FINGER
        List<string> GetFinger(string userID);
        #endregion: END FINGER

        #region: DATE TIME
        bool SetDateTime(DateTime DateTime);
        bool SyncDateTime();
        bool GetDateTime(ref DateTime dateTime);
        #endregion: END DATETIME

        #region: TIMEZONE
        //SET
        bool SetTimezone(int timezoneID, string[] timezoneString);
        //GET
        bool GetTimeZone(ref string timeZoneString, int TimeZoneID);
        //DELETE
        bool DeleteTimeZone(int timezoneID);
        #endregion: END TIMEZONE

        #region: LOG

        bool ClearTransactionLog();
        void GetTransactionLog();

        #endregion: END LOG

        #region: CONTROL RELAY
        bool PulseOnDoor(int relayIndex, int second);
        bool PulseOnAUX(int relayIndex, int second);
        bool OpenDoor(int relayIndex, int second);
        bool OpenMultyDoor(List<int> relayIndexs);
        bool OpenAUX(int relayIndex, int second);
        bool CloseDoor(int relayIndex);
        bool CloseAUX(int relayIndex);
        bool SetRelayDelayTiime(int delayTimeInMilisecond);
        #endregion: END CONTROL RELAY

        #region:TCP_IP
        //GET
        bool GetIP(ref string IP);
        bool GetMac(ref string macAddr);
        bool GetDefaultGateway(ref string defaultGateway);
        bool GetPort(ref int port);
        bool GetComkey(ref string Comkey);
        //SET
        bool SetMac(string macAddr);
        bool SetNetWorkInfor(string ip, string subnetMask, string defaultGateway, string macAddr);
        bool SetComKey(string comKey);
        #endregion: END TCP_IP

        #region: SYSTEM
        bool RestartDevice();
        bool InitCardEvent();
        bool ResetDefault();
        bool GetFirmwareVersion(ref string firmwareVersion);
        #endregion: END SYSTEM

        #region:EVENT
        event CardEventHandler CardEvent;
        event ErrorEventHandler ErrorEvent;
        event InputEventHandler InputEvent;
        event ConnectStatusChangeEventHandler ConnectStatusChangeEvent;
        #endregion: END EVENT

        #region: ANTI PASS BACK
        bool SetAntiPassBack(int antyPassBackIndex, int mode);

        bool GetAntiPassBack(int antiPassBackIndex, ref int mode);
        #endregion
    }
}
