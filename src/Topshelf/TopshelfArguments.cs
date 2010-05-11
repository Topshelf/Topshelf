// Copyright 2007-2010 The Apache Software Foundation.
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
    public class TopshelfArguments
    {
        // ts  -- runs as console | service (auto detected)
        // ts install -- installs to SCM
        // ts install /instance=bob
        // ts uninstall -- unistalls from SCM
        // ts uninstall /instance=bob

        public string Instance { get; set; }
        public ServiceActions Action { get; set; }
    }
}