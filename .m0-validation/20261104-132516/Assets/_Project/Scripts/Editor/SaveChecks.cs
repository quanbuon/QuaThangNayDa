using System;
using System.IO;
using QuaThangNayDa.Core;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    public static class SaveChecks
    {
        private static void Check(bool value, string message)
        { if (!value) throw new InvalidOperationException("[M4 Check] " + message); }
        private static MorningSimulation RoundTrip(MorningSimulation sim)
            => MorningSimulation.Restore(JsonUtility.FromJson<MorningSnapshot>(JsonUtility.ToJson(sim.Capture())));
        private static SaveGame Wrap(MorningSimulation sim) => new SaveGame
        {
            Simulation = sim.Capture(), Travel = new TravelSnapshot { Position = new Vector3(-2, 0.1f, -2),
                ScooterPosition = new Vector3(0, 0.55f, 0), ScooterRotation = Quaternion.identity },
            MinuteFraction = 0.375f, RealSecondsPerMinute = 6, Started = true, SavedUtc = DateTime.UtcNow.ToString("O")
        };
        public static void Run()
        {
            var meal = new MorningSimulation(new MorningRules());
            meal.Start(MorningActivity.Breakfast, out _); meal.Advance(7);
            var loadedMeal = RoundTrip(meal);
            Check(loadedMeal.Minute == meal.Minute && loadedMeal.Money == 80000, "Mid-meal clock and cost");
            Check(loadedMeal.Advance(100) == 8 && loadedMeal.Activity == MorningActivity.None, "Mid-meal remaining duration");

            var story = new MorningSimulation(new MorningRules(), true);
            story.Advance(60); story.Start(MorningActivity.Study, out _); story.Advance(10);
            var offered = RoundTrip(story);
            Check(offered.StoryPending && offered.Advance(100) == 0, "Unchosen story stays frozen after load");
            story.ChooseCommitment(CommitmentChoice.CoverShift); story.Advance(5);
            story.Start(MorningActivity.CoverShift, out _); story.Advance(9);
            var active = RoundTrip(story);
            Check(active.Activity == MorningActivity.CoverShift && active.Commitment.Progress == 9 && active.Commitment.Deadline == 535,
                "Activity, progress and deadline preserved");
            story.Advance(11); active.Advance(11);
            Check(JsonUtility.ToJson(story.Capture()) == JsonUtility.ToJson(active.Capture()), "Loaded branch equals uninterrupted branch");
            var completed = RoundTrip(active);
            completed.Advance(100);
            Check(completed.Money == 120000 && completed.Relations == 53 && completed.ClassMinutes == 10, "No duplicate commitment reward");
            completed.Advance(30); completed.Start(MorningActivity.Work, out _); completed.Advance(25);
            var midway = RoundTrip(completed); midway.Advance(35);
            Check(midway.WorkMinutes == 60 && midway.Money == 150000, "Work resumes and pays once");
            var paid = RoundTrip(midway); paid.Advance(200);
            Check(paid.Money == 150000 && paid.Ended, "Settled wages not repeated after load");

            var failure = new MorningSimulation(new MorningRules(), true);
            failure.Advance(60); failure.Start(MorningActivity.Study, out _); failure.Advance(10);
            failure.ChooseCommitment(CommitmentChoice.CoverShift); failure.Advance(45);
            var failedLoad = RoundTrip(failure); failedLoad.Advance(5);
            Check(failedLoad.Relations == 46, "Failed commitment not penalized twice");
            var invalid = failure.Capture(); invalid.ClassSettled = true;
            bool rejected = false;
            try { MorningSimulation.Restore(invalid); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "Reject inconsistent settlement flags");

            string folder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Logs", "M4Checks", Guid.NewGuid().ToString("N"));
            var store = new SaveStore(folder);
            var first = Wrap(meal);
            store.Write(1, first);
            var second = Wrap(loadedMeal);
            store.Write(1, second);
            var fromDisk = store.Load(1, out bool backup);
            Check(!backup && fromDisk.Simulation.Minute == 435 && fromDisk.MinuteFraction == 0.375f, "Atomic disk round trip and fractional time");
            File.WriteAllText(store.SlotPath(1), "INTENTIONALLY CORRUPTED TEST FIXTURE");
            var recovered = store.Load(1, out backup);
            Check(backup && recovered.Simulation.Minute == 427, "Corrupt primary recovers previous good backup");
            store.Write(1, second); // Must not rotate the corrupt primary onto the good backup.
            File.WriteAllText(store.SlotPath(1), "CORRUPT AGAIN");
            recovered = store.Load(1, out backup);
            Check(backup && recovered.Simulation.Minute == 427, "Keep good backup across repair write");
            for (int slot = 0; slot <= 3; slot++) store.Write(slot, first);
            rejected = false;
            try { store.Write(0, Wrap(paid)); } catch (InvalidOperationException) { rejected = true; }
            Check(rejected && store.Load(0, out _).Simulation.Minute == 427, "Ended game cannot overwrite safe autosave");
            var future = Wrap(meal); future.Version = 999;
            rejected = false;
            try { store.Write(2, future); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "Unsupported version rejected");
            Debug.Log("[M4 Check] PASS mid-activity, commitment, fractional time, settlement once, disk replacement, corrupt-file fallback, backup preservation, autosave end guard.");
        }
    }
}
