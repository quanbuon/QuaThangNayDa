using System;
using System.Collections.Generic;

namespace QuaThangNayDa.Core
{
    public enum MorningActivity { None, Breakfast, Study, Work, Rest, CoverShift }

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
        private int _hospitalTimeLimit;
        private int _hospitalTreatmentMinutes;
        private double _hospitalDischargeHealth;
        public HospitalRules HospitalRules { get; set; } = new HospitalRules();
        public bool Hospitalized { get; private set; }
        public int TreatmentRemaining { get; private set; }
        public int MedicalDebt { get; private set; }
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
        public bool Ended => (!Hospitalized && Minute >= _rules.EndMinute) || Health <= 0 || Energy <= 0;
        public IReadOnlyList<string> Journal => _journal;
        public MorningRules Rules => _rules.Copy(); // Session schedule cannot be edited through the config.
        public ShiftCommitment Commitment { get; private set; }
        public bool StoryPending => Commitment != null && Commitment.NeedsChoice;

        public MorningSnapshot Capture()
        {
            return new MorningSnapshot
            {
                Rules = _rules.Copy(), Minute = Minute, Money = Money, Health = Health, Energy = Energy,
                HospitalRules = HospitalRules.Copy(), Hospitalized = Hospitalized,
                TreatmentRemaining = TreatmentRemaining, MedicalDebt = MedicalDebt,
                HospitalTimeLimit = _hospitalTimeLimit, HospitalDischargeHealth = _hospitalDischargeHealth,
                HospitalTreatmentMinutes = _hospitalTreatmentMinutes,
                Learning = Learning, Relations = Relations, ClassMinutes = ClassMinutes, WorkMinutes = WorkMinutes,
                Activity = Activity, Remaining = _remaining, ClassSettled = _classSettled, WorkSettled = _workSettled,
                StoryEnabled = Commitment != null, CommitmentState = Commitment?.State ?? CommitmentState.NotOffered,
                CommitmentChoice = Commitment?.Choice ?? CommitmentChoice.None,
                CommitmentProgress = Commitment?.Progress ?? 0, CommitmentDeadline = Commitment?.Deadline ?? 0,
                Journal = _journal.ToArray()
            };
        }

        public static MorningSimulation Restore(MorningSnapshot data)
        {
            if (data == null || data.Rules == null) throw new ArgumentException("Missing simulation snapshot.");
            data.Rules.Validate();
            var hospitalRules = data.HospitalRules == null ? new HospitalRules() : data.HospitalRules.Copy();
            hospitalRules.Validate();
            // Missing hospital fields in legacy saves mean no injury, debt, or extension.
            int hospitalTimeLimit = data.HospitalTimeLimit == 0 ? data.Rules.EndMinute : data.HospitalTimeLimit;
            if (hospitalTimeLimit < data.Rules.EndMinute || data.Minute > hospitalTimeLimit ||
                data.HospitalTreatmentMinutes < 0 ||
                (long)hospitalTimeLimit != (long)data.Rules.EndMinute + data.HospitalTreatmentMinutes ||
                data.TreatmentRemaining < 0 || data.TreatmentRemaining > data.HospitalTreatmentMinutes || data.MedicalDebt < 0 ||
                (!data.Hospitalized && data.TreatmentRemaining != 0) ||
                (data.Hospitalized && (data.Activity != MorningActivity.None || data.Remaining != 0 ||
                    data.Health < 1 || data.Energy < 1 ||
                    (long)data.Minute + data.TreatmentRemaining > hospitalTimeLimit ||
                    double.IsNaN(data.HospitalDischargeHealth) || data.HospitalDischargeHealth < 1 || data.HospitalDischargeHealth > 100)) ||
                (!data.Hospitalized && data.HospitalDischargeHealth != 0) ||
                (data.HospitalRules == null && (data.Hospitalized || data.MedicalDebt != 0 || hospitalTimeLimit != data.Rules.EndMinute)))
                throw new ArgumentException("Invalid hospital snapshot.");
            bool ValidStat(double value) => !double.IsNaN(value) && !double.IsInfinity(value) && value >= 0 && value <= 100;
            if (data.Minute < data.Rules.StartMinute || data.Money < 0 ||
                !ValidStat(data.Health) || !ValidStat(data.Energy) || !ValidStat(data.Learning) || !ValidStat(data.Relations) ||
                !Enum.IsDefined(typeof(MorningActivity), data.Activity) ||
                data.ClassMinutes < 0 || data.ClassMinutes > Math.Max(0, Math.Min(data.Minute, data.Rules.ClassEnd) - data.Rules.ClassStart) ||
                data.WorkMinutes < 0 || data.WorkMinutes > Math.Max(0, Math.Min(data.Minute, data.Rules.WorkEnd) - data.Rules.WorkStart) ||
                data.ClassSettled != (data.Minute >= data.Rules.ClassEnd) || data.WorkSettled != (data.Minute >= data.Rules.WorkEnd) ||
                data.Journal == null || data.Journal.Length > 10000 ||
                (data.Activity == MorningActivity.Study && (data.Minute < data.Rules.ClassStart || data.Minute >= data.Rules.ClassEnd)) ||
                (data.Activity == MorningActivity.Work && (data.Minute < data.Rules.WorkStart || data.Minute >= data.Rules.WorkEnd)) ||
                (data.Activity == MorningActivity.Breakfast && (data.Remaining <= 0 || data.Remaining > data.Rules.BreakfastMinutes)) ||
                (data.Activity == MorningActivity.Rest && (data.Remaining <= 0 || data.Remaining > data.Rules.RestMinutes)))
                throw new ArgumentException("Invalid simulation snapshot.");
            var sim = new MorningSimulation(data.Rules, data.StoryEnabled)
            {
                Minute = data.Minute, Money = data.Money, Health = data.Health, Energy = data.Energy,
                HospitalRules = hospitalRules, Hospitalized = data.Hospitalized,
                TreatmentRemaining = data.TreatmentRemaining, MedicalDebt = data.MedicalDebt,
                _hospitalTimeLimit = hospitalTimeLimit, _hospitalDischargeHealth = data.HospitalDischargeHealth,
                _hospitalTreatmentMinutes = data.HospitalTreatmentMinutes,
                Learning = data.Learning, Relations = data.Relations, ClassMinutes = data.ClassMinutes, WorkMinutes = data.WorkMinutes,
                Activity = data.Activity, _remaining = data.Remaining, _classSettled = data.ClassSettled, _workSettled = data.WorkSettled,
                Commitment = data.StoryEnabled ? ShiftCommitment.Restore(data) : null
            };
            if (sim.Activity == MorningActivity.CoverShift && (sim.Commitment == null || !sim.Commitment.Active || sim.Commitment.Choice != CommitmentChoice.CoverShift))
                throw new ArgumentException("Cover shift without active commitment.");
            if (sim.Ended && sim.Activity != MorningActivity.None) throw new ArgumentException("Ended snapshot has active activity.");
            sim._journal.Clear();
            foreach (string line in data.Journal)
            {
                if (line == null || line.Length > 4096) throw new ArgumentException("Invalid journal entry.");
                sim._journal.Add(line);
            }
            return sim;
        }

        public MorningSimulation(MorningRules rules, bool enableStory = false)
        {
            rules.Validate();
            _rules = rules.Copy();
            Minute = rules.StartMinute;
            _hospitalTimeLimit = rules.EndMinute;
            if (enableStory) Commitment = new ShiftCommitment();
            Record("Lich da khoa cho luot thu. Chuyen can va luong tinh theo phut thuc hien.");
        }

        public string Preview(MorningActivity activity)
        {
            if (activity == MorningActivity.CoverShift)
                return "Gap quan ly, lam thay 20 phut sau khi nhan loi; du viec +20.000d, +3 quan he. Phut vang lop van bi tinh rieng.";
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
            if (Paused || StoryPending || Ended || Hospitalized || Activity != MorningActivity.None || activity == MorningActivity.None)
            { reason = "Chua the bat dau hoat dong."; return false; }
            if (activity == MorningActivity.CoverShift && (Commitment == null || !Commitment.Active ||
                Commitment.Choice != CommitmentChoice.CoverShift || Minute >= Commitment.Deadline))
            { reason = "Chi lam thay sau khi nhan loi quan ly qua dien thoai, trong han cam ket."; return false; }
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
            if (StoryPending) return;
            if (Activity != MorningActivity.None) Record("Dung " + Activity + "; giu so phut da thuc hien.");
            Activity = MorningActivity.None;
        }

        // Returns true only for an applied injury. Repeated collision callbacks during admission are inert.
        public bool ReceiveImpact(double speed)
        {
            if (double.IsNaN(speed) || double.IsInfinity(speed) || speed < 0)
                throw new ArgumentOutOfRangeException(nameof(speed));
            if (Paused || StoryPending || Ended || Hospitalized) return false;
            if (HospitalRules == null) throw new InvalidOperationException("Hospital rules are missing.");
            HospitalRules.Validate();
            if (speed < HospitalRules.ScrapeSpeed) return false;
            if (speed < HospitalRules.LightSpeed)
            {
                Health = Clamp(Health - HospitalRules.ScrapeHealthLoss);
                Record("Va quet nhe: giam suc khoe.");
                if (Ended) FinishMorning();
                return true;
            }
            bool severe = speed >= HospitalRules.SevereSpeed;
            int duration = severe ? HospitalRules.SevereTreatmentMinutes : 0;
            int fee = severe ? HospitalRules.SevereFee : HospitalRules.LightFee;
            int paid = Math.Min(Money, fee);
            // Check before mutating: neither time nor accumulated debt may wrap around.
            int limit = checked(_rules.EndMinute + duration);
            int debt = checked(MedicalDebt + (fee - paid));
            Cancel();
            _remaining = 0;
            Hospitalized = true;
            TreatmentRemaining = duration;
            _hospitalTreatmentMinutes = duration;
            _hospitalTimeLimit = limit;
            _hospitalDischargeHealth = HospitalRules.DischargeHealth;
            Health = Math.Max(1, Clamp(Health - (severe ? HospitalRules.SevereHealthLoss : HospitalRules.LightHealthLoss)));
            Energy = Math.Max(1, Energy);
            Money -= paid;
            MedicalDebt = debt;
            Record($"Nhap vien: phi {fee}d, tra {paid}d, no them {fee - paid}d; dieu tri {duration} phut.");
            return true;
        }

        public int Treat(int minutes) => AdvanceMinutes(minutes, true);

        public bool Discharge()
        {
            if (Paused || StoryPending || Ended || !Hospitalized || TreatmentRemaining != 0) return false;
            Health = Math.Max(Health, _hospitalDischargeHealth);
            _hospitalDischargeHealth = 0;
            Hospitalized = false;
            Record("Xuat vien: da hoi phuc, khong thu phi lan nua.");
            if (Ended) FinishMorning();
            return true;
        }

        public int Advance(int minutes) => AdvanceMinutes(minutes, false);

        private void FinishMorning()
        {
            Activity = MorningActivity.None;
            Record("Ket thuc buoi sang.");
            if (Commitment != null && Commitment.FailAtEnd()) SettleCommitment();
        }

        private int AdvanceMinutes(int minutes, bool treatment)
        {
            if (minutes < 0) throw new ArgumentOutOfRangeException(nameof(minutes));
            if (Paused || StoryPending || Ended) return 0;
            if (treatment && (!Hospitalized || TreatmentRemaining == 0)) return 0;
            int advanced = 0;
            for (int i = 0; i < minutes && !Ended; i++)
            {
                // Only explicit treatment can extend the clock beyond morning end.
                if (!treatment && Minute >= _rules.EndMinute) break;
                var previous = Activity;
                int intervalStart = Minute;
                // Credit [Minute, Minute + 1), never credit minutes outside the slot.
                if (Activity == MorningActivity.Study && Minute >= _rules.ClassStart && Minute < _rules.ClassEnd) ClassMinutes++;
                if (Activity == MorningActivity.Work && Minute >= _rules.WorkStart && Minute < _rules.WorkEnd) WorkMinutes++;
                Energy = Clamp(Energy - (Activity == MorningActivity.Work || Activity == MorningActivity.CoverShift ? 0.12 : 0.04));
                if (Hospitalized) Energy = Math.Max(1, Energy);
                Minute++;
                advanced++;
                if (treatment) TreatmentRemaining--;
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
                bool commitmentBoundary = false;
                if (Commitment != null)
                {
                    if (Commitment.Tick(previous, intervalStart))
                    { SettleCommitment(); commitmentBoundary = true; }
                    if (previous == MorningActivity.Study && Commitment.TryOffer(Minute, _rules.ClassEnd, ClassMinutes))
                        Record("DIEN THOAI / Quan ly: Quan dang thieu nguoi. Em co the sang lam thay 20 phut khong? Mo tin de chon. DONG HO TAM DUNG.");
                    if (Minute >= _rules.ClassEnd && Commitment.MarkUnavailable())
                        Record("Tinh huong khong kich hoat: can vao hoc som va tham gia it nhat 10 phut.");
                }
                if (Ended) FinishMorning();
                // A fast-forward batch must return control at schedule boundaries or activity completion.
                if ((treatment && TreatmentRemaining == 0) || Minute == _rules.EndMinute ||
                    StoryPending || commitmentBoundary || Activity != previous || Minute == _rules.ClassStart || Minute == _rules.ClassEnd ||
                    Minute == _rules.WorkStart || Minute == _rules.WorkEnd) break;
            }
            return advanced;
        }

        public bool ChooseCommitment(CommitmentChoice choice)
        {
            if (Paused || Ended || Commitment == null || !Commitment.Choose(choice, Minute, _rules.ClassEnd)) return false;
            if (choice == CommitmentChoice.CoverShift) Cancel();
            Record($"CAM KET / Da chon {choice}. CHUA nhan thuong. Can lam them 20 phut truoc {Commitment.Deadline / 60:00}:{Commitment.Deadline % 60:00}.");
            return true;
        }

        private void SettleCommitment()
        {
            if (Commitment.State == CommitmentState.Completed)
            {
                if (Commitment.Choice == CommitmentChoice.CoverShift)
                { Money += 20000; Relations = Clamp(Relations + 3); Record("CAM KET / Lam thay du 20 phut: +20.000d, +3 quan he. Khong tinh vao luong ca thuong."); }
                else
                { Relations = Clamp(Relations - 2); Record("CAM KET / O lai hoc du 20 phut: quan ly that vong, -2 quan he. Hoc tap tinh theo lich rieng."); }
            }
            else
            { Relations = Clamp(Relations - 4); Record("CAM KET / Qua han, chua du so phut: -4 quan he, khong thuong. Hau qua lich tinh rieng."); }
            if (Activity == MorningActivity.CoverShift) Activity = MorningActivity.None;
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
