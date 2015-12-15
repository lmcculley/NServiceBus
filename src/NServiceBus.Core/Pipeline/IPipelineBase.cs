namespace NServiceBus
{
    using System.Threading.Tasks;


    interface IPipelineBase<T> : IPipeline
    {
        Task Invoke(T context);
    }
}