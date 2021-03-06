namespace NServiceBus.Config
{
    using System;
    using System.Configuration;

    /// <summary>
    /// A configuration section for UnicastBus specific settings.
    /// </summary>
    public class UnicastBusConfig : ConfigurationSection
    {
        /// <summary>
        /// Gets/sets the address for sending control messages to the distributor.
        /// </summary>
        [ConfigurationProperty("DistributorControlAddress", IsRequired = false)]
        [ObsoleteEx(
            TreatAsErrorFromVersion = "6",
            RemoveInVersion = "7",
            Message = "Please switch to the code first API by using 'EndpointConfiguration.EnlistWithLegacyMSMQDistributor' instead.")]
        public string DistributorControlAddress
        {
            get
            {
                var result = this["DistributorControlAddress"] as string;
                if (string.IsNullOrWhiteSpace(result))
                    result = null;

                return result;
            }
            set
            {
                this["DistributorControlAddress"] = value;
            }
        }

        /// <summary>
        /// Gets/sets the distributor's data address - used as the return address of messages sent by this endpoint.
        /// </summary>
        [ConfigurationProperty("DistributorDataAddress", IsRequired = false)]
        [ObsoleteEx(
            TreatAsErrorFromVersion = "6",
            RemoveInVersion = "7",
            Message = "Please switch to the code first API by using 'EndpointConfiguration.EnlistWithLegacyMSMQDistributor' instead.")]
        public string DistributorDataAddress
        {
            get
            {
                var result = this["DistributorDataAddress"] as string;
                if (string.IsNullOrWhiteSpace(result))
                    result = null;

                return result;
            }
            set
            {
                this["DistributorDataAddress"] = value;
            }
        }

        /// <summary>
        /// Gets/sets the address to which messages received will be forwarded.
        /// </summary>
        [ConfigurationProperty("ForwardReceivedMessagesTo", IsRequired = false)]
        [ObsoleteEx(
            TreatAsErrorFromVersion = "6",
            RemoveInVersion = "7",
            Message = "Please use 'EndpointConfiguration.ForwardReceivedMessagesTo' to configure the forwarding address.")]
        public string ForwardReceivedMessagesTo
        {
            get
            {
                var result = this["ForwardReceivedMessagesTo"] as string;
                if (string.IsNullOrWhiteSpace(result))
                    result = null;

                return result;
            }
            set
            {
                this["ForwardReceivedMessagesTo"] = value;
            }
        }


        /// <summary>
        /// Gets/sets the time to be received set on forwarded messages.
        /// </summary>
        [ConfigurationProperty("TimeToBeReceivedOnForwardedMessages", IsRequired = false)]
        public TimeSpan TimeToBeReceivedOnForwardedMessages
        {
            get
            {
                return (TimeSpan)this["TimeToBeReceivedOnForwardedMessages"];
            }
            set
            {
                this["TimeToBeReceivedOnForwardedMessages"] = value;
            }
        }

        /// <summary>
        /// Gets/sets the address that the timeout manager will use to send and receive messages.
        /// </summary>
        [ConfigurationProperty("TimeoutManagerAddress", IsRequired = false)]
        public string TimeoutManagerAddress
        {
            get
            {
                var result = this["TimeoutManagerAddress"] as string;
                if (string.IsNullOrWhiteSpace(result))
                    result = null;

                return result;
            }
            set
            {
                this["TimeoutManagerAddress"] = value;
            }
        }

        /// <summary>
        /// Contains the mappings from message types (or groups of them) to endpoints.
        /// </summary>
        [ConfigurationProperty("MessageEndpointMappings", IsRequired = false)]
        public MessageEndpointMappingCollection MessageEndpointMappings
        {
            get
            {
                return this["MessageEndpointMappings"] as MessageEndpointMappingCollection;
            }
            set
            {
                this["MessageEndpointMappings"] = value;
            }
        }
    }
}
