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
        EncryptionPropertyInspector messagePropertyInspector;
        IEncryptionService encryptionService;
        OutgoingLogicalMessageContext context;

        public EncryptBehavior(EncryptionPropertyInspector messagePropertyInspector, IEncryptionService encryptionService)
        {
            this.messagePropertyInspector = messagePropertyInspector;
            this.encryptionService = encryptionService;
        }

        public override Task Invoke(OutgoingLogicalMessageContext context, Func<Task> next)
        {
            var currentMessageToSend = context.Message.Instance;

            this.context = context;

            messagePropertyInspector.ForEachMember(
                currentMessageToSend,
                EncryptMember
                );

            context.UpdateMessageInstance(currentMessageToSend);

            return next();
        }

        string EncryptUserSpecifiedProperty(object valueToEncrypt)
        {
            var stringToEncrypt = valueToEncrypt as string;

            if (stringToEncrypt == null)
            {
                throw new Exception("Only string properties is supported for convention based encryption, please check your convention");
            }

            var encryptedValue = Encrypt(stringToEncrypt);

            return $"{encryptedValue.EncryptedBase64Value}@{encryptedValue.Base64Iv}";
        }

        void EncryptWireEncryptedString(WireEncryptedString wireEncryptedString)
        {
            wireEncryptedString.EncryptedValue = Encrypt(wireEncryptedString.Value);
            wireEncryptedString.Value = null;
        }

        void EncryptMember(object target, MemberInfo member)
        {
            var valueToEncrypt = member.GetValue(target);

            if (valueToEncrypt == null)
            {
                return;
            }

            var wireEncryptedString = valueToEncrypt as WireEncryptedString;
            if (wireEncryptedString != null)
            {
                var encryptedString = wireEncryptedString;
                EncryptWireEncryptedString(encryptedString);
            }
            else
            {
                member.SetValue(target, EncryptUserSpecifiedProperty(valueToEncrypt));
            }
        }

        EncryptedValue Encrypt(string value)
        {
            return encryptionService.Encrypt(value, context);
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