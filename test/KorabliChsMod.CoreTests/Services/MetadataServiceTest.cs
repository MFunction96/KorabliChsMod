using System;

namespace Xanadu.Test.KorabliChsMod.Core.Services
{
    /// <summary>
    /// 元信息获取实现，Transient 生命周期
    /// </summary>
    [TestClass]
    public class MetadataServiceTest
    {
        private IServiceProvider _serviceProvider = null!;

        [TestInitialize]
        public void Setup()
        {
            
        }

        [TestCleanup]
        public void Cleanup()
        {
            
        }
    }
}
