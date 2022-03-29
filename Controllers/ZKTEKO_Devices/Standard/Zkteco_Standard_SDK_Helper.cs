using KztekObject.Objects;
using System;
using System.Collections.Generic;
using System.Text;
using zkemkeeper;
using static KztekObject.enums.CommunicationType;

namespace KztekObject.Controllers.ZKTEKO_Devices.Standard
{
    public class Zkteco_Standard_SDK_Helper
    {
        public CZKEMClass axCZKEM1;

        public Controller ControllerInfor { get; set; }
        private int MachineNumber = 1;
        public Zkteco_Standard_SDK_Helper()
        {
            axCZKEM1 = new CZKEMClass();
        }

        #region:CONNECT
        //Connect
        public bool Connect()
        {
            switch (this.ControllerInfor.ControllerCommunicationType)
            {
                case EM_CommunicationType.TCP_IP:
                    return ConnectTcpIP();
                case EM_CommunicationType.SERIAL:
                    return ConnectRS();
                case EM_CommunicationType.USB:
                    return ConnectUSB();
                default:
                    return false;
            }
        }

        /// <summary>
        /// Connect Through TCP IP: Available For R2_Controller
        /// </summary>
        /// <returns></returns>
        private bool ConnectTcpIP()
        {
            axCZKEM1.SetCommPassword(Convert.ToInt32(this.ControllerInfor.Comkey));
            if (this.ControllerInfor.IsConnect)
            {
                axCZKEM1.Disconnect();
                this.ControllerInfor.IsConnect = false;
            }
            this.ControllerInfor.IsConnect = axCZKEM1.Connect_Net(this.ControllerInfor.Comport, this.ControllerInfor.Baudrate);
            return this.ControllerInfor.IsConnect;
        }

        /// <summary>
        /// Connect Through RS485/RS232
        /// </summary>
        /// <returns></returns>
        private bool ConnectRS()
        {
            return false;
        }

        /// <summary>
        /// Connect Through USB
        /// </summary>
        /// <returns></returns>
        private bool ConnectUSB()
        {
            return false;
        }

        /// <summary>
        /// Disconnect With Device: Available For R2_FigerTec Controller
        /// </summary>
        /// <returns></returns>
        public bool Disconnect()
        {
            if (this.ControllerInfor.IsConnect)
            {
                axCZKEM1.Disconnect();
            }
            return true;
        }

        #endregion: END CONNECT

        //////////////////////////////////////////////////////////////////////////////////////
        #region:DEVICE MANAGEMENT
        /// <summary>
        /// To check whether the machine is a color-screen one, Applicable to BW, TFT and IFACE devices
        /// </summary>
        /// <param name="machineNumber">Machine ID</param>
        /// <returns></returns>
        public bool IsColorScreenMachine(int machineNumber)
        {
            return axCZKEM1.IsTFTMachine(machineNumber);
        }

        /// <summary>
        /// To query the data storage status on the machine, such as the number of administrators and number of users. Applicable to BW, TFT, and IFACE devices
        /// </summary>
        /// <param name="machineNumber"> [IN] Machine ID</param>
        /// <param name="dwStatus"> [IN] specifies the data to be obtained</param>
        /// <param name="dwValue"> [OUT] Content of the data specified by dwStatus</param>
        /// <returns></returns>
        public bool GetDeviceStatus(int machineNumber, EM_Zkteco_DwStatus dwStatus, ref int dwValue)
        {
            return axCZKEM1.GetDeviceStatus(machineNumber, GetDwStatusIndex(dwStatus), ref dwValue);
        }

        /// <summary>
        /// To obtain machine information, such as the language and baud rate.
        /// </summary>
        /// <param name="machineNumber"> [IN] Machine ID</param>
        /// <param name=""> [IN] Information type </param>
        /// <param name="dwValue"> [OUT] Informatin of the type specified by dwInfo </param>
        /// <returns></returns>
        public bool GetDeviceInfor(int machineNumber, EM_Zkteco_DwInfor dwInfor, ref int dwValue)
        {
            if (dwInfor != EM_Zkteco_DwInfor.UNKNOWN)
                return axCZKEM1.GetDeviceInfo(machineNumber, (int)dwInfor, ref dwValue);
            return false;
        }

        /// <summary>
        /// To set machine information, such as the language and duplicate record time
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="dwInfor">[IN] Information type, ranging from 1-20, 80, 81</param>
        /// <param name="dwValue">Information of the type specified by dwInfo</param>
        /// <returns></returns>
        public bool SetDeviceInfor(int machineNumber, EM_Zkteco_DwInfor dwInfor, int dwValue)
        {
            return axCZKEM1.SetDeviceInfo(machineNumber, (int)dwInfor, dwValue);
        }

        //Date Time
        /// <summary>
        /// To set the time of the machine to be the same as that of the local computer<para />
        /// Available For: Finger Tec R2 Controller
        /// </summary>
        /// <param name="machineNumber">Machine ID</param>
        /// <returns></returns>
        public bool SyncTime(int machineNumber)
        {
            return axCZKEM1.SetDeviceTime(machineNumber);
        }

        /// <summary>
        /// To set the time of the machine.<para />
        /// Available For: FingerTec R2 Controller <para />
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="time">[IN] Set Time</param>
        /// <returns></returns>
        public bool SetDeviceTime(int machineNumber, DateTime time)
        {
            return axCZKEM1.SetDeviceTime2(machineNumber, time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second);
        }

        /// <summary>
        /// Get Device Time <para />
        /// Available For: Finger Tec R2 Controller <para />
        /// </summary>
        /// <param name="machineNumber">[IN] Machine Number</param>
        /// <param name="deviceTime">[OUT] Device Time</param>
        /// <returns></returns>
        public bool GetDeviceTIme(int machineNumber, ref DateTime deviceTime)
        {
            int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0;
            bool result = axCZKEM1.GetDeviceTime(machineNumber, ref year, ref month, ref day, ref hour, ref month, ref second);
            if (result)
                deviceTime = new DateTime(year, month, day, hour, minute, second);
            return result;
        }
        //End Datetime

        /// <summary>
        /// To query the serial number of the machine.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine Number</param>
        /// <param name="serialNumber">[Out] Serial </param>
        /// <returns></returns>
        public bool GetSerialNumber(int machineNumber, out string serialNumber)
        {
            return axCZKEM1.GetSerialNumber(machineNumber, out serialNumber);
        }

        /// <summary>
        /// To query the product code of the machine.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine Number</param>
        /// <param name="productCode">[OUT] Product Code</param>
        /// <returns></returns>
        public bool GetProductCode(int machineNumber, out string productCode)
        {
            return axCZKEM1.GetProductCode(machineNumber, out productCode);
        }

        /// <summary>
        /// Get Firmware version
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="version">[OUT] version</param>
        /// <returns></returns>
        public bool GetFirmwareVersion(int machineNumber, ref string version)
        {
            return axCZKEM1.GetFirmwareVersion(machineNumber, ref version);
        }

        /// <summary>
        /// To query the SDK version
        /// </summary>
        /// <param name="SDK">[OUT] SDK Version</param>
        /// <returns></returns>
        public bool GetSDKVersion(ref string SDK)
        {
            return axCZKEM1.GetSDKVersion(ref SDK);
        }

        /// <summary>
        /// To query the IP address of the machine 
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="IP">[OUT] IP Address</param>
        /// <returns></returns>
        public bool GetDeviceIp(int machineNumber, ref string IP)
        {
            return axCZKEM1.GetDeviceIP(machineNumber, ref IP);
        }

        /// <summary>
        /// To set the IP address of the machine
        /// </summary>
        /// <param name="machineNumber">[IN] Machine Number</param>
        /// <param name="IP">[IN] IP Address</param>
        /// <returns></returns>
        public bool SetDeviceIP(int machineNumber, string IP)
        {
            return axCZKEM1.SetDeviceIP(machineNumber, IP);
        }

        /// <summary>
        /// To query the MAC address of the machine
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="Mac">[OUT] Mac Address</param>
        /// <returns></returns>
        public bool GetDeviceMac(int machineNumber, ref string Mac)
        {
            return axCZKEM1.GetDeviceMAC(machineNumber, ref Mac);
        }

        /// <summary>
        /// To set the MAC address of the machine
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="Mac">[IN] Mac Address</param>
        /// <returns></returns>
        public bool SetDeviceMac(int machineNumber, string Mac)
        {
            return axCZKEM1.SetDeviceMAC(machineNumber, Mac);
        }

        /// <summary>
        /// To query the Wiegand format of the machine.Applicable to BW, TFT and IFACE devices
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="wiegandFmt">[OUT] Wiegand Format</param>
        /// <returns></returns>
        public bool GetWiegandFmt(int machineNumber, ref string wiegandFmt)
        {
            return axCZKEM1.GetWiegandFmt(machineNumber, ref wiegandFmt);
        }

        /// <summary>
        /// To set the Wiegand format of the machine.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine Number</param>
        /// <param name="wiegandFmt">[IN] Wiegand Fmt</param>
        /// <returns></returns>
        public bool SetWiegandFmt(int machineNumber, string wiegandFmt)
        {
            return axCZKEM1.SetWiegandFmt(machineNumber, wiegandFmt);
        }

        /// <summary>
        /// To set the communication password of the machine, which will be saved on the machine.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="comkey">[IN] Comkey</param>
        /// <returns></returns>
        public bool SetDeviceCommPwd(int machineNumber, int comkey)
        {
            return axCZKEM1.SetDeviceCommPwd(machineNumber, comkey);
        }

        /// <summary>
        /// To set the communication password of the PC. A connection can be set up between the machine and the PC only if their communication passwords are the same.
        /// </summary>
        /// <param name="comkey">[IN] Communication password of the PC </param>
        /// <returns></returns>
        public bool SetCommPWD(int comkey)
        {
            return axCZKEM1.SetCommPassword(comkey);
        }

        /// <summary>
        /// To query the current status of the machine.
        /// </summary>
        /// <param name="state">[OUT] Device State </param>
        /// <returns></returns>
        public bool GetDeviceState(ref EM_DeviceState state)
        {
            int _state = -1;
            bool result = axCZKEM1.QueryState(ref _state);
            if (result)
            {
                state = (EM_DeviceState)_state;
            }
            else
                state = EM_DeviceState.UNKNOWN;
            return result;
        }

        /// <summary>
        /// To enable or disable the machine. After the machine is disabled, the fingerprint, keyboard, and card modules are unavailable.<para />
        /// Available For R2 Controller<para />
        /// </summary>
        /// <param name="MachineNumber">[IN] Machine Number</param>
        /// <param name="isEnable">[IN] isEnable</param>
        /// <returns></returns>
        public bool EnableDevice(int MachineNumber, bool isEnable)
        {
            return axCZKEM1.EnableDevice(MachineNumber, isEnable);
        }

        public bool Restart(int machineNumber)
        {
            return axCZKEM1.RestartDevice(machineNumber);
        }

        #endregion: END DEVICE MANAGEMENT

        #region: TIMEZONE
        /// <summary>
        /// To obtain the information about a time segment with a specified index
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="timezoneIndex">[IN] Timezone Index</param>
        /// <param name="timezones">[Out] timezoneDatas</param>
        /// <returns></returns>
        public bool GetTimezone(int machineNumber, int timezoneIndex, ref Dictionary<string, Dictionary<string, string>> timezones)
        {
            string sTimeZone = "";
            bool result = axCZKEM1.GetTZInfo(machineNumber, timezoneIndex, ref sTimeZone);
            if (result)
            {
                string[] array = new string[sTimeZone.Length / 2];
                int i, j = 0;
                for (i = 0; (i + 2) <= sTimeZone.Length && sTimeZone.Length >= i;)
                {
                    array[j] = sTimeZone.Substring(i, 2);
                    j++;
                    i = i + 2;
                }
                string SUNs = array[0] + ":" + array[1];
                string SUNe = array[2] + ":" + array[3];

                string MONs = array[4] + ":" + array[5];
                string MONe = array[6] + ":" + array[7];

                string TUEs = array[8] + ":" + array[9];
                string TUEe = array[10] + ":" + array[11];

                string WENs = array[12] + ":" + array[13];
                string WENe = array[14] + ":" + array[15];

                string THUs = array[16] + ":" + array[17];
                string THUe = array[18] + ":" + array[19];

                string FRIs = array[20] + ":" + array[21];
                string FRIe = array[22] + ":" + array[23];

                string SATs = array[24] + ":" + array[25];
                string SATe = array[26] + ":" + array[27];
                timezones = new Dictionary<string, Dictionary<string, string>>();

                timezones.Add("SUN", new Dictionary<string, string>() { { "START", SUNs }, { "END", SUNe } });
                timezones.Add("MON", new Dictionary<string, string>() { { "START", MONs }, { "END", MONe } });
                timezones.Add("TUE", new Dictionary<string, string>() { { "START", TUEs }, { "END", TUEe } });
                timezones.Add("WEN", new Dictionary<string, string>() { { "START", WENs }, { "END", WENe } });
                timezones.Add("THU", new Dictionary<string, string>() { { "START", THUs }, { "END", THUe } });
                timezones.Add("FRI", new Dictionary<string, string>() { { "START", FRIs }, { "END", FRIe } });
                timezones.Add("SAT", new Dictionary<string, string>() { { "START", SATs }, { "END", SATe } });
            }
            return result;
        }

        /// <summary>
        /// To set the information about a time segment with a specified index.
        /// </summary>
        /// <param name="machineID">[IN] Machine ID</param>
        /// <param name="timezoneIndex">[IN] timezoneIndex</param>
        /// <param name="timezones">[IN] Timezone Datas</param>
        /// <returns></returns>
        public bool SetTimezone(int machineID, int timezoneIndex, Dictionary<string, Dictionary<string, string>> timezones)
        {
            string SUNs = timezones["SUN"]["START"].Replace(":", string.Empty);
            string SUNe = timezones["SUN"]["END"].Replace(":", string.Empty);

            string MONs = timezones["MON"]["START"].Replace(":", string.Empty);
            string MONe = timezones["MON"]["END"].Replace(":", string.Empty);

            string TUEs = timezones["TUE"]["START"].Replace(":", string.Empty);
            string TUEe = timezones["TUE"]["END"].Replace(":", string.Empty);

            string WENs = timezones["WEN"]["START"].Replace(":", string.Empty);
            string WENe = timezones["WEN"]["END"].Replace(":", string.Empty);

            string THUs = timezones["THU"]["START"].Replace(":", string.Empty);
            string THUe = timezones["THU"]["END"].Replace(":", string.Empty);

            string FRIs = timezones["FRI"]["START"].Replace(":", string.Empty);
            string FRIe = timezones["FRI"]["END"].Replace(":", string.Empty);

            string SATs = timezones["SAT"]["START"].Replace(":", string.Empty);
            string SATe = timezones["SAT"]["END"].Replace(":", string.Empty);
            string timezonedata = SUNs + SUNe + MONs + MONe + TUEs + TUEe + WENs + WENe + THUs + THUe + FRIs + FRIe + SATs + SATe;
            return axCZKEM1.SetTZInfo(machineID, timezoneIndex, timezonedata);
        }

        #endregion: END TIMEZONE

        #region:USER

        #region: GET USER INFO
        //READ DATA TO BUFFER

        /// <summary>
        /// To read all user information to the memory of the PC, including the user ID, password, name, and card number. <para />
        /// Fingerprint templates are not read. <para />
        /// After this function is executed, invoke the function GetAllUserID to get the user information. <para />
        /// Available For: FingerTec R2 Controller <para />
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <returns></returns>
        public bool ReadAllUserID(int machineNumber)
        {
            return axCZKEM1.ReadAllUserID(machineNumber);
        }

        //GET DATA FROM BUFFER
        #region: GET ALL USER
        /// <summary>
        /// To get all user information. 
        /// Before executing this function, invoke the function ReadAllUserID to read all user information to the memory. 
        /// Each time GetAllUserID is executed, the pointer moves to the next user information record. 
        /// After all user information is read, the function returns False. 
        /// This function differs from GetAllUserInfo in that the GetAllUserInfo function can obtain user names and passwords.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="dwEnrollNumber">[OUT] User ID</param>
        /// <param name="dwEMachineNumber">[OUT] Invalid parameter</param>
        /// <param name="dwBackupNumber">[OUT] Invalid parameter</param>
        /// <param name="privilege">[OUT] User Privilege</param>
        /// <param name="isEnable">[OUT] Whether a user account is enabled</param>
        /// <returns></returns>
        public bool GetAllUserID(int machineNumber, ref int dwEnrollNumber, ref int dwEMachineNumber, ref int dwBackupNumber, ref EM_UserPrivilege privilege, ref bool isEnable)
        {
            int _enable = 0;
            int _privilege = 0;
            bool result = axCZKEM1.GetAllUserID(machineNumber, ref dwEnrollNumber, ref dwEMachineNumber, ref dwBackupNumber, ref _privilege, ref _enable);
            if (result)
            {
                privilege = (EM_UserPrivilege)_privilege;
                isEnable = Convert.ToBoolean(_enable);
            }
            return result;
        }

        /// <summary>
        /// To get all user information. <para />
        /// Before executing this function, invoke the function ReadAllUserID to read all user information to the memory. <para />
        /// Each time GetAllUserInfo is executed, the pointer moves to the next user information record. <para />
        /// After all user information is read, the function returns False. <para />
        /// The GetAllUserInfo function differs from GetAllUserID in that it can obtain more information.<para />
        /// Available For: FingerTec R2 Controller <para />
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="enrollNumber">[IN] User ID</param>
        /// <param name="name">[IN] User name</param>
        /// <param name="password">[IN] User password</param>
        /// <param name="privilege">[IN] User privilege</param>
        /// <param name="isEnable">[IN] Whether a user account is enabled </param>
        /// <returns></returns>
        public bool GetAllUserInfo(int machineNumber, ref int enrollNumber, ref string name, ref string password, ref EM_UserPrivilege privilege, ref bool isEnable)
        {
            int _privilege = 0;
            bool result = axCZKEM1.GetAllUserInfo(machineNumber, ref enrollNumber, ref name, ref password, ref _privilege, ref isEnable);
            if (result)
            {
                privilege = (EM_UserPrivilege)_privilege;
            }
            return result;
        }

        /// <summary>
        /// To get all user information. 
        /// Before executing this function, invoke the function ReadAllUserID to read all user information to the memory.
        /// Each time SSR_GetAllUserInfo is executed, the pointer moves to the next user information record. 
        /// After all user information is read, the function returns False.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="enrollNumber">[OUT] User ID</param>
        /// <param name="name">[OUT] User name</param>
        /// <param name="password">[OUT] User password</param>
        /// <param name="privilege">[OUT] User privilege</param>
        /// <param name="isEnable">[OUT] Flag that indicates whether a user account is enabled</param>
        /// <returns></returns>
        public bool SSR_GetAllUserInfoEx(int machineNumber, out string enrollNumber, out string name, out string password, out EM_UserPrivilege privilege, out bool isEnable)
        {
            int _privilege = 0;
            bool result = axCZKEM1.SSR_GetAllUserInfo(machineNumber, out enrollNumber, out name, out password, out _privilege, out isEnable);
            if (result)
            {
                privilege = (EM_UserPrivilege)_privilege;
            }
            else
            {
                privilege = EM_UserPrivilege.COMMON;
            }
            return result;
        }
        #region: END GET ALL USER

        /// <summary>
        /// To obtain the user verification mode.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="enrollNumber">[IN] User ID</param>
        /// <param name="verifyStyle">[OUT] Verify Style</param>
        /// <param name="reserved">[OUT] Reserved</param>
        /// <returns></returns>
        public bool GetUserVerifyStyle(int machineNumber, int enrollNumber, out int verifyStyle, out byte reserved)
        {
            return axCZKEM1.GetUserInfoEx(machineNumber, enrollNumber, out verifyStyle, out reserved);
        }
        #endregion: END GET USER INFO

        #region: ADD USER INFO

        /// <summary>
        /// To set information about a user.<para />
        /// If the user does not exist on the machine, the user will be created.<para />
        /// Available For: FingerTec R2 Controller <para />
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="enrollNumber">[IN] User ID</param>
        /// <param name="Name">[IN] User name to be set</param>
        /// <param name="password">[IN] User password to be set. If this parameter is left blank, the password of the user will be cleared on the machine.</param>
        /// <param name="privilege">[IN]User Privilege</param>
        /// <param name="isEnable">[IN] Flag that indicates whether a user account is enabled.</param>
        /// <returns></returns>
        public bool SetUserInfo(int machineNumber, int enrollNumber, string Name, string password, EM_UserPrivilege privilege, bool isEnable)
        {
            return axCZKEM1.SetUserInfo(machineNumber, enrollNumber, Name, password, (int)privilege, isEnable);
        }

        /// <summary>
        /// To set information about a user. If the user does not exist on the machine, the user will be created
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="enrollNumber">[IN] User ID</param>
        /// <param name="Name">[IN] User name to be set</param>
        /// <param name="password">[IN] User password to be set. If this parameter is left blank, the password of the user will be cleared on the machine.</param>
        /// <param name="privilege">[IN]User Privilege</param>
        /// <param name="isEnable">[IN] Flag that indicates whether a user account is enabled.</param>
        /// <returns></returns>
        public bool SSR_SetUserInfo(int machineNumber, string enrollNumber, string Name, string password, EM_UserPrivilege privilege, bool isEnable)
        {
            return axCZKEM1.SSR_SetUserInfo(machineNumber, enrollNumber, Name, password, (int)privilege, isEnable);
        }

        #endregion: END ADD USER INFO

        #region: MODIFY USER INFO

        /// <summary>
        /// To set whether a user account is available.
        /// </summary>
        /// <param name="MachineNumber">[IN] Machine ID</param>
        /// <param name="EnrollNumber">[IN] User ID</param>
        /// <param name="EMachineNumber">[IN] Invalid parameter. It is meaningless </param>
        /// <param name="BackupNumber">[IN] Invalid parameter. It is meaningless</param>
        /// <param name="Flag">[IN] Flag that indicates whether a user account is enabled</param>
        /// <returns></returns>
        public bool EnableUser(int MachineNumber, int EnrollNumber, int EMachineNumber, int BackupNumber, bool Flag)
        {
            return axCZKEM1.EnableUser(MachineNumber, EnrollNumber, EMachineNumber, BackupNumber, Flag);
        }

        /// <summary>
        /// To set whether a user account is available.
        /// </summary>
        /// <param name="MachineNumber">[IN] Machine ID</param>
        /// <param name="EnrollNumber">[IN] User ID</param>
        /// <param name="Flag">[IN] Flag that indicates whether a user account is enabled</param>
        /// <returns></returns>
        public bool SSR_EnableUser(int MachineNumber, string EnrollNumber, bool Flag)
        {
            return axCZKEM1.SSR_EnableUser(MachineNumber, EnrollNumber, Flag);
        }

        /// <summary>
        /// To upload the user verification mode or group verification mode.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="EnrollNumber">[IN] Enroll Number</param>
        /// <param name="verifyStyle">[IN] Verification mode</param>
        /// <param name="Reserved">[IN] Reserved</param>
        /// <returns></returns>
        public bool SetUserVerifyStyle(int machineNumber, int EnrollNumber, int verifyStyle, ref byte Reserved)
        {
            return axCZKEM1.SetUserInfoEx(machineNumber, EnrollNumber, verifyStyle, ref Reserved);
        }

        /// <summary>
        /// To modify user privilege.
        /// </summary>
        /// <param name="machineNUmber">[IN] Machine ID</param>
        /// <param name="enrollNumber">[IN] User Id</param>
        /// <param name="EMachineNumber">[IN] Invalid parameter. It is meaningless</param>
        /// <param name="BackupNumber">[IN] Invalid parameter. It is meaningless</param>
        /// <param name="privilege">[IN] User Privilege</param>
        /// <returns></returns>
        public bool SetUserPrivelege(int machineNUmber, int enrollNumber, int EMachineNumber, int BackupNumber, EM_UserPrivilege privilege)
        {
            return axCZKEM1.ModifyPrivilege(machineNUmber, enrollNumber, EMachineNumber, BackupNumber, (int)privilege);
        }

        #endregion: END MODIFY USER INFO


        #region: DELETE USER
        /// <summary>
        /// To delete the multiple verification modes set by a specified user
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="enrollNumber">[IN] User ID</param>
        /// <returns></returns>
        public bool DeleteUserVerifyStyle(int machineNumber, int enrollNumber)
        {
            return axCZKEM1.DeleteUserInfoEx(machineNumber, enrollNumber);
        }

        /// <summary>
        /// To delete registration data.<para />
        /// Available For: FingerTec R2 Controller
        /// </summary>
        /// <param name="machineID">[IN] MachineID</param>
        /// <param name="userID">[IN] User ID</param>
        /// <param name="dwBackupNumber">[IN] 0-9: Delete user With FInger Indexs, 10:Delete Passwords, 11: Delete all fingerData of user, 12: deleteUSer</param>
        /// <returns></returns>
        public bool DeleteEnrollData(int machineID, int userID, int dwBackupNumber)
        {
            return axCZKEM1.DeleteEnrollData(machineID, userID, machineID, dwBackupNumber);
        }

        #endregion: END DELETE USER

        public bool GetUserTZs(int machineNumber, int userID, ref int timezones)
        {
            return axCZKEM1.GetUserTZs(machineNumber, userID, ref timezones);
        }

        public bool SetUserTZs(int machineNumber, int userID, ref int timezones)
        {
            return axCZKEM1.SetUserTZs(machineNumber, userID, ref timezones);
        }

        public bool GetUserTZstr(int machineNumber, int userID, ref string timezones)
        {
            return axCZKEM1.GetUserTZStr(machineNumber, userID, ref timezones);
        }

        public bool SetUserTZstr(int machineNumber, int userID, string timezones)
        {
            return axCZKEM1.SetUserTZStr(machineNumber, userID, timezones);
        }



        #endregion: END USER

        #region: GET SPECIFIC USER
        /// <summary>
        /// To get information about a specified user.<para />
        /// Available For: FInger Tec R2 Controller
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="userID">[IN] User ID</param>
        /// <param name="name">[OUT] User Name</param>
        /// <param name="password">[OUT] User Password</param>
        /// <param name="privilege">[OUT] User Privilege</param>
        /// <param name="isEnable">Flag that indicates whether a user account is enabled</param>
        /// <returns></returns>
        public bool GetUserInfoByID(int machineNumber, int userID, ref string name, ref string password, ref EM_UserPrivilege privilege, ref bool isEnable)
        {
            int _privilege = 0;
            bool result = axCZKEM1.GetUserInfo(machineNumber, userID, ref name, ref password, ref _privilege, ref isEnable);
            if (result)
            {
                privilege = (EM_UserPrivilege)_privilege;
            }
            return result;
        }

        /// <summary>
        /// To obtain information about a specified user
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="userID">[IN] User ID</param>
        /// <param name="name">[OUT] User Name</param>
        /// <param name="password">[OUT] User Password</param>
        /// <param name="privilege">[OUT] User Privilege</param>
        /// <param name="isEnable">Flag that indicates whether a user account is enabled</param>
        /// <returns></returns>
        public bool SSR_GetUserInfoByID(int machineNumber, string userID, out string name, out string password, out EM_UserPrivilege privilege, out bool isEnable)
        {
            int _privilege = 0;
            bool result = axCZKEM1.SSR_GetUserInfo(machineNumber, userID, out name, out password, out _privilege, out isEnable);
            if (result)
            {
                privilege = (EM_UserPrivilege)_privilege;
            }
            else
            {
                privilege = EM_UserPrivilege.COMMON;
            }
            return result;
        }
        #endregion: END GET SPECIFIC USER
        #endregion
        #endregion

        #region: FINGER

        #region: GET
        //READ DATA TO BUFFER
        public bool ReadAllTemplate(int machineNumber)
        {
            return axCZKEM1.ReadAllTemplate(machineNumber);
        }
        //GET ALL FINGER TEMPLATE
        public bool SSR_GetUserTmp(int machineNumber, string userID, int fingerIndex, out byte TmpData, out int length)
        {
            return axCZKEM1.SSR_GetUserTmp(machineNumber, userID, fingerIndex, out TmpData, out length);
        }

        public bool SSR_GetUserTmpStr(int machineID, int userID, int fingerIndex, ref string fingerData, ref int dataLength)
        {
            return axCZKEM1.GetUserTmpStr(machineID, userID, fingerIndex, ref fingerData, ref dataLength);
        }

        //GET SPECIFIC FINGER TEMPLATE
        public bool GetUserTmp(int machineID, int userID, int fingerIndex, ref byte tmpData, ref int dataLength)
        {
            return axCZKEM1.GetUserTmp(machineID, userID, fingerIndex, ref tmpData, ref dataLength);
        }

        /// <summary>
        /// To obtain a fingerprint template in character string format. This function differs from GetUserTmp only in the fingerprint template format.
        /// Available For: Fingertec R2
        /// </summary>
        /// <param name="machineID">[IN] Machine ID</param>
        /// <param name="userID">[IN] User ID</param>
        /// <param name="fingerIndex">[IN] Fingerprint index</param>
        /// <param name="tmpData">[OUT] Fingerprint template data</param>
        /// <param name="dataLength">[OUT] Length of the fingerprint template</param>
        /// <returns></returns>
        public bool GetUserTmpStr(int machineID, int userID, int fingerIndex, ref string tmpData, ref int dataLength)
        {
            return axCZKEM1.GetUserTmpStr(machineID, userID, fingerIndex, ref tmpData, ref dataLength);
        }

        public bool GetUserTmpEx(int machineID, string userID, int fingerIndex, out int flag, out byte tmpData, out int dataLength)
        {
            return axCZKEM1.GetUserTmpEx(machineID, userID, fingerIndex, out flag, out tmpData, out dataLength);
        }

        /// <summary>
        /// To obtain fingerprint template ZKFinger 10.0 in character string format <para />
        /// Available For: FingerTec R2 Controller <para />
        /// </summary>
        /// <param name="machineID">[IN] Machine ID</param>
        /// <param name="userID">[IN] User ID</param>
        /// <param name="fingerIndex">[IN] Finger Index </param>
        /// <param name="flag">[OUT] Flag that indicates whether the fingerprint template is valid or a duress fingerprint</param>
        /// <param name="tmpData">[OUT] Fingerprint template</param>
        /// <param name="dataLength">[OUT] Length of the fingerprint template</param>
        /// <returns></returns>
        public bool GetUserTmpExStr(int machineID, string userID, int fingerIndex, out int flag, out string tmpData, out int dataLength)
        {
            return axCZKEM1.GetUserTmpExStr(machineID, userID, fingerIndex, out flag, out tmpData, out dataLength);
        }

        #endregion: END GET

        #region: ADD
        /// <summary>
        /// To upload a fingerprint template in binary format. This function differs from SSR_SetUserTmpStr only in the fingerprint template format.
        /// </summary>
        /// <param name="machineNumber">[IN]Machine ID</param>
        /// <param name="userId">[IN] User ID</param>
        /// <param name="fingerIndex">[IN] Fingerprint index, ranging from 0 to 9 </param>
        /// <param name="TmpData">[IN] Fingerprint template</param>
        /// <returns></returns>
        public bool SetUserTmp(int machineNumber, int userId, int fingerIndex, ref byte TmpData)
        {
            return axCZKEM1.SetUserTmp(machineNumber, userId, fingerIndex, ref TmpData);
        }

        /// <summary>
        /// To upload a fingerprint template in character string format. This function differs from SSR_SetUserTmp only in the fingerprint template format.
        /// </summary>
        /// <param name="machineNumber">[IN]Machine ID</param>
        /// <param name="userId">[IN] User ID</param>
        /// <param name="fingerIndex">[IN] Fingerprint index, ranging from 0 to 9 </param>
        /// <param name="TmpData">[IN] Fingerprint template</param>
        /// <returns></returns>
        public bool SSR_SetUserTmpStr(int machineNUmber, int userID, int fingerIndex, string TmpData)
        {
            return axCZKEM1.SetUserTmpStr(machineNUmber, userID, fingerIndex, TmpData);
        }

        /// <summary>
        /// To upload a fingerprint template. This function is an enhanced version of SSR_SetUserTmp.
        /// </summary>
        /// <param name="machineNUmber">[IN] Machine ID</param>
        /// <param name="isOverrided">[IN] Is Overrided</param>
        /// <param name="userID">[IN] User ID</param>
        /// <param name="fingerIndex">[IN] Fingerprint index, ranging from 0 to 9 </param>
        /// <param name="TmpData">[IN] Fingerprint template</param>
        /// <returns></returns>
        public bool SSR_SetUserTmpExt(int machineNUmber, bool isOverrided, string userID, int fingerIndex, ref byte TmpData)
        {
            return axCZKEM1.SSR_SetUserTmpExt(machineNUmber, isOverrided == true ? 1 : 0, userID, fingerIndex, ref TmpData);
        }
        /// <summary>
        /// Set User Finger Print Template <para />
        /// Available For: FingerTec R2 Controller
        /// </summary>
        /// <param name="machineNumber"></param>
        /// <param name="userID"></param>
        /// <param name="fingerIndex"></param>
        /// <param name="flag"></param>
        /// <param name="tmpData"></param>
        /// <returns></returns>
        public bool SetUserTmpExStr(int machineNumber, string userID, int fingerIndex, bool flag, string tmpData)
        {
            return axCZKEM1.SetUserTmpExStr(machineNumber, userID, fingerIndex, flag == true ? 1 : 0, tmpData);
        }
        #endregion: END ADD

        #region: DELETE
        /// <summary>
        /// To delete a specified fingerprint template
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="userID">[IN] User ID</param>
        /// <param name="fingerIndex">[IN] Fingerprint index, ranging from 0 to 9</param>
        /// <returns></returns>
        public bool DelUserTmp(int machineNumber, int userID, int fingerIndex)
        {
            return axCZKEM1.DelUserTmp(machineNumber, userID, fingerIndex);
        }

        /// <summary>
        /// To delete a fingerprint template.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="userID">[IN] User ID</param>
        /// <param name="fingerIndex">[IN] Fingerprint index, ranging from 0 to 9</param>
        /// <returns></returns>
        public bool SSR_DelUserTmp(int machineNumber, string userID, int fingerIndex)
        {
            return axCZKEM1.SSR_DelUserTmp(machineNumber, userID, fingerIndex);
        }

        /// <summary>
        /// Deletes the specified fingerprint template for the specified user.
        /// </summary>
        /// <param name="machineNumnber">[IN] Machine ID</param>
        /// <param name="userID">[IN]User ID</param>
        /// <param name="fingerIndex">[In]</param>
        /// <returns></returns>
        public bool SSR_DelUserTmpExt(int machineNumnber, string userID, int fingerIndex)
        {
            return axCZKEM1.SSR_DelUserTmpExt(machineNumnber, userID, fingerIndex);
        }

        #endregion: END DELETE

        #endregion: END FINGER

        #region: FACE
        #endregion: END FACE

        #region:LOGS

        #region:GENERAL LOGs
        //READ DATA TO BUFFER
        public bool ReadGeneralLogs(int machineNumber)
        {
            return axCZKEM1.ReadGeneralLogData(machineNumber);
        }

        /// <summary>
        /// Read All GLog To Buffer <para />
        ///  Available for: FingerTec R2 <para />
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <returns></returns>
        public bool ReadAllGLogs(int machineNumber)
        {
            return axCZKEM1.ReadAllGLogData(machineNumber);
        }

        /// <summary>
        /// Get General Log By Time
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="startTime">[IN] Start Time</param>
        /// <param name="endTime">[IN] End Time</param>
        /// <returns></returns>
        public bool ReadTimeGLogData(int machineNumber, string startTime, string endTime)
        {
            return axCZKEM1.ReadTimeGLogData(machineNumber, startTime, endTime);
        }

        /// <summary>
        /// Download the new generated attendance records
        /// </summary>
        /// <param name="machineNumber"></param>
        /// <returns></returns>
        public bool ReadNewGLogData(int machineNumber)
        {
            return axCZKEM1.ReadNewGLogData(machineNumber);
        }

        //GET DATA FROM BUFFER
        /// <summary>
        /// To read attendance records from the internal buffer one by one.
        /// Before using this function, execute ReadAllGLogData or ReadGeneralLogData to read the attendance records from the machine to the internal buffer of the PC.
        /// Each time this function is executed, the pointer moves to the next attendance record. 
        /// This function is the same as GetAllGLogData. They differ only in the interface name for compatibility
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="dwTMachineNumber">[OUT]</param>
        /// <param name="dwEnrollNumber">[OUT] User ID</param>
        /// <param name="dwEMachineNumber">[OUT]</param>
        /// <param name="dwVerifyMode">[OUT] Verify Mode</param>
        /// <param name="dwInOutMode">[OUT] In Out Mode</param>
        /// <param name="eventTime">[OUT] Event Time</param>
        /// <returns></returns>
        public bool GetGeneralLogData(int machineNumber, ref int dwTMachineNumber, ref int dwEnrollNumber, ref int dwEMachineNumber, ref int dwVerifyMode, ref int dwInOutMode, ref DateTime eventTime)
        {
            int year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = 0, minute = 0;
            bool result = axCZKEM1.GetGeneralLogData(machineNumber, ref dwTMachineNumber, ref dwEnrollNumber, ref dwEMachineNumber, ref dwVerifyMode, ref dwInOutMode, ref year, ref month, ref day, ref hour, ref minute);
            if (result)
            {
                eventTime = new DateTime(year, month, day, hour, minute, 0);
            }
            return result;
        }

        /// <summary>
        /// To read attendance records from the internal buffer one by one.<para />
        /// Before using this function, execute ReadAllGLogData or ReadGeneralLogData to read the attendance records from the machine to the internal buffer of the PC.<para />
        /// Each time this function is executed, the pointer moves to the next attendance record. <para />
        /// This function is the same as GetAllGLogData. They differ only in the interface name for compatibility<para />
        /// Available For: Finger Tec R2<para />
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="dwTMachineNumber">[OUT]</param>
        /// <param name="dwEnrollNumber">[OUT] User ID</param>
        /// <param name="dwEMachineNumber">[OUT]</param>
        /// <param name="dwVerifyMode">[OUT] Verify Mode</param>
        /// <param name="dwInOutMode">[OUT] In Out Mode</param>
        /// <param name="eventTime">[OUT] Event Time</param>
        /// <returns></returns>
        public bool GetAllGLogData(int machineNumber, ref int dwTMachineNumber, ref int dwEnrollNumber, ref int dwEMachineNumber, ref int dwVerifyMode, ref int dwInOutMode, ref DateTime eventTime)
        {
            int year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = 0, minute = 0;
            bool result = axCZKEM1.GetGeneralLogData(machineNumber, ref dwTMachineNumber, ref dwEnrollNumber, ref dwEMachineNumber, ref dwVerifyMode, ref dwInOutMode, ref year, ref month, ref day, ref hour, ref minute);
            if (result)
            {
                eventTime = new DateTime(year, month, day, hour, minute, 0);
            }
            return result;
        }

        /// <summary>
        /// To read attendance records from the internal buffer one by one.
        /// Before using this function, execute ReadAllGLogData or ReadGeneralLogData to read the attendance records from the machine to the internal buffer of the PC.
        /// Each time this function is executed, the pointer moves to the next attendance record. 
        /// This function is the same as GetAllGLogData. They differ only in the interface name for compatibility
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="dwEnrollNumber">[OUT] User ID</param>
        /// <param name="dwVerifyMode">[OUT] Verify Mode</param>
        /// <param name="dwInOutMode">[OUT] In Out Mode</param>
        /// <param name="eventTime">[OUT] Event Time</param>
        /// <returns></returns>
        public bool GetGeneralLogDataStr(int machineNumber, ref int dwEnrollNumber, ref int dwVerifyMode, ref int dwInOutMode, ref string eventTime)
        {
            return axCZKEM1.GetGeneralLogDataStr(machineNumber, ref dwEnrollNumber, ref dwVerifyMode, ref dwInOutMode, ref eventTime);
        }

        /// <summary>
        /// To read attendance records from the internal buffer one by one. 
        /// Before using this function, execute ReadAllGLogData or ReadGeneralLogData to read the attendance records from the machine to the internal buffer of the PC.
        /// Each time this function is executed, the pointer moves to the next attendance record.
        /// This function is an enhanced version of GetGeneralLogData. They are compatible.
        /// </summary>
        /// <param name="machineNumber"></param>
        /// <param name="dwEnrollNumber"></param>
        /// <param name="dwVerifyMode"></param>
        /// <param name="dwInOutMode"></param>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        public bool GetGeneralExtLogData(int machineNumber, ref int dwEnrollNumber, ref int dwVerifyMode, ref int dwInOutMode, ref DateTime eventTime)
        {
            int year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = 0, minute = 0, second = 0, dwReserved = 0, workCode = 0;
            bool result = axCZKEM1.GetGeneralExtLogData(machineNumber, ref dwEnrollNumber, ref dwVerifyMode, ref dwInOutMode, ref year, ref month, ref day, ref hour, ref minute, ref second, ref workCode, ref dwReserved);
            if (result)
            {
                eventTime = new DateTime(year, month, day, hour, minute, 0);
            }
            return result;
        }

        /// <summary>
        /// To read attendance records from the internal buffer one by one.
        /// Before using this function, execute ReadAllGLogData or ReadGeneralLogData to read the attendance records from the machine to the internal buffer of the PC. Each time this function is executed, the pointer moves to the next attendance record. 
        /// This function is the same as GetGeneralLogData. 
        /// The difference is that this function applies to color-screen machines.
        /// </summary>
        /// <param name="machineNumber"></param>
        /// <param name="dwEnrollNumber"></param>
        /// <param name="dwVerifyMode"></param>
        /// <param name="dwInOutMode"></param>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        public bool SSR_GetGeneralLogData(int machineNumber, out string dwEnrollNumber, out int dwVerifyMode, out int dwInOutMode, ref DateTime eventTime)
        {
            int year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = 0, minute = 0, second = 0, dwReserved = 0, workCode = 0;
            bool result = axCZKEM1.SSR_GetGeneralLogData(machineNumber, out dwEnrollNumber, out dwVerifyMode, out dwInOutMode, out year, out month, out day, out hour, out minute, out second, ref workCode);
            if (result)
            {
                eventTime = new DateTime(year, month, day, hour, minute, 0);
            }
            return result;
        }

        //DELETE LOG
        /// <summary>
        /// To clear all attendance records on the machine <para />
        /// Available for: FingerTec R2 Controller <para />
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <returns></returns>
        public bool ClearGLog(int machineNumber)
        {
            return axCZKEM1.ClearGLog(machineNumber);
        }

        /// <summary>
        /// To delete attendance records based on the specified start time and end time, accurate to seconds
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="startTime">[IN] Start time in the format of YYYY-MM-DD hh:mm:ss</param>
        /// <param name="endTime">[IN] Start time in the format of YYYY-MM-DD hh:mm:ss</param>
        /// <returns></returns>
        public bool DeleteAttlogBetweenTheDate(int machineNumber, string startTime, string endTime)
        {
            return axCZKEM1.DeleteAttlogBetweenTheDate(machineNumber, startTime, endTime);
        }

        /// <summary>
        /// To delete all attendance records generated before the specified time point, accurate to seconds
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="sTime">[IN] Start time in the format of YYYY-MM-DD hh:mm:ss</param>
        /// <returns></returns>
        public bool DeleteAttlogByTime(int machineNumber, string sTime)
        {
            return axCZKEM1.DeleteAttlogByTime(machineNumber, sTime);
        }

        #endregion: END GENERAL LOGs

        #region:SUPER LOGs
        //READ DATA TO BUFFER

        /// <summary>
        /// To read operation records to the internal buffer of the PC. The function is the same as ReadAllSLogData.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <returns></returns>
        public bool ReadSuperLog(int machineNumber)
        {
            return axCZKEM1.ReadSuperLogData(machineNumber);
        }

        /// <summary>
        /// To read operation records to the internal buffer of the PC. The function is the same as ReadSuperLogData.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <returns></returns>
        public bool ReadAllSLogData(int machineNumber)
        {
            return axCZKEM1.ReadAllSLogData(machineNumber);
        }

        //GET DATA FROM BUFFER

        /// <summary>
        /// To read operation records from the internal buffer one by one.
        /// Before using this function, execute ReadAllSLogData or ReadSuperLogData to read the operation records from the machine to the internal buffer of the PC.
        /// Each time this function is executed, the pointer moves to the next operation record.
        /// This function differs from GetSuperLogData2 in that the GetSuperLogData2 function can obtain the operation record time accurate to seconds.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="dwTMachineNumber">[OUT] Pointer that points to the LONG variable. Its value is the machine ID of an operation record.</param>
        /// <param name="dwSEnrollNumber">[OUT] Pointer that points to the LONG variable. Its value is the administrator ID of an operation record.</param>
        /// <param name="Params">[OUT] arrays of Pointer that points to the LONG variable. The value varies according to that of dwManipulation.</param>
        /// <param name="dwManipulation">[OUT] Pointer that points to the LONG variable. Its value is the operation type.</param>
        /// <param name="eventTime">[OUT] Event Time</param>
        /// <returns></returns>
        public bool GetSuperLogData(int machineNumber, ref int dwTMachineNumber, ref int dwSEnrollNumber, ref int[] Params, ref int dwManipulation, ref DateTime eventTime)
        {
            int year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = 0, minute = 0, second = 0, dwReserved = 0, workCode = 0;
            int params1 = 0, params2 = 0, params3 = 0, params4 = 0;
            bool result = axCZKEM1.GetSuperLogData(machineNumber, ref dwTMachineNumber, ref dwSEnrollNumber, ref params4, ref params1, ref params2, ref dwManipulation, ref params3, ref year, ref month, ref day, ref hour, ref minute);
            if (result)
            {
                eventTime = new DateTime(year, month, day, hour, minute, 0);
                Params = new int[] { params1, params2, params3, params4 };
            }
            return result;
        }

        /// <summary>
        /// To read operation records from the internal buffer one by one.
        /// Before using this function, execute ReadAllSLogData or ReadSuperLogData to read the operation records from the machine to the internal buffer of the PC. Each time this function is executed, the pointer moves to the next operation record.
        /// This function is the same as GetSuperLogData. 
        /// They differ only in the interface name for compatibility
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <param name="dwTMachineNumber">[OUT]</param>
        /// <param name="dwSEnrollNumber">[OUT]</param>
        /// <param name="dwSMachineNumber">[OUT]</param>
        /// <param name="dwGEnrollNumber">[OUT]</param>
        /// <param name="dwGMachineNumber">[OUT]</param>
        /// <param name="dwManipulation">OUT</param>
        /// <param name="dwBackupNumber">[OUT]</param>
        /// <param name="eventTime">[OUT] Event Time</param>
        /// <returns></returns>
        public bool GetAllSLogData(int machineNumber, ref int dwTMachineNumber, ref int dwSEnrollNumber, ref int dwSMachineNumber, ref int dwGEnrollNumber, ref int dwGMachineNumber, ref int dwManipulation, ref int dwBackupNumber, ref DateTime eventTime)
        {
            int year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = 0, minute = 0, second = 0, dwReserved = 0, workCode = 0;
            bool result = axCZKEM1.GetAllSLogData(machineNumber, ref dwTMachineNumber, ref dwSEnrollNumber, dwSMachineNumber, ref dwGEnrollNumber, ref dwGMachineNumber, ref dwManipulation, ref dwBackupNumber, ref year, ref month, ref day, ref hour, ref minute);
            if (result)
            {
                eventTime = new DateTime(year, month, day, hour, minute, 0);
            }
            return result;
        }

        /// <summary>
        /// To read operation records from the internal buffer one by one.
        /// Before using this function, execute ReadAllSLogData or ReadSuperLogData to read the operation records from the machine to the internal buffer of the PC.
        /// Each time this function is executed, the pointer moves to the next operation record. 
        /// GetSuperLogData and GetSuperLogData2 differ in that the GetSuperLogData2 function can obtain the operation record time accurate to seconds
        /// </summary>
        /// <param name="dwMachineNumber"></param>
        /// <param name="dwTMachineNumber"></param>
        /// <param name="dwSEnrollNumber"></param>
        /// <param name="Params"></param>
        /// <param name="dwManipulation"></param>
        /// <param name="eventTime"></param>
        /// <returns></returns>
        public bool GetSuperLogData2(int dwMachineNumber, ref int dwTMachineNumber, ref int dwSEnrollNumber, ref int[] Params, ref int dwManipulation, ref DateTime eventTime)
        {
            int year = DateTime.Now.Year, month = DateTime.Now.Month, day = DateTime.Now.Day, hour = 0, minute = 0, second = 0, dwReserved = 0, workCode = 0;
            int params1 = 0, params2 = 0, params3 = 0, params4 = 0;
            bool result = axCZKEM1.GetSuperLogData2(dwMachineNumber, ref dwTMachineNumber, ref dwSEnrollNumber, ref params4, ref params1, ref params2, ref dwManipulation, ref params3, ref year, ref month, ref day, ref hour, ref minute, ref second);
            if (result)
            {
                eventTime = new DateTime(year, month, day, hour, minute, second);
                Params = new int[] { params1, params2, params3, params4 };
            }
            return result;
        }

        //DELETE SUPER LOG
        /// <summary>
        /// To clear all operation records on the machine.
        /// </summary>
        /// <param name="machineNumber">[IN] Machine ID</param>
        /// <returns></returns>
        public bool ClearSuperLog(int machineNumber)
        {
            return axCZKEM1.ClearSLog(machineNumber);
        }
        #endregion: END SUPER LOGs

        #endregion: END LOGS

        #region: ENUMS
        //USER
        public enum EM_UserPrivilege
        {
            COMMON,
            REGISTRAR,
            ADMINISTRATOR,
            SUPER_ADMINISTRATOR
        }

        //Device Status
        public enum EM_Zkteco_DwStatus
        {
            OTHER_CONDITION,
            NUMBER_OF_ADMINS,
            NUMBER_OF_REGISTERED_USERS,
            NUMBER_OF_FINGERPRINT_TEMPLATES,
            NUMBER_OF_PASSWORD,
            NUMBER_OF_OPERATION_RECORDS,
            NUMBER_OF_ATTENDANCE_RECORDS,
            CAPACITY_FINGERPRINT,
            CAPACITY_USER,
            CAPACITY_ATTENDANCE_RECORD,
            CAPACITY_REMAINING_FINGERPRINT_TEMPLATE,
            CAPACITY_REMAINING_USER,
            CAPACITY_REMAINING_ATTENDANCE_RECORD,
            NUMBER_OF_FACES,
            CAPACITY_FACES,
        }

        public int GetDwStatusIndex(EM_Zkteco_DwStatus dwStatus)
        {
            switch (dwStatus)
            {
                case EM_Zkteco_DwStatus.NUMBER_OF_FACES:
                    return 21;
                case EM_Zkteco_DwStatus.CAPACITY_FACES:
                    return 22;
                default:
                    return (int)dwStatus;
            }
        }
        //Device Infor
        public enum EM_Zkteco_DwInfor
        {
            UNKNOWN,
            MAX_NUMBER_OF_ADMIN, //Default 500
            MACHINE_ID,
            LANGUAGE,
            IDLE_DURATION, //(in minutes): After the specified idle duration elapses, the machine will enter the standby state or be shut down
            LOCK_CONTROL_DURATION, //that is, the lock drive duration.
            ATTENDANCE_RECORD_QUANTITY_ALARM, //When the specified attendance record quantity is reached, the machine will raise an alarm to remind the user.
            OPERATION_RECORD_QUANTITY_ALARM,
            DUPLICATE_RECORD_TIME,
            BAUD_RATE_FOR_RS232or485_COMMUNICATION,
            _PARITY_CHECK_BIT,
            STOP_BIT,
            _DATE_SEPARATOR,
            ENABLE_NETWORK_FUNCTIONS,
            ENABLE_RS232,
            ENABLE_RS485,
            ENABLE_ANNOUNCEMENTS,
            PERFORM_HIGH_SPEED_COMPARISON, IDLE_MODE,
            AUTOMATIC_SHUTDOWN_TIME,
            AUTOMATIC_STARTUP_TIME,
            AUTOMATIC_HIBERNATION_TIME,
            AUTOMATIC_RINGING_TIME,
            _1_TO_N_COMPARISON_THRESHOLD,
            REGISTRATION_THRESHOLD,
            _1_TO_1_COMPARISON_THRESHOLD,
            DISPLAY_THE_MATCHING_SCORE_DURING_VERIFICATION,
            NUMBER_OF_PEOPLE_THAT_UNLOCK_THE_DOOR_CONCURRENTLY,
            VERIFY_THE_CARD_NUMBER_ONLY,
            NETWORK_SPEED,
            _WHETHER_A_CARD_MUST_BE_REGISTERED,
            WAITING_TIME_BEFORE_THE_MACHINE_AUTOMATICALLY_RETURNS_TO_THE_INITIAL_STATE_IF_NO_OPERATION_IS_PERFORMED,
            WAITING_TIME_BEFORE_THE_MACHINE_AUTOMATICALLY_RETURNS_TO_THE_INITIAL_STATE_IF_NO_RESPONSE_IS_RETURNED_AFTER_THE_PIN_IS_INPUT,
            WAITING_TIME_BEFORE_THE_MACHINE_AUTOMATICALLY_RETURNS_TO_THE_INITIAL_STATE_IF_NO_OPERATION_IS_PERFORMED_AFTER_ENTERING_THE_MENU,
            TIME_FORMAT,
            _1_1_COMPARISON_IS_MANDATORY,
        }

        public enum EM_Zkteco_Language
        {
            ENGLISH,
            OTHER,
            CHINESE,
            THAI
        }

        //Device State
        public enum EM_DeviceState
        {
            WAITING,
            FingerprintRegistrantion,
            FingerprintIdentification,
            MenuAccess,
            Busy, //handling Other Work
            WaitingForCardWriting,
            UNKNOWN
        }

        public enum EM_VerifyMode
        {
            FP_OR_PW_OR_RF,
            FP,
            PIN,
            PW,
            RF,
            FP_OR_PW,
            FP_OR_RF,
            PW_OR_RF,
            PIN_AND_FP,
            FP_AND_PW,
            FP_AND_RF,
            PW_AND_RF,
            FP_AND_PW_AND_RF,
            PIN_AND_FP_AND_PW,
            FP_AND_RF_OR_PIN
        }
        public enum EM_InOutMode
        {
            CHECK_IN_DEFAULT,
            CHECK_OUT,
            BREAK_OUT,
            BREAK_IN,
            OT_IN,
            OT_OUT
        }
        #endregion: END ENUMS

    }
}
