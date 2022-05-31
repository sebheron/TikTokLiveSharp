using System.Collections.Generic;

namespace TikTokLiveSharp.Client
{
    public static class TikTokRequestSettings
    {
        public const string TIKTOK_URL_WEB = "https://www.tiktok.com/";
        public const string TIKTOK_URL_WEBCAST = "https://webcast.tiktok.com/webcast/";

        public static readonly IReadOnlyDictionary<string, object> DEFAULT_CLIENT_PARAMS = new Dictionary<string, object>()
        {
            { "aid", 1988 },
            { "app_name", "tiktok_web" },
            { "browser_name", "Mozilla" },
            { "browser_online", true },
            { "browser_platform", "Win32" },
            { "version_code", 180800 },
            { "browser_version", "5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/97.0.4692.99 Safari/537.36" },
            { "cookie_enabled", true },
            { "cursor", "" },
            { "device_platform", "web" },
            { "did_rule", 3 },
            { "fetch_rule", 1 },
            { "identity", "audience" },
            { "internal_ext", "" },
            { "last_rtt", 0 },
            { "live_id", 12 },
            { "resp_content_type", "protobuf" },
            { "screen_height", 1152 },
            { "screen_width", 2048 },
            { "tz_name", "Europe/Berlin" },
            { "browser_language", "en" },
            { "priority_region", "US" },
            { "region", "US" }
        };

        public static readonly IReadOnlyDictionary<string, string> DEFAULT_REQUEST_HEADERS = new Dictionary<string, string>()
        {
            { "Connection", "keep-alive" },
            { "Cache-Control", "max-age=0" },
            { "Accept", "text/html,application/json,application/protobuf" },
            { "User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.127 Mobile Safari/537.36" },
            { "Referer", "https://www.tiktok.com/" },
            { "Origin", "https://www.tiktok.com" },
            { "Accept-Language", "en-US,en; q=0.9" },
            { "Accept-Encoding", "gzip, deflate" }
        };
    }
}