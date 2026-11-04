using System;
using System.Collections.Generic;

namespace QuaThangNayDa.Core
{
    public enum MorningActivity { None, Breakfast, Study, Work, Rest }

    [Serializable]
    public sealed class MorningRules
    {
        public int StartMinute = 7 * 60, EndMinute = 12 * 60;
        public int ClassStart = 8 * 60, ClassEnd = 9 * 60;
        public int WorkStart = 9 * 60 + 30, WorkEnd = 10 * 60 + 30;
        public int BreakfastMinutes = 15, RestMinutes = 20;
        public int BreakfastCost = 20000, WagePerMinute = 500;
        public MorningRules Copy() => (MorningRules)MemberwiseClone();
        public void Validate()
        {
            if (StartMinute < 0 || EndMinute <= StartMinute || ClassStart < StartMinute ||
                ClassEnd <= ClassStart || WorkStart < ClassEnd || WorkEnd <= WorkStart || WorkEnd > EndMinute ||
                BreakfastMinutes <= 0 || RestMinutes <= 0 || BreakfastCost < 0 || WagePerMinute < 0)
                throw new ArgumentException("Invalid morning schedule or activity settings.");
        }
    }

    // No scene, UI or Unity API. Every time jump runs the same minute-boundary logic.
    public sealed class MorningSimulation
    {
        private readonly MorningRules _rules;
        private readonly List<string> _journal = new List<string>();
        private int _remaining;
        private bool _classSettled, _workSettled;
        public int Minute { get; private set; }
        public int Money { get; private set; } = 100000;
        public double Health { get; private set; } = 80;
        public double Energy { get; private set; } = 70;
        public double Learning { get; private set; } = 40;
        public double Relations { get; private set; } = 50;
        public int ClassMinutes { get; private set; }
        public int WorkMinutes { get; private set; }
        public MorningActivity Activity { get; private set; }
        public bool Paused { get; set; }
        public bool Ended => Minute >= _rules.EndMinute || Health <= 0 || Energy <= 0;
        public IReadOnlyList<string> Journal => _journal;
        public MorningRules Rules => _rules.Copy(); // Session schedule cannot be edited through the config.

        public MorningSimulation(MorningRules rules)
        {
            rules.Validate();
            _rules = rules.Copy();
            Minute = rules.StartMinute;
            Record("Lich da khoa cho luot thu. Chuyen can va luong tinh theo phut thuc hien.");
        }

        public string Preview(MorningActivity activity)
        {
            if (activity == MorningActivity.Study && (Minute < _rules.ClassStart || Minute >= _rules.ClassEnd))
                return ScheduleReason("Lop hoc", _rules.ClassStart, _rules.ClassEnd);
            if (activity == MorningActivity.Work && (Minute < _rules.WorkStart || Minute >= _rules.WorkEnd))
                return ScheduleReason("Ca lam", _rules.WorkStart, _rules.WorkEnd);
            int duration = activity == MorningActivity.Breakfast ? _rules.BreakfastMinutes :
                activity == MorningActivity.Rest ? _rules.RestMinutes :
                activity == MorningActivity.Study ? Math.Max(0, _rules.ClassEnd - Minute) : Math.Max(0, _rules.WorkEnd - Minute);
            string message = $"{activity}: {duration} phut";
            if (activity == MorningActivity.Breakfast) message += $", tra {_rules.BreakfastCost}d khi bat dau; huy khong hoan tien";
            if (activity != MorningActivity.Study && Minute < _rules.ClassEnd && Minute + duration > _rules.ClassStart)
                message += " | CANH BAO: trung gio hoc, mat phut chuyen can";
            if (activity != MorningActivity.Work && Minute < _rules.WorkEnd && Minute + duration > _rules.WorkStart)
                message += " | CANH BAO: trung ca lam, mat phut luong/quan he";
            return message;
        }

        public bool CanStart(MorningActivity activity, out string reason)
        {
            reason = "";
            if (Paused || Ended || Activity != MorningActivity.None || activity == MorningActivity.None)
            { reason = "Chua the bat dau hoat dong."; return false; }
            if (activity == MorningActivity.Study && (Minute < _rules.ClassStart || Minute >= _rules.ClassEnd))
            { reason = ScheduleReason("Lop hoc", _rules.ClassStart, _rules.ClassEnd); return false; }
            if (activity == MorningActivity.Work && (Minute < _rules.WorkStart || Minute >= _rules.WorkEnd))
            { reason = ScheduleReason("Ca lam", _rules.WorkStart, _rules.WorkEnd); return false; }
            if (activity == MorningActivity.Breakfast && Money < _rules.BreakfastCost)
            { reason = "Khong du tien an sang."; return false; }
            return true;
        }

        private string ScheduleReason(string label, int start, int end)
        {
            string slot = $"{start / 60:00}:{start % 60:00} - {end / 60:00}:{end % 60:00}";
            return Minute < start
                ? $"{label}: CHUA TOI GIO. Lich {slot}; con {start - Minute} phut. Co the nghi o bang NGHI NGOI."
                : $"{label}: DA HET GIO. Lich {slot}. Khong the tham gia them trong luot nay.";
        }

        public bool Start(MorningActivity activity, out string reason)
        {
            if (!CanStart(activity, out reason)) return false;
            _remaining = activity == MorningActivity.Breakfast ? _rules.BreakfastMinutes :
                activity == MorningActivity.Rest ? _rules.RestMinutes : int.MaxValue;
            if (activity == MorningActivity.Breakfast)
            {
                Money -= _rules.BreakfastCost;
                Record($"An sang: -{_rules.BreakfastCost}d. Chi nhan hoi phuc khi an xong.");
            }
            Activity = activity;
            Record("Bat dau " + activity);
            return true;
        }

        public void Cancel()
        {
            if (Activity != MorningActivity.None) Record("Dung " + Activity + "; giu so phut da thuc hien.");
            Activity = MorningActivity.None;
        }

        public int Advance(int minutes)
        {
            if (minutes < 0) throw new ArgumentOutOfRangeException(nameof(minutes));
            if (Paused || Ended) return 0;
            int advanced = 0;
            for (int i = 0; i < minutes && !Ended; i++)
            {
                var previous = Activity;
                // Credit [Minute, Minute + 1), never credit minutes outside the slot.
                if (Activity == MorningActivity.Study && Minute >= _rules.ClassStart && Minute < _rules.ClassEnd) ClassMinutes++;
                if (Activity == MorningActivity.Work && Minute >= _rules.WorkStart && Minute < _rules.WorkEnd) WorkMinutes++;
                Energy = Clamp(Energy - (Activity == MorningActivity.Work ? 0.12 : 0.04));
                Minute++;
                advanced++;
                if (Activity == MorningActivity.Breakfast || Activity == MorningActivity.Rest)
                {
                    _remaining--;
                    if (_remaining == 0)
                    {
                        Energy = Clamp(Energy + (Activity == MorningActivity.Breakfast ? 20 : 15));
                        Health = Clamp(Health + (Activity == MorningActivity.Breakfast ? 3 : 1));
                        Record("Hoan thanh " + Activity + ": da nhan hoi phuc.");
                        Activity = MorningActivity.None;
                    }
                }
                SettleSchedule();
                if (Ended) { Activity = MorningActivity.None; Record("Ket thuc buoi sang."); }
                // A fast-forward batch must return control at schedule boundaries or activity completion.
                if (Activity != previous || Minute == _rules.ClassStart || Minute == _rules.ClassEnd ||
                    Minute == _rules.WorkStart || Minute == _rules.WorkEnd) break;
            }
            return advanced;
        }

        private void SettleSchedule()
        {
            if (!_classSettled && Minute >= _rules.ClassEnd)
            {
                _classSettled = true;
                double attended = ClassMinutes / (double)(_rules.ClassEnd - _rules.ClassStart);
                double change = 12 * attended - 6 * (1 - attended);
                Learning = Clamp(Learning + change);
                Record($"Lich hoc: {ClassMinutes} phut; hoc tap {change:+0.0;-0.0;0} (thuong du hoc +12, vang het -6).");
                if (Activity == MorningActivity.Study) Activity = MorningActivity.None;
            }
            if (!_workSettled && Minute >= _rules.WorkEnd)
            {
                _workSettled = true;
                int pay = WorkMinutes * _rules.WagePerMinute;
                Money += pay;
                double change = -4 * (1 - WorkMinutes / (double)(_rules.WorkEnd - _rules.WorkStart));
                Relations = Clamp(Relations + change);
                Record($"Lich lam: {WorkMinutes} phut; luong +{pay}d; quan he {change:+0.0;-0.0;0} (vang het -4).");
                if (Activity == MorningActivity.Work) Activity = MorningActivity.None;
            }
        }
        private static double Clamp(double value) => Math.Max(0, Math.Min(100, value));
        private void Record(string text) => _journal.Add($"{Minute / 60:00}:{Minute % 60:00} {text}");
    }
}
