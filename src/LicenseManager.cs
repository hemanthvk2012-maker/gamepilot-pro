using System;
using System.Security.Cryptography;
using System.Text;

namespace GamePilotNative
{
    public class LicenseValidationResult
    {
        public bool IsValid { get; set; }
        public string Tier { get; set; }
        public string Message { get; set; }
    }

    public class LicenseManager
    {
        private static readonly object lockObj = new object();
        private static LicenseManager instance;
        private const string SALT_SECRET = "GPPRO-VALORANT-ESPORTS-SALT-2026";

        public static LicenseManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new LicenseManager();
                        }
                    }
                }
                return instance;
            }
        }

        private LicenseManager() { }

        public bool IsPro
        {
            get
            {
                var lic = GamePilotDatabase.Instance.Store.License;
                if (lic == null || !lic.IsActive) return false;
                if (lic.ExpiresAt < DateTime.Now) return false;
                return true;
            }
        }

        public string CurrentTier
        {
            get
            {
                var lic = GamePilotDatabase.Instance.Store.License;
                if (lic == null || !lic.IsActive) return "FREE";
                return lic.Tier;
            }
        }

        public string GetBadgeText()
        {
            if (IsPro)
            {
                string tier = CurrentTier;
                if (tier == "PRO_LIFETIME") return "⭐ PRO LIFETIME VIP";
                return "⭐ PRO MONTHLY SUBSCRIBER";
            }
            return "FREE TRIAL (LIMITED)";
        }

        public bool CanUseVoice(int personalityIdx)
        {
            // Free Tier only has index 1 (Chad Hype-Bro)
            // Pro unlocks Savage Roaster (0), Desi Hinglish (2), and Radiant Coach (3)
            if (IsPro) return true;
            return (personalityIdx == 1);
        }

        public string GetVoiceTeaserMessage(int personalityIdx)
        {
            if (personalityIdx == 0)
                return "Savage Toxic Roaster is locked in Free Tier. Upgrade to GamePilot Pro to unleash brutal banter!";
            if (personalityIdx == 2)
                return "Desi Hinglish Roaster is locked in Free Tier. Upgrade to GamePilot Pro to unlock viral Indian college roasts!";
            if (personalityIdx == 3)
                return "Radiant Tactical Coach is locked in Free Tier. Upgrade to GamePilot Pro to unlock pro elevation calls!";
            return "";
        }

        public bool CanExportUnlimitedClips(int currentClipCount)
        {
            if (IsPro) return true;
            return (currentClipCount < 3);
        }

        public LicenseValidationResult ValidateKey(string rawKey)
        {
            var res = new LicenseValidationResult();
            if (string.IsNullOrEmpty(rawKey))
            {
                res.IsValid = false;
                res.Message = "Please enter a valid license key.";
                return res;
            }

            string clean = rawKey.Trim().ToUpper();

            // 1. Check Master Founder / VIP Testing Keys
            if (clean == "GPPRO-LIFETIME-FOUNDER-2026-RADIANT")
            {
                res.IsValid = true;
                res.Tier = "PRO_LIFETIME";
                res.Message = "Lifetime Radiant Founder License Verified! All Pro features unlocked permanently.";
                return res;
            }

            if (clean == "GPPRO-COLLEGE-RAZORPAY-PASS-999")
            {
                res.IsValid = true;
                res.Tier = "PRO_LIFETIME";
                res.Message = "India College Fund Lifetime License Verified! Thank you for supporting college tuition.";
                return res;
            }

            if (clean == "GPPRO-LEMON-SQUEEZY-PRO-VIP")
            {
                res.IsValid = true;
                res.Tier = "PRO_MONTHLY";
                res.Message = "Lemon Squeezy Pro Monthly Subscription Verified!";
                return res;
            }

            // 2. Cryptographic Checksum Key Format: GPPRO-XXXX-XXXX-XXXX-YYYY
            string[] parts = clean.Split('-');
            if (parts.Length == 5 && parts[0] == "GPPRO")
            {
                string payload = parts[0] + "-" + parts[1] + "-" + parts[2] + "-" + parts[3];
                string expectedChecksum = ComputeShortChecksum(payload);

                if (parts[4] == expectedChecksum)
                {
                    res.IsValid = true;
                    res.Tier = parts[1].StartsWith("LIFE") ? "PRO_LIFETIME" : "PRO_MONTHLY";
                    res.Message = "Cryptographic Pro License Verified Successfully!";
                    return res;
                }
            }

            res.IsValid = false;
            res.Message = "Invalid License Key. Check your confirmation email or purchase at gamepilot.gg";
            return res;
        }

        public string GenerateCommercialKey(string tierPrefix)
        {
            // Format: GPPRO-TIER-RAND-RAND-CHECKSUM
            Random rng = new Random();
            string p1 = (tierPrefix == "LIFETIME") ? "LIFE" : "SUBS";
            string p2 = rng.Next(1000, 9999).ToString();
            string p3 = rng.Next(1000, 9999).ToString();
            string payload = "GPPRO-" + p1 + "-" + p2 + "-" + p3;
            string checksum = ComputeShortChecksum(payload);
            return payload + "-" + checksum;
        }

        private string ComputeShortChecksum(string input)
        {
            using (SHA256 sha = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input + SALT_SECRET);
                byte[] hash = sha.ComputeHash(bytes);
                // Take 4 chars in hex
                return string.Format("{0:X2}{1:X2}", hash[0], hash[1]);
            }
        }

        public bool Activate(string key, string email, string refId, out string msg)
        {
            var result = ValidateKey(key);
            if (result.IsValid)
            {
                GamePilotDatabase.Instance.UpdateLicense(key.Trim().ToUpper(), result.Tier, email, refId);
                msg = result.Message;
                return true;
            }
            msg = result.Message;
            return false;
        }

        public string SimulateRazorpayPurchase()
        {
            string key = GenerateCommercialKey("LIFETIME");
            string orderId = "order_rzp_" + Guid.NewGuid().ToString().Substring(0, 8);
            string email = "indian_gamer@college.edu";
            string msg;
            Activate(key, email, orderId, out msg);
            return key;
        }

        public string SimulateLemonSqueezyPurchase()
        {
            string key = GenerateCommercialKey("SUBS");
            string orderId = "ls_sub_" + Guid.NewGuid().ToString().Substring(0, 8);
            string email = "global_esports@gmail.com";
            string msg;
            Activate(key, email, orderId, out msg);
            return key;
        }
    }
}
