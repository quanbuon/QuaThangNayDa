using System;
using QuaThangNayDa.Core;
using UnityEngine;

namespace QuaThangNayDa.Editor
{
    public static class MorningChecks
    {
        private static void Require(bool value, string name)
        { if (!value) throw new InvalidOperationException("[M2 Check] " + name); }
        public static void Run()
        {
            var rules = new MorningRules();
            var sim = new MorningSimulation(rules);
            rules.ClassEnd = 500; // External config edits must not alter this session.
            Require(sim.Rules.ClassEnd == 540, "Schedule immutable");
            sim.Paused = true;
            Require(sim.Advance(100) == 0 && sim.Minute == 420, "Pause stops clock");
            sim.Paused = false;
            Require(!sim.Start(MorningActivity.Study, out _), "Reject early attendance");
            Require(sim.Start(MorningActivity.Breakfast, out _), "Start breakfast");
            Require(sim.Advance(100) == 15 && sim.Minute == 435 && sim.Money == 80000, "Meal stops jump and charges once");
            Require(sim.Advance(100) == 45 && sim.Minute == 480, "Jump stops at class start");
            Require(sim.Start(MorningActivity.Study, out _), "Class start");
            sim.Advance(20);
            sim.Cancel();
            sim.Advance(40);
            Require(sim.ClassMinutes == 20 && Math.Abs(sim.Learning - 40) < 0.001, "Partial attendance and one settlement");
            sim.Advance(30);
            sim.Start(MorningActivity.Work, out _);
            sim.Advance(10);
            sim.Cancel();
            sim.Advance(10);
            sim.Start(MorningActivity.Work, out _);
            sim.Advance(40);
            Require(sim.WorkMinutes == 50 && sim.Money == 105000, "Pay only actual minutes across cancellation");
            int paid = sim.Money;
            sim.Advance(999);
            sim.Advance(999);
            Require(sim.Ended && sim.Minute == 720 && sim.Money == paid, "End clamp and no duplicate pay");
            var meal = new MorningSimulation(new MorningRules());
            meal.Advance(55);
            meal.Start(MorningActivity.Breakfast, out _);
            Require(meal.Advance(10) == 5 && meal.Minute == 480, "Meal stops at class boundary");
            Require(meal.Activity == MorningActivity.Breakfast, "Meal progress survives boundary");
            meal.Advance(10);
            Require(meal.Activity == MorningActivity.None && meal.Money == 80000, "Meal completes once after boundary");
            var cancelled = new MorningSimulation(new MorningRules());
            cancelled.Start(MorningActivity.Breakfast, out _);
            cancelled.Advance(5);
            cancelled.Cancel();
            Require(cancelled.Energy < 70 && cancelled.Money == 80000, "No meal reward on cancel");
            Debug.Log("[M2 Check] PASS pause, schedule lock, meal, boundary jumps, attendance, wages, cancellation, settlement once, end.");
        }
    }
}
