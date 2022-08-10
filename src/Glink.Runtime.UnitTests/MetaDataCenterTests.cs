using Glink.Runtime.DataCenter;
using Glink.Runtime.DataCenter.MetaData;
using NUnit.Framework;

namespace Glink.Runtime.UnitTests
{
    [TestFixture]
    public class MetaDataCenterTests
    {
        [Test]
        public async Task ProductTest_empty()
        {
            var center = new MetaDataCenter();
            var producer = new DataProducer<MetaDataCenter>(center);
            Assert.IsNotNull(center);

            await producer.Produce(default);
        }
    }
}
