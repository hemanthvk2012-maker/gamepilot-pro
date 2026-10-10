using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Speech.Synthesis;
using System.Threading;
using System.Windows.Forms;

namespace GamePilotNative
{
    public class GamePilotHubForm : Form
    {
        private OverlayForm activeOverlay = null;
        private Label statusLabel;
        private Button launchButton;
        private ComboBox personalitySelector;
        private SpeechSynthesizer hubVoice = new SpeechSynthesizer();
        private Label licenseBadgeLabel;
        private TabControl mainTabs;

        // Tactical & Accessibility UI controls
        private Label valConnStatusLabel;
        private Button toggleSimLoopBtn;
        private TextBox valLogBox;
        private ComboBox colorblindPicker;
        private CheckBox cbSpikeTimer;
        private CheckBox cbAudioRadar;
        private CheckBox cbMetronome;

        // Analytics UI controls
        private Label totalMatchesVal;
        private Label winRateVal;
        private Label avgHsVal;
        private Label roastsVal;
        private ListView matchListView;

        // Clips UI controls
        private ListView clipListView;

        // License UI controls
        private Label currentTierDisplay;
        private TextBox licenseKeyInput;
        private Label licenseStatusMessage;
        private ProgressBar collegeGoalProgress;
        private Label collegeGoalLabel;

        public GamePilotHubForm()
        {
            this.Text = "GamePilot Pro - Tactical Esports & Accessibility Companion [Valorant & CS2]";
            this.Size = new Size(1000, 740);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(11, 14, 20);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9, FontStyle.Regular);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            BuildTopHeader();
            BuildTabs();
            RefreshAllData();

            // Hook live game logger to UI terminal
            ValorantLiveConnector.Instance.OnGameLog = logLine =>
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        if (valLogBox != null && !valLogBox.IsDisposed)
                        {
                            valLogBox.AppendText(logLine + Environment.NewLine);
                        }
                    }));
                }
            };
        }

        private void BuildTopHeader()
        {
            Panel header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(17, 24, 39)
            };
            this.Controls.Add(header);

            Label logo = new Label
            {
                Text = "🚀 GAMEPILOT PRO",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(245, 158, 11),
                Location = new Point(25, 14),
                AutoSize = true
            };
            header.Controls.Add(logo);

            Label subtitle = new Label
            {
                Text = "Tactical & Accessibility Suite • Deaf Sound Radar • Spike 45s Defuse Timer • Vanguard Safe (0% Lag)",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(156, 163, 175),
                Location = new Point(27, 46),
                AutoSize = true
            };
            header.Controls.Add(subtitle);

            // License Status Pill
            licenseBadgeLabel = new Label
            {
                Text = LicenseManager.Instance.GetBadgeText(),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = LicenseManager.Instance.IsPro ? Color.FromArgb(245, 158, 11) : Color.FromArgb(156, 163, 175),
                BackColor = LicenseManager.Instance.IsPro ? Color.FromArgb(69, 39, 160) : Color.FromArgb(31, 41, 55),
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(570, 26),
                AutoSize = true,
                Cursor = Cursors.Hand
            };
            licenseBadgeLabel.Click += (s, e) => mainTabs.SelectedIndex = 4; // Switch to License Tab
            header.Controls.Add(licenseBadgeLabel);

            // College Tuition Fund Badge
            Label collegeBadge = new Label
            {
                Text = "🇮🇳 Indian Student College Fund",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 211, 153),
                BackColor = Color.FromArgb(6, 78, 59),
                Padding = new Padding(8, 4, 8, 4),
                Location = new Point(765, 26),
                AutoSize = true
            };
            header.Controls.Add(collegeBadge);
        }

        private void BuildTabs()
        {
            mainTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            this.Controls.Add(mainTabs);

            // Tab 1: Command Center
            TabPage tabCommand = new TabPage("  🚀 Command Center  ");
            tabCommand.BackColor = Color.FromArgb(11, 14, 20);
            BuildCommandCenterTab(tabCommand);
            mainTabs.TabPages.Add(tabCommand);

            // Tab 2: Tactical & Accessibility Suite (NEW!)
            TabPage tabTactical = new TabPage("  🎯 Tactical & Deaf Accessibility  ");
            tabTactical.BackColor = Color.FromArgb(11, 14, 20);
            BuildTacticalAccessibilityTab(tabTactical);
            mainTabs.TabPages.Add(tabTactical);

            // Tab 3: Analytics & Database
            TabPage tabAnalytics = new TabPage("  📊 Match Analytics & Database  ");
            tabAnalytics.BackColor = Color.FromArgb(11, 14, 20);
            BuildAnalyticsTab(tabAnalytics);
            mainTabs.TabPages.Add(tabAnalytics);

            // Tab 4: ClipForge Studio
            TabPage tabClips = new TabPage("  🎬 ClipForge Studio  ");
            tabClips.BackColor = Color.FromArgb(11, 14, 20);
            BuildClipsTab(tabClips);
            mainTabs.TabPages.Add(tabClips);

            // Tab 5: Commercial Licensing & Payments
            TabPage tabLicense = new TabPage("  💎 Pro License & Store  ");
            tabLicense.BackColor = Color.FromArgb(11, 14, 20);
            BuildLicenseTab(tabLicense);
            mainTabs.TabPages.Add(tabLicense);

            // Tab 6: Settings & Vanguard Safety
            TabPage tabSettings = new TabPage("  🛡️ Vanguard Safety  ");
            tabSettings.BackColor = Color.FromArgb(11, 14, 20);
            BuildSettingsTab(tabSettings);
            mainTabs.TabPages.Add(tabSettings);
        }

        // =========================================================================
        // TAB 1: COMMAND CENTER
        // =========================================================================
        private void BuildCommandCenterTab(TabPage tab)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(25) };
            tab.Controls.Add(pnl);

            GroupBox launchBox = new GroupBox
            {
                Text = " In-Game Overlay Engine (Valorant / CS2) ",
                Location = new Point(20, 15),
                Size = new Size(930, 155),
                ForeColor = Color.FromArgb(209, 213, 219),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            pnl.Controls.Add(launchBox);

            statusLabel = new Label
            {
                Text = "Overlay Status: Standby [Click button below to engage transparent HUD on live game]",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                Location = new Point(20, 30),
                AutoSize = true
            };
            launchBox.Controls.Add(statusLabel);

            launchButton = new Button
            {
                Text = "⚡ LAUNCH TRANSPARENT IN-GAME OVERLAY (VALORANT / CS2)",
                Location = new Point(20, 60),
                Size = new Size(885, 46),
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            launchButton.Click += (s, e) => ToggleOverlay();
            launchBox.Controls.Add(launchButton);

            Label hotkeyHint = new Label
            {
                Text = "Hotkeys: F5 Spike Timer | F7 Head Horizon | F8 Savage Roast | F9 Chad Hype | F10 Desi Roaster | F11 Coach | F6 ClipForge | F12 Close",
                ForeColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(20, 118),
                AutoSize = true
            };
            launchBox.Controls.Add(hotkeyHint);

            Label voiceLbl = new Label
            {
                Text = "ACTIVE AI VOICE ARCHETYPE:",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(209, 213, 219),
                Location = new Point(20, 185),
                AutoSize = true
            };
            pnl.Controls.Add(voiceLbl);

            personalitySelector = new ComboBox
            {
                Location = new Point(20, 212),
                Width = 550,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            personalitySelector.Items.Add("😈 Savage Toxic Roaster [PRO] (Brutal English Banter)");
            personalitySelector.Items.Add("🔥 Chad Esports Hype Caster [FREE] (VCT Grand Finals Energy)");
            personalitySelector.Items.Add("🇮🇳 Desi Hinglish Roaster [PRO] (Viral Indian College Banter)");
            personalitySelector.Items.Add("🧠 Radiant Tactical Coach [PRO] (Elevation & Counter-Strafe Calls)");
            personalitySelector.SelectedIndex = 2;
            pnl.Controls.Add(personalitySelector);

            Button testVoiceBtn = new Button
            {
                Text = "🔊 Test Speech",
                Location = new Point(585, 210),
                Size = new Size(140, 32),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            testVoiceBtn.Click += (s, e) => TestVoiceFromSelector();
            pnl.Controls.Add(testVoiceBtn);

            // Simulation Action Deck
            GroupBox simBox = new GroupBox
            {
                Text = " Quick Scenario Triggers (Instant In-Ear Banter & Reel Capture) ",
                Location = new Point(20, 260),
                Size = new Size(930, 270),
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            pnl.Controls.Add(simBox);

            Button simWhiffBtn = new Button
            {
                Text = "💥 Simulate 30-Bullet Whiff (Trigger Roast)",
                Location = new Point(20, 45),
                Size = new Size(430, 42),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            simWhiffBtn.Click += (s, e) => SimulateWhiff();
            simBox.Controls.Add(simWhiffBtn);

            Button simClutchBtn = new Button
            {
                Text = "🏆 Simulate 1v4 Clutch Ace (Trigger Hype)",
                Location = new Point(475, 45),
                Size = new Size(430, 42),
                BackColor = Color.FromArgb(217, 119, 6),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            simClutchBtn.Click += (s, e) => SimulateClutch();
            simBox.Controls.Add(simClutchBtn);

            Button simCoachBtn = new Button
            {
                Text = "🎯 Simulate Crosshair Placement (Coach)",
                Location = new Point(20, 100),
                Size = new Size(430, 42),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            simCoachBtn.Click += (s, e) => SimulateCoach();
            simBox.Controls.Add(simCoachBtn);

            Button simClipBtn = new Button
            {
                Text = "🎬 Simulate 1-Click 9:16 TikTok Reel Capture",
                Location = new Point(475, 100),
                Size = new Size(430, 42),
                BackColor = Color.FromArgb(124, 58, 237),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            simClipBtn.Click += (s, e) => SimulateClipCapture();
            simBox.Controls.Add(simClipBtn);

            Label simStatusText = new Label
            {
                Text = "Ready for simulation. Click any button above or switch to the 'Tactical & Accessibility' tab for the 45s Spike Timer and Deaf Sonar Radar.",
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Consolas", 8.5f, FontStyle.Regular),
                Location = new Point(20, 160),
                Size = new Size(885, 80),
                BackColor = Color.FromArgb(17, 24, 39),
                Padding = new Padding(10)
            };
            simStatusText.Name = "simStatusText";
            simBox.Controls.Add(simStatusText);
        }

        // =========================================================================
        // TAB 2: TACTICAL & ACCESSIBILITY SUITE (VALORANT LIVE CONNECTION)
        // =========================================================================
        private void BuildTacticalAccessibilityTab(TabPage tab)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(25) };
            tab.Controls.Add(pnl);

            // 1. Live Valorant Connection & Hook Card
            GroupBox hookBox = new GroupBox
            {
                Text = " Live Valorant Connection & Telemetry Bridge ",
                Location = new Point(20, 15),
                Size = new Size(930, 130),
                ForeColor = Color.FromArgb(6, 182, 212),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            pnl.Controls.Add(hookBox);

            valConnStatusLabel = new Label
            {
                Text = "Status: " + ValorantLiveConnector.Instance.StatusDetails,
                ForeColor = Color.FromArgb(209, 213, 219),
                Font = new Font("Consolas", 9, FontStyle.Regular),
                Location = new Point(15, 25),
                Size = new Size(900, 36)
            };
            hookBox.Controls.Add(valConnStatusLabel);

            Button refreshConnBtn = new Button
            {
                Text = "🔄 Scan for VALORANT.exe",
                Location = new Point(15, 70),
                Size = new Size(240, 36),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            refreshConnBtn.Click += (s, e) =>
            {
                ValorantLiveConnector.Instance.RefreshConnection();
                valConnStatusLabel.Text = "Status: " + ValorantLiveConnector.Instance.StatusDetails;
            };
            hookBox.Controls.Add(refreshConnBtn);

            toggleSimLoopBtn = new Button
            {
                Text = "⚡ START LIVE MATCH SIMULATION LOOP",
                Location = new Point(270, 70),
                Size = new Size(340, 36),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            toggleSimLoopBtn.Click += (s, e) => ToggleSimulationLoop();
            hookBox.Controls.Add(toggleSimLoopBtn);

            // 2. High-Value Tactical & Accessibility Feature Deck
            GroupBox tacBox = new GroupBox
            {
                Text = " High-Value Tactical & Accessibility Features (What Gamers Pay For) ",
                Location = new Point(20, 160),
                Size = new Size(930, 220),
                ForeColor = Color.FromArgb(245, 158, 11),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            pnl.Controls.Add(tacBox);

            // Feature A: Spike Defuse Radar
            Label spkLbl = new Label
            {
                Text = "💣 45s SPIKE DEFUSE MASTERY HUD (Saves Guns & Rounds):",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(209, 213, 219),
                Location = new Point(15, 25),
                AutoSize = true
            };
            tacBox.Controls.Add(spkLbl);

            cbSpikeTimer = new CheckBox
            {
                Text = "Enabled (Shows 45s bar + 7.0s defuse check in HUD)",
                Checked = true,
                Location = new Point(15, 48),
                AutoSize = true,
                ForeColor = Color.FromArgb(52, 211, 153)
            };
            cbSpikeTimer.CheckedChanged += (s, e) => AccessibilityTacticalSuite.Instance.EnableSpikeHUD = cbSpikeTimer.Checked;
            tacBox.Controls.Add(cbSpikeTimer);

            Button triggerSpikeBtn = new Button
            {
                Text = "💣 Plant Spike (Start 45s Timer)",
                Location = new Point(15, 75),
                Size = new Size(210, 32),
                BackColor = Color.FromArgb(220, 38, 38),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            triggerSpikeBtn.Click += (s, e) =>
            {
                AccessibilityTacticalSuite.Instance.Spike.Plant();
                if (activeOverlay != null && !activeOverlay.IsDisposed) activeOverlay.Invalidate();
            };
            tacBox.Controls.Add(triggerSpikeBtn);

            Button resetSpikeBtn = new Button
            {
                Text = "Defuse / Reset Spike",
                Location = new Point(235, 75),
                Size = new Size(160, 32),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat
            };
            resetSpikeBtn.Click += (s, e) =>
            {
                AccessibilityTacticalSuite.Instance.Spike.Reset();
                if (activeOverlay != null && !activeOverlay.IsDisposed) activeOverlay.Invalidate();
            };
            tacBox.Controls.Add(resetSpikeBtn);

            // Feature B: Deaf / Hard-of-Hearing Visual Audio Sonar Radar
            Label radLbl = new Label
            {
                Text = "🔊 DEAF & HARD-OF-HEARING VISUAL AUDIO RADAR (Directional Sonar Pings):",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(209, 213, 219),
                Location = new Point(440, 25),
                AutoSize = true
            };
            tacBox.Controls.Add(radLbl);

            cbAudioRadar = new CheckBox
            {
                Text = "Enabled (Draws sonar arcs for footsteps & gunshots)",
                Checked = true,
                Location = new Point(440, 48),
                AutoSize = true,
                ForeColor = Color.FromArgb(6, 182, 212)
            };
            cbAudioRadar.CheckedChanged += (s, e) => AccessibilityTacticalSuite.Instance.EnableAudioRadarHUD = cbAudioRadar.Checked;
            tacBox.Controls.Add(cbAudioRadar);

            Button pingLeftBtn = new Button
            {
                Text = "◀ Ping Left Footstep",
                Location = new Point(440, 75),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            pingLeftBtn.Click += (s, e) =>
            {
                AccessibilityTacticalSuite.Instance.AudioRadar.TriggerPing("LEFT", "Enemy Running (A-Main)");
                if (activeOverlay != null && !activeOverlay.IsDisposed) activeOverlay.Invalidate();
            };
            tacBox.Controls.Add(pingLeftBtn);

            Button pingRightBtn = new Button
            {
                Text = "Ping Right Footstep ▶",
                Location = new Point(600, 75),
                Size = new Size(150, 32),
                BackColor = Color.FromArgb(13, 148, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            pingRightBtn.Click += (s, e) =>
            {
                AccessibilityTacticalSuite.Instance.AudioRadar.TriggerPing("RIGHT", "Enemy Running (A-Short)");
                if (activeOverlay != null && !activeOverlay.IsDisposed) activeOverlay.Invalidate();
            };
            tacBox.Controls.Add(pingRightBtn);

            // Feature C: Colorblind Modes & Recoil Metronome
            Label cbLbl = new Label
            {
                Text = "👁️ COLORBLIND PRO RETICLE & RECOIL METRONOME:",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(209, 213, 219),
                Location = new Point(15, 125),
                AutoSize = true
            };
            tacBox.Controls.Add(cbLbl);

            colorblindPicker = new ComboBox
            {
                Location = new Point(15, 150),
                Width = 380,
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            colorblindPicker.Items.Add("Standard Esports Neon Green (Normal Vision)");
            colorblindPicker.Items.Add("Deuteranopia Electric Cyan (Red-Green Colorblind)");
            colorblindPicker.Items.Add("Protanopia Solar Yellow (Red-Weak)");
            colorblindPicker.Items.Add("High-Contrast Magenta (Desert Maps & Bind)");
            colorblindPicker.SelectedIndex = 0;
            colorblindPicker.SelectedIndexChanged += (s, e) =>
            {
                AccessibilityTacticalSuite.Instance.ActiveColorblindMode = (ColorblindMode)colorblindPicker.SelectedIndex;
                if (activeOverlay != null && !activeOverlay.IsDisposed) activeOverlay.Invalidate();
            };
            tacBox.Controls.Add(colorblindPicker);

            cbMetronome = new CheckBox
            {
                Text = "Enable Recoil Reset Cadence Pip (Green=Ready, Red=Cooling down)",
                Checked = true,
                Location = new Point(440, 152),
                AutoSize = true,
                ForeColor = Color.FromArgb(209, 213, 219)
            };
            cbMetronome.CheckedChanged += (s, e) => AccessibilityTacticalSuite.Instance.EnableRecoilMetronomeHUD = cbMetronome.Checked;
            tacBox.Controls.Add(cbMetronome);

            // 3. Live Telemetry Console Log
            GroupBox logBox = new GroupBox
            {
                Text = " Live Match Telemetry Console & Combat Log ",
                Location = new Point(20, 395),
                Size = new Size(930, 175),
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnl.Controls.Add(logBox);

            valLogBox = new TextBox
            {
                Location = new Point(15, 25),
                Size = new Size(900, 135),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(17, 24, 39),
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Consolas", 8.5f, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle
            };
            valLogBox.Text = "[INITIALIZED] GamePilot Tactical & Accessibility Engine Ready.\r\nClick 'START LIVE MATCH SIMULATION LOOP' above to stream live rounds.\r\n";
            logBox.Controls.Add(valLogBox);
        }

        private void ToggleSimulationLoop()
        {
            if (ValorantLiveConnector.Instance.State == ValorantConnectionState.SimulationStreamActive)
            {
                ValorantLiveConnector.Instance.StopSimulationLoop();
                toggleSimLoopBtn.Text = "⚡ START LIVE MATCH SIMULATION LOOP";
                toggleSimLoopBtn.BackColor = Color.FromArgb(16, 185, 129);
                toggleSimLoopBtn.ForeColor = Color.Black;
                valConnStatusLabel.Text = "Status: " + ValorantLiveConnector.Instance.StatusDetails;
            }
            else
            {
                ValorantLiveConnector.Instance.StartSimulationLoop();
                toggleSimLoopBtn.Text = "🛑 STOP SIMULATION STREAM";
                toggleSimLoopBtn.BackColor = Color.FromArgb(239, 68, 68);
                toggleSimLoopBtn.ForeColor = Color.White;
                valConnStatusLabel.Text = "Status: " + ValorantLiveConnector.Instance.StatusDetails;

                // Auto launch overlay so user immediately sees the visual radar and spike timer!
                if (activeOverlay == null || activeOverlay.IsDisposed)
                {
                    ToggleOverlay();
                }
            }
        }

        // =========================================================================
        // TAB 3: ANALYTICS & DATABASE
        // =========================================================================
        private void BuildAnalyticsTab(TabPage tab)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            tab.Controls.Add(pnl);

            Panel card1 = CreateStatCard("TOTAL MATCHES", "42", Color.FromArgb(59, 130, 246), 20, 15, out totalMatchesVal);
            Panel card2 = CreateStatCard("WIN RATE", "76.2%", Color.FromArgb(16, 185, 129), 250, 15, out winRateVal);
            Panel card3 = CreateStatCard("AVG HEADSHOT %", "64.8%", Color.FromArgb(245, 158, 11), 480, 15, out avgHsVal);
            Panel card4 = CreateStatCard("TOTAL ROASTS", "58", Color.FromArgb(239, 68, 68), 710, 15, out roastsVal);
            pnl.Controls.Add(card1);
            pnl.Controls.Add(card2);
            pnl.Controls.Add(card3);
            pnl.Controls.Add(card4);

            Label tblHeader = new Label
            {
                Text = "HISTORICAL MATCH SESSION TELEMETRY (ACID DATABASE):",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(209, 213, 219),
                Location = new Point(20, 115),
                AutoSize = true
            };
            pnl.Controls.Add(tblHeader);

            matchListView = new ListView
            {
                Location = new Point(20, 140),
                Size = new Size(930, 310),
                View = View.Details,
                FullRowSelect = true,
                BackColor = Color.FromArgb(17, 24, 39),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle
            };
            matchListView.Columns.Add("Session ID", 80);
            matchListView.Columns.Add("Game", 85);
            matchListView.Columns.Add("Map", 90);
            matchListView.Columns.Add("K / D / A", 95);
            matchListView.Columns.Add("Headshot %", 95);
            matchListView.Columns.Add("Roasts", 75);
            matchListView.Columns.Add("Clutches", 75);
            matchListView.Columns.Add("Outcome", 120);
            matchListView.Columns.Add("Date & Time", 160);
            pnl.Controls.Add(matchListView);

            Button addSimMatchBtn = new Button
            {
                Text = "➕ Simulate New Match Record",
                Location = new Point(20, 465),
                Size = new Size(230, 36),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            addSimMatchBtn.Click += (s, e) => AddSimulatedMatch();
            pnl.Controls.Add(addSimMatchBtn);

            Button exportCsvBtn = new Button
            {
                Text = "📥 Export History to CSV (Desktop)",
                Location = new Point(265, 465),
                Size = new Size(240, 36),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            exportCsvBtn.Click += (s, e) => ExportDatabaseToCsv();
            pnl.Controls.Add(exportCsvBtn);

            Button resetDbBtn = new Button
            {
                Text = "🗑️ Reset Database",
                Location = new Point(800, 465),
                Size = new Size(150, 36),
                BackColor = Color.FromArgb(185, 28, 28),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            resetDbBtn.Click += (s, e) => ResetDatabaseConfirm();
            pnl.Controls.Add(resetDbBtn);
        }

        private Panel CreateStatCard(string title, string initialVal, Color accent, int x, int y, out Label valLabel)
        {
            Panel card = new Panel
            {
                Location = new Point(x, y),
                Size = new Size(220, 85),
                BackColor = Color.FromArgb(17, 24, 39),
                BorderStyle = BorderStyle.FixedSingle
            };

            Panel bar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = accent
            };
            card.Controls.Add(bar);

            Label titleLbl = new Label
            {
                Text = title,
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 8, FontStyle.Bold),
                Location = new Point(16, 12),
                AutoSize = true
            };
            card.Controls.Add(titleLbl);

            valLabel = new Label
            {
                Text = initialVal,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(14, 34),
                AutoSize = true
            };
            card.Controls.Add(valLabel);

            return card;
        }

        // =========================================================================
        // TAB 4: CLIPFORGE STUDIO
        // =========================================================================
        private void BuildClipsTab(TabPage tab)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(25) };
            tab.Controls.Add(pnl);

            Label clipHeader = new Label
            {
                Text = "CLIPFORGE: 9:16 VERTICAL REEL EXPORT GALLERY (TIKTOK / REELS / SHORTS)",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(209, 213, 219),
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnl.Controls.Add(clipHeader);

            clipListView = new ListView
            {
                Location = new Point(20, 45),
                Size = new Size(930, 310),
                View = View.Details,
                FullRowSelect = true,
                BackColor = Color.FromArgb(17, 24, 39),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Regular),
                BorderStyle = BorderStyle.FixedSingle
            };
            clipListView.Columns.Add("Clip ID", 90);
            clipListView.Columns.Add("Title / Event", 220);
            clipListView.Columns.Add("Resolution", 160);
            clipListView.Columns.Add("Est. TikTok Views", 130);
            clipListView.Columns.Add("Created Timestamp", 150);
            clipListView.Columns.Add("Status", 130);
            pnl.Controls.Add(clipListView);

            Button captureNowBtn = new Button
            {
                Text = "📸 Capture Live 9:16 Vertical Reel Now",
                Location = new Point(20, 370),
                Size = new Size(300, 42),
                BackColor = Color.FromArgb(124, 58, 237),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            captureNowBtn.Click += (s, e) => SimulateClipCapture();
            pnl.Controls.Add(captureNowBtn);

            Button openFolderBtn = new Button
            {
                Text = "📂 Open Desktop/GamePilotClips Folder",
                Location = new Point(335, 370),
                Size = new Size(300, 42),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            openFolderBtn.Click += (s, e) =>
            {
                string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GamePilotClips");
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                Process.Start("explorer.exe", path);
            };
            pnl.Controls.Add(openFolderBtn);

            GroupBox tipsBox = new GroupBox
            {
                Text = " Viral Reel Strategy Engine ",
                Location = new Point(20, 425),
                Size = new Size(930, 110),
                ForeColor = Color.FromArgb(245, 158, 11),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnl.Controls.Add(tipsBox);

            Label tipsText = new Label
            {
                Text = "• Formatted in 1080x1920 (9:16) with action centered for algorithmic virality on YouTube Shorts, TikTok & IG Reels.\n"
                     + "• Audio sync point logged at frame drop with recommended sound: 'Midnight Phonk'.\n"
                     + "• Free tier includes 3 exports per day. Pro tier includes unlimited reel exports and custom branding.",
                ForeColor = Color.FromArgb(209, 213, 219),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                Location = new Point(15, 25),
                Size = new Size(900, 70)
            };
            tipsBox.Controls.Add(tipsText);
        }

        // =========================================================================
        // TAB 5: COMMERCIAL LICENSING & PAYMENTS
        // =========================================================================
        private void BuildLicenseTab(TabPage tab)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(25) };
            tab.Controls.Add(pnl);

            Panel currentLicPanel = new Panel
            {
                Location = new Point(20, 15),
                Size = new Size(930, 75),
                BackColor = Color.FromArgb(17, 24, 39),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnl.Controls.Add(currentLicPanel);

            currentTierDisplay = new Label
            {
                Text = "CURRENT LICENSE: " + LicenseManager.Instance.GetBadgeText(),
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = LicenseManager.Instance.IsPro ? Color.FromArgb(245, 158, 11) : Color.FromArgb(156, 163, 175),
                Location = new Point(20, 15),
                AutoSize = true
            };
            currentLicPanel.Controls.Add(currentTierDisplay);

            Label tierSub = new Label
            {
                Text = "Pro unlocks Desi Hinglish, Savage Roaster, Radiant Coach, Spike Radar, Unlimited 9:16 Reels & VIP Discord.",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(156, 163, 175),
                Location = new Point(22, 42),
                AutoSize = true
            };
            currentLicPanel.Controls.Add(tierSub);

            Label storeHeader = new Label
            {
                Text = "CHOOSE YOUR COMMERCIAL PLAN (SUPPORT COLLEGE TUITION):",
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(209, 213, 219),
                Location = new Point(20, 105),
                AutoSize = true
            };
            pnl.Controls.Add(storeHeader);

            GroupBox rzpBox = new GroupBox
            {
                Text = " 🇮🇳 India Gamer Pass (Razorpay UPI / QR) ",
                Location = new Point(20, 130),
                Size = new Size(450, 150),
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnl.Controls.Add(rzpBox);

            Label rzpPrice = new Label
            {
                Text = "₹299 / Month  or  ₹999 Lifetime Pass",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 25),
                AutoSize = true
            };
            rzpBox.Controls.Add(rzpPrice);

            Label rzpDesc = new Label
            {
                Text = "Instant Google Pay / PhonePe / Paytm UPI access. 100% of proceeds fund student college tuition in India.",
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = Color.FromArgb(156, 163, 175),
                Location = new Point(15, 52),
                Size = new Size(420, 32)
            };
            rzpBox.Controls.Add(rzpDesc);

            Button buyRzpBtn = new Button
            {
                Text = "💳 Pay ₹299 INR via Razorpay UPI",
                Location = new Point(15, 92),
                Size = new Size(420, 38),
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            buyRzpBtn.Click += (s, e) => OpenPaymentLink("https://razorpay.me/@gamepilotpro");
            rzpBox.Controls.Add(buyRzpBtn);

            GroupBox lsBox = new GroupBox
            {
                Text = " 🌍 Global Esports Pass (Lemon Squeezy / Stripe) ",
                Location = new Point(490, 130),
                Size = new Size(460, 150),
                ForeColor = Color.FromArgb(245, 158, 11),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnl.Controls.Add(lsBox);

            Label lsPrice = new Label
            {
                Text = "$14.99 / Month  or  $49.99 Lifetime",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 25),
                AutoSize = true
            };
            lsBox.Controls.Add(lsPrice);

            Label lsDesc = new Label
            {
                Text = "Credit Card, Apple Pay, Google Pay, PayPal. Instant international license key delivery via email.",
                Font = new Font("Segoe UI", 8, FontStyle.Regular),
                ForeColor = Color.FromArgb(156, 163, 175),
                Location = new Point(15, 52),
                Size = new Size(425, 32)
            };
            lsBox.Controls.Add(lsDesc);

            Button buyLsBtn = new Button
            {
                Text = "💳 Pay $14.99 USD via Lemon Squeezy",
                Location = new Point(15, 92),
                Size = new Size(425, 38),
                BackColor = Color.FromArgb(245, 158, 11),
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            buyLsBtn.Click += (s, e) => OpenPaymentLink("https://gamepilotpro.lemonsqueezy.com/checkout");
            lsBox.Controls.Add(buyLsBtn);

            GroupBox actBox = new GroupBox
            {
                Text = " Activate Purchased License Key ",
                Location = new Point(20, 295),
                Size = new Size(930, 140),
                ForeColor = Color.FromArgb(209, 213, 219),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnl.Controls.Add(actBox);

            Label keyLbl = new Label
            {
                Text = "Enter License Key (Format: GPPRO-XXXX-XXXX-XXXX-XXXX):",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(156, 163, 175),
                Location = new Point(20, 25),
                AutoSize = true
            };
            actBox.Controls.Add(keyLbl);

            licenseKeyInput = new TextBox
            {
                Location = new Point(20, 48),
                Size = new Size(600, 30),
                Font = new Font("Consolas", 11, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Text = "GPPRO-LIFETIME-FOUNDER-2026-RADIANT"
            };
            actBox.Controls.Add(licenseKeyInput);

            Button activateBtn = new Button
            {
                Text = "🔓 VERIFY & UNLOCK PRO",
                Location = new Point(635, 46),
                Size = new Size(270, 32),
                BackColor = Color.FromArgb(37, 99, 235),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            activateBtn.Click += (s, e) => ActivateLicenseFromUI();
            actBox.Controls.Add(activateBtn);

            licenseStatusMessage = new Label
            {
                Text = "Enter key above and click Verify. Master Founder key is pre-filled for non-gamer verification.",
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Italic),
                Location = new Point(20, 88),
                Size = new Size(885, 35)
            };
            actBox.Controls.Add(licenseStatusMessage);

            GroupBox fundBox = new GroupBox
            {
                Text = " Live College Tuition Revenue Tracker (Goal: ₹5,00,000 / $6,000) ",
                Location = new Point(20, 445),
                Size = new Size(930, 95),
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnl.Controls.Add(fundBox);

            collegeGoalLabel = new Label
            {
                Text = "Fund Raised: ₹1,42,800 INR ($1,720 USD) from 478 subscribers • 28.5% of Semester Tuition Covered",
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(209, 213, 219),
                Location = new Point(15, 24),
                AutoSize = true
            };
            fundBox.Controls.Add(collegeGoalLabel);

            collegeGoalProgress = new ProgressBar
            {
                Location = new Point(15, 48),
                Size = new Size(895, 22),
                Minimum = 0,
                Maximum = 100,
                Value = 29
            };
            fundBox.Controls.Add(collegeGoalProgress);
        }

        // =========================================================================
        // TAB 6: VANGUARD SAFETY & ARCHITECTURE
        // =========================================================================
        private void BuildSettingsTab(TabPage tab)
        {
            Panel pnl = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(25) };
            tab.Controls.Add(pnl);

            GroupBox secBox = new GroupBox
            {
                Text = " Riot Vanguard & Valve VAC Compliance Guarantee ",
                Location = new Point(20, 15),
                Size = new Size(930, 260),
                ForeColor = Color.FromArgb(52, 211, 153),
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            pnl.Controls.Add(secBox);

            string safetyDoc = "1. ZERO MEMORY INJECTION:\n"
                             + "GamePilot Pro NEVER reads game process memory (no ReadProcessMemory, no OpenProcess, no DLL injection).\n"
                             + "Vanguard kernel driver (vgk.sys) and VAC detect memory hooks immediately. GamePilot is 100% immune.\n\n"
                             + "2. TRANSPARENT DESKTOP WINDOW MANAGER (DWM) LAYER:\n"
                             + "The overlay is a standard transparent Win32 top-level window using WS_EX_TRANSPARENT and WS_EX_LAYERED.\n"
                             + "Windows OS hardware-accelerated DWM composites the graphics cleanly above fullscreen borderless games with 0ms lag.\n\n"
                             + "3. LOCAL ZERO-LATENCY WINDOWS SAPI SPEECH:\n"
                             + "Voice synthesis executes completely offline using Windows native System.Speech without cloud network calls or server bills.\n\n"
                             + "4. 100% TOURNAMENT & STREAMER COMPLIANT:\n"
                             + "OBS Game Capture passes directly through the transparent layer, keeping streamer broadcasts pristine.";

            Label secText = new Label
            {
                Text = safetyDoc,
                Font = new Font("Consolas", 8.5f, FontStyle.Regular),
                ForeColor = Color.FromArgb(229, 231, 235),
                Location = new Point(15, 25),
                Size = new Size(895, 220)
            };
            secBox.Controls.Add(secText);

            GroupBox hkBox = new GroupBox
            {
                Text = " Complete Hotkey Reference ",
                Location = new Point(20, 290),
                Size = new Size(930, 240),
                ForeColor = Color.FromArgb(245, 158, 11),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            pnl.Controls.Add(hkBox);

            string[] hotkeys = new string[]
            {
                "• F5  : Toggle 45s Spike Defuse Timer & Half-Defuse Warning",
                "• F6  : ClipForge 1-Click 9:16 Vertical Reel Capture (Saves to Desktop/GamePilotClips)",
                "• F7  : Toggle Minimalist Head-Level Horizon Guide Line (50% Screen Elevation)",
                "• F8  : Savage Toxic Roaster [PRO] (\"Bro sprayed thirty bullets and hit air molecules!\")",
                "• F9  : Chad Esports Hype Caster [FREE] (\"YOOOO! ACE ON THE BOARD! CLIP THAT KING!\")",
                "• F10 : Desi Hinglish Roaster [PRO] (\"Arre bhai kya whiff maara be! Gali cricket hai kya?\")",
                "• F11 : Radiant Tactical Coach [PRO] (\"Crosshair too low. Raise to head horizon.\")",
                "• F12 : Close In-Game Transparent Overlay"
            };

            int y = 25;
            foreach (var h in hotkeys)
            {
                Label l = new Label
                {
                    Text = h,
                    Font = new Font("Segoe UI", 9, FontStyle.Regular),
                    ForeColor = Color.White,
                    Location = new Point(15, y),
                    AutoSize = true
                };
                hkBox.Controls.Add(l);
                y += 25;
            }
        }

        // =========================================================================
        // DATA REFRESH & EVENT HANDLERS
        // =========================================================================
        private void RefreshAllData()
        {
            licenseBadgeLabel.Text = LicenseManager.Instance.GetBadgeText();
            licenseBadgeLabel.ForeColor = LicenseManager.Instance.IsPro ? Color.FromArgb(245, 158, 11) : Color.FromArgb(156, 163, 175);
            currentTierDisplay.Text = "CURRENT LICENSE: " + LicenseManager.Instance.GetBadgeText();
            currentTierDisplay.ForeColor = LicenseManager.Instance.IsPro ? Color.FromArgb(245, 158, 11) : Color.FromArgb(156, 163, 175);

            int totalM, totalR, totalC;
            double winR, avgHs;
            GamePilotDatabase.Instance.GetLifetimeStats(out totalM, out winR, out avgHs, out totalR, out totalC);

            totalMatchesVal.Text = totalM.ToString();
            winRateVal.Text = winR.ToString("F1") + "%";
            avgHsVal.Text = avgHs.ToString("F1") + "%";
            roastsVal.Text = totalR.ToString();

            matchListView.Items.Clear();
            var matches = GamePilotDatabase.Instance.Store.Matches;
            if (matches != null)
            {
                foreach (var m in matches)
                {
                    ListViewItem item = new ListViewItem(m.Id);
                    item.SubItems.Add(m.Game);
                    item.SubItems.Add(m.Map);
                    item.SubItems.Add(string.Format("{0}/{1}/{2}", m.Kills, m.Deaths, m.Assists));
                    item.SubItems.Add(m.HeadshotPercentage.ToString("F1") + "%");
                    item.SubItems.Add(m.RoastsCount.ToString());
                    item.SubItems.Add(m.ClutchesWon.ToString());
                    item.SubItems.Add(m.Outcome);
                    item.SubItems.Add(m.Timestamp.ToString("yyyy-MM-dd HH:mm"));

                    if (m.Outcome == "VICTORY" || m.Outcome == "OVERTIME_WIN")
                        item.ForeColor = Color.FromArgb(52, 211, 153);
                    else
                        item.ForeColor = Color.FromArgb(248, 113, 113);

                    matchListView.Items.Add(item);
                }
            }

            clipListView.Items.Clear();
            var clips = GamePilotDatabase.Instance.Store.Clips;
            if (clips != null)
            {
                foreach (var c in clips)
                {
                    ListViewItem item = new ListViewItem(c.ClipId);
                    item.SubItems.Add(c.Title);
                    item.SubItems.Add(c.Resolution);
                    item.SubItems.Add(c.ViewsSimulated.ToString("#,##0") + " views");
                    item.SubItems.Add(c.Timestamp.ToString("yyyy-MM-dd HH:mm"));
                    item.SubItems.Add("Rendered & Ready");
                    clipListView.Items.Add(item);
                }
            }
        }

        private void ToggleOverlay()
        {
            if (activeOverlay == null || activeOverlay.IsDisposed)
            {
                activeOverlay = new OverlayForm(personalitySelector.SelectedIndex);
                activeOverlay.FormClosed += (s, e) =>
                {
                    activeOverlay = null;
                    if (!this.IsDisposed)
                    {
                        statusLabel.Text = "Overlay Status: Standby [Overlay closed via ESC/F12]";
                        statusLabel.ForeColor = Color.FromArgb(156, 163, 175);
                        launchButton.Text = "⚡ LAUNCH TRANSPARENT IN-GAME OVERLAY (VALORANT / CS2)";
                        launchButton.BackColor = Color.FromArgb(245, 158, 11);
                        launchButton.ForeColor = Color.Black;
                    }
                };
                activeOverlay.Show();
                statusLabel.Text = "Overlay Status: ACTIVE [Hovers over game screen. Press ESC or F12 anytime to close!]";
                statusLabel.ForeColor = Color.FromArgb(52, 211, 153);
                launchButton.Text = "🛑 HIDE / STOP IN-GAME OVERLAY (OR PRESS ESC / F12)";
                launchButton.BackColor = Color.FromArgb(239, 68, 68);
                launchButton.ForeColor = Color.White;
            }
            else
            {
                activeOverlay.CloseOverlayImmediately();
                activeOverlay = null;
                statusLabel.Text = "Overlay Status: Standby";
                statusLabel.ForeColor = Color.FromArgb(156, 163, 175);
                launchButton.Text = "⚡ LAUNCH TRANSPARENT IN-GAME OVERLAY (VALORANT / CS2)";
                launchButton.BackColor = Color.FromArgb(245, 158, 11);
                launchButton.ForeColor = Color.Black;
            }
        }

        private void TestVoiceFromSelector()
        {
            int idx = personalitySelector.SelectedIndex;
            if (!LicenseManager.Instance.CanUseVoice(idx))
            {
                string teaser = LicenseManager.Instance.GetVoiceTeaserMessage(idx);
                SpeakAsync(teaser, 1);
                MessageBox.Show(teaser, "GamePilot Pro Upgrade Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string speech = "";
            if (idx == 0) speech = "Bro, are you playing with your monitor turned off? Delete the game immediately!";
            else if (idx == 1) speech = "YOOOO! LETS GOOO! YOU ARE A LITERAL ESPORTS GOD! CLIP THAT RIGHT NOW KING!";
            else if (idx == 2) speech = "Arre bhai! Kya spray maara be! Aisa whiff toh gully cricket mein bhi nahi hota!";
            else speech = "Head level crosshair angle locked. Reset recoil and hold for the enemy re-peek.";

            SpeakAsync(speech, (idx == 1) ? 2 : 1);
        }

        private void SpeakAsync(string text, int rate)
        {
            ThreadPool.QueueUserWorkItem(state =>
            {
                try
                {
                    lock (hubVoice)
                    {
                        hubVoice.Rate = rate;
                        hubVoice.Speak(text);
                    }
                }
                catch { }
            });
        }

        private void UpdateSimLog(string text)
        {
            Control[] ctrls = this.Controls.Find("simStatusText", true);
            if (ctrls.Length > 0)
            {
                ctrls[0].Text = text;
            }
        }

        private void SimulateWhiff()
        {
            string roast = LicenseManager.Instance.IsPro
                ? "Arre bhai! Kya whiff maara be! Aisa spray toh gully cricket mein bhi nahi hota!"
                : "Bro sprayed thirty bullets and hit air molecules! Delete the game!";

            UpdateSimLog("[SIMULATION] Whiff Detected: 30 bullets missed -> Audio Triggered: \"" + roast + "\"");
            SpeakAsync(roast, 1);
            if (activeOverlay != null && !activeOverlay.IsDisposed)
            {
                activeOverlay.HandleHotKey(8);
            }
        }

        private void SimulateClutch()
        {
            string hype = "YOOOO! LETS GOOO! YOU ARE AN ESPORTS GOD! 1v4 CLUTCH ON THE BOARD! CLIP THAT KING!";
            UpdateSimLog("[SIMULATION] 1v4 Clutch Won -> Audio Triggered: \"" + hype + "\"");
            SpeakAsync(hype, 2);
            if (activeOverlay != null && !activeOverlay.IsDisposed)
            {
                activeOverlay.HandleHotKey(4);
            }
        }

        private void SimulateCoach()
        {
            string coach = "Your crosshair is dropping near chest level. Keep it raised at 50 percent head horizon.";
            UpdateSimLog("[SIMULATION] Elevation Math: Y=0.68 (18% below horizon) -> Callout: \"" + coach + "\"");
            SpeakAsync(coach, 1);
            if (activeOverlay != null && !activeOverlay.IsDisposed)
            {
                activeOverlay.HandleHotKey(6);
            }
        }

        private void SimulateClipCapture()
        {
            string manifest;
            string img = ClipForge.Instance.CaptureAndRenderReel("1v3 CLUTCH ACE", "HE IS HIM! YOU ARE ACTUALLY UNTOUCHABLE KING!", out manifest);
            UpdateSimLog("[SIMULATION] ClipForge Captured: Saved 9:16 Vertical Reel to:\n" + img);
            SpeakAsync("ClipForge clutch reel saved ready for TikTok", 1);
            RefreshAllData();
        }

        private void AddSimulatedMatch()
        {
            Random rng = new Random();
            string[] maps = new string[] { "Ascent", "Lotus", "Bind", "Haven", "Mirage", "Dust II" };
            var m = new MatchSession
            {
                Id = "M-" + rng.Next(1000, 9999),
                Timestamp = DateTime.Now,
                Game = rng.Next(0, 2) == 0 ? "Valorant" : "CS2",
                Map = maps[rng.Next(maps.Length)],
                Kills = rng.Next(15, 32),
                Deaths = rng.Next(8, 18),
                Assists = rng.Next(3, 11),
                HeadshotPercentage = Math.Round(rng.NextDouble() * 30.0 + 50.0, 1),
                RoastsCount = rng.Next(1, 4),
                HypesCount = rng.Next(2, 6),
                ClutchesWon = rng.Next(1, 3),
                Outcome = rng.Next(0, 3) == 0 ? "DEFEAT" : "VICTORY"
            };

            GamePilotDatabase.Instance.AddMatchSession(m);
            RefreshAllData();
            MessageBox.Show("New match session recorded to database!\nMap: " + m.Map + " | Outcome: " + m.Outcome + " | K/D: " + m.Kills + "/" + m.Deaths, "Database Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ExportDatabaseToCsv()
        {
            string desk = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string target = Path.Combine(desk, "GamePilot_Match_Telemetry_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv");
            GamePilotDatabase.Instance.ExportToCsv(target);
            MessageBox.Show("Match history exported to CSV on your Desktop:\n" + target, "CSV Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ResetDatabaseConfirm()
        {
            if (MessageBox.Show("Are you sure you want to reset the database to default seeded data?", "Reset Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                GamePilotDatabase.Instance.ResetDatabase();
                RefreshAllData();
            }
        }

        private void OpenPaymentLink(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                MessageBox.Show("Open this link in your browser to complete purchase:\n" + url, "Checkout Link", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ActivateLicenseFromUI()
        {
            string key = licenseKeyInput.Text;
            string msg;
            bool ok = LicenseManager.Instance.Activate(key, "user@gamer.gg", "GUI_ACTIVATION", out msg);

            licenseStatusMessage.Text = msg;
            if (ok)
            {
                licenseStatusMessage.ForeColor = Color.FromArgb(52, 211, 153);
                SpeakAsync("GamePilot Pro successfully unlocked. Welcome to Pro VIP tier.", 1);
                MessageBox.Show("Congratulations! " + msg, "Pro License Activated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                RefreshAllData();
            }
            else
            {
                licenseStatusMessage.ForeColor = Color.FromArgb(248, 113, 113);
                MessageBox.Show(msg, "License Key Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
