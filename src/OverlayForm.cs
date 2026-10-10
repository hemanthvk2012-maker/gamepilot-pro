using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;

namespace GamePilotNative
{
    public class OverlayForm : Form
    {
        private const int WS_EX_TOPMOST = 0x00000008;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_LAYERED = 0x00000080;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int GWL_EXSTYLE = -20;

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref Margins pMarInset);

        [StructLayout(LayoutKind.Sequential)]
        public struct Margins
        {
            public int cxLeftWidth, cxRightWidth, cyTopHeight, cyBottomHeight;
        }

        private SpeechSynthesizer voiceSynthesizer;
        private Random rng = new Random();
        private bool showHeadLevelLine = true;
        private string activeBannerText = "GAMEPILOT PRO // TACTICAL SUITE ENGAGED";
        private Color bannerColor = Color.FromArgb(0, 240, 255);
        private System.Windows.Forms.Timer bannerTimer;
        private System.Windows.Forms.Timer renderTimer;
        private int activePersonalityIdx;

        // Key debounce states
        private bool f5Prev, f6Prev, f7Prev, f8Prev, f9Prev, f10Prev, f11Prev;
        private float waveAnimationProgress = 0f;

        public OverlayForm(int defaultPersonality = 2)
        {
            this.activePersonalityIdx = defaultPersonality;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.BackColor = Color.Black;
            this.DoubleBuffered = true;

            voiceSynthesizer = new SpeechSynthesizer();
            voiceSynthesizer.SetOutputToDefaultAudioDevice();

            // Ultra-smooth 60 FPS rendering timer
            renderTimer = new System.Windows.Forms.Timer { Interval = 16 };
            renderTimer.Tick += OnRenderTick;
            renderTimer.Start();

            bannerTimer = new System.Windows.Forms.Timer { Interval = 4000 };
            bannerTimer.Tick += (s, e) =>
            {
                activeBannerText = "GAMEPILOT PRO // RADIANT TACTICAL ENGINE [F5: Spike | F7: HUD | F8: Roast | F9: Hype | F10: Desi | F12/ESC: Exit]";
                bannerColor = LicenseManager.Instance.IsPro ? Color.FromArgb(255, 215, 0) : Color.FromArgb(0, 240, 255);
                bannerTimer.Stop();
            };

            // Connect live game callbacks
            ValorantLiveConnector.Instance.OnSpikePlanted += () =>
            {
                SetBanner("🚨 SPIKE PLANTED! 45.0s COUNTDOWN ENGAGED", Color.FromArgb(255, 70, 85));
                SpeakVoiceAsync("Spike planted. 45 seconds ticking.", 1);
            };

            ValorantLiveConnector.Instance.OnAudioRadarPing += (dir, src) =>
            {
                SetBanner("🔊 SONAR AUDIO: " + dir + " [" + src + "]", Color.FromArgb(0, 240, 255));
                waveAnimationProgress = 0f; // restart ripple
            };
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            int initialStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, initialStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);

            Margins margins = new Margins { cxLeftWidth = -1, cxRightWidth = -1, cyTopHeight = -1, cyBottomHeight = -1 };
            DwmExtendFrameIntoClientArea(this.Handle, ref margins);

            string initAudio = LicenseManager.Instance.IsPro
                ? "Tactical engine online. Press Escape or F 12 anytime to close."
                : "Tactical HUD online. Press Escape or F 12 anytime to close.";
            SpeakVoiceAsync(initAudio, 1);
        }

        private void OnRenderTick(object sender, EventArgs e)
        {
            // 1. HARDWARE KEY POLLER: Guaranteed Instant Exit (F12 or Escape)
            // GetAsyncKeyState bypasses Windows debugger reservations and works 100% of the time!
            bool f12Down = (GetAsyncKeyState((int)Keys.F12) & 0x8000) != 0;
            bool escDown = (GetAsyncKeyState((int)Keys.Escape) & 0x8000) != 0;

            if (f12Down || escDown)
            {
                CloseOverlayImmediately();
                return;
            }

            // 2. Hotkey Edge Detection with GetAsyncKeyState
            PollHotkey((int)Keys.F5, ref f5Prev, () => HandleAction(8));  // Spike
            PollHotkey((int)Keys.F6, ref f6Prev, () => HandleAction(1));  // Clip
            PollHotkey((int)Keys.F7, ref f7Prev, () => HandleAction(2));  // HUD Line
            PollHotkey((int)Keys.F8, ref f8Prev, () => HandleAction(3));  // Savage
            PollHotkey((int)Keys.F9, ref f9Prev, () => HandleAction(4));  // Hype
            PollHotkey((int)Keys.F10, ref f10Prev, () => HandleAction(5)); // Desi
            PollHotkey((int)Keys.F11, ref f11Prev, () => HandleAction(6)); // Coach

            // 3. Smooth animation progress
            waveAnimationProgress += 0.04f;
            if (waveAnimationProgress > 1f) waveAnimationProgress = 0f;

            // Invalidate screen for smooth redraw
            this.Invalidate();
        }

        private void PollHotkey(int vk, ref bool prevState, Action onTrigger)
        {
            bool isDown = (GetAsyncKeyState(vk) & 0x8000) != 0;
            if (isDown && !prevState)
            {
                onTrigger();
            }
            prevState = isDown;
        }

        public void CloseOverlayImmediately()
        {
            if (renderTimer != null) renderTimer.Stop();
            if (bannerTimer != null) bannerTimer.Stop();

            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    if (voiceSynthesizer != null)
                    {
                        voiceSynthesizer.SpeakAsyncCancelAll();
                        voiceSynthesizer.Rate = 1;
                        voiceSynthesizer.Speak("Overlay closed.");
                        voiceSynthesizer.Dispose();
                    }
                }
                catch { }
            });

            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (renderTimer != null) renderTimer.Stop();
            if (bannerTimer != null) bannerTimer.Stop();
            base.OnFormClosing(e);
        }

        public void HandleHotKey(int id)
        {
            HandleAction(id);
        }

        public void HandleAction(int id)
        {
            if (id == 8) // F5 Spike Timer
            {
                var spike = AccessibilityTacticalSuite.Instance.Spike;
                if (spike.State == SpikeState.Planted)
                {
                    spike.Reset();
                    SetBanner("SPIKE DEFUSED // DEFUSE COMPLETE", Color.FromArgb(0, 255, 136));
                    SpeakVoiceAsync("Spike defused.", 1);
                }
                else
                {
                    spike.Plant();
                    SetBanner("🚨 SPIKE PLANTED // 45.0s COUNTDOWN ACTIVE", Color.FromArgb(255, 70, 85));
                    SpeakVoiceAsync("Spike planted. 45 seconds ticking.", 1);
                }
            }
            else if (id == 2) // F7 HUD Line
            {
                showHeadLevelLine = !showHeadLevelLine;
                SetBanner("HEAD-LEVEL HORIZON: " + (showHeadLevelLine ? "ENABLED (50% ANGLE)" : "DISABLED"), Color.FromArgb(0, 240, 255));
            }
            else if (id == 3) // F8 Savage Roast
            {
                if (!LicenseManager.Instance.CanUseVoice(0))
                {
                    string teaser = LicenseManager.Instance.GetVoiceTeaserMessage(0);
                    SetBanner("🔒 PRO FEATURE LOCKED: Savage Roaster requires GamePilot Pro", Color.FromArgb(255, 70, 85));
                    SpeakVoiceAsync(teaser, 1);
                    return;
                }

                string[] roasts = new string[]
                {
                    "Bro, did you close your eyes? You sprayed thirty bullets and hit air molecules!",
                    "How do you miss a stationary target with an Operator?! Delete the game immediately!",
                    "I've seen blindfolded pigeons with better recoil control than whatever that was."
                };
                string roast = roasts[rng.Next(roasts.Length)];
                SetBanner("SAVAGE ROASTER: \"" + roast + "\"", Color.FromArgb(255, 70, 85));
                SpeakVoiceAsync(roast, 0);
            }
            else if (id == 4) // F9 Chad Hype
            {
                string[] hypes = new string[]
                {
                    "YOOOO! LETS GOOO! YOU ARE A LITERAL ESPORTS PHENOMENON! CLIP THAT RIGHT NOW!",
                    "HE IS HIM! YOU ARE ACTUALLY UNTOUCHABLE KING! SIT DOWN ENEMIES!",
                    "ABSOLUTELY DISGUSTING! SOMEONE GET THIS GUY A CONTRACT! ACE ON THE BOARD!"
                };
                string hype = hypes[rng.Next(hypes.Length)];
                SetBanner("CHAD HYPE-BRO: \"" + hype + "\"", Color.FromArgb(255, 215, 0));
                SpeakVoiceAsync(hype, 2);
            }
            else if (id == 5) // F10 Desi Roaster
            {
                if (!LicenseManager.Instance.CanUseVoice(2))
                {
                    string teaser = LicenseManager.Instance.GetVoiceTeaserMessage(2);
                    SetBanner("🔒 PRO FEATURE LOCKED: Desi Hinglish requires GamePilot Pro", Color.FromArgb(255, 70, 85));
                    SpeakVoiceAsync(teaser, 1);
                    return;
                }

                string[] desiLines = new string[]
                {
                    "Arre bhai! Kya spray maara be! Monitor bandh karke khel raha hai kya?!",
                    "Aisa whiff toh gully cricket mein bhi nahi hota! Uninstall kar de game!",
                    "O bhai maro mujhe maro! Ek goli nahi lagi usko! Wall pe painting bana raha hai kya?!",
                    "Bhai kya clutch kiya! Tu toh Radiant nikla guru! Clip kar isko abhi ke abhi!"
                };
                string desi = desiLines[rng.Next(desiLines.Length)];
                SetBanner("DESI HINGLISH: \"" + desi + "\"", Color.FromArgb(255, 140, 0));
                SpeakVoiceAsync(desi, 1);
            }
            else if (id == 6) // F11 Coach
            {
                if (!LicenseManager.Instance.CanUseVoice(3))
                {
                    string teaser = LicenseManager.Instance.GetVoiceTeaserMessage(3);
                    SetBanner("🔒 PRO FEATURE LOCKED: Tactical Coach requires GamePilot Pro", Color.FromArgb(255, 70, 85));
                    SpeakVoiceAsync(teaser, 1);
                    return;
                }

                string[] calls = new string[]
                {
                    "Excellent trigger discipline. Isolating those duels was textbook fundamentals.",
                    "Your crosshair is dropping too low near chest level. Keep it raised at head horizon.",
                    "You panic sprayed before counter strafing. Stand completely still before bursting."
                };
                string call = calls[rng.Next(calls.Length)];
                SetBanner("TACTICAL COACH: \"" + call + "\"", Color.FromArgb(0, 240, 255));
                SpeakVoiceAsync(call, 1);
            }
            else if (id == 1) // F6 ClipForge
            {
                int currentClips = GamePilotDatabase.Instance.Store.Clips != null ? GamePilotDatabase.Instance.Store.Clips.Count : 0;
                if (!LicenseManager.Instance.CanExportUnlimitedClips(currentClips))
                {
                    SetBanner("🔒 CLIP LIMIT REACHED (3/3): Upgrade to Pro for Unlimited 9:16 Reels!", Color.FromArgb(255, 70, 85));
                    SpeakVoiceAsync("Daily clip limit reached. Upgrade to GamePilot Pro for unlimited vertical reel exports.", 1);
                    return;
                }

                string manifest;
                string imgPath = ClipForge.Instance.CaptureAndRenderReel("1v3 CLUTCH ACE", "HE IS HIM! YOU ARE ACTUALLY UNTOUCHABLE KING!", out manifest);
                SetBanner("🎬 CLIPFORGE: 9:16 Vertical Reel Captured & Saved to Desktop/GamePilotClips!", Color.FromArgb(255, 215, 0));
                SpeakVoiceAsync("ClipForge clutch reel saved ready for TikTok", 1);
            }
        }

        private void SetBanner(string text, Color color)
        {
            activeBannerText = text;
            bannerColor = color;
            bannerTimer.Stop();
            bannerTimer.Start();
        }

        private void SpeakVoiceAsync(string text, int rate)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    lock (voiceSynthesizer)
                    {
                        voiceSynthesizer.Rate = rate;
                        voiceSynthesizer.Speak(text);
                    }
                }
                catch { }
            });
        }

        // =========================================================================
        // AAA ESPORTS GRAPHICS RENDERING (VALORANT-GRADE GDI+ HUD)
        // =========================================================================
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int screenW = this.Width;
            int screenH = this.Height;
            int centerX = screenW / 2;
            int centerY = screenH / 2;

            Color reticleColor = AccessibilityTacticalSuite.Instance.GetReticleColor();

            // 1. TOP HEADER: VALORANT CHAMFERED HUD STATUS PILL & EMERGENCY EXIT BADGE
            int pillW = 860;
            int pillH = 46;
            int pillX = 25;
            int pillY = 20;

            DrawChamferedBox(g, new Rectangle(pillX, pillY, pillW, pillH), 10,
                Color.FromArgb(220, 7, 10, 16), bannerColor, 2f);

            using (Font titleFont = new Font("Segoe UI", 9.5f, FontStyle.Bold))
            using (SolidBrush textBrush = new SolidBrush(bannerColor))
            {
                g.DrawString("⚡ " + activeBannerText, titleFont, textBrush, pillX + 16, pillY + 12);
            }

            // Top Right Emergency Exit Pill: [ ✕ ESC / F12 TO EXIT ]
            int exitW = 190;
            int exitH = 46;
            int exitX = screenW - exitW - 25;
            DrawChamferedBox(g, new Rectangle(exitX, pillY, exitW, exitH), 10,
                Color.FromArgb(220, 255, 70, 85), Color.FromArgb(255, 255, 255), 1.5f);

            using (Font exitFont = new Font("Segoe UI", 9f, FontStyle.Bold))
            using (SolidBrush exitTextBrush = new SolidBrush(Color.White))
            {
                g.DrawString("✕ PRESS ESC / F12", exitFont, exitTextBrush, exitX + 18, pillY + 14);
            }

            // 2. SPIKE DEFUSE MASTERY RADIAL DIAL (CIRCULAR ARC GAUGE AT TOP CENTER)
            var spike = AccessibilityTacticalSuite.Instance.Spike;
            if (AccessibilityTacticalSuite.Instance.EnableSpikeHUD && spike.State == SpikeState.Planted)
            {
                double remaining = spike.SecondsRemaining;
                Color spikeTheme = spike.GetIndicatorColor();
                string guidance = spike.GetDefuseGuidanceText();

                int dialSize = 100;
                int dialX = centerX - (dialSize / 2);
                int dialY = 75;

                // Glowing background plate
                DrawChamferedBox(g, new Rectangle(centerX - 180, dialY - 5, 360, 110), 12,
                    Color.FromArgb(230, 8, 12, 18), spikeTheme, 2.5f);

                // Circular countdown ring
                float sweepAngle = (float)((remaining / SpikeTimer.TOTAL_DURATION_SEC) * 360f);

                // Track ring
                using (Pen trackPen = new Pen(Color.FromArgb(60, 255, 255, 255), 6f))
                {
                    g.DrawEllipse(trackPen, dialX, dialY + 5, dialSize - 10, dialSize - 10);
                }

                // Active glowing sweep arc
                using (Pen arcPen = new Pen(spikeTheme, 7f))
                {
                    arcPen.StartCap = LineCap.Round;
                    arcPen.EndCap = LineCap.Round;
                    g.DrawArc(arcPen, dialX, dialY + 5, dialSize - 10, dialSize - 10, -90, sweepAngle);
                }

                // Digital countdown readout in center
                using (Font digiFont = new Font("Consolas", 14f, FontStyle.Bold))
                using (SolidBrush digiBrush = new SolidBrush(Color.White))
                {
                    string timeStr = string.Format("{0:0.0}s", remaining);
                    SizeF sz = g.MeasureString(timeStr, digiFont);
                    g.DrawString(timeStr, digiFont, digiBrush, centerX - (sz.Width / 2), dialY + 38);
                }

                // Guidance pill below dial
                using (Font subGuide = new Font("Segoe UI", 8.5f, FontStyle.Bold))
                using (SolidBrush guideBrush = new SolidBrush(spikeTheme))
                {
                    SizeF sz2 = g.MeasureString(guidance, subGuide);
                    g.DrawString(guidance, subGuide, guideBrush, centerX - (sz2.Width / 2), dialY + 84);
                }
            }

            // 3. DEAF & HARD-OF-HEARING VISUAL AUDIO SONAR (SMOOTH RIPPLE WAVES)
            var radar = AccessibilityTacticalSuite.Instance.AudioRadar;
            if (AccessibilityTacticalSuite.Instance.EnableAudioRadarHUD && radar.IsActive)
            {
                if (radar.ShowLeftPing)
                {
                    // Multi-layer expanding soundwave ripples on Left border
                    float r1 = 60 + (waveAnimationProgress * 50);
                    float r2 = 90 + (waveAnimationProgress * 50);
                    int alpha = (int)(255 * (1f - waveAnimationProgress));
                    if (alpha < 0) alpha = 0;

                    using (Pen ripplePen1 = new Pen(Color.FromArgb(alpha, 0, 240, 255), 4f))
                    using (Pen ripplePen2 = new Pen(Color.FromArgb(220, 0, 240, 255), 3f))
                    {
                        g.DrawArc(ripplePen1, -r1 / 2, centerY - (r1 / 2), r1, r1, -60, 120);
                        g.DrawArc(ripplePen2, -r2 / 2, centerY - (r2 / 2), r2, r2, -60, 120);
                    }

                    // Cyberpunk bracket callout
                    DrawChamferedBox(g, new Rectangle(35, centerY - 20, 240, 40), 8,
                        Color.FromArgb(220, 10, 20, 30), Color.FromArgb(0, 240, 255), 1.5f);

                    using (Font sonarFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (SolidBrush sonarBrush = new SolidBrush(Color.FromArgb(0, 240, 255)))
                    {
                        g.DrawString("◀ FOOTSTEP: " + radar.LastAudioSource, sonarFont, sonarBrush, 45, centerY - 9);
                    }
                }

                if (radar.ShowRightPing)
                {
                    float r1 = 60 + (waveAnimationProgress * 50);
                    float r2 = 90 + (waveAnimationProgress * 50);
                    int alpha = (int)(255 * (1f - waveAnimationProgress));
                    if (alpha < 0) alpha = 0;

                    using (Pen ripplePen1 = new Pen(Color.FromArgb(alpha, 0, 240, 255), 4f))
                    using (Pen ripplePen2 = new Pen(Color.FromArgb(220, 0, 240, 255), 3f))
                    {
                        g.DrawArc(ripplePen1, screenW - (r1 / 2), centerY - (r1 / 2), r1, r1, 120, 120);
                        g.DrawArc(ripplePen2, screenW - (r2 / 2), centerY - (r2 / 2), r2, r2, 120, 120);
                    }

                    DrawChamferedBox(g, new Rectangle(screenW - 275, centerY - 20, 240, 40), 8,
                        Color.FromArgb(220, 10, 20, 30), Color.FromArgb(0, 240, 255), 1.5f);

                    using (Font sonarFont = new Font("Segoe UI", 9f, FontStyle.Bold))
                    using (SolidBrush sonarBrush = new SolidBrush(Color.FromArgb(0, 240, 255)))
                    {
                        g.DrawString("FOOTSTEP: " + radar.LastAudioSource + " ▶", sonarFont, sonarBrush, screenW - 265, centerY - 9);
                    }
                }
            }

            // 4. RADIANT HEAD-LEVEL HORIZON GUIDE & RETICLE
            if (showHeadLevelLine)
            {
                using (Pen headLinePen = new Pen(Color.FromArgb(120, 0, 255, 136), 1.5f))
                {
                    headLinePen.DashStyle = DashStyle.Dash;
                    g.DrawLine(headLinePen, 0, centerY, screenW, centerY);
                }

                using (Font smallFont = new Font("Segoe UI", 8, FontStyle.Bold))
                using (SolidBrush greenBrush = new SolidBrush(Color.FromArgb(200, 0, 255, 136)))
                {
                    g.DrawString("HEAD-LEVEL (50% HORIZON)", smallFont, greenBrush, 25, centerY - 18);
                    g.DrawString("RADIANT ELEVATION", smallFont, greenBrush, screenW - 145, centerY - 18);
                }

                // Crisp esports crosshair with drop-shadow outline
                using (Pen shadowPen = new Pen(Color.FromArgb(180, 0, 0, 0), 3.5f))
                using (Pen reticlePen = new Pen(reticleColor, 2f))
                {
                    // Shadows
                    g.DrawLine(shadowPen, centerX - 14, centerY, centerX - 4, centerY);
                    g.DrawLine(shadowPen, centerX + 4, centerY, centerX + 14, centerY);
                    g.DrawLine(shadowPen, centerX, centerY - 14, centerX, centerY - 4);
                    g.DrawLine(shadowPen, centerX, centerY + 4, centerX, centerY + 14);

                    // Foreground
                    g.DrawLine(reticlePen, centerX - 14, centerY, centerX - 4, centerY);
                    g.DrawLine(reticlePen, centerX + 4, centerY, centerX + 14, centerY);
                    g.DrawLine(reticlePen, centerX, centerY - 14, centerX, centerY - 4);
                    g.DrawLine(reticlePen, centerX, centerY + 4, centerX, centerY + 14);
                }

                // 5. RECOIL RESET CADENCE METRONOME (Diamond below reticle)
                var metronome = AccessibilityTacticalSuite.Instance.Metronome;
                if (AccessibilityTacticalSuite.Instance.EnableRecoilMetronomeHUD)
                {
                    Color metroColor = metronome.IsResetReady ? Color.FromArgb(0, 255, 136) : Color.FromArgb(255, 70, 85);
                    Point[] diamond = new Point[]
                    {
                        new Point(centerX, centerY + 20),
                        new Point(centerX + 5, centerY + 25),
                        new Point(centerX, centerY + 30),
                        new Point(centerX - 5, centerY + 25)
                    };
                    using (SolidBrush metroBrush = new SolidBrush(metroColor))
                    using (Pen metroBorder = new Pen(Color.Black, 1.5f))
                    {
                        g.FillPolygon(metroBrush, diamond);
                        g.DrawPolygon(metroBorder, diamond);
                    }
                }
            }
        }

        // Custom GDI+ Drawing: Angled Chamfer Polygons (Valorant Aesthetics)
        private void DrawChamferedBox(Graphics g, Rectangle r, int cut, Color fill, Color border, float borderWidth)
        {
            Point[] points = new Point[]
            {
                new Point(r.Left + cut, r.Top),
                new Point(r.Right - cut, r.Top),
                new Point(r.Right, r.Top + cut),
                new Point(r.Right, r.Bottom - cut),
                new Point(r.Right - cut, r.Bottom),
                new Point(r.Left + cut, r.Bottom),
                new Point(r.Left, r.Bottom - cut),
                new Point(r.Left, r.Top + cut)
            };

            using (SolidBrush b = new SolidBrush(fill))
            using (Pen p = new Pen(border, borderWidth))
            {
                g.FillPolygon(b, points);
                g.DrawPolygon(p, points);
            }
        }
    }
}
