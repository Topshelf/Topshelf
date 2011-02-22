// Copyright 2007-2011 The Apache Software Foundation.
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
namespace Topshelf.Specs.Config
{
	using Magnum.TestFramework;
	using NUnit.Framework;


	[Scenario]
	public class Setting_service_recovery_options
	{
        [Ignore("Failing")]
        [Then]
		public void Should_be_fluent_in_the_dsl()
		{
			Assert.Throws<HostConfigurationException>(() =>
				{
					HostFactory.Run(x =>
						{
							x.SetServiceName("bob");

							x.UseServiceRecovery(r =>
								{
									r.OnFirstFailure()
										.RestartService()
										.Wait(1);

									r.OnSecondFailure()
										.RestartService()
										.Wait(5);

									r.OnSubsequentFailures()
										.RunProgram("notepad.exe")
										.WithParameters(@"c:\autoexec.bat");

									r.OnSubsequentFailures()
										.RestartComputer()
										.DisplayMessage("OMG LOLZ FIRE!")
										.Wait(2);

									r.ResetFailureCountAfter(1);
								});
						});
				});
		}
	}
}