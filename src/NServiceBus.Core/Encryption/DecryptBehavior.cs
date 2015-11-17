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

        public DecryptBehavior(EncryptionInspector messageInspector, IEncryptionService encryptionService)
        {
            this.messageInspector = messageInspector;
            this.encryptionService = encryptionService;
        }

        public override async Task Invoke(LogicalMessageProcessingContext context, Func<Task> next)
        {
            var current = context.Message.Instance;

            foreach (var item in messageInspector.ScanObject(current))
            {
                DecryptMember(item.Item1, item.Item2, context);
            }

            context.Message.UpdateMessageInstance(current);

            await next().ConfigureAwait(false);
        }


        void DecryptMember(object target, MemberInfo property, LogicalMessageProcessingContext context)
        {
            var encryptedValue = property.GetValue(target);

            var wireEncryptedString = encryptedValue as WireEncryptedString;
            if (wireEncryptedString != null)
            {
                if (wireEncryptedString.EncryptedValue == null)
                {
                    throw new Exception("Encrypted property is missing encryption data");
                }

                wireEncryptedString.Value = encryptionService.Decrypt(wireEncryptedString.EncryptedValue, context);
            }

            var stringToDecrypt = encryptedValue as string;
            if (stringToDecrypt != null)
            {
                var parts = stringToDecrypt.Split(new[] { '@' }, StringSplitOptions.None);

                var result = encryptionService.Decrypt(new EncryptedValue
                {
                    EncryptedBase64Value = parts[0],
                    Base64Iv = parts[1]
                },
                context
                );

                property.SetValue(target, result);
            }

            throw new Exception("Only string properties is supported for convention based encryption, please check your convention");
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