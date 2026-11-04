using System;
using QuaThangNayDa.Core;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    public static class CommitmentChecks
    {
        private static void Check(bool value, string name)
        { if (!value) throw new InvalidOperationException("[M3 Check] " + name); }
        private static MorningSimulation Offer()
        {
            var sim = new MorningSimulation(new MorningRules(), true);
            sim.Advance(60);
            sim.Start(MorningActivity.Study, out _);
            Check(sim.Advance(50) == 10 && sim.Minute == 490 && sim.StoryPending, "Fast-forward interrupted by story");
            return sim;
        }
        public static void Run()
        {
            var stay = Offer();
            Check(stay.Advance(100) == 0, "Story freezes clock");
            Check(stay.ClassMinutes == 10, "No extra attendance while reading");
            Check(stay.ChooseCommitment(CommitmentChoice.StayInClass), "Choose stay");
            Check(stay.Relations == 50 && stay.Money == 100000, "Choice alone has no reward or penalty");
            Check(!stay.ChooseCommitment(CommitmentChoice.CoverShift), "Cannot change accepted choice");
            stay.Advance(20);
            Check(stay.Commitment.State == CommitmentState.Completed && stay.Relations == 48, "Actually study to complete");
            stay.Advance(30);
            Check(stay.Learning == 52 && stay.Relations == 48, "Class reward separate; commitment settles once");

            var work = Offer();
            work.ChooseCommitment(CommitmentChoice.CoverShift);
            Check(work.Activity == MorningActivity.None && work.Commitment.Deadline == 535, "Leave class, deadline starts at confirm");
            work.Advance(5); // Travel time alone earns no commitment progress.
            Check(work.Commitment.Progress == 0 && work.Money == 100000, "Travel / click do not earn reward");
            Check(work.Start(MorningActivity.CoverShift, out _), "Start at manager adapter");
            work.Advance(10);
            work.Cancel();
            work.Advance(5);
            work.Start(MorningActivity.CoverShift, out _);
            work.Advance(10);
            Check(work.Commitment.State == CommitmentState.Completed && work.Money == 120000 && work.WorkMinutes == 0,
                "Actual cover minutes only, no regular-wage double count");
            work.Advance(100);
            Check(work.ClassMinutes == 10 && work.Learning == 37 && work.Money == 120000,
                "Absence consequence separate from commitment reward");

            var fail = Offer();
            fail.ChooseCommitment(CommitmentChoice.CoverShift);
            Check(fail.Advance(100) == 45 && fail.Minute == 535, "Fast-forward stops exactly at deadline");
            Check(fail.Commitment.State == CommitmentState.Failed && fail.Relations == 46 && fail.Money == 100000, "Broken commitment penalty once");
            fail.Advance(5);
            Check(fail.Relations == 46, "No duplicate commitment penalty");
            Check(!fail.Start(MorningActivity.CoverShift, out _), "No cover work after expiry");

            var exact = Offer();
            exact.ChooseCommitment(CommitmentChoice.CoverShift);
            exact.Advance(25);
            exact.Start(MorningActivity.CoverShift, out _);
            exact.Advance(20);
            Check(exact.Minute == 535 && exact.Commitment.State == CommitmentState.Completed, "Last eligible minute counts before deadline failure");
            var paused = Offer();
            paused.Paused = true;
            Check(!paused.ChooseCommitment(CommitmentChoice.StayInClass), "Pause blocks confirmation");
            Debug.Log("[M3 Check] PASS story interrupt/pause, both branches, travel, cancellation, deadlines, choice without reward, separate settlement.");
        }
    }
}
