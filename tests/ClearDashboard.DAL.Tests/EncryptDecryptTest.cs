using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ClearDashboard.DAL.Tests
{
    public class EncryptDecryptTest : TestBase
    {
        public EncryptDecryptTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void EncryptAndDecryptTest()
        {
            var original = "Text to Hide By Encryption";
            var encrypted = DataAccessLayer.Encryption.Encrypt(original);
            var decrypted = DataAccessLayer.Encryption.Decrypt(encrypted);

            Assert.Equal(original, decrypted);
        }
    }
}
