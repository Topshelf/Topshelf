using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Configuration.Install
{
    public class InstallContext
    {
        private StringDictionary parameters;

        public StringDictionary Parameters => this.parameters;

        public InstallContext() : this(null, null)
        {
        }

        public InstallContext(string logFilePath, string[] commandLine)
        {
            this.parameters = InstallContext.ParseCommandLine(commandLine);
            if (this.Parameters["logfile"] == null && logFilePath != null)
            {
                this.Parameters["logfile"] = logFilePath;
            }
        }

        public bool IsParameterTrue(string paramName)
        {
            string text = this.Parameters[paramName.ToLower(CultureInfo.InvariantCulture)];
            if (text == null)
            {
                return false;
            }

            if (string.Compare(text, "true", StringComparison.OrdinalIgnoreCase) != 0
                && string.Compare(text, "yes", StringComparison.OrdinalIgnoreCase) != 0
                && string.Compare(text, "1", StringComparison.OrdinalIgnoreCase) != 0)
            {
                return "".Equals(text);
            }
            return true;
        }

        public void LogMessage(string message)
        {
            try
            {
                this.LogMessageHelper(message);
            }
            catch (Exception)
            {
                try
                {
                    this.Parameters["logfile"] = Path.Combine(Path.GetTempPath(), Path.GetFileName(this.Parameters["logfile"]));
                    this.LogMessageHelper(message);
                }
                catch (Exception)
                {
                    this.Parameters["logfile"] = null;
                }
            }
            if (!this.IsParameterTrue("LogToConsole") && this.Parameters["logtoconsole"] != null)
            {
                return;
            }
            Console.WriteLine(message);
        }

        internal void LogMessageHelper(string message)
        {
            StreamWriter streamWriter = null;
            try
            {
                if (!string.IsNullOrEmpty(this.Parameters["logfile"]))
                {
                    streamWriter = new StreamWriter(this.Parameters["logfile"], true, Encoding.UTF8);
                    streamWriter.WriteLine(message);
                }
            }
            finally
            {
                if (streamWriter != null)
                {
                    streamWriter.Close();
                }
            }
        }

        protected static StringDictionary ParseCommandLine(string[] args)
        {
            StringDictionary stringDictionary = new StringDictionary();
            if (args == null)
            {
                return stringDictionary;
            }
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].StartsWith("/", StringComparison.Ordinal) || args[i].StartsWith("-", StringComparison.Ordinal))
                {
                    args[i] = args[i].Substring(1);
                }
                int num = args[i].IndexOf('=');
                if (num < 0)
                {
                    stringDictionary[args[i].ToLower(CultureInfo.InvariantCulture)] = "";
                }
                else
                {
                    stringDictionary[args[i].Substring(0, num).ToLower(CultureInfo.InvariantCulture)] = args[i].Substring(num + 1);
                }
            }
            return stringDictionary;
        }
    }
}
