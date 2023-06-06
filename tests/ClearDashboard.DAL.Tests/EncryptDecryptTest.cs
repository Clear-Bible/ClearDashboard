using System.Threading.Tasks;
using ClearDashboard.DataAccessLayer.Models;
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
            var encrypted = Encryption.Encrypt(original);
            var decrypted = Encryption.Decrypt(encrypted);

            Assert.Equal(original, decrypted);
        }
    }
}
