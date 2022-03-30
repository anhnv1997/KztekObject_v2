using System;
using System.Collections.Generic;
using System.Text;

namespace KztekObject.enums
{
    public class CommunicationType
    {
        public enum EM_CommunicationType
        {
            TCP_IP,
            SERIAL,
            USB
        }

        public static bool IS_TCP(EM_CommunicationType communicationType)
        {
            return communicationType == EM_CommunicationType.TCP_IP;
        }
    }
}
