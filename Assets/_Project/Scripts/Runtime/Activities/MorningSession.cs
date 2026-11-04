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
        public MorningSimulation Simulation { get; private set; }
        private bool _started, _panel = true;
        private float _minuteFraction;
        private ActivityStation _target, _selected;
        private string _message = "";
        private Vector2 _scroll;

        private void Start()
        {
            Simulation = new MorningSimulation(Config.Rules);
            Player.ShowDiagnostics = false;
            SyncControls();
        }
        private void Update()
        {
            if (Simulation == null) return;
            _target = null;
            if (!Player.IsRiding && Physics.Raycast(Player.View.transform.position, Player.View.transform.forward,
                out RaycastHit hit, 3f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                _target = hit.collider.GetComponent<ActivityStation>();
            var keyboard = Keyboard.current;
            if (_started && !Player.IsPaused && keyboard != null)
            {
                if (keyboard.tabKey.wasPressedThisFrame) { _panel = !_panel; _selected = null; _scroll = Vector2.zero; _message = ""; }
                if (!_panel && Simulation.Activity == MorningActivity.None && _target != null && keyboard.fKey.wasPressedThisFrame)
                { _selected = _target; _panel = true; _scroll = Vector2.zero; _message = ""; }
                else if (!_panel && keyboard.fKey.wasPressedThisFrame)
                    _message = Player.IsRiding ? "Can xuong xe (E) truoc khi tuong tac."
                        : Simulation.Activity != MorningActivity.None ? "Dang hoat dong. Bam Tab de xem/dung hoat dong."
                        : "Chua nham dung bang: den gan duoi 3m, dua dau + vao phan than bang roi bam F.";
            }
            Simulation.Paused = !_started || Player.IsPaused;
            if (!Simulation.Paused && !Simulation.Ended)
            {
                _minuteFraction += Time.deltaTime / Mathf.Max(0.1f, Config.RealSecondsPerMinute);
                while (_minuteFraction >= 1 && !Simulation.Ended)
                {
                    _minuteFraction -= 1;
                    Simulation.Advance(1);
                }
            }
            SyncControls();
        }

        private void SyncControls()
        {
            Player.OverlayOpen = _panel || !_started || Simulation.Ended;
            Player.ControlsBlocked = Player.OverlayOpen || Simulation.Activity != MorningActivity.None;
        }
        private bool CanUseSelected()
        {
            if (_selected == null || Player.IsRiding) return false;
            Vector3 origin = Player.View.transform.position;
            Vector3 direction = _selected.transform.position - origin;
            return direction.magnitude <= 3.5f && Physics.Raycast(origin, direction.normalized, out RaycastHit hit, 3.5f)
                && hit.collider.GetComponent<ActivityStation>() == _selected;
        }
        private static string Clock(int minute) => $"{minute / 60:00}:{minute % 60:00}";
        private void OnGUI()
        {
            if (Simulation == null) return;
            var sim = Simulation;
            var rules = sim.Rules;
            float width = Mathf.Min(680, Screen.width - 24);
            GUILayout.BeginArea(new Rect(12, 12, width, 90), GUI.skin.box);
            GUILayout.Label($"M2 | {Clock(sim.Minute)} | {(sim.Paused ? "DUNG GIO" : "GIO DANG CHAY")} | Tab: lich/nhat ky | Esc: tam dung");
            GUILayout.Label($"Tien {sim.Money:N0}d | Suc khoe {sim.Health:0} | Nang luong {sim.Energy:0} | Hoc tap {sim.Learning:0} | Quan he {sim.Relations:0}");
            GUILayout.Label($"{sim.Activity} | Hoc {sim.ClassMinutes} phut | Lam {sim.WorkMinutes} phut | WASD/chuot/E/Space/R nhu M1");
            GUILayout.EndArea();
            if (!_panel && _started && !sim.Ended)
            {
                GUI.Label(new Rect(Screen.width / 2f - 6, Screen.height / 2f - 10, 20, 20), "+");
                GUI.Label(new Rect(20, 110, width, 40), _target != null ? "F: " + _target.Label : "Toi bang AN SANG / LOP HOC / CA PHE / NGHI; nhin bang va bam F.");
                GUI.Label(new Rect(20, 195, width, 65), _message);
                if (sim.Activity != MorningActivity.None && GUI.Button(new Rect(20, 155, 300, 35), "Tab de xem hoat dong / tien nhanh")) _panel = true;
                return;
            }
            GUILayout.BeginArea(new Rect(12, 108, width, Mathf.Max(100, Screen.height - 175)), GUI.skin.box);
            // Keep the selected station / rejection visible even after scrolling the journal.
            if (_selected != null)
            {
                GUILayout.Label("DANG TUONG TAC: " + _selected.Label);
                if (sim.Activity == MorningActivity.None) GUILayout.Label(sim.Preview(_selected.Activity));
            }
            if (!string.IsNullOrEmpty(_message)) GUILayout.Label(_message);
            _scroll = GUILayout.BeginScrollView(_scroll);
            GUI.enabled = !Player.IsPaused;
            if (!_started)
            {
                GUILayout.Label("HUONG DAN AN TOAN: dong ho chua chay. Day la M2, chua co tinh huong va luu game.");
                GUILayout.Label("Thuc day luc 07:00; an sang o bang gan xe, di hoc ben trai, lam o ca phe ben phai.");
                GUILayout.Label("Can di bo toi bang, nhin bang va bam F. Panel thuong VAN CHAY GIO. Esc de tam dung.");
                if (GUILayout.Button("THUC DAY - BAT DAU BUOI SANG")) { _started = true; _panel = false; sim.Paused = false; }
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
                else if (_selected != null)
                {
                    bool eligible = sim.CanStart(_selected.Activity, out string reason);
                    GUI.enabled = !Player.IsPaused && eligible;
                    if (GUILayout.Button(eligible ? "XAC NHAN BAT DAU" : "CHUA THE BAT DAU - xem thong bao o tren"))
                    {
                        if (!CanUseSelected()) _message = "Can xuong xe va dung sat bang hoat dong.";
                        else if (sim.Start(_selected.Activity, out _message))
                        { _minuteFraction = 0; _message = "DA BAT DAU: " + _selected.Label + ". So phut se tang; co the bam THUC HIEN TIEP."; }
                    }
                    GUI.enabled = !Player.IsPaused;
                    if (!eligible && !reason.Contains("GIO")) GUILayout.Label(reason);
                }
                if (GUILayout.Button("DONG BANG (Tab)")) { _panel = false; _selected = null; }
            }
            if (sim.Ended) GUILayout.Label("TONG KET BUOI SANG M2 - xem tung khoan ben duoi. Choi lai bang Stop/Play; chua co luu game.");
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
