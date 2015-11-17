namespace NServiceBus.Core.Tests.Encryption
{
    using System;
    using NUnit.Framework;

    [TestFixture]
    public class When_decrypting_a_member_that_is_missing_encryption_data : WireEncryptedStringContext
    {
        [Test]
        public void Should_throw_an_exception()
        {

            var message = new MessageWithMissingData
            {
                Secret = new WireEncryptedString { Value = "The real value" }
            };

            var exception = Assert.Throws<Exception>(() => inspector.MutateIncoming(message));
            Assert.AreEqual("Encrypted property is missing encryption data", exception.Message);
        }
    }
}