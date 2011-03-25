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
namespace Topshelf.Dashboard
{
	using Spark;


	public abstract class TopshelfView :
		SparkViewBase
	{
		public object Model { get; set; }
	}

	public abstract class TopshelfView<TViewData> :
		TopshelfView
	{
		public new TViewData Model { get; set; }

		public void SetModel(object model)
		{
			Model = model is TViewData ? (TViewData)model : default(TViewData);
		}
	}
}