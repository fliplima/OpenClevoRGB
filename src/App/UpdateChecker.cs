using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace OpenClevoRGB
{
    public class ReleaseInfo
    {
        public string TagName { get; set; }
        public Version ParsedVersion { get; set; }
        public string ReleaseNotes { get; set; }
        public string HtmlUrl { get; set; }
        public string InstallerDownloadUrl { get; set; }
        public string InstallerFileName { get; set; }
    }

    public class UpdateCheckResult
    {
        public bool Success { get; set; }
        public bool UpdateAvailable { get; set; }
        public Version CurrentVersion { get; set; }
        public ReleaseInfo LatestRelease { get; set; }
        public string ErrorMessage { get; set; }
    }

    public static class UpdateChecker
    {
        public const string LatestReleaseApiUrl = "https://api.github.com/repos/fliplima/OpenClevoRGB/releases/latest";
        public const string ReleasesPageUrl = "https://github.com/fliplima/OpenClevoRGB/releases/latest";

        public static Version GetCurrentVersion()
        {
            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            return NormalizeVersion(v ?? new Version(1, 0, 0, 0));
        }

        public static string FormatVersion(Version v)
        {
            if (v == null) return "0.0.0";
            Version n = NormalizeVersion(v);
            if (n.Revision > 0)
            {
                return string.Format("v{0}.{1}.{2}.{3}", n.Major, n.Minor, n.Build, n.Revision);
            }
            return string.Format("v{0}.{1}.{2}", n.Major, n.Minor, n.Build);
        }

        public static Version NormalizeVersion(Version v)
        {
            if (v == null) return new Version(0, 0, 0, 0);
            return new Version(
                Math.Max(0, v.Major),
                Math.Max(0, v.Minor),
                Math.Max(0, v.Build),
                Math.Max(0, v.Revision)
            );
        }

        public static Version ParseVersion(string versionText)
        {
            if (string.IsNullOrWhiteSpace(versionText))
            {
                return null;
            }

            string clean = versionText.Trim();
            if (clean.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                clean = clean.Substring(1);
            }

            // Remove sufixos de pré-release ou metadados (ex: 1.5.0-beta.1)
            int dashIdx = clean.IndexOf('-');
            if (dashIdx >= 0) clean = clean.Substring(0, dashIdx);
            int plusIdx = clean.IndexOf('+');
            if (plusIdx >= 0) clean = clean.Substring(0, plusIdx);

            Version parsed;
            if (Version.TryParse(clean, out parsed))
            {
                return NormalizeVersion(parsed);
            }

            return null;
        }

        public static bool IsNewerVersion(Version candidate, Version current)
        {
            if (candidate == null || current == null) return false;
            return NormalizeVersion(candidate) > NormalizeVersion(current);
        }

        public static bool IsTrustedDownloadUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            Uri uri;
            if (!Uri.TryCreate(url, UriKind.Absolute, out uri))
            {
                return false;
            }

            if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string host = uri.Host.ToLowerInvariant();
            return host == "github.com" ||
                   host == "objects.githubusercontent.com" ||
                   host == "release-assets.githubusercontent.com" ||
                   host.EndsWith(".githubusercontent.com");
        }

        public static UpdateCheckResult CheckForUpdate(
            Version currentVersionOverride = null,
            Func<string, string> httpGetOverride = null)
        {
            Version current = NormalizeVersion(currentVersionOverride ?? GetCurrentVersion());

            try
            {
                string json = (httpGetOverride != null)
                    ? httpGetOverride(LatestReleaseApiUrl)
                    : HttpGetString(LatestReleaseApiUrl, current);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return new UpdateCheckResult
                    {
                        Success = false,
                        CurrentVersion = current,
                        ErrorMessage = "Resposta vazia do servidor de atualizações."
                    };
                }

                ReleaseInfo release = ParseGitHubReleaseJson(json);
                if (release == null || release.ParsedVersion == null)
                {
                    return new UpdateCheckResult
                    {
                        Success = false,
                        CurrentVersion = current,
                        ErrorMessage = "Não foi possível interpretar os dados da versão mais recente."
                    };
                }

                bool hasUpdate = IsNewerVersion(release.ParsedVersion, current);
                return new UpdateCheckResult
                {
                    Success = true,
                    UpdateAvailable = hasUpdate,
                    CurrentVersion = current,
                    LatestRelease = release
                };
            }
            catch (WebException wex)
            {
                string msg = "Não foi possível conectar ao GitHub para verificar atualizações.";
                HttpWebResponse resp = wex.Response as HttpWebResponse;
                if (resp != null && resp.StatusCode == HttpStatusCode.NotFound)
                {
                    msg = "Nenhuma release publicada foi encontrada no repositório ainda.";
                }
                else if (resp != null && (int)resp.StatusCode == 403)
                {
                    msg = "Limite de consultas à API do GitHub atingido temporariamente. Tente mais tarde.";
                }

                return new UpdateCheckResult
                {
                    Success = false,
                    CurrentVersion = current,
                    ErrorMessage = msg
                };
            }
            catch (Exception ex)
            {
                return new UpdateCheckResult
                {
                    Success = false,
                    CurrentVersion = current,
                    ErrorMessage = "Erro ao verificar atualizações: " + ex.Message
                };
            }
        }

        public static ReleaseInfo ParseGitHubReleaseJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return null;

            string tagName = ExtractJsonStringField(json, "tag_name");
            Version parsedVersion = ParseVersion(tagName);
            if (parsedVersion == null) return null;

            string body = ExtractJsonStringField(json, "body") ?? string.Empty;

            // Garante pegar a html_url da release (e não do campo author)
            string htmlUrl = ReleasesPageUrl;
            foreach (string candidateUrl in ExtractAllJsonStringFields(json, "html_url"))
            {
                if (candidateUrl.IndexOf("/releases/", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    htmlUrl = candidateUrl;
                    break;
                }
            }

            string installerUrl = null;
            string installerName = null;

            // Extrai assets do array "assets": [...]
            List<string> downloadUrls = ExtractAllJsonStringFields(json, "browser_download_url");
            foreach (string url in downloadUrls)
            {
                if (!url.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) continue;

                string fileName = Path.GetFileName(url);
                if (string.Equals(fileName, "Instalador_OpenClevoRGB.exe", StringComparison.OrdinalIgnoreCase))
                {
                    installerUrl = url;
                    installerName = fileName;
                    break;
                }

                if (installerUrl == null &&
                    fileName != null &&
                    fileName.IndexOf("Instalador", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    installerUrl = url;
                    installerName = fileName;
                }
                else if (installerUrl == null)
                {
                    installerUrl = url;
                    installerName = fileName;
                }
            }

            return new ReleaseInfo
            {
                TagName = tagName,
                ParsedVersion = parsedVersion,
                ReleaseNotes = body.Trim(),
                HtmlUrl = htmlUrl,
                InstallerDownloadUrl = installerUrl,
                InstallerFileName = installerName
            };
        }

        public static string DownloadInstaller(ReleaseInfo release, Action<int> onProgressChanged = null)
        {
            if (release == null || string.IsNullOrWhiteSpace(release.InstallerDownloadUrl))
            {
                throw new InvalidOperationException("URL do instalador não disponível nesta release.");
            }

            if (!IsTrustedDownloadUrl(release.InstallerDownloadUrl))
            {
                throw new InvalidOperationException("URL de download rejeitada por não pertencer ao domínio oficial do GitHub.");
            }

            EnableTls12();

            string safeVersion = Regex.Replace(release.TagName ?? "latest", @"[^a-zA-Z0-9._-]", "_");
            string updateDir = Path.Combine(Path.GetTempPath(), "OpenClevoRGB_Update_" + safeVersion);
            if (!Directory.Exists(updateDir))
            {
                Directory.CreateDirectory(updateDir);
            }

            string fileName = string.IsNullOrWhiteSpace(release.InstallerFileName)
                ? "Instalador_OpenClevoRGB.exe"
                : Path.GetFileName(release.InstallerFileName);
            string destPath = Path.Combine(updateDir, fileName);

            if (File.Exists(destPath))
            {
                try { File.Delete(destPath); } catch { }
            }

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(release.InstallerDownloadUrl);
            req.Method = "GET";
            req.UserAgent = "OpenClevoRGB/" + FormatVersion(GetCurrentVersion());
            req.Timeout = 30000;
            req.ReadWriteTimeout = 30000;
            req.AllowAutoRedirect = true;

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (Stream remoteStream = resp.GetResponseStream())
            using (FileStream fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                long totalBytes = resp.ContentLength;
                long downloaded = 0;
                byte[] buffer = new byte[8192];
                int lastPercent = -1;
                int bytesRead;

                while ((bytesRead = remoteStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    fileStream.Write(buffer, 0, bytesRead);
                    downloaded += bytesRead;

                    if (onProgressChanged != null && totalBytes > 0)
                    {
                        int percent = (int)Math.Min(100, (downloaded * 100L) / totalBytes);
                        if (percent != lastPercent)
                        {
                            lastPercent = percent;
                            onProgressChanged(percent);
                        }
                    }
                }
            }

            FileInfo fi = new FileInfo(destPath);
            if (!fi.Exists || fi.Length == 0)
            {
                throw new IOException("O arquivo baixado está vazio ou corrompido.");
            }

            if (onProgressChanged != null)
            {
                onProgressChanged(100);
            }

            return destPath;
        }

        public static bool LaunchInstaller(string installerPath)
        {
            if (string.IsNullOrWhiteSpace(installerPath) || !File.Exists(installerPath))
            {
                return false;
            }

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = installerPath;
            psi.UseShellExecute = true;
            Process.Start(psi);
            return true;
        }

        private static void EnableTls12()
        {
            try
            {
                // 3072 = SecurityProtocolType.Tls12 (compatível com .NET 4.0+)
                ServicePointManager.SecurityProtocol |= (SecurityProtocolType)3072;
            }
            catch { }
        }

        private static string HttpGetString(string url, Version currentVersion)
        {
            EnableTls12();

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.UserAgent = "OpenClevoRGB/" + FormatVersion(currentVersion);
            req.Accept = "application/vnd.github+json";
            req.Timeout = 10000;
            req.ReadWriteTimeout = 10000;

            using (HttpWebResponse resp = (HttpWebResponse)req.GetResponse())
            using (Stream stream = resp.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        private static string ExtractJsonStringField(string json, string fieldName)
        {
            List<string> matches = ExtractAllJsonStringFields(json, fieldName);
            return matches.Count > 0 ? matches[0] : null;
        }

        private static List<string> ExtractAllJsonStringFields(string json, string fieldName)
        {
            List<string> results = new List<string>();
            string pattern = "\"" + Regex.Escape(fieldName) + "\"\\s*:\\s*\"((?:\\\\.|[^\"\\\\])*)\"";
            MatchCollection matches = Regex.Matches(json, pattern);
            foreach (Match m in matches)
            {
                if (m.Groups.Count > 1)
                {
                    results.Add(UnescapeJsonString(m.Groups[1].Value));
                }
            }
            return results;
        }

        internal static string UnescapeJsonString(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return string.Empty;

            StringBuilder sb = new StringBuilder(raw.Length);
            for (int i = 0; i < raw.Length; i++)
            {
                char c = raw[i];
                if (c == '\\' && i + 1 < raw.Length)
                {
                    char next = raw[++i];
                    switch (next)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'n': sb.Append('\n'); break;
                        case 'r': sb.Append('\r'); break;
                        case 't': sb.Append('\t'); break;
                        case 'u':
                            if (i + 4 < raw.Length)
                            {
                                string hex = raw.Substring(i + 1, 4);
                                int code;
                                if (int.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out code))
                                {
                                    sb.Append((char)code);
                                    i += 4;
                                }
                            }
                            break;
                        default:
                            sb.Append(next);
                            break;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }

    public class UpdateDownloadDialog : Form
    {
        private readonly ReleaseInfo release;
        private Label lblTitle;
        private Label lblStatus;
        private ProgressBar progressBar;

        public string DownloadedInstallerPath { get; private set; }
        public string DownloadError { get; private set; }

        public UpdateDownloadDialog(ReleaseInfo release)
        {
            this.release = release;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Atualizando OpenClevoRGB";
            this.Size = new Size(460, 175);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ControlBox = false;
            this.BackColor = Color.FromArgb(16, 18, 25);
            this.ForeColor = Color.FromArgb(240, 243, 250);
            this.Font = new Font("Segoe UI", 9.5f);

            lblTitle = new Label();
            lblTitle.Text = "Baixando OpenClevoRGB " + (release != null ? release.TagName : "") + "...";
            lblTitle.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(0, 229, 255);
            lblTitle.Location = new Point(22, 20);
            lblTitle.Size = new Size(400, 26);
            this.Controls.Add(lblTitle);

            progressBar = new ProgressBar();
            progressBar.Location = new Point(22, 58);
            progressBar.Size = new Size(400, 24);
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Style = ProgressBarStyle.Continuous;
            this.Controls.Add(progressBar);

            lblStatus = new Label();
            lblStatus.Text = "Conectando ao GitHub...";
            lblStatus.Font = new Font("Segoe UI", 9f);
            lblStatus.ForeColor = Color.FromArgb(140, 148, 168);
            lblStatus.Location = new Point(22, 94);
            lblStatus.Size = new Size(400, 24);
            this.Controls.Add(lblStatus);

            this.Shown += (s, e) => StartDownload();
        }

        private void StartDownload()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                try
                {
                    string path = UpdateChecker.DownloadInstaller(release, percent =>
                    {
                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            try
                            {
                                this.BeginInvoke(new Action(() =>
                                {
                                    progressBar.Value = Math.Max(0, Math.Min(100, percent));
                                    lblStatus.Text = string.Format("Baixando instalador... {0}%", percent);
                                }));
                            }
                            catch { }
                        }
                    });

                    DownloadedInstallerPath = path;
                    if (!this.IsDisposed && this.IsHandleCreated)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            this.DialogResult = DialogResult.OK;
                            this.Close();
                        }));
                    }
                }
                catch (Exception ex)
                {
                    DownloadError = ex.Message;
                    if (!this.IsDisposed && this.IsHandleCreated)
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            this.DialogResult = DialogResult.Abort;
                            this.Close();
                        }));
                    }
                }
            });
        }
    }
}
