namespace NServiceBus
{
    using NServiceBus.Pipeline;

    interface IForkConnector
    {   
    }

    // ReSharper disable once UnusedTypeParameter
    interface IForkConnector<TFork> : IForkConnector
        where TFork : IBehaviorContext
    {
    }
}