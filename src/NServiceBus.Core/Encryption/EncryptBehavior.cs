namespace NServiceBus
{
    using System;
    using System.Reflection;
    using System.Threading.Tasks;
    using Encryption;
    using Pipeline;
    using Pipeline.Contexts;

    class EncryptBehavior : Behavior<OutgoingLogicalMessageContext>
    {
        EncryptionInspector messageInspector;
        IEncryptionService encryptionService;

        public EncryptBehavior(EncryptionInspector messageInspector, IEncryptionService encryptionService)
        {
            this.messageInspector = messageInspector;
            this.encryptionService = encryptionService;
        }

        public override Task Invoke(OutgoingLogicalMessageContext context, Func<Task> next)
        {
            var currentMessageToSend = context.Message.Instance;

            messageInspector.ForEachMember(
                currentMessageToSend,
                (a, b) => EncryptMember(a, b, context)
                );

            context.UpdateMessageInstance(currentMessageToSend);

            return next();
        }

        void EncryptMember(object message, MemberInfo member, OutgoingLogicalMessageContext context)
        {
            var valueToEncrypt = member.GetValue(message);

            var wireEncryptedString = valueToEncrypt as WireEncryptedString;
            if (wireEncryptedString != null)
            {
                wireEncryptedString.EncryptedValue = encryptionService.Encrypt(wireEncryptedString.Value, context);
                wireEncryptedString.Value = null;
                return;
            }

            var stringToEncrypt = valueToEncrypt as string;
            if (stringToEncrypt != null)
            {
                var encryptedValue = encryptionService.Encrypt(stringToEncrypt, context);

                var result = $"{encryptedValue.EncryptedBase64Value}@{encryptedValue.Base64Iv}";

                member.SetValue(message, result);
                return;
            }

            throw new Exception("Only string properties is supported for convention based encryption, please check your convention");
        }

        public class EncryptRegistration : RegisterStep
        {
            public EncryptRegistration()
                : base("InvokeEncryption", typeof(EncryptBehavior), "Invokes the encryption logic")
            {
                InsertAfter(WellKnownStep.MutateOutgoingMessages);
            }

        }
    }
}