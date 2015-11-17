namespace NServiceBus.Core.Tests.Encryption
{
    using NUnit.Framework;

    [TestFixture]
    public class When_sending_a_message_with_2x_compatibility_disabled : WireEncryptedStringContext
    {
        [Test]
        public void Should_clear_the_compatibility_properties()
        {
            var message = new Customer
            {
                Secret = MySecretMessage
            };
            inspector.MutateOutgoing(message);

            Assert.AreEqual(message.Secret.EncryptedValue.EncryptedBase64Value, EncryptedBase64Value);
        }
    }
}