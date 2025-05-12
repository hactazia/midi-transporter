namespace Hactazia.MidiTransporter
{
    [System.Flags]
    public enum MidiEvents : byte
    {
        None = 0,
        Everything = Note & ControlChange,
        NoteOn = 1 << 1,
        NoteOff = 1 << 2,
        ControlChange = 1 << 3,
        Note = NoteOn & NoteOff,
    }
}