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
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Builders;
    using Configurators;

    public class PrefixHelpTextHostConfigurator :
        HostBuilderConfigurator
    {
        public PrefixHelpTextHostConfigurator(Assembly assembly, string resourceName)
        {
            this.Assembly = assembly;
            this.ResourceName = resourceName;
        }

        public PrefixHelpTextHostConfigurator(string text)
        {
            this.Text = text;
        }

        public Assembly Assembly { get; private set; }
        public string ResourceName { get; private set; }
        public string Text { get; private set; }

        public IEnumerable<ValidateResult> Validate()
        {
            ValidateResult loadResult = null;
            if (this.Assembly != null)
            {
                if (this.ResourceName == null)
                    yield return this.Failure("A resource name must be specified");

                try
                {
                    Stream stream = this.Assembly.GetManifestResourceStream(this.ResourceName);
                    if (stream == null)
                        loadResult = this.Failure("Resource", "Unable to load resource stream: " + this.ResourceName);
                    else
                    {
                        using (TextReader reader = new StreamReader(stream))
                        {
                            this.Text = reader.ReadToEnd();
                        }
                    }
                }
                catch (Exception ex)
                {
                    loadResult = this.Failure("Failed to load help source: " + ex.Message);
                }

                if (loadResult != null)
                    yield return loadResult;
            }

            if (this.Text == null)
                yield return this.Failure("No additional help text was specified");
        }

        public HostBuilder Configure(HostBuilder builder)
        {
            builder.Match<HelpBuilder>(x => x.SetAdditionalHelpText(this.Text));

            return builder;
        }
    }
}