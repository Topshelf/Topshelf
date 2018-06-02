using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.ServiceProcess
{
    public static class ServiceControllerExtensions
    {
        public static bool ValidServiceName(string serviceName)
        {
            if (serviceName == null)
            {
                return false;
            }
            if (serviceName.Length <= 80 && serviceName.Length != 0)
            {
                char[] array = serviceName.ToCharArray();
                foreach (char c in array)
                {
                    if (c == '\\' || c == '/')
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
