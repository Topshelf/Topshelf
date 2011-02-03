namespace Topshelf.Dashboard
{
    using Stact.ServerFramework;


    public static class ResponseContextExtensions
    {

        public static void RenderSparkView<TViewData>(this ResponseContext cxt, TViewData data, string template)
        {
            var render = new SparkRender();
            string output = render.Render(template, data);
            cxt.WriteHtml(output);
        }
    }
}