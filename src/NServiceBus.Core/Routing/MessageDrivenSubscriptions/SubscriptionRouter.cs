﻿namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Routing;
    using NServiceBus.Routing.MessageDrivenSubscriptions;
    using NServiceBus.Transports;

    class SubscriptionRouter
    {
        public SubscriptionRouter(Publishers publishers, EndpointInstances endpointInstances, TransportAddresses physicalAddresses)
        {
            this.publishers = publishers;
            this.endpointInstances = endpointInstances;
            this.physicalAddresses = physicalAddresses;
        }

        public async Task<IEnumerable<string>> GetAddressesForEventType(Type messageType)
        {
            var results = new List<string>();
            foreach (var publisherAddress in publishers.GetPublisherFor(messageType))
            {
                results.AddRange(await publisherAddress.Resolve(
                    ResolveInstances, 
                    i => physicalAddresses.GetTransportAddress(new LogicalAddress(i))).ConfigureAwait(false));
            }
            return results;
        }

        Task<IEnumerable<EndpointInstance>> ResolveInstances(EndpointName endpoint)
        {
            return endpointInstances.FindInstances(endpoint);
        }

        Publishers publishers;
        EndpointInstances endpointInstances;
        TransportAddresses physicalAddresses;
    }
}