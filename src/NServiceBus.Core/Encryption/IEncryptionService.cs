namespace NServiceBus.Encryption
{
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Pipeline.OutgoingPipeline;

    /// <summary>
    /// Abstraction for encryption capabilities.
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Encrypts the given value returning an EncryptedValue.
        /// </summary>
        EncryptedValue Encrypt(string value, OutgoingLogicalMessageContext context);

        /// <summary>
        /// Decrypts the given EncryptedValue object returning the source string.
        /// </summary>
        string Decrypt(EncryptedValue encryptedValue, LogicalMessageProcessingContext context);
    }
}
