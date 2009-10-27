// Copyright 2007-2008 The Apache Software Foundation.
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
namespace Topshelf
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Internal;
    using Internal.ArgumentParsing;

    public static class Extensions
    {
        public static T CreateInstanceAndUnwrap<T>(this AppDomain domain)
        {
            var type = typeof(T);
            return (T) domain.CreateInstanceAndUnwrap(type.Assembly.GetName().FullName, type.FullName);
        }

        public static T CreateInstanceAndUnwrap<T>(this AppDomain domain, params object[] args)
        {
            var type = typeof(T);
            return (T) domain.CreateInstanceAndUnwrap(type.Assembly.GetName().FullName, type.FullName, true, 0, null, args, null, null, null);
        }

        public static string FormatWith(this string format, params string[] args)
        {
            return string.Format(format, args);
        }

        public static void IfNotNull(this IServiceController serviceController, Action<IServiceController> action)
        {
            if (serviceController != null)
                action(serviceController);
        }

        public static TT IfNotNull<TT>(this IServiceController serviceController, Func<IServiceController, TT> action, TT ifNull)
        {
            if (serviceController != null)
                return action(serviceController);

            return ifNull;
        }

        public static string AsCommandLine(this IEnumerable<IArgument> arguments)
        {
            var keyValueArguments = arguments.Where(x => !string.IsNullOrEmpty(x.Key));

            var commandLine = keyValueArguments.Select(x => "/{0}:{1}".FormatWith(x.Key, x.Value));

            commandLine = commandLine.Union(arguments.Except(keyValueArguments).Select(x => x.Value));

            return string.Join(" ", commandLine.ToArray());
        }
    }
}