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
namespace Topshelf.WindowsServiceCode
{
	public class ServiceRecoveryOptions
	{
		public ServiceRecoveryOptions()
		{
			FirstFailureAction = ServiceRecoveryAction.TakeNoAction;
			SecondFailureAction = ServiceRecoveryAction.TakeNoAction;
			SubsequentFailureActions = ServiceRecoveryAction.TakeNoAction;
			DaysToResetFailAcount = 0;
			MinutesToRestartService = 1;
		}

		public ServiceRecoveryAction FirstFailureAction { get; set; }
		public ServiceRecoveryAction SecondFailureAction { get; set; }
		public ServiceRecoveryAction SubsequentFailureActions { get; set; }
		public int DaysToResetFailAcount { get; set; }
		public int MinutesToRestartService { get; set; }
		public string RebootMessage { get; set; }
		public string CommandToLaunchOnFailure { get; set; }

		public void Validate()
		{
		}

		public override bool Equals(object other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			if (other.GetType() != typeof(ServiceRecoveryOptions))
				return false;
			return Equals((ServiceRecoveryOptions)other);
		}

		public bool Equals(ServiceRecoveryOptions other)
		{
			if (ReferenceEquals(null, other))
				return false;
			if (ReferenceEquals(this, other))
				return true;
			return Equals(other.FirstFailureAction, FirstFailureAction) && Equals(other.SecondFailureAction, SecondFailureAction)
			       && Equals(other.SubsequentFailureActions, SubsequentFailureActions)
			       && other.DaysToResetFailAcount == DaysToResetFailAcount
			       && other.MinutesToRestartService == MinutesToRestartService && Equals(other.RebootMessage, RebootMessage)
			       && Equals(other.CommandToLaunchOnFailure, CommandToLaunchOnFailure);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = FirstFailureAction.GetHashCode();
				result = (result*397) ^ SecondFailureAction.GetHashCode();
				result = (result*397) ^ SubsequentFailureActions.GetHashCode();
				result = (result*397) ^ DaysToResetFailAcount;
				result = (result*397) ^ MinutesToRestartService;
				result = (result*397) ^ (RebootMessage != null ? RebootMessage.GetHashCode() : 0);
				result = (result*397) ^ (CommandToLaunchOnFailure != null ? CommandToLaunchOnFailure.GetHashCode() : 0);
				return result;
			}
		}
	}
}