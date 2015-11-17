namespace NServiceBus
{
    using System;
    using NServiceBus.Encryption;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Pipeline.OutgoingPipeline;

    static class EncryptionServiceConversions
    {
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

        public static void Encrypt(this IEncryptionService encryptionService, ref string stringToEncrypt, OutgoingLogicalMessageContext context)
        {
            var encryptedValue = encryptionService.Encrypt(stringToEncrypt, context);

            stringToEncrypt = $"{encryptedValue.EncryptedBase64Value}@{encryptedValue.Base64Iv}";
        }

        public static void Decrypt(this IEncryptionService encryptionService, ref string stringToDecrypt, LogicalMessageProcessingContext context)
        {
            var parts = stringToDecrypt.Split(new[] { '@' }, StringSplitOptions.None);

            stringToDecrypt = encryptionService.Decrypt(new EncryptedValue
            {
                EncryptedBase64Value = parts[0],
                Base64Iv = parts[1]
            },
                context
                );
        }
    }
}