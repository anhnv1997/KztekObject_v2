using KztekObject.Objects;
using System;
using System.Collections.Generic;
using System.Text;

namespace KztekObject
{
    public class ControllerType
    {
        public enum EM_ControllerType
        {
            FingerTech_R2_SDK3,
            TCP_IP,
            KZE05,
            KZE32,
            ZKTEKO_PULL
        }

        public static Controller GetControllerByName(string controllerName)
        {
            return null;
        }

        public static Controller GetControllerByType(EM_ControllerType controllerType)
        {
            return null;
        }

        public static Controller GetControllerByType(int controllerType)
        {
            return null;
        }
    }
}
