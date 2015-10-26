namespace NServiceBus.Transports
{
    using NServiceBus.Settings;

    /// <summary>
    /// Provides context for transport start-up checks.
    /// </summary>
    public class TransportStartUpCheckContext
    {
        /// <summary>
        /// Creates a new instance of the context.
        /// </summary>
        /// <param name="settings">Settings bag.</param>
        /// <param name="queueBindings">Queue bindings.</param>
        public TransportStartUpCheckContext(ReadOnlySettings settings, QueueBindings queueBindings)
        {
            Settings = settings;
            QueueBindings = queueBindings;
        }

        /// <summary>
        /// Settings.
        /// </summary>
        public ReadOnlySettings Settings { get; }
        /// <summary>
        /// Queue bindings.
        /// </summary>
        public QueueBindings QueueBindings { get; }
    }
}