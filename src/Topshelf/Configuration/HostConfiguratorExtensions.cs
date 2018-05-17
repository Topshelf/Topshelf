// Copyright 2007-2013 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using HostConfigurators;
    using HostConfigurators.AssemblyExtensions;

    public static class HostConfiguratorExtensions
    {
        public static void UseAssemblyInfoForServiceInfo(this HostConfigurator hostConfigurator, Assembly assembly)
        {
            hostConfigurator.SetDisplayName(assembly.GetAttribute<AssemblyTitleAttribute>().TryGetProperty(x => x.Title));
            hostConfigurator.SetServiceName(assembly.GetAttribute<AssemblyTitleAttribute>().TryGetProperty(x => x.Title).ToServiceNameSafeString());
            hostConfigurator.SetDescription(assembly.GetAttribute<AssemblyDescriptionAttribute>().TryGetProperty(x => x.Description));
        }

        public static void UseAssemblyInfoForServiceInfo(this HostConfigurator hostConfigurator)
        {
            hostConfigurator.UseAssemblyInfoForServiceInfo(Assembly.GetEntryAssembly());
        }
    }

    namespace HostConfigurators.AssemblyExtensions
    {
        public static class AssemblyExtentions
        {
            public static T GetAttribute<T>(this Assembly assembly) 
                where T : Attribute
            {
                return assembly.GetCustomAttributes(typeof (T), false)
                               .Cast<T>()
                               .FirstOrDefault();
            }

            public static Return TryGetProperty<Input, Return>(this Input attribute, Func<Input, Return> accessor)
                where Input : Attribute
            {
                if (attribute == null)
                    return default(Return);

                return accessor(attribute);
            }

            public static string ToServiceNameSafeString(this String input)
            {
                return input == null ? null : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input).Replace(" ", string.Empty);
            }
        }
    }
}