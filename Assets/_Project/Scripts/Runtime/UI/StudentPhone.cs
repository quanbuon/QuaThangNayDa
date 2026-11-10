using QuaThangNayDa.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace QuaThangNayDa
{
    // Code-drawn prototype icon: no downloaded assets, fonts or external notification service.
    [DefaultExecutionOrder(100)]
    public sealed class StudentPhone : MonoBehaviour
    {
        public MorningSession Session;
        public bool IsOpen { get; private set; }
        private int _readCount, _knownCount;
        private float _toastUntil;
        private Vector2 _scroll;
        private GUIStyle _wrapped;
        public int ReadMessages => _readCount;
        public void RestoreMessages(int count)
        {
            _readCount = count;
            _knownCount = Session.Simulation.Journal.Count;
            _scroll = Vector2.zero;
            _toastUntil = 0;
            IsOpen = false;
        }

        private void Update()
        {
            var sim = Session.Simulation;
            if (sim == null) return;
            if (sim.Journal.Count > _knownCount)
            { _knownCount = sim.Journal.Count; _toastUntil = Time.unscaledTime + 4; }
            if (sim.StoryPending) IsOpen = true;
            else if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame && !Session.Player.IsPaused)
                IsOpen = !IsOpen;
            if (IsOpen)
            {
                Session.Player.OverlayOpen = Session.Player.ControlsBlocked = true;
            }
        }

        private static string Clock(int minute) => $"{minute / 60:00}:{minute % 60:00}";
        private void OnGUI()
        {
            var sim = Session.Simulation;
            if (sim == null) return;
            _wrapped ??= new GUIStyle(GUI.skin.label) { wordWrap = true, fontSize = 14 };
            int unread = Mathf.Max(0, sim.Journal.Count - _readCount);
            float iconX = Screen.width - 82;
            var previousColor = GUI.color;
            GUI.color = unread > 0 ? new Color(1, 0.75f, 0.25f) : Color.white;
            if (GUI.Button(new Rect(iconX, 14, 64, 86), ""))
                IsOpen = sim.StoryPending || !IsOpen;
            GUI.Box(new Rect(iconX + 13, 23, 38, 48), "");
            GUI.Label(new Rect(iconX + 8, 73, 55, 25), "P  [" + unread + "]");
            GUI.color = previousColor;
            if (!IsOpen)
            {
                if (Time.unscaledTime < _toastUntil && unread > 0)
                    GUI.Box(new Rect(Mathf.Max(12, Screen.width - 310), 110, 290, 50), "DIEN THOAI: " + unread + " tin chua doc\nBam P de xem");
                return;
            }
            // Reading the screen counts as seeing current messages, not future arrivals.
            _readCount = sim.Journal.Count;
            float width = Mathf.Min(420, Screen.width - 24);
            float height = Mathf.Max(180, Screen.height - 125);
            GUILayout.BeginArea(new Rect((Screen.width - width) / 2, 110, width, height), GUI.skin.window);
            GUILayout.Label("DIEN THOAI  |  " + Clock(sim.Minute), _wrapped);
            GUILayout.Label($"Tien: {sim.Money:N0}d | NO VIEN PHI: {sim.MedicalDebt:N0}d", _wrapped);
            GUILayout.Label(sim.StoryPending ? "TIN QUAN TRONG - DONG HO DANG DUNG" : sim.Hospitalized ? "DANG NAM VIEN - DONG HO DANG DUNG" : sim.Paused ? "GAME DANG TAM DUNG" : "Dong ho van chay khi doc tin thuong", _wrapped);
            GUI.enabled = !Session.Player.IsPaused;
            if (!sim.StoryPending && GUILayout.Button("DONG DIEN THOAI (P)")) IsOpen = false;
            _scroll = GUILayout.BeginScrollView(_scroll);
            if (Session.Saves != null)
            {
                GUI.enabled = true; // Save/load remains available while paused or at game over.
                if (Session.Saves.DrawControls())
                { GUILayout.EndScrollView(); GUILayout.EndArea(); return; }
                GUI.enabled = !Session.Player.IsPaused;
            }
            if (sim.StoryPending)
            {
                GUILayout.Label("QUAN LY CA PHE\nQuan dang thieu nguoi. Em co the sang lam thay 20 phut khong?", _wrapped);
                GUILayout.Label("Chon CHUA co thuong. O lai: hoc them 20 phut truoc " + Clock(sim.Rules.ClassEnd) + ", quan he -2 khi hoan thanh. Bo do/qua han: quan he -4.", _wrapped);
                GUILayout.Label("Lam thay: tu toi QUAN LY, bat dau lam va du 20 phut trong 45 phut ke tu luc xac nhan. Hoan thanh +20.000d, +3 quan he; qua han -4, khong thuong. Mat phut hoc tinh rieng.", _wrapped);
                if (GUILayout.Button("O LAI LOP - CAM KET HOC TIEP 20 PHUT", GUILayout.MinHeight(36))) Choose(CommitmentChoice.StayInClass);
                if (GUILayout.Button("NHAN LAM THAY - TU TOI GAP QUAN LY", GUILayout.MinHeight(36))) Choose(CommitmentChoice.CoverShift);
            }
            var commitment = sim.Commitment;
            if (commitment != null && commitment.Choice != CommitmentChoice.None)
                GUILayout.Label($"CAM KET: {commitment.Choice} | {commitment.State}\n{commitment.Progress}/{commitment.RequiredMinutes} phut | Han {Clock(commitment.Deadline)}", _wrapped);
            GUILayout.Label("HOP THU - TIN MOI NHAT O TREN", _wrapped);
            for (int i = sim.Journal.Count - 1; i >= 0; i--)
            {
                GUILayout.Label(sim.Journal[i], _wrapped);
                GUILayout.Space(8);
            }
            GUILayout.EndScrollView();
            GUI.enabled = true;
            GUILayout.EndArea();
        }
        private void Choose(CommitmentChoice choice)
        {
            if (!Session.Simulation.ChooseCommitment(choice)) return;
            IsOpen = false;
            Session.CloseActivityPanel();
        }
    }
}
