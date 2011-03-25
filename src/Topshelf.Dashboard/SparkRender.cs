﻿// Copyright 2007-2011 The Apache Software Foundation.
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
	using System.IO;
	using System.Text;
	using Spark;
	using Spark.FileSystem;


	public class SparkRender
	{
		static readonly EmbeddedViewFolder _viewFolder;

		static SparkRender()
		{
			_viewFolder = new EmbeddedViewFolder(typeof(SparkRender).Assembly, "Topshelf.Dashboard.views");
		}

		public string Render<TViewData>(string template, TViewData data)
		{
			var settings = new SparkSettings();
			settings.AddNamespace("Topshelf.Dashboard");
			settings.PageBaseType = typeof(TopshelfView).FullName;

			var engine = new SparkViewEngine(settings)
				{
					ViewFolder = _viewFolder,
				};

			ISparkView instance = engine.CreateInstance(new SparkViewDescriptor().AddTemplate(template));

			var view = (TopshelfView<TViewData>)instance;
			view.SetModel(data);

			var sb = new StringBuilder();

			using (var writer = new StringWriter(sb))
				view.RenderView(writer);

			return sb.ToString();
		}
	}
}