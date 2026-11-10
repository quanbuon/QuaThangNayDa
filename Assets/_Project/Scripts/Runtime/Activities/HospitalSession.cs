using System.Collections.Generic;
using UnityEngine;

namespace QuaThangNayDa
{
    public sealed class HospitalSession : MonoBehaviour
    {
        public MorningSession Session;
        public Transform Reception, Ward, Exit, BikeParking, WardBedView;
        public float ImpactCooldown = 4;
        public float ProtectionRemaining { get; private set; }
        private readonly HashSet<TrafficVehicle> _contacts = new HashSet<TrafficVehicle>();
        private readonly List<TrafficVehicle> _released = new List<TrafficVehicle>();
        private GUIStyle _wrapped;

        private void Update()
        {
            if (Session.Simulation == null || Session.Player.IsPaused || Session.Simulation.StoryPending) return;
            ProtectionRemaining = Mathf.Max(0, ProtectionRemaining - Time.deltaTime);
            _released.Clear();
            Vector3 position = Session.Player.IsRiding ? Session.Player.Scooter.transform.position : Session.Player.transform.position;
            foreach (var vehicle in _contacts)
                if (vehicle == null || Vector3.Distance(vehicle.transform.position, position) > 4) _released.Add(vehicle);
            foreach (var vehicle in _released) _contacts.Remove(vehicle);
        }

        public void Impact(TrafficVehicle vehicle, float speed)
        {
            var sim = Session.Simulation;
            if (vehicle == null || sim == null || !Session.HasStarted || sim.Ended || sim.Paused
                || sim.StoryPending || sim.Hospitalized || Session.Player.IsPaused || ProtectionRemaining > 0 || _contacts.Contains(vehicle)) return;
            _contacts.Add(vehicle);
            ProtectionRemaining = ImpactCooldown;
            if (!sim.ReceiveImpact(speed)) return;
            if (sim.Hospitalized)
            {
                Session.Player.TransferToHospital(Ward, BikeParking);
                RestoreWardPose();
                Session.OnHospitalAdmission();
            }
        }

        public void RestoreProtection(float remaining)
        {
            ProtectionRemaining = Mathf.Max(remaining, ImpactCooldown);
            _contacts.Clear();
        }

        public void RestoreWardPose()
        {
            Session.Player.EnterBed(WardBedView, Ward);
        }

        public void DrawPanel()
        {
            var sim = Session.Simulation;
            float width = Mathf.Min(600, Screen.width - 24);
            GUILayout.BeginArea(new Rect(12, 112, width, Mathf.Max(150, Screen.height - 180)), GUI.skin.box);
            GUILayout.Label("BENH VIEN / DIEU TRI");
            GUILayout.Label($"Con {sim.TreatmentRemaining} phut dieu tri | No vien phi: {sim.MedicalDebt:N0}d");
            GUILayout.Label("Phi da thu khi nhap vien: tru tien dang co truoc, phan thieu ghi no. Xuat vien khong thu lai.");
            GUILayout.Label("Gio dung cho lua chon. Bam dieu tri de tua gio; van tinh vang hoc/lam va han cam ket.");
            GUILayout.Label("Khong tru hoc tap truc tiep vi bi thuong. Chi tinh phan vang hoc theo lich.");
            GUILayout.Label("P: dien thoai de luu/tai. Xe may duoc dua ve bai xe benh vien.");
            GUI.enabled = !Session.Player.IsPaused && !sim.StoryPending;
            if (sim.TreatmentRemaining > 0)
            {
                if (GUILayout.Button("DIEU TRI - toi da 10 phut (dung tai moc quan trong)")) sim.Treat(10);
            }
            else if (GUILayout.Button("XUAT VIEN"))
            {
                DischargeToWard();
            }
            GUI.enabled = true;
            GUILayout.EndArea();
        }

        public void DrawReception()
        {
            var sim = Session.Simulation;
            var rules = sim.HospitalRules;
            _wrapped ??= new GUIStyle(GUI.skin.label) { wordWrap = true };
            GUILayout.Label("NHAN VIEN TIEP NHAN: Chao ban! Day la thong tin vien phi cua ban.", _wrapped);
            GUILayout.Label($"Tien dang co: {sim.Money:N0}d", _wrapped);
            GUILayout.Label($"NO VIEN PHI CON LAI: {sim.MedicalDebt:N0}d", _wrapped);
            GUILayout.Label(sim.MedicalDebt == 0 ? "Ban hien khong no vien phi." : "Khoan no nay duoc giu sau khi xuat vien va khi luu/tai game.", _wrapped);
            GUILayout.Label($"Chan thuong nhe: {rules.LightFee:N0}d, co the xuat vien ngay. Chan thuong nang: {rules.SevereFee:N0}d, dieu tri {rules.SevereTreatmentMinutes} phut.", _wrapped);
            GUILayout.Label("Khi bi tai nan can nhap vien, ban se duoc dua den day tu dong. Tien hien co duoc tru truoc; chi phan thieu moi thanh no.", _wrapped);
            GUILayout.Label("Vi du: con 5.000d, phi 20.000d => tra 5.000d va no 15.000d. Hoi thong tin tai quay khong mat tien.", _wrapped);
            GUILayout.Label("Tang 2: phong dieu tri. Loi cau thang o ben phai quay. Ban co the xem no bat cu luc nao tren thanh trang thai va dien thoai.", _wrapped);
        }
        public bool DischargeToWard()
        {
            if (Session.Player.IsPaused || !Session.Simulation.Discharge()) return false;
            Session.Player.TransferToHospital(Ward, null);
            Session.CloseActivityPanel();
            ProtectionRemaining = ImpactCooldown;
            return true;
        }
    }
}
