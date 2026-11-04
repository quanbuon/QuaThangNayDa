namespace QuaThangNayDa.Core
{
    public enum CommitmentState { NotOffered, AwaitingChoice, Active, Completed, Failed, Unavailable }
    public enum CommitmentChoice { None, StayInClass, CoverShift }

    // Tracks only work performed AFTER accepting. Schedule settlement remains independent.
    public sealed class ShiftCommitment
    {
        public CommitmentState State { get; private set; }
        public CommitmentChoice Choice { get; private set; }
        public int Progress { get; private set; }
        public int RequiredMinutes => 20;
        public int Deadline { get; private set; }
        public bool NeedsChoice => State == CommitmentState.AwaitingChoice;
        public bool Active => State == CommitmentState.Active;

        public bool TryOffer(int minute, int classEnd, int attended)
        {
            if (State != CommitmentState.NotOffered || attended < 10 || minute > classEnd - RequiredMinutes) return false;
            State = CommitmentState.AwaitingChoice;
            return true;
        }
        public bool Choose(CommitmentChoice choice, int minute, int classEnd)
        {
            if (!NeedsChoice || (choice != CommitmentChoice.StayInClass && choice != CommitmentChoice.CoverShift)) return false;
            Choice = choice;
            Deadline = choice == CommitmentChoice.StayInClass ? classEnd : minute + 45;
            State = CommitmentState.Active;
            return true;
        }
        public bool Tick(MorningActivity activity, int intervalStart)
        {
            if (!Active) return false;
            bool matches = Choice == CommitmentChoice.StayInClass ? activity == MorningActivity.Study : activity == MorningActivity.CoverShift;
            if (intervalStart < Deadline && matches) Progress++;
            if (Progress >= RequiredMinutes) { State = CommitmentState.Completed; return true; }
            if (intervalStart + 1 >= Deadline) { State = CommitmentState.Failed; return true; }
            return false;
        }
        public bool FailAtEnd()
        {
            if (!Active) return false;
            State = CommitmentState.Failed;
            return true;
        }
        public bool MarkUnavailable()
        {
            if (State != CommitmentState.NotOffered) return false;
            State = CommitmentState.Unavailable;
            return true;
        }
    }
}
