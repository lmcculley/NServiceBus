namespace NServiceBus
{
    using System;
    using NServiceBus.Encryption;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Pipeline.OutgoingPipeline;

    static class WireEncryptedStringConversions
    {
        public static bool IsType(object instance)
        {
            return instance is WireEncryptedString;
        }

        public static void Encrypt(this IEncryptionService encryptionService, WireEncryptedString wireEncryptedString, OutgoingLogicalMessageContext context)
        {
            wireEncryptedString.EncryptedValue = encryptionService.Encrypt(wireEncryptedString.Value, context);
            wireEncryptedString.Value = null;
        }

        public static void Decrypt(this IEncryptionService encryptionService, WireEncryptedString wireEncryptedString, LogicalMessageProcessingContext context)
        {
            if (wireEncryptedString.EncryptedValue == null)
            {
                throw new Exception("Encrypted property is missing encryption data");
            }

            wireEncryptedString.Value = encryptionService.Decrypt(wireEncryptedString.EncryptedValue, context);
        }
    }
}