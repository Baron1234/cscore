namespace CSCore
{
    /// <summary>
    ///     Defines the base for all <see cref="ISampleSource" /> aggregators.
    /// </summary>
    public interface ISampleAggregator : ISampleSource, IAggregator<float, ISampleSource>
    {
    }
}