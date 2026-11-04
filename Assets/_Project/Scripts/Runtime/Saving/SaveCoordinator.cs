using System;
using System.IO;
using UnityEngine;

namespace QuaThangNayDa
{
    [DefaultExecutionOrder(200)]
    public sealed class SaveCoordinator : MonoBehaviour
    {
        public MorningSession Session;
        private SaveStore _store;
        private int _savedJournal = -1, _savedMinute = -1, _pendingSlot = -1;
        private bool _pendingLoad;
        private float _nextAutoAttempt;
        private string _status = "";
        private void Awake() => _store = new SaveStore(Path.Combine(Application.persistentDataPath, "Saves-v1"));
        private void LateUpdate()
        {
            var sim = Session.Simulation;
            if (sim == null || !Session.HasStarted || sim.Ended || sim.StoryPending || Time.unscaledTime < _nextAutoAttempt) return;
            if (Session.Player.Scooter.SpeedKph > 0.5f || Session.Player.transform.position.y < -1) return;
            if (_savedJournal == sim.Journal.Count && sim.Minute - _savedMinute < 15) return;
            _nextAutoAttempt = Time.unscaledTime + 5;
            try
            {
                _store.Write(0, Session.CaptureSave());
                _savedJournal = sim.Journal.Count; _savedMinute = sim.Minute;
                _status = $"Autosave luc {sim.Minute / 60:00}:{sim.Minute % 60:00}.";
            }
            catch (Exception ex) when (Expected(ex)) { _status = "Autosave chua thanh cong: " + ex.Message; }
        }
        private static bool Expected(Exception ex) => ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException || ex is InvalidOperationException;
        // Returns true after load so the caller does not keep rendering an obsolete session snapshot.
        public bool DrawControls()
        {
            GUILayout.Label("LUU / TAI OFFLINE - autosave + 3 o thu cong");
            GUILayout.Label(_status);
            if (_pendingSlot >= 0)
            {
                GUILayout.Label(_pendingLoad ? "Tai se thay tien trinh dang choi. Xac nhan?" : "Ghi de o nay? Ban cu gan nhat duoc giu trong .bak.");
                if (GUILayout.Button("XAC NHAN"))
                {
                    int slot = _pendingSlot; _pendingSlot = -1;
                    try
                    {
                        if (_pendingLoad)
                        {
                            var data = _store.Load(slot, out bool backup);
                            Session.RestoreSave(data);
                            _savedJournal = Session.Simulation.Journal.Count;
                            _savedMinute = Session.Simulation.Minute;
                            _nextAutoAttempt = Time.unscaledTime + 5;
                            _status = backup ? "Da phuc hoi tu ban du phong. Game dang tam dung." : "Da tai. Game dang tam dung; bam Esc de tiep tuc.";
                            return true;
                        }
                        _store.Write(slot, Session.CaptureSave());
                        _status = "Da luu o " + slot + ".";
                    }
                    catch (Exception ex) when (Expected(ex)) { _status = ex.Message; }
                }
                if (GUILayout.Button("HUY")) _pendingSlot = -1;
            }
            for (int slot = 0; slot <= 3; slot++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(slot == 0 ? "Tu dong" : "O " + slot);
                if (slot != 0 && GUILayout.Button("Luu")) { _pendingSlot = slot; _pendingLoad = false; }
                bool enabled = GUI.enabled;
                GUI.enabled = enabled && _store.Exists(slot);
                if (GUILayout.Button("Tai")) { _pendingSlot = slot; _pendingLoad = true; }
                GUI.enabled = enabled;
                GUILayout.EndHorizontal();
            }
            GUILayout.Space(10);
            return false;
        }
    }
}
