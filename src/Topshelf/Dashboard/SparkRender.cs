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

        public SparkRender()
        {
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

            var instance = engine.CreateInstance(new SparkViewDescriptor().AddTemplate(template));

            var view = (TopshelfView<TViewData>)instance;
            view.SetModel(data);

            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            {
                view.RenderView(writer);
            }

            return sb.ToString();
        }
    }
}