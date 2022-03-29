using KztekObject.Controllers.KZE05NET_Controller;
using KztekObject.Controllers.ZKTEKO_Devices.Standard;
using System;
using System.Collections.Generic;
using System.Text;
using static KztekObject.ControllerType;

namespace KztekObject.Controllers
{
    public class ControllerFactory
    {
        public static IController GetControllerByType(EM_ControllerType controllerType)
        {
            switch (controllerType)
            {
                case EM_ControllerType.KZE05:
                    return new KZE05_NET_v3_4();
                case EM_ControllerType.FingerTech_R2_SDK3:
                    return new Zkteko_Standard_Controller();
                default:
                    return null;
            }
        }
    }
}
