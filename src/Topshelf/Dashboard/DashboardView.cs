namespace Topshelf.Dashboard
{
    using System.Collections.Generic;
    using Model;


    public class DashboardView
    {
        public DashboardView(IEnumerable<ServiceInfo> infos)
        {
            Statuses = infos;
        }

        public IEnumerable<ServiceInfo> Statuses { get; private set; }
    }
}