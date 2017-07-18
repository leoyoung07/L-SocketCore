using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace L_SocketCore
{
    public class Util
    {
        public static byte[] ConvertInt32ToBytes(Int32 intValue)
        {
            const int INT_SIZE = 4;
            byte[] buffer = new byte[INT_SIZE];
            buffer[0] = (byte)(intValue >> 24 & 0xFF);
            buffer[1] = (byte)(intValue >> 16 & 0xFF);
            buffer[2] = (byte)(intValue >> 8 & 0xFF);
            buffer[3] = (byte)(intValue & 0xFF);
            return buffer;
        }

        public static void WriteLog(string message, string fileName = "log.txt")
        {
            string root = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
            string logPath = string.Format("{0}\\{1}", root, fileName);
            using (StreamWriter sw = new StreamWriter(logPath, true, Encoding.UTF8))
            {
                sw.WriteLine(message);
            }
        }
    }
}
