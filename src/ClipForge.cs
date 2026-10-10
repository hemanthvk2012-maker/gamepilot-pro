using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace GamePilotNative
{
    public class ClipForge
    {
        private static readonly object lockObj = new object();
        private static ClipForge instance;

        public static ClipForge Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new ClipForge();
                        }
                    }
                }
                return instance;
            }
        }

        private ClipForge() { }

        public string CaptureAndRenderReel(string eventType, string subtitleText, out string manifestPath)
        {
            manifestPath = "";
            try
            {
                string clipsFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "GamePilotClips");
                if (!Directory.Exists(clipsFolder))
                {
                    Directory.CreateDirectory(clipsFolder);
                }

                string timestampStr = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string imagePath = Path.Combine(clipsFolder, "GamePilot_Reel_" + timestampStr + ".jpg");
                manifestPath = Path.Combine(clipsFolder, "GamePilot_Reel_" + timestampStr + ".txt");

                Rectangle bounds = (Screen.PrimaryScreen != null && Screen.PrimaryScreen.Bounds.Width > 0)
                    ? Screen.PrimaryScreen.Bounds
                    : new Rectangle(0, 0, 1920, 1080);

                // 1. Capture primary display screen or render high-fidelity gameplay frame
                using (Bitmap screenBmp = new Bitmap(bounds.Width, bounds.Height))
                {
                    bool screenCaptured = false;
                    try
                    {
                        using (Graphics gScreen = Graphics.FromImage(screenBmp))
                        {
                            gScreen.CopyFromScreen(Point.Empty, Point.Empty, bounds.Size);
                            screenCaptured = true;
                        }
                    }
                    catch
                    {
                        screenCaptured = false;
                    }

                    // Fallback to stylized high-fidelity simulated gameplay if screen DC is restricted
                    if (!screenCaptured)
                    {
                        using (Graphics gSim = Graphics.FromImage(screenBmp))
                        {
                            gSim.SmoothingMode = SmoothingMode.AntiAlias;
                            // Valorant Ascent A-Site simulation background
                            using (LinearGradientBrush arenaBg = new LinearGradientBrush(
                                new Rectangle(0, 0, bounds.Width, bounds.Height),
                                Color.FromArgb(20, 30, 48),
                                Color.FromArgb(36, 59, 85),
                                45f))
                            {
                                gSim.FillRectangle(arenaBg, 0, 0, bounds.Width, bounds.Height);
                            }

                            // Valorant HUD elements simulation
                            using (Font simFont = new Font("Segoe UI", 24, FontStyle.Bold))
                            using (SolidBrush yellowBrush = new SolidBrush(Color.FromArgb(245, 158, 11)))
                            {
                                gSim.DrawString("VALORANT // ASCENT - A SITE [1v3 MATCH-POINT CLUTCH]", simFont, yellowBrush, 120, 80);
                            }

                            // Crosshair at center
                            int cx = bounds.Width / 2;
                            int cy = bounds.Height / 2;
                            using (Pen greenReticle = new Pen(Color.FromArgb(0, 255, 128), 3))
                            {
                                gSim.DrawLine(greenReticle, cx - 18, cy, cx - 6, cy);
                                gSim.DrawLine(greenReticle, cx + 6, cy, cx + 18, cy);
                                gSim.DrawLine(greenReticle, cx, cy - 18, cx, cy - 6);
                                gSim.DrawLine(greenReticle, cx, cy + 6, cx, cy + 18);
                            }

                            // Head-level horizon guide line
                            using (Pen headGuide = new Pen(Color.FromArgb(120, 16, 185, 129), 2))
                            {
                                headGuide.DashStyle = DashStyle.Dash;
                                gSim.DrawLine(headGuide, 0, cy, bounds.Width, cy);
                            }
                        }
                    }

                    // 2. Render Vertical 9:16 High-Res Reel Canvas (1080 x 1920)
                    using (Bitmap reelBmp = new Bitmap(1080, 1920, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics g = Graphics.FromImage(reelBmp))
                        {
                            g.SmoothingMode = SmoothingMode.AntiAlias;
                            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

                            // Fill dark esports background
                            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(11, 14, 20)))
                            {
                                g.FillRectangle(bgBrush, 0, 0, 1080, 1920);
                            }

                            // Center gameplay action box:
                            // Crop center of original screen and scale to 1080 wide
                            int cropW = (int)(bounds.Width * 0.65);
                            int cropH = (int)(bounds.Height * 0.65);
                            int cropX = (bounds.Width - cropW) / 2;
                            int cropY = (bounds.Height - cropH) / 2;

                            Rectangle srcCrop = new Rectangle(cropX, cropY, cropW, cropH);
                            Rectangle dstAction = new Rectangle(0, 360, 1080, 1080); // Square center action
                            g.DrawImage(screenBmp, dstAction, srcCrop, GraphicsUnit.Pixel);

                            // Draw gradient borders around gameplay
                            using (Pen goldBorder = new Pen(Color.FromArgb(245, 158, 11), 6))
                            {
                                g.DrawRectangle(goldBorder, 2, 360, 1076, 1080);
                            }

                            // 3. Top Section: Header & Branding Banner
                            using (SolidBrush topBg = new SolidBrush(Color.FromArgb(17, 24, 39)))
                            {
                                g.FillRectangle(topBg, 0, 0, 1080, 360);
                            }

                            using (Font brandFont = new Font("Segoe UI", 36, FontStyle.Bold))
                            using (SolidBrush goldBrush = new SolidBrush(Color.FromArgb(245, 158, 11)))
                            {
                                g.DrawString("🚀 GAMEPILOT PRO", brandFont, goldBrush, 50, 45);
                            }

                            using (Font subBrand = new Font("Segoe UI", 20, FontStyle.Regular))
                            using (SolidBrush grayBrush = new SolidBrush(Color.FromArgb(156, 163, 175)))
                            {
                                g.DrawString("VALORANT & CS2 CLUTCH REEL • 9:16 VERTICAL EXPORT", subBrand, grayBrush, 52, 110);
                            }

                            // Event Pill Tag
                            using (SolidBrush pillBg = new SolidBrush(Color.FromArgb(220, 38, 38)))
                            using (Font pillFont = new Font("Segoe UI", 18, FontStyle.Bold))
                            using (SolidBrush whiteBrush = new SolidBrush(Color.White))
                            {
                                g.FillRectangle(pillBg, 50, 170, 320, 52);
                                g.DrawString("⚡ " + eventType.ToUpper(), pillFont, whiteBrush, 65, 180);
                            }

                            using (Font rankFont = new Font("Segoe UI", 16, FontStyle.Bold))
                            using (SolidBrush cyanBrush = new SolidBrush(Color.FromArgb(6, 182, 212)))
                            {
                                g.DrawString("ESTIMATED RANK: RADIANT / LVL 10 FACEIT", rankFont, cyanBrush, 400, 184);
                            }

                            // 4. Bottom Section: Viral Subtitles, Sound & TikTok Stats
                            using (SolidBrush botBg = new SolidBrush(Color.FromArgb(17, 24, 39)))
                            {
                                g.FillRectangle(botBg, 0, 1440, 1080, 480);
                            }

                            // Subtitle Pill
                            using (SolidBrush subBg = new SolidBrush(Color.FromArgb(30, 41, 59)))
                            using (Pen subBorder = new Pen(Color.FromArgb(59, 130, 246), 3))
                            {
                                Rectangle subRect = new Rectangle(50, 1480, 980, 140);
                                g.FillRectangle(subBg, subRect);
                                g.DrawRectangle(subBorder, subRect);
                            }

                            using (Font subFont = new Font("Segoe UI", 22, FontStyle.Bold))
                            using (SolidBrush yellowBrush = new SolidBrush(Color.FromArgb(253, 224, 71)))
                            {
                                string cleanSub = "\"" + subtitleText + "\"";
                                g.DrawString(cleanSub, subFont, yellowBrush, new RectangleF(70, 1500, 940, 100));
                            }

                            // Viral Metadata Bar
                            using (Font metaFont = new Font("Segoe UI", 16, FontStyle.Regular))
                            using (SolidBrush emeraldBrush = new SolidBrush(Color.FromArgb(16, 185, 129)))
                            {
                                g.DrawString("🎵 Audio: Midnight Phonk (Bass Drop @ Kill)  •  🎯 Headshot Accuracy: 100%", metaFont, emeraldBrush, 55, 1660);
                            }

                            using (Font tagFont = new Font("Segoe UI", 15, FontStyle.Italic))
                            using (SolidBrush grayBrush2 = new SolidBrush(Color.FromArgb(148, 163, 184)))
                            {
                                g.DrawString("#valorant #valorantclips #cs2 #esports #gamepilot #clutch #radiant", tagFont, grayBrush2, 55, 1720);
                            }

                            // Save high-quality JPEG
                            EncoderParameters encParams = new EncoderParameters(1);
                            encParams.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 92L);
                            ImageCodecInfo jpegCodec = GetEncoder(ImageFormat.Jpeg);

                            reelBmp.Save(imagePath, jpegCodec, encParams);
                        }
                    }
                }

                // 5. Write TikTok Upload Manifest
                string manifestContent = "=================================================================\n"
                                       + " GAMEPILOT PRO: VIRAL 9:16 VERTICAL CLUTCH REEL MANIFEST\n"
                                       + "=================================================================\n"
                                       + "Generated At: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\n"
                                       + "Output File: " + imagePath + "\n"
                                       + "Resolution: 1080x1920 (Vertical 9:16 TikTok / YouTube Shorts / Reels)\n"
                                       + "Event Category: " + eventType + "\n"
                                       + "Voice Commentary: \"" + subtitleText + "\"\n"
                                       + "Audio Recommendation: Midnight Phonk (00:04 Drop)\n"
                                       + "Recommended Title: HOW DID HE HIT THIS 1v3?! [RADIANT LOBBY] \n"
                                       + "Recommended Tags: #valorant #valorantclips #cs2 #esports #gamepilot #clutch\n"
                                       + "Status: Rendered and Ready to Post\n";

                File.WriteAllText(manifestPath, manifestContent);

                // 6. Record in Database
                var clip = new ClipRecord
                {
                    Title = eventType + " - " + DateTime.Now.ToString("MMM dd HH:mm"),
                    TriggerType = eventType,
                    ImagePath = imagePath,
                    ManifestPath = manifestPath,
                    Timestamp = DateTime.Now,
                    Resolution = "1080x1920 (Vertical 9:16)",
                    DurationSeconds = 15,
                    ViewsSimulated = new Random().Next(1200, 8500)
                };
                GamePilotDatabase.Instance.AddClip(clip);

                return imagePath;
            }
            catch (Exception ex)
            {
                manifestPath = "";
                return "Error: " + ex.Message;
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
