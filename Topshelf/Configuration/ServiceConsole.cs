using System.Windows.Forms;

namespace Topshelf.Configuration
{
    public partial class ServiceConsole : 
        Form
    {
        public ServiceConsole()
        {
            InitializeComponent();
        }


        public void RegisterCoordinator(IServiceCoordinator coordinator)
        {
            foreach (var information in coordinator.GetServiceInfo())
            {
                string[] items = new[]
                                     {
                                         information.Name,
                                         information.Type,
                                         information.State.ToString()
                                     };
                viewServices.Items.Add(new ListViewItem(items));
            }
            
        }
    }
}
