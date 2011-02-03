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