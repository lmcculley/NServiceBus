namespace NServiceBus
{
    using System;
    using System.Linq;
    using NServiceBus.Pipeline;

    static class RegisterStepExtensions
    {
        static Type BehaviorInterfaceType = typeof(IBehavior<,>);
        static Type ForkInterfaceType = typeof(IForkConnector<>);

        public static bool IsStageConnector(this RegisterStep step)
        {
            return typeof(IStageConnector).IsAssignableFrom(step.BehaviorType);
        }

        public static bool IsForkConnector(this RegisterStep step)
        {
            return typeof(IForkConnector).IsAssignableFrom(step.BehaviorType);
        }

        public static Type GetContextType(this Type behaviorType)
        {
            var behaviorInterface = behaviorType.GetBehaviorInterface();
            return behaviorInterface.GetGenericArguments()[0];
        }

        public static bool IsBehavior(this Type behaviorType)
        {
            return behaviorType.GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == BehaviorInterfaceType);
        }

        static Type GetBehaviorInterface(this Type behaviorType)
        {
            return behaviorType.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == BehaviorInterfaceType);
        }

        static Type GetForkInterface(this Type behaviorType)
        {
            return behaviorType.GetInterfaces()
                .First(x => x.IsGenericType && x.GetGenericTypeDefinition() == ForkInterfaceType);
        }

        public static Type GetForkContext(this RegisterStep step)
        {
            return step.BehaviorType.GetForkContext();
        }

        public static Type GetForkContext(this Type behaviorType)
        {
            var forkInterface = GetForkInterface(behaviorType);
            return forkInterface.GetGenericArguments()[0];
        }

        public static Type GetOutputContext(this RegisterStep step)
        {
            return step.BehaviorType.GetOutputContext();
        }

        public static Type GetOutputContext(this Type behaviorType)
        {
            var behaviorInterface = GetBehaviorInterface(behaviorType);
            return behaviorInterface.GetGenericArguments()[1];
        }

        public static Type GetInputContext(this RegisterStep step)
        {
            return step.BehaviorType.GetInputContext();
        }

        public static Type GetInputContext(this Type behaviorType)
        {
            var behaviorInterface = GetBehaviorInterface(behaviorType);
            return behaviorInterface.GetGenericArguments()[0];
        }

    }
}