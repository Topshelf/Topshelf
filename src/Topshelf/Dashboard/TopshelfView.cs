namespace Topshelf.Dashboard
{
    using System;
    using Spark;

    public abstract class TopshelfView :
        SparkViewBase
    {
        public object Model { get; set; }
    }

//    public abstract class TopshelfView<TViewData> :
//        TopshelfView
//    {
//        public override void  SetModel(object data)
//        {
//            Model = (TViewData)data;
//        }
//        public TViewData Model { get; set; }
//    }
}