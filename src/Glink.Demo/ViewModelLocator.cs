using Glink.Demo.Sdk.Grpc;

namespace Glink.Demo
{
    public  class ViewModelLocator : IViewModelLocator
    {
        public static MainViewModel Main { get; private set; } = new MainViewModel();

        public void UpdateData((string, byte[]) data) => Main.UpdateData(data);
    }
}
