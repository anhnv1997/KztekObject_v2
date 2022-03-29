using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KztekObject.Cards
{
    public class KzHelper
    {
        public enum EM_ByteLength
        {
            _1BYTE,
            _4BYTE,
            _8BYTE
        }
        private static int GetByteLenght(EM_ByteLength byteLength)
        {
            switch (byteLength)
            {
                case EM_ByteLength._1BYTE:
                    return 1;
                case EM_ByteLength._4BYTE:
                    return 4;
                case EM_ByteLength._8BYTE:
                    return 8;
                default:
                    return 4;
            }
        }
        public static string GetOpenDoor(string doors, EM_ByteLength byteLengthType)
        {
            int byteLength = GetByteLenght(byteLengthType);
            string[] door = doors.Split(';');
            try
            {
                int[] _door = new int[byteLength];
                for (int i = 0; i < byteLength; i++)
                {
                    _door[i] = 0;
                }
                for (int i = 0; i < door.Length; i++)
                {
                    int division = Convert.ToInt32(door[i]) / 8;
                    int remainder = Convert.ToInt32(door[i]) % 8 == 0 ? 8 : Convert.ToInt32(door[i]) % 8;
                    division = remainder == 8 ? division - 1 : division;
                    _door[byteLength - 1 - division] += (int)Math.Pow(2, remainder - 1);
                }
                List<string> hexValues = _door.Select(x => x.ToString("X2")).ToList();
                return String.Join(".", hexValues);
            }
            catch
            {
                if (byteLength == 1)
                {
                    return "00";
                }
                else if (byteLength == 4)
                {
                    return "00.00.00.00";
                }
                return "00.00.00.00.00.00.00.00";
            }
        }
        public static string GetOpenDoor(List<int> doors, EM_ByteLength byteLengthType)
        {
            int byteLength = GetByteLenght(byteLengthType);
            try
            {
                int[] _door = new int[byteLength];
                for (int i = 0; i < byteLength; i++)
                {
                    _door[i] = 0;
                }
                for (int i = 0; i < doors.Count; i++)
                {
                    int division = Convert.ToInt32(doors[i]) / 8;
                    int remainder = Convert.ToInt32(doors[i]) % 8 == 0 ? 8 : Convert.ToInt32(doors[i]) % 8;
                    division = remainder == 8 ? division - 1 : division;
                    _door[byteLength - 1 - division] += (int)Math.Pow(2, remainder - 1);
                }
                List<string> hexValues = _door.Select(x => x.ToString("X2")).ToList();
                return String.Join(".", hexValues);
            }
            catch
            {
                if (byteLength == 1)
                {
                    return "00";
                }
                else if (byteLength == 4)
                {
                    return "00.00.00.00";
                }
                return "00.00.00.00.00.00.00.00";
            }
        }

        public static string GetCloseDoor(int[] doors, EM_ByteLength byteLengthType)
        {
            int byteLength = GetByteLenght(byteLengthType);
            try
            {
                int[] _door = new int[byteLength];
                for (int i = 0; i < byteLength; i++)
                {
                    _door[i] = 0;
                }
                for (int i = 0; i < doors.Length; i++)
                {
                    int division = Convert.ToInt32(doors[i]) / 8;
                    int remainder = Convert.ToInt32(doors[i]) % 8 == 0 ? 8 : Convert.ToInt32(doors[i]) % 8;
                    division = remainder == 8 ? division - 1 : division;
                    _door[byteLength - 1 - division] += (int)Math.Pow(2, remainder - 1);
                }
                //string response = "";
                //for (int i = 0; i < byteLength - 1; i++)
                //{
                //    response += _door[i].ToString("X2") + ".";
                //}
                //response += _door[_door.Length - 1].ToString("X2");
                //return response;
                return String.Join('.', _door);
            }
            catch
            {
                if (byteLength == 1)
                {
                    return "00";
                }
                else if (byteLength == 4)
                {
                    return "00.00.00.00";
                }
                return "00.00.00.00.00.00.00.00";
            }
        }


        public static List<int> GetDoorIndexs(string[] doors)
        {
            string door = "";
            for (int i = doors.Length - 1; i >= 0; i--)
            {
                string temp = Convert.ToString(Convert.ToInt32(doors[i], 16), 2);
                while (temp.Length < 8)
                    temp = "0" + temp;
                for (int j = 7; j >= 0; j--)
                {
                    if (i == 0 && j == 0)
                    {
                        if (temp[j] == '1')
                        {
                            door += (7 - j + 1) + (8 * (doors.Length - 1 - i)) + "";
                        }
                    }
                    else
                    {
                        if (temp[j] == '1')
                        {
                            door += (7 - j + 1) + (8 * (doors.Length - 1 - i)) + ";";
                        }
                    }
                }
            }
            if (door[door.Length-1] == ';')
            {
                door = door.Substring(0,door.Length-1);
            }
            return Array.ConvertAll<string, int>(door.Split(';'), new Converter<string, int>(Convert.ToInt32)).ToList();
        }
    }
}
