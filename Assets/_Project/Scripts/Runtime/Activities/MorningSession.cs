using QuaThangNayDa.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QuaThangNayDa
{
    [DefaultExecutionOrder(50)]
    public sealed class MorningSession : MonoBehaviour
    {
        public MorningConfig Config;
        public TravelController Player;
        public StudentPhone Phone;
        public SaveCoordinator Saves;
        public HospitalSession Hospital;
        public Transform BedView, BedExit;
        private float _sleepShade;
        private bool _bedSleeping;
        public bool HasStarted => _started;
        private float _secondsPerMinute;
        public MorningSimulation Simulation { get; private set; }
        private bool _started, _panel = true;
        private float _minuteFraction;
        private ActivityStation _target, _selected;
        private string _message = "";
        private Vector2 _scroll;
        private Vector3 _targetHitPoint, _selectedLocalPoint;
        private float _hintAlpha;
        private string _hintText = "";

        private void Start()
        {
            Simulation = new MorningSimulation(Config.Rules, Config.EnableStory);
            Simulation.HospitalRules = (Config.HospitalRules ?? new HospitalRules()).Copy();
            _secondsPerMinute = Config.RealSecondsPerMinute;
            Player.ShowDiagnostics = false;
            Player.EnterBed(BedView, BedExit);
            SyncControls();
        }
        // TravelController updates the camera in LateUpdate (order 0).
        // Resolve both the prompt and F after that update, against the rendered view.
        private void LateUpdate()
        {
            if (Simulation == null) return;
            if (!Player.IsPaused)
                _sleepShade = Mathf.MoveTowards(_sleepShade,
                    Player.IsInBed && _bedSleeping ? .82f : 0, Time.deltaTime * .7f);
            _target = null;
            if (!Player.IsRiding && !Player.IsInBed)
                _target = ActivityTargeting.Find(Player.View.transform.position, Player.View.transform.forward, out _targetHitPoint);
            var keyboard = Keyboard.current;
            bool showHint = _started && !_panel && !Player.IsPaused && !Simulation.StoryPending
                && !Simulation.Ended && !Simulation.Hospitalized && Simulation.Activity == MorningActivity.None
                && !(Phone != null && Phone.IsOpen) && _target != null;
            if (showHint) _hintText = "[F] " + _target.Label;
            else { _hintText = ""; _hintAlpha = 0; }
            _hintAlpha = Mathf.MoveTowards(_hintAlpha, showHint ? 1f : 0f, Time.unscaledDeltaTime * 7f);
            if (_started && !Player.IsPaused && !Simulation.StoryPending && !Simulation.Hospitalized && !(Phone != null && Phone.IsOpen) && keyboard != null)
            {
                if (keyboard.tabKey.wasPressedThisFrame) { _panel = !_panel; _selected = null; _scroll = Vector2.zero; _message = ""; }
                if (!_panel && Simulation.Activity == MorningActivity.None && _target != null && keyboard.fKey.wasPressedThisFrame)
                {
                    _selected = _target;
                    _selectedLocalPoint = _selected.transform.InverseTransformPoint(_targetHitPoint);
                    _panel = true; _scroll = Vector2.zero; _message = "";
                }
                else if (!_panel && keyboard.fKey.wasPressedThisFrame)
                    _message = Player.IsRiding ? "Can xuong xe (E) truoc khi tuong tac."
                        : Simulation.Activity != MorningActivity.None ? "Dang hoat dong. Bam Tab de xem/dung hoat dong."
                        : "Lai gan do vat (duoi 3m), nhin vao do vat de hien goi y [F].";
            }
            Simulation.Paused = !_started || Player.IsPaused;
            if (!Simulation.Paused && !Simulation.StoryPending && !Simulation.Ended && !Simulation.Hospitalized)
            {
                _minuteFraction += Time.deltaTime / Mathf.Max(0.1f, _secondsPerMinute);
                while (_minuteFraction >= 1 && !Simulation.StoryPending && !Simulation.Ended)
                {
                    _minuteFraction -= 1;
                    Simulation.Advance(1);
                }
                if (Simulation.StoryPending) _minuteFraction = 0;
            }
            SyncControls();
        }

        private void SyncControls()
        {
            Player.OverlayOpen = _panel || Player.IsInBed || !_started || Simulation.Ended || Simulation.StoryPending || Simulation.Hospitalized || (Phone != null && Phone.IsOpen);
            Player.ControlsBlocked = Player.OverlayOpen || Simulation.Activity != MorningActivity.None;
        }
        private bool CanUseSelected(out string reason)
        {
            reason = "";
            if (Player.IsRiding)
            { reason = "Ban dang ngoi tren xe. Dong bang va bam E de xuong xe."; return false; }
            if (_selected == null)
            { reason = "Chua chon bang hoat dong. Dong bang, nhin bang va bam F."; return false; }
            Vector3 origin = Player.View.transform.position;
            // Recheck the visible point accepted by F, not a different ray to the object's centre.
            Vector3 direction = _selected.transform.TransformPoint(_selectedLocalPoint) - origin;
            if (direction.magnitude > ActivityTargeting.Reach + .00001f)
            { reason = "Ban dung qua xa bang. Dong bang va tien lai gan duoi 3m."; return false; }
            if (!ActivityTargeting.Visible(_selected, origin, origin + direction))
            { reason = "Khong con thay bang da chon. Dong bang, lui mot chut va bam F lai."; return false; }
            return true;
        }
        private static string Clock(int minute) => $"{minute / 60:00}:{minute % 60:00}";
        public void CloseActivityPanel() { _panel = false; _selected = null; SyncControls(); }
        public void WakeMorning()
        {
            if (_started || Player.IsPaused) return;
            _started = true; _panel = false; Simulation.Paused = false;
            SyncControls();
        }
        public void StopBedAction()
        {
            if (!Player.IsInBed || Player.IsPaused || Simulation.StoryPending || Simulation.Hospitalized) return;
            Simulation.Cancel();
            Player.LeaveBed();
            _bedSleeping = false;
            _panel = false; _selected = null; _minuteFraction = 0;
            SyncControls();
        }
        public bool StartBedRest()
        {
            if (!_started || Player.IsPaused || BedView == null || BedExit == null || Player.IsInBed || Player.IsRiding ||
                !Simulation.Start(MorningActivity.Rest, out _message)) return false;
            Player.EnterBed(BedView, BedExit);
            _bedSleeping = true;
            _minuteFraction = 0;
            SyncControls();
            return true;
        }
        public void OnHospitalAdmission()
        {
            _bedSleeping = false;
            _panel = false; _selected = _target = null; _minuteFraction = 0; _message = "";
            if (Phone != null) Phone.RestoreMessages(Phone.ReadMessages);
            SyncControls();
        }
        public SaveGame CaptureSave()
        {
            return new SaveGame { SavedUtc = System.DateTime.UtcNow.ToString("O"), Simulation = Simulation.Capture(),
                Travel = Player.CaptureTravel(), Started = _started, MinuteFraction = _minuteFraction,
                RealSecondsPerMinute = _secondsPerMinute, ReadMessages = Phone != null ? Phone.ReadMessages : 0,
                HospitalProtection = Hospital != null ? Hospital.ProtectionRemaining : 0, BedSleeping = _bedSleeping && Player.IsInBed };
        }
        public void RestoreSave(SaveGame data)
        {
            data.Validate();
            if (data.Simulation.Hospitalized && (Hospital == null || Hospital.WardBedView == null || Hospital.Ward == null))
                throw new System.InvalidOperationException("This scene is missing the hospital bed view.");
            if (data.Travel.InBed && !data.Simulation.Hospitalized && (BedView == null || BedExit == null))
                throw new System.InvalidOperationException("This scene is missing the saved bedroom.");
            var restored = MorningSimulation.Restore(data.Simulation);
            Player.RestoreTravel(data.Travel);
            Simulation = restored;
            Simulation.Paused = true;
            _started = data.Started;
            _bedSleeping = data.BedSleeping;
            if (restored.Hospitalized)
            {
                // Also migrate old admission saves that stored a standing reception pose.
                Hospital.RestoreWardPose();
                _bedSleeping = false; _sleepShade = 0;
            }
            else if (data.Travel.InBed)
            {
                Player.EnterBed(BedView, BedExit);
                _sleepShade = _bedSleeping ? .82f : 0;
            }
            else _sleepShade = 0;
            _minuteFraction = data.MinuteFraction;
            _secondsPerMinute = data.RealSecondsPerMinute;
            _selected = _target = null;
            _panel = true; _scroll = Vector2.zero; _message = "Da tai. Esc/Tiep tuc de choi; khong tinh thoi gian ngoai doi.";
            if (Phone != null) Phone.RestoreMessages(data.ReadMessages);
            if (Hospital != null) Hospital.RestoreProtection(data.HospitalProtection);
            SyncControls();
        }
        private void OnGUI()
        {
            if (Simulation == null) return;
            var sim = Simulation;
            var rules = sim.Rules;
            float width = Mathf.Min(680, Screen.width - 24);
            if (_sleepShade > 0)
            {
                var color = GUI.color;
                GUI.color = new Color(0, 0, 0, _sleepShade);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = color;
            }
            GUILayout.BeginArea(new Rect(12, 12, width, 90), GUI.skin.box);
            GUILayout.Label($"{(Hospital != null ? "M6" : Saves != null ? "M4" : Config.EnableStory ? "M3" : "M2")} | {Clock(sim.Minute)} | {(sim.Paused || sim.StoryPending || sim.Hospitalized ? "DUNG GIO" : "GIO DANG CHAY")} | P: dien thoai | Tab: lich | Esc: tam dung");
            GUILayout.Label($"Tien {sim.Money:N0}d | Suc khoe {sim.Health:0} | Nang luong {sim.Energy:0} | Hoc tap {sim.Learning:0} | Quan he {sim.Relations:0}");
            GUILayout.Label($"{(Player.IsInBed ? "DANG NAM TREN GIUONG" : Player.IsRiding ? "DANG TREN XE" : "DANG DI BO")} | {sim.Activity} | Hoc {sim.ClassMinutes} phut | Lam {sim.WorkMinutes} phut | No vien phi: {sim.MedicalDebt:N0}d");
            GUILayout.EndArea();
            if (Phone != null && Phone.IsOpen) return;
            if (sim.Hospitalized && Hospital != null) { Hospital.DrawPanel(); return; }
            if (Player.IsInBed && _started)
            {
                GUILayout.BeginArea(new Rect(12, 112, width, 180), GUI.skin.box);
                GUILayout.Label(sim.Activity == MorningActivity.Rest ? "DANG NGU TREN GIUONG..." : "BAN VAN DANG NAM TREN GIUONG");
                GUILayout.Label("Chi xuong giuong khi bam DUNG HANH DONG. P: luu/tai game.");
                GUI.enabled = !Player.IsPaused && !sim.StoryPending;
                if (sim.Activity == MorningActivity.Rest && !sim.Ended && GUILayout.Button("NGU TIEP - toi da 10 phut"))
                { sim.Advance(10); _minuteFraction = 0; }
                if (GUILayout.Button("DUNG HANH DONG - XUONG GIUONG")) StopBedAction();
                if (sim.Ended) GUILayout.Label("Buoi sang da ket thuc. P: xem nhat ky / tai game.");
                GUI.enabled = true;
                GUILayout.EndArea();
                return;
            }
            if (!_panel && _started && !sim.Ended)
            {
                GUI.Label(new Rect(Screen.width / 2f - 6, Screen.height / 2f - 10, 20, 20), "+");
                if (_hintAlpha > 0 && !Player.IsPaused && !sim.StoryPending)
                {
                    Color previous = GUI.color;
                    GUI.color = new Color(1, 1, 1, _hintAlpha);
                    float hintWidth = Mathf.Min(360, Screen.width - 24);
                    GUI.Box(new Rect((Screen.width - hintWidth) / 2, Screen.height * .68f, hintWidth, 30), _hintText);
                    GUI.color = previous;
                }
                GUI.Label(new Rect(20, 195, width, 65), _message);
                if (sim.Activity != MorningActivity.None && GUI.Button(new Rect(20, 155, 300, 35), "Tab de xem hoat dong / tien nhanh")) _panel = true;
                return;
            }
            GUILayout.BeginArea(new Rect(12, 108, width, Mathf.Max(100, Screen.height - 175)), GUI.skin.box);
            // Keep the selected station / rejection visible even after scrolling the journal.
            if (_selected != null)
            {
                GUILayout.Label("DANG TUONG TAC: " + _selected.Label);
                if (sim.Activity == MorningActivity.None && !_selected.HospitalReception) GUILayout.Label(sim.Preview(_selected.Activity));
            }
            if (!string.IsNullOrEmpty(_message)) GUILayout.Label(_message);
            _scroll = GUILayout.BeginScrollView(_scroll);
            GUI.enabled = !Player.IsPaused && !sim.StoryPending;
            if (!_started)
            {
                GUILayout.Label("HUONG DAN AN TOAN: dong ho chua chay. P mo dien thoai." + (Saves != null ? " Luu/tai trong dien thoai." : " Ban thu chua co luu game."));
                GUILayout.Label("Thuc day luc 07:00; tim diem an sang, lop hoc ben trai, ca phe ben phai.");
                GUILayout.Label("Di bo lai gan, nhin vao do vat. Khi hien [F], bam F roi xac nhan bat dau. Esc tam dung/tiep tuc.");
                if (GUILayout.Button("THUC DAY - BAT DAU BUOI SANG")) WakeMorning();
            }
            GUILayout.Label($"LICH KHOA: Hoc {Clock(rules.ClassStart)}–{Clock(rules.ClassEnd)} | Lam {Clock(rules.WorkStart)}–{Clock(rules.WorkEnd)} | Ket thuc {Clock(rules.EndMinute)}");
            GUILayout.Label($"Hoc du +12 hoc tap; vang het -6 (theo ti le). Luong {rules.WagePerMinute}d/phut; vang ca het -4 quan he (theo ti le).");
            if (_started && !sim.Ended)
            {
                if (sim.Activity != MorningActivity.None)
                {
                    GUILayout.Label("TIEN NHANH: toi da 10 phut, dung tai moc lich/hoan thanh hoat dong. Khong tu dong chay tiep.");
                    if (GUILayout.Button("THUC HIEN TIEP - toi da 10 phut")) { sim.Advance(10); _minuteFraction = 0; }
                    if (GUILayout.Button("DUNG HOAT DONG - giu phut da lam")) sim.Cancel();
                }
                else if (_selected != null && _selected.HospitalReception && Hospital != null)
                {
                    Hospital.DrawReception();
                }
                else if (_selected != null)
                {
                    bool eligible = sim.CanStart(_selected.Activity, out string reason);
                    GUI.enabled = !Player.IsPaused && eligible;
                    if (GUILayout.Button(eligible ? "XAC NHAN BAT DAU" : "CHUA THE BAT DAU - xem thong bao o tren"))
                    {
                        if (!CanUseSelected(out string interactionReason)) _message = interactionReason;
                        else if (_selected.Activity == MorningActivity.Rest && BedView != null && BedExit != null
                            ? StartBedRest() : sim.Start(_selected.Activity, out _message))
                        { _minuteFraction = 0; _message = "DA BAT DAU: " + _selected.Label + ". So phut se tang; co the bam THUC HIEN TIEP."; }
                    }
                    GUI.enabled = !Player.IsPaused;
                    if (!eligible && !reason.Contains("GIO")) GUILayout.Label(reason);
                }
                if (GUILayout.Button("DONG BANG (Tab)")) { _panel = false; _selected = null; }
            }
            if (sim.Ended) GUILayout.Label("TONG KET BUOI SANG - xem tung khoan ben duoi. Stop/Play de choi lai; P mo dien thoai de tai neu co ban luu.");
            foreach (string line in sim.Journal) GUILayout.Label(line);
            GUI.enabled = true;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
            SyncControls();
        }
        private void OnDisable()
        {
            if (Player == null) return;
            Player.ControlsBlocked = Player.OverlayOpen = false;
        }
    }
}
