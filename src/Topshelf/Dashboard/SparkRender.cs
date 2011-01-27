namespace Topshelf.Dashboard
{
    using System.IO;
    using System.Text;
    using Spark;
    using Spark.FileSystem;


    public class SparkRender
    {
        readonly EmbeddedViewFolder _viewFolder;
        readonly SparkViewEngine _engine;

        public SparkRender()
        {
            _viewFolder = new EmbeddedViewFolder(typeof(SparkRender).Assembly, "Topshelf.Dashboard.views");

            _engine = new SparkViewEngine
                {
                    DefaultPageBaseType = typeof(TopshelfView).FullName,
                    ViewFolder = _viewFolder
                };
        }

        public string Render<TViewData>(string template, TViewData data)
        {
            var instance = _engine.CreateInstance(new SparkViewDescriptor().AddTemplate(template));

            var view = (TopshelfView)instance;
            view.Model = data;

            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            {
                view.RenderView(writer);
            }

            return sb.ToString();
        }
    }
}