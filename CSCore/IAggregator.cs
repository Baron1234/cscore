namespace CSCore
{
    /// <summary>
    ///     Defines the base for all aggregators.
    /// </summary>
    /// <typeparam name="T">The type of data, the aggregator provides.</typeparam>
    /// <typeparam name="TAggregator">The type of the aggreator type.</typeparam>
    public interface IAggregator<in T, out TAggregator>
        : IReadableAudioSource<T> where TAggregator : IReadableAudioSource<T>
    {
        /// <summary>
        ///     Gets the underlying <see cref="IReadableAudioSource{T}" />.
        /// </summary>
        /// <value>
        ///     The underlying <see cref="IReadableAudioSource{T}" />.
        /// </value>
        TAggregator BaseSource { get; }
    }
}