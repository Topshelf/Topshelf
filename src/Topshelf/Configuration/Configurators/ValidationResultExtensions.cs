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
    public static class ValidationResultExtensions
    {
        public static ValidateResult Failure(this Configurator configurator, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Failure, message);
        }

        public static ValidateResult Failure(this Configurator configurator, string key, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Failure, key, message);
        }

        public static ValidateResult Failure(this Configurator configurator, string key, string value, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Failure, key, value, message);
        }

        public static ValidateResult Warning(this Configurator configurator, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Warning, message);
        }

        public static ValidateResult Warning(this Configurator configurator, string key, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Warning, key, message);
        }

        public static ValidateResult Warning(this Configurator configurator, string key, string value, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Warning, key, value, message);
        }

        public static ValidateResult Success(this Configurator configurator, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Success, message);
        }

        public static ValidateResult Success(this Configurator configurator, string key, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Success, key, message);
        }

        public static ValidateResult Success(this Configurator configurator, string key, string value, string message)
        {
            return new ValidateResultImpl(ValidationResultDisposition.Success, key, value, message);
        }

        public static ValidateResult WithParentKey(this ValidateResult result, string parentKey)
        {
            string key = parentKey + "." + result.Key;

            return new ValidateResultImpl(result.Disposition, key, result.Value, result.Message);
        }
    }
}