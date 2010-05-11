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
namespace Topshelf.Shelving
{
    using System;
    using System.Runtime.Remoting;

    public class ShelfMaker
    {
        public void MakeShelf(string name)
        {
            AppDomainSetup settings = AppDomain.CurrentDomain.SetupInformation;
            settings.ShadowCopyFiles = "true";
            AppDomain ad = AppDomain.CreateDomain(name, null, settings);
            Type type = typeof (Shelf);
            ObjectHandle s = ad.CreateInstance(type.Assembly.GetName().FullName, type.FullName, true, 0, null, null,
                                               null, null);
        }
    }
}