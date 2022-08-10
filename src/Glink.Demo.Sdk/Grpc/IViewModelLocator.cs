namespace Glink.Demo.Sdk.Grpc
{
    public interface IViewModelLocator
    {
        public void UpdateData((string, byte[]) data);
    }
}
