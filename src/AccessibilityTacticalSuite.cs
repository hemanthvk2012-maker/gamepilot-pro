using System;
using System.Drawing;

namespace GamePilotNative
{
    public enum ColorblindMode
    {
        StandardNeonGreen,
        DeuteranopiaCyan,
        ProtanopiaYellow,
        HighContrastMagenta
    }

    public enum SpikeState
    {
        Idle,
        Planted,
        Defusing,
        Exploded,
        Defused
    }

    public class SpikeTimer
    {
        public SpikeState State { get; private set; }
        public DateTime PlantTime { get; private set; }
        public const double TOTAL_DURATION_SEC = 45.0;
        public const double DEFUSE_TIME_SEC = 7.0;
        public const double HALF_DEFUSE_TIME_SEC = 3.5;

        public SpikeTimer()
        {
            State = SpikeState.Idle;
        }

        public void Plant()
        {
            State = SpikeState.Planted;
            PlantTime = DateTime.Now;
        }

        public void Reset()
        {
            State = SpikeState.Idle;
        }

        public double SecondsRemaining
        {
            get
            {
                if (State != SpikeState.Planted) return 0;
                double elapsed = (DateTime.Now - PlantTime).TotalSeconds;
                double remaining = TOTAL_DURATION_SEC - elapsed;
                return remaining > 0 ? remaining : 0;
            }
        }

        public bool CanDefuse
        {
            get { return SecondsRemaining >= DEFUSE_TIME_SEC; }
        }

        public bool CanHalfDefuse
        {
            get { return SecondsRemaining >= HALF_DEFUSE_TIME_SEC; }
        }

        public Color GetIndicatorColor()
        {
            double sec = SecondsRemaining;
            if (sec > 15.0) return Color.FromArgb(16, 185, 129);  // Green (Safe)
            if (sec > 7.0) return Color.FromArgb(245, 158, 11);   // Yellow/Orange (Critical Defuse Window)
            return Color.FromArgb(239, 68, 68);                   // Flashing Red (RUN / SAVE WEAPON)
        }

        public string GetDefuseGuidanceText()
        {
            double sec = SecondsRemaining;
            if (State != SpikeState.Planted) return "";
            if (sec > 15.0) return string.Format("SPIKE TICKING: {0:F1}s [SAFE WINDOW]", sec);
            if (sec >= DEFUSE_TIME_SEC) return string.Format("DEFUSE NOW: {0:F1}s [DEFUSE TIME: 7.0s REQUIRED]", sec);
            if (sec >= HALF_DEFUSE_TIME_SEC) return string.Format("CRITICAL: {0:F1}s [CAN ONLY HALF DEFUSE 3.5s!]", sec);
            return string.Format("CANNOT DEFUSE: {0:F1}s [SAVE WEAPON / RUN!]", sec);
        }
    }

    public class VisualAudioRadar
    {
        public bool ShowLeftPing { get; set; }
        public bool ShowRightPing { get; set; }
        public bool ShowCenterPing { get; set; }
        public string LastAudioSource { get; set; }
        public DateTime LastPingTime { get; set; }

        public VisualAudioRadar()
        {
            LastAudioSource = "Ready";
            LastPingTime = DateTime.MinValue;
        }

        public void TriggerPing(string direction, string sourceName)
        {
            LastAudioSource = sourceName;
            LastPingTime = DateTime.Now;

            ShowLeftPing = (direction == "LEFT" || direction == "BOTH");
            ShowRightPing = (direction == "RIGHT" || direction == "BOTH");
            ShowCenterPing = (direction == "CENTER");
        }

        public bool IsActive
        {
            get { return (DateTime.Now - LastPingTime).TotalMilliseconds < 1800; }
        }
    }

    public class RecoilCadenceMetronome
    {
        public bool IsFiring { get; set; }
        public DateTime LastShotTime { get; set; }
        public const double RESET_WINDOW_SEC = 0.285; // Optimal Vandal/Phantom 2-burst reset delay

        public RecoilCadenceMetronome()
        {
            LastShotTime = DateTime.MinValue;
        }

        public void FireShot()
        {
            LastShotTime = DateTime.Now;
            IsFiring = true;
        }

        public bool IsResetReady
        {
            get
            {
                if (!IsFiring) return true;
                return (DateTime.Now - LastShotTime).TotalSeconds >= RESET_WINDOW_SEC;
            }
        }
    }

    public class AccessibilityTacticalSuite
    {
        private static readonly object lockObj = new object();
        private static AccessibilityTacticalSuite instance;

        public SpikeTimer Spike { get; private set; }
        public VisualAudioRadar AudioRadar { get; private set; }
        public RecoilCadenceMetronome Metronome { get; private set; }
        public ColorblindMode ActiveColorblindMode { get; set; }
        public bool EnableSpikeHUD { get; set; }
        public bool EnableAudioRadarHUD { get; set; }
        public bool EnableRecoilMetronomeHUD { get; set; }

        public static AccessibilityTacticalSuite Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new AccessibilityTacticalSuite();
                        }
                    }
                }
                return instance;
            }
        }

        private AccessibilityTacticalSuite()
        {
            Spike = new SpikeTimer();
            AudioRadar = new VisualAudioRadar();
            Metronome = new RecoilCadenceMetronome();
            ActiveColorblindMode = ColorblindMode.StandardNeonGreen;
            EnableSpikeHUD = true;
            EnableAudioRadarHUD = true;
            EnableRecoilMetronomeHUD = true;
        }

        public Color GetReticleColor()
        {
            switch (ActiveColorblindMode)
            {
                case ColorblindMode.DeuteranopiaCyan:
                    return Color.FromArgb(0, 240, 255); // Electric Cyan
                case ColorblindMode.ProtanopiaYellow:
                    return Color.FromArgb(255, 230, 0); // Vibrant Solar Yellow
                case ColorblindMode.HighContrastMagenta:
                    return Color.FromArgb(255, 0, 128); // High Contrast Magenta
                default:
                    return Color.FromArgb(0, 255, 128); // Standard Neon Green
            }
        }
    }
}
