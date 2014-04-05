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
namespace Topshelf.HostConfigurators
{
    using CommandLineParser;
    using Options;

    static class CommandLineParserOptions
    {
        internal static void AddTopshelfOptions(ICommandLineElementParser<Option> x)
        {
            int n;

            x.Add((from arg in x.Argument("install")
                   select (Option)new InstallOption())
                .Or(from arg in x.Argument("uninstall")
                    select (Option)new UninstallOption())
                .Or(from arg in x.Argument("start")
                    select (Option)new StartOption())
                .Or(from arg in x.Argument("command")
                    from cmd in x.Argument()
                    where int.TryParse(cmd.Id, out n)
                    select (Option)new CommandOption(cmd.Id))
                .Or(from arg in x.Argument("help")
                    select (Option)new HelpOption())
                .Or(from arg in x.Argument("stop")
                    select (Option)new StopOption())
                .Or(from arg in x.Switch("sudo")
                    select (Option)new SudoOption())
                .Or(from arg in x.Argument("run")
                    select (Option)new RunOption())
                .Or(from username in x.Definition("username")
                    from password in x.Definition("password")
                    select (Option)new ServiceAccountOption(username.Value, password.Value))
                .Or(from autostart in x.Switch("autostart")
                    select (Option)new AutostartOption())
                .Or(from manual in x.Switch("manual")
                    select (Option)new ManualStartOption())
                .Or(from disabled in x.Switch("disabled")
                    select (Option)new DisabledOption())
                .Or(from delayed in x.Switch("delayed")
                    select (Option)new DelayedOption())
                .Or(from interactive in x.Switch("interactive")
                    select (Option)new InteractiveOption())
                .Or(from autostart in x.Switch("localsystem")
                    select (Option)new LocalSystemOption())
                .Or(from autostart in x.Switch("localservice")
                    select (Option)new LocalServiceOption())
                .Or(from autostart in x.Switch("networkservice")
                    select (Option)new NetworkServiceOption())
                .Or(from help in x.Switch("help")
                    select (Option)new HelpOption())
                .Or(from systemHelp in x.Switch("systemonly")
                    select (Option)new SystemOnlyHelpOption())
                .Or(from name in x.Definition("servicename")
                    select (Option)new ServiceNameOption(name.Value))
                .Or(from desc in x.Definition("description")
                    select (Option)new ServiceDescriptionOption(desc.Value))
                .Or(from disp in x.Definition("displayname")
                    select (Option)new DisplayNameOption(disp.Value))
                .Or(from instance in x.Definition("instance")
                    select (Option)new InstanceOption(instance.Value)));
        }

        internal static void AddUnknownOptions(ICommandLineElementParser<Option> x)
        {
            x.Add((from unknown in x.Definition()
                   select (Option)new UnknownOption(unknown.ToString()))
                .Or(from unknown in x.Switch()
                    select (Option)new UnknownOption(unknown.ToString()))
                .Or(from unknown in x.Argument()
                    select (Option)new UnknownOption(unknown.ToString())));
        }
    }
}