namespace CSCore
{
    /// <summary>
    ///     Defines the base for all <see cref="IWaveSource" /> aggregators.
    /// </summary>
    public interface IWaveAggregator : IWaveSource, IAggregator<byte, IWaveSource>
    {
    }
}