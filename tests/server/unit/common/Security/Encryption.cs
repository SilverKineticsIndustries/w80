using System.Text;

namespace SilverKinetics.w80.Common.UnitTests.Security;

[TestFixture(TestOf = typeof(Common.Security.Encryption))]
public class Encryption
{
    [Test]
    public void Encrypt_encryptString_verifyEncryptedStringIsNotPlaintext()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var key = "encryption-key";
            var toEncrypt = "silverkinetics";
            var encrypted = Common.Security.Encryption.Encrypt(Encoding.Unicode.GetBytes(toEncrypt), key);
            var encryptedString = Encoding.Unicode.GetString(encrypted);
            Assert.That(toEncrypt, Is.Not.EqualTo(encryptedString));
        }
    }

    [Test]
    public void Encrypt_encryptStringWithEmptyKey_exceptionShouldBeThrown()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var key = "";
            var toEncrypt = "silverkinetics";
            Assert.Throws<ArgumentNullException>(() => {
                Common.Security.Encryption.Encrypt(Encoding.Unicode.GetBytes(toEncrypt), key);
            });
        }
    }

    [Test]
    public void Encrypt_encryptStringMultipleTimes_verifyThatIVisDiffentEachTime()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var key = "encryption-key";
            var toEncrypt = "silverkinetics";

            var encrypted1 = Common.Security.Encryption.Encrypt(Encoding.Unicode.GetBytes(toEncrypt), key);
            var encrypted2 = Common.Security.Encryption.Encrypt(Encoding.Unicode.GetBytes(toEncrypt), key);
            var encrypted3 = Common.Security.Encryption.Encrypt(Encoding.Unicode.GetBytes(toEncrypt), key);

            var iv1 = encrypted1.Take(16).ToArray();
            var iv2 = encrypted2.Take(16).ToArray();
            var iv3 = encrypted3.Take(16).ToArray();

            Assert.That(iv1.SequenceEqual(iv2), Is.EqualTo(false));
            Assert.That(iv1.SequenceEqual(iv3), Is.EqualTo(false));
            Assert.That(iv2.SequenceEqual(iv3), Is.EqualTo(false));
        }
    }

    [Test]
    public void Decrypt_decryptString_verifyStringDecrypted()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var key = "encryption-key";
            var toEncrypt = "silverkinetics";
            var encrypted = Common.Security.Encryption.Encrypt(Encoding.Unicode.GetBytes(toEncrypt), key);
            var decrypted = Common.Security.Encryption.Decrypt(encrypted, key);
            var decryptedString = Encoding.Unicode.GetString(decrypted);
            Assert.That(toEncrypt, Is.EqualTo(decryptedString));
        }
    }

    [Test]
    public void Decrypt_decryptStringWithEmptyKey_exceptionShouldBeThrown()
    {
        using(var ctx = TestContextFactory.Create())
        {
            var key = "encryption-key";
            var toEncrypt = "silverkinetics";
            var encrypted = Common.Security.Encryption.Encrypt(Encoding.Unicode.GetBytes(toEncrypt), key);
            key = "";
            Assert.Throws<ArgumentNullException>(() => {
                Common.Security.Encryption.Decrypt(encrypted, key);
            });
        }
    }
}