namespace NServiceBus
{
    /// <summary>
    /// Provides configuration options for unit of work behavior.
    /// </summary>
    public static class UnitOfWorkSettingsExtensions
    {
        /// <summary>
        /// Entry point for unit of work related configuration.
        /// </summary>
        /// <param name="config">The <see cref="BusConfiguration"/> instance to apply the settings to.</param>
        public static UnitOfWorkSettings UnitOfWork(this BusConfiguration config)
        {
            Guard.AgainstNull(nameof(config), config);
            return new UnitOfWorkSettings(config);
        }
    }
}