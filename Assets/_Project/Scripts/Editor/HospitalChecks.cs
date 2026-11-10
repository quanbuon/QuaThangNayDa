using System;
using QuaThangNayDa.Core;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    public static class HospitalChecks
    {
        private static void Check(bool value, string name)
        { if (!value) throw new InvalidOperationException("[Hospital Check] " + name); }

        private static MorningSimulation Load(MorningSimulation sim)
            => MorningSimulation.Restore(JsonUtility.FromJson<MorningSnapshot>(JsonUtility.ToJson(sim.Capture())));

        private static void Until(MorningSimulation sim, int minute)
        {
            while (sim.Minute < minute)
                Check(sim.Advance(minute - sim.Minute) > 0, "Clock reaches requested minute");
        }

        private static void Reject(MorningSnapshot data)
        {
            bool rejected = false;
            try { MorningSimulation.Restore(data); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "Reject inconsistent hospital snapshot");
        }

        public static void Run()
        {
            var scrape = new MorningSimulation(new MorningRules());
            scrape.Start(MorningActivity.Rest, out _);
            Check(!scrape.ReceiveImpact(1.99), "Below scrape threshold ignored");
            Check(scrape.ReceiveImpact(2) && scrape.Health == 78 && !scrape.Hospitalized &&
                scrape.Activity == MorningActivity.Rest && scrape.Money == 100000 && scrape.Learning == 40,
                "Scrape only loses health, does not cancel activity");

            var light = new MorningSimulation(new MorningRules());
            light.Start(MorningActivity.Breakfast, out _);
            Check(light.ReceiveImpact(5) && light.Hospitalized && !light.Paused && light.TreatmentRemaining == 0 &&
                light.Activity == MorningActivity.None && light.Money == 60000, "Light admission cancels without meal refund");
            Check(!light.Start(MorningActivity.Rest, out _) && light.Treat(10) == 0, "No activity or unnecessary treatment");
            Check(!light.ReceiveImpact(100) && light.Money == 60000, "Collision callback cannot charge twice");
            light = Load(light);
            Check(light.Discharge() && !light.Discharge() && light.Money == 60000 && light.Health >= 60,
                "Explicit immediate light discharge and reload do not charge again");

            var severe = new MorningSimulation(new MorningRules());
            severe.HospitalRules.SevereHealthLoss = 100;
            Check(severe.ReceiveImpact(10) && severe.Health == 1 && !severe.Ended && severe.Money == 40000,
                "Severe injury is recoverable at health floor");
            Check(!severe.Discharge() && severe.Treat(10) == 10 && severe.TreatmentRemaining == 20, "Severe requires actual treatment");
            severe.Paused = true;
            Check(severe.Treat(10) == 0 && severe.Advance(10) == 0 && !severe.Discharge(), "Pause guards hospital actions");
            severe.Paused = false;
            var saved = severe.Capture();
            severe.HospitalRules.SevereTreatmentMinutes = 99;
            Check(saved.HospitalRules.SevereTreatmentMinutes == 30, "Captured rules are copied");
            severe.HospitalRules.SevereTreatmentMinutes = 30;
            var loaded = Load(severe);
            Check(loaded.TreatmentRemaining == 20 && loaded.Hospitalized && loaded.Money == 40000, "Mid-treatment save/load");
            severe.Treat(100); loaded.Treat(100);
            Check(JsonUtility.ToJson(severe.Capture()) == JsonUtility.ToJson(loaded.Capture()), "Reload deterministic with no duplicate fee");
            Check(loaded.Hospitalized && loaded.TreatmentRemaining == 0 && loaded.Discharge() && loaded.Health == 60,
                "Treatment completion waits for explicit discharge and heals");

            var tuned = new MorningSimulation(new MorningRules());
            tuned.HospitalRules.ScrapeSpeed = 3; tuned.HospitalRules.LightSpeed = 7; tuned.HospitalRules.SevereSpeed = 12;
            Check(!tuned.ReceiveImpact(2) && tuned.ReceiveImpact(6) && !tuned.Hospitalized, "Configured relative speed thresholds");
            tuned.ReceiveImpact(12);
            tuned.HospitalRules.SevereTreatmentMinutes = 1; tuned.HospitalRules.DischargeHealth = 99;
            Check(tuned.Advance(1) == 1 && tuned.TreatmentRemaining == 30, "Ordinary advancement does not grant treatment");
            tuned = Load(tuned);
            Check(tuned.Treat(100) == 30 && tuned.Discharge() && tuned.Health == 60, "Admission duration and heal survive later tuning edits");

            var tiredData = new MorningSimulation(new MorningRules()).Capture();
            tiredData.Energy = 0.01;
            var tired = MorningSimulation.Restore(tiredData);
            tired.ReceiveImpact(10); tired.Treat(30);
            Check(tired.Energy >= 1 && !tired.Ended && tired.Discharge(), "Energy depletion cannot strand recoverable treatment");

            var poorData = new MorningSimulation(new MorningRules()).Capture();
            poorData.Money = 5000;
            var poor = MorningSimulation.Restore(poorData);
            poor.ReceiveImpact(5);
            Check(poor.Money == 0 && poor.MedicalDebt == 15000, "Spend available money then accrue debt");
            poor = Load(poor); poor.Discharge(); poor.ReceiveImpact(5); poor.Discharge();
            Check(poor.MedicalDebt == 35000 && poor.Money == 0, "Only distinct admissions accumulate debt");

            var late = new MorningSimulation(new MorningRules());
            Until(late, 715); late.ReceiveImpact(10);
            Check(late.Treat(10) == 5 && late.Minute == 720 && !late.Ended && late.TreatmentRemaining == 25,
                "Treatment stops at morning end without ending admission");
            Check(late.Advance(10) == 0, "Ordinary clock cannot extend morning");
            late = Load(late);
            Check(late.Treat(100) == 25 && late.Minute == 745 && !late.Ended, "Treatment finishes beyond morning end");
            late = Load(late);
            Check(late.Discharge() && late.Ended && late.Advance(1) == 0 && late.Treat(1) == 0 &&
                !late.ReceiveImpact(10) && !late.Discharge(), "Discharge ends extended morning exactly once");
            Check(Load(late).Minute == 745 && late.ClassMinutes == 0 && late.WorkMinutes == 0, "Extended ended save preserves fixed windows");

            var deadlines = new MorningSimulation(new MorningRules(), true);
            Until(deadlines, 480); deadlines.Start(MorningActivity.Study, out _); deadlines.Advance(10);
            Check(deadlines.StoryPending && deadlines.Treat(10) == 0 && !deadlines.ReceiveImpact(10), "Pending story guards hospital actions");
            deadlines.ChooseCommitment(CommitmentChoice.CoverShift);
            deadlines.HospitalRules.SevereTreatmentMinutes = 160;
            deadlines.ReceiveImpact(10);
            Check(deadlines.Treat(100) == 45 && deadlines.Minute == 535 && deadlines.Commitment.State == CommitmentState.Failed &&
                deadlines.Relations == 46, "Treatment stops at missed commitment deadline");
            Check(deadlines.Treat(100) == 5 && deadlines.Minute == 540 && deadlines.Learning == 37, "Only existing missed-school penalty");
            Check(deadlines.Treat(100) == 30 && deadlines.Minute == 570, "Treatment stops at work start");
            Check(deadlines.Treat(100) == 60 && deadlines.Minute == 630 && deadlines.WorkMinutes == 0 && deadlines.Relations == 42,
                "Missed work settles once without wages");
            Check(deadlines.Treat(100) == 20 && deadlines.Discharge() && deadlines.Money == 40000, "Remaining treatment and fixed fee");

            var old = new MorningSimulation(new MorningRules()).Capture();
            old.HospitalRules = null; old.HospitalTimeLimit = 0;
            Check(!MorningSimulation.Restore(old).Hospitalized && MorningSimulation.Restore(old).HospitalRules.SevereSpeed == 10,
                "Legacy snapshots default to healthy hospital state and default tuning");
            var invalid = loaded.Capture(); invalid.TreatmentRemaining = 1; Reject(invalid);
            invalid = severe.Capture(); invalid.Health = 0; Reject(invalid);
            invalid = severe.Capture(); invalid.Activity = MorningActivity.Rest; Reject(invalid);
            invalid = severe.Capture(); invalid.MedicalDebt = -1; Reject(invalid);
            invalid = severe.Capture(); invalid.TreatmentRemaining = invalid.HospitalTreatmentMinutes + 1; Reject(invalid);
            invalid = severe.Capture(); invalid.HospitalRules.LightSpeed = double.NaN; Reject(invalid);
            invalid = severe.Capture(); invalid.Minute = invalid.HospitalTimeLimit + 1; Reject(invalid);
            Debug.Log("[Hospital Check] PASS injuries, explicit discharge, fees/debt, pause/story guards, save/load, legacy defaults, overtime, deadlines and fixed schedules.");
        }
    }
}
