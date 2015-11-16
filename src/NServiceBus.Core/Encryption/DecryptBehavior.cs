namespace NServiceBus
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Encryption;
    using Pipeline;
    using Pipeline.Contexts;
    using Unicast.Transport;

    class DecryptBehavior : Behavior<LogicalMessageProcessingContext>
    {
        EncryptionInspector messageInspector;
        IEncryptionService encryptionService;
        LogicalMessageProcessingContext context;

        public DecryptBehavior(EncryptionInspector messageInspector, IEncryptionService encryptionService)
        {
            this.messageInspector = messageInspector;
            this.encryptionService = encryptionService;
        }
        public override async Task Invoke(LogicalMessageProcessingContext context, Func<Task> next)
        {
            if (TransportMessageExtensions.IsControlMessage(context.Headers))
            {
                await next().ConfigureAwait(false);
                return;
            }

            this.context = context;

            var current = context.Message.Instance;

            messageInspector.ForEachMember(
                current,
                DecryptMember
                );

            context.Message.UpdateMessageInstance(current);

            await next().ConfigureAwait(false);
        }


        void DecryptMember(object target, MemberInfo property)
        {
            var encryptedValue = property.GetValue(target);

            if (encryptedValue == null)
            {
                return;
            }

            var wireEncryptedString = encryptedValue as WireEncryptedString;
            if (wireEncryptedString != null)
            {
                Decrypt(wireEncryptedString);
            }
            else
            {
                property.SetValue(target, DecryptUserSpecifiedProperty(encryptedValue));
            }
        }

        string DecryptUserSpecifiedProperty(object encryptedValue)
        {
            var stringToDecrypt = encryptedValue as string;

            if (stringToDecrypt == null)
            {
                throw new Exception("Only string properties is supported for convention based encryption, please check your convention");
            }

            var parts = stringToDecrypt.Split(new[] { '@' }, StringSplitOptions.None);

            return Decrypt(new EncryptedValue
            {
                EncryptedBase64Value = parts[0],
                Base64Iv = parts[1]
            });
        }

        void Decrypt(WireEncryptedString encryptedValue)
        {
            if (encryptedValue.EncryptedValue == null)
            {
                throw new Exception("Encrypted property is missing encryption data");
            }

            encryptedValue.Value = Decrypt(encryptedValue.EncryptedValue);
        }

        string Decrypt(EncryptedValue value)
        {
            return encryptionService.Decrypt(value, context);
        }

        public class DecryptRegistration : RegisterStep
        {
            public DecryptRegistration()
                : base("InvokeDecryption", typeof(DecryptBehavior), "Invokes the decryption logic")
            {
                InsertBefore(WellKnownStep.MutateIncomingMessages);
            }
        }
    }
}