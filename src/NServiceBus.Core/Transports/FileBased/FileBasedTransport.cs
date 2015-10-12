﻿namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using NServiceBus.Performance.TimeToBeReceived;
    using Settings;
    using Transports;
    using Transports.FileBased;

    /// <summary>
    /// A file based transport.
    /// </summary>
    public class FileBasedTransport : TransportDefinition
    {
        /// <summary>
        /// Gives implementations access to the <see cref="BusConfiguration"/> instance at configuration time.
        /// </summary>
        protected internal override void Configure(BusConfiguration config)
        {
            config.EnableFeature<FileBasedTransportConfigurator>();
        }


        /// <summary>
        /// Returns the discriminator for this endpoint instance.
        /// </summary>
        public override string GetDiscriminatorForThisEndpointInstance()
        {
            return "\\";
        }

        /// <summary>
        /// Gets the supported transactionallity for this transport.
        /// </summary>
        public override TransactionSupport GetTransactionSupport() => TransactionSupport.MultiQueue;

        /// <summary>
        /// Will be called if the transport has indicated that it has native support for pub sub.
        /// Creates a transport address for the input queue defined by a logical address.
        /// </summary>
        public override IManageSubscriptions GetSubscriptionManager()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the list of supported delivery constraints for this transport.
        /// </summary>
        public override IEnumerable<Type> GetSupportedDeliveryConstraints()
        {
            return new List<Type> { typeof(DiscardIfNotReceivedBefore) };
        }

        /// <summary>
        /// Converts a given logical address to the transport address.
        /// </summary>
        /// <param name="logicalAddress">The logical address.</param>
        /// <returns>The transport address.</returns>
        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            return Path.Combine(logicalAddress.EndpointInstanceName.EndpointName.ToString(), logicalAddress.Qualifier ?? "");
        }

        /// <summary>
        /// Returns the outbound routing policy selected for the transport.
        /// </summary>
        public override OutboundRoutingPolicy GetOutboundRoutingPolicy(ReadOnlySettings settings)
        {
            return new OutboundRoutingPolicy(OutboundRoutingType.DirectSend, OutboundRoutingType.DirectSend, OutboundRoutingType.DirectSend);
        }
    }
}