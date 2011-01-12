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
namespace Topshelf.Specs.Bottle
{
	using System;
	using Bottles;
	using Magnum.Extensions;
	using Magnum.FileSystem;
	using Stact;


	public class Bottle_Specs
	{
		public void Bob()
		{
			var n = new BottleWatcher();
			var f = new Future<Directory>();
			n.Watch(".\\watch", f.Complete);

			System.IO.File.Copy(".\\bottle\\sample.zip", ".\\watch\\sample.bottle");
			f.WaitUntilCompleted(5.Seconds());
			Directory d = f.Value;
			Console.WriteLine(d.Name.GetName());
			foreach (File file in d.GetFiles())
				Console.WriteLine(file.Name.GetPath());
		}
	}
}