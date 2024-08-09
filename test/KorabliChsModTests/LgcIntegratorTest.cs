using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Xanadu.KorabliChsMod.DI;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

namespace Xanadu.Test.KorabliChsMod
{
    [TestClass]
    public class LgcIntegratorTest
    {
        private readonly Mock<ILogger<LgcIntegrator>> _mockLogger = new();

        public TestContext TestContext { get; set; }

        [TestMethod]
        public void LgcTest()
        {
            var lgc = new LgcIntegrator(_mockLogger.Object);
            lgc.Load();
            this.TestContext.WriteLine(JsonConvert.SerializeObject(lgc));
        }
    }
}
