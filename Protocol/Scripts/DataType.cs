namespace Hactazia.MidiTransporter.Protocol
{
    public enum Events : byte
    {
        Unknown = 0,
        UnknownJson = 1,
        Register = 2,
        Unregister = 3,
        EventBinary = 4,
        EventJson = 5,
        Discover = 6,
        DiscoverCallback = 7
    }
}