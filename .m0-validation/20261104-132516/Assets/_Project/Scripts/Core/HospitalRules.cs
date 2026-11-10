using System;

namespace QuaThangNayDa.Core
{
    // Fictional gameplay tuning: speeds are relative game units, not medical guidance.
    [Serializable]
    public sealed class HospitalRules
    {
        public double ScrapeSpeed = 2, LightSpeed = 5, SevereSpeed = 10;
        public double ScrapeHealthLoss = 2, LightHealthLoss = 15, SevereHealthLoss = 40;
        public int LightFee = 20000, SevereFee = 60000, SevereTreatmentMinutes = 30;
        public double DischargeHealth = 60;

        public HospitalRules Copy() => (HospitalRules)MemberwiseClone();

        public void Validate()
        {
            bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
            bool Loss(double value) => Finite(value) && value >= 0 && value <= 100;
            if (!Finite(ScrapeSpeed) || !Finite(LightSpeed) || !Finite(SevereSpeed) ||
                ScrapeSpeed < 0 || LightSpeed <= ScrapeSpeed || SevereSpeed <= LightSpeed ||
                !Loss(ScrapeHealthLoss) || !Loss(LightHealthLoss) || !Loss(SevereHealthLoss) ||
                LightFee < 0 || SevereFee < 0 || SevereTreatmentMinutes <= 0 ||
                !Finite(DischargeHealth) || DischargeHealth < 1 || DischargeHealth > 100)
                throw new ArgumentException("Invalid hospital gameplay settings.");
        }
    }
}
