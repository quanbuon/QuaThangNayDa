using System;

namespace QuaThangNayDa.Core
{
    [Serializable]
    public sealed class MorningSnapshot
    {
        public MorningRules Rules;
        public HospitalRules HospitalRules;
        public bool Hospitalized;
        public int TreatmentRemaining, MedicalDebt, HospitalTimeLimit, HospitalTreatmentMinutes;
        public double HospitalDischargeHealth;
        public int Minute, Money, ClassMinutes, WorkMinutes, Remaining;
        public double Health, Energy, Learning, Relations;
        public MorningActivity Activity;
        public bool ClassSettled, WorkSettled, StoryEnabled;
        public CommitmentState CommitmentState;
        public CommitmentChoice CommitmentChoice;
        public int CommitmentProgress, CommitmentDeadline;
        public string[] Journal;
    }
}
