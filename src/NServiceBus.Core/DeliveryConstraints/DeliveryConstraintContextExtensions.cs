namespace NServiceBus.DeliveryConstraints
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.Extensibility;
    using NServiceBus.Features;
    using NServiceBus.Transports;

    /// <summary>
    /// Gives access to <see cref="DeliveryConstraint"/>s that exist in the various <see cref="ContextBag"/>s.
    /// </summary>
    public static class DeliveryConstraintContextExtensions
    {
        /// <summary>
        /// Adds a <see cref="DeliveryConstraint"/> to a <see cref="ContextBag"/>.
        /// </summary>
        public static void AddDeliveryConstraint(this ContextBag context, DeliveryConstraint constraint)
        {
            List<DeliveryConstraint> constraints;

            if (!context.TryGet(out constraints))
            {
                constraints = new List<DeliveryConstraint>();

                context.Set(constraints);
            }

            if (constraints.Any(c => c.GetType() == constraint.GetType()))
            {
                throw new InvalidOperationException("Constraint of type " + constraint.GetType().FullName + " already exists");
            }

            constraints.Add(constraint);
        }

        /// <summary>
        /// Tries to retrieves an instance of <typeparamref name="T"/> from a <see cref="ContextBag"/>.
        /// </summary>
        public static bool TryGetDeliveryConstraint<T>(this ContextBag context, out T constraint) where T : DeliveryConstraint
        {
            List<DeliveryConstraint> constraints;

            if (context.TryGet(out constraints))
            {
                return constraints.TryGet(out constraint);
            }
            constraint = null;
            return false;
        }

        /// <summary>
        /// Removes a <see cref="DeliveryConstraint"/> to a <see cref="ContextBag"/>.
        /// </summary>
        public static IEnumerable<DeliveryConstraint> GetDeliveryConstraints(this ContextBag context)
        {
            List<DeliveryConstraint> constraints;

            if (context.TryGet(out constraints))
            {
                return constraints;
            }

            return new List<DeliveryConstraint>();
        }

        /// <summary>
        /// Removes a <see cref="DeliveryConstraint"/> to a <see cref="ContextBag"/>.
        /// </summary>
        public static void RemoveDeliveryConstaint(this ContextBag context, DeliveryConstraint constraint)
        {
            List<DeliveryConstraint> constraints;

            if (!context.TryGet(out constraints))
            {
                return;
            }

            constraints.Remove(constraint);
        }

        internal static bool TryGet<T>(this IEnumerable<DeliveryConstraint> list, out T constraint) where T : DeliveryConstraint
        {
            constraint = list.OfType<T>().FirstOrDefault();

            return constraint != null;
        }

        internal static bool DoesTransportSupportConstraint<T>(this FeatureConfigurationContext context) where T : DeliveryConstraint
        {
            return context.Settings.Get<TransportInfrastructure>()
                .DeliveryConstraints.Any(t => typeof(T).IsAssignableFrom(t));
        }
    }
}