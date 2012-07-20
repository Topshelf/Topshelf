// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Topshelf.Configurators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    [Serializable, DebuggerDisplay("{DebuggerString()}")]
    public class ConfigurationResultImpl :
        ConfigurationResult
    {
        readonly IList<ValidateResult> _results;

        ConfigurationResultImpl(IEnumerable<ValidateResult> results)
        {
            _results = results.ToList();
        }

        public bool ContainsFailure
        {
            get { return _results.Any(x => x.Disposition == ValidationResultDisposition.Failure); }
        }

        public IEnumerable<ValidateResult> Results
        {
            get { return _results; }
        }

        string Message()
        {
#if NET40
            var debuggerString = string.Join(", ", _results);
#else
            string debuggerString = string.Join(", ", _results.Select(x => x.ToString()).ToArray());
#endif

#if NET40
            return string.IsNullOrWhiteSpace(debuggerString)
#else
            return string.IsNullOrEmpty(debuggerString)
#endif
                       ? "No Obvious Problems says ConfigurationResult"
                       : debuggerString;
        }

        public static ConfigurationResult CompileResults(IEnumerable<ValidateResult> results)
        {
            var result = new ConfigurationResultImpl(results);

            if (result.ContainsFailure)
            {
                string message = "The service bus was not properly configured: " + result.Message();

                throw new HostConfigurationException(message);
            }

            return result;
        }
    }
}