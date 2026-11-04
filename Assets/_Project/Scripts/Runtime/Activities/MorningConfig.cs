using QuaThangNayDa.Core;
using UnityEngine;

namespace QuaThangNayDa
{
    [CreateAssetMenu(menuName = "Qua Thang Nay Da/Morning Rules")]
    public sealed class MorningConfig : ScriptableObject
    {
        public MorningRules Rules = new MorningRules();
        [Min(0.1f)] public float RealSecondsPerMinute = 6;
    }
}
