using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

static class Program {
    private static Mutex mutex = null;
    private static HttpListener listener = null;
    private static NotifyIcon trayIcon = null;
    private static string serverUrl = "";

    [STAThread]
    static void Main() {
        bool createdNew;
        mutex = new Mutex(true, "Global\\ImpedanceContourPlot_App_Mutex", out createdNew);

        int port = 8080;
        serverUrl = string.Format("http://localhost:{0}/", port);

        if (!createdNew) {
            // 已有執行個體在運行，直接開啟瀏覽器後安靜退出
            try { Process.Start(serverUrl); } catch {}
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        string appDir = AppDomain.CurrentDomain.BaseDirectory;

        // 啟動本地輕量 HTTP 伺服器
        Thread serverThread = new Thread(() => {
            try {
                listener = new HttpListener();
                listener.Prefixes.Add(serverUrl);
                listener.Start();

                while (listener != null && listener.IsListening) {
                    var context = listener.GetContext();
                    ThreadPool.QueueUserWorkItem((c) => {
                        var ctx = (HttpListenerContext)c;
                        try {
                            string localPath = ctx.Request.Url.LocalPath.TrimStart('/');
                            if (string.IsNullOrEmpty(localPath)) localPath = "index.html";

                            string filePath = Path.Combine(appDir, localPath.Replace('/', Path.DirectorySeparatorChar));

                            if (File.Exists(filePath)) {
                                byte[] bytes = File.ReadAllBytes(filePath);
                                string ext = Path.GetExtension(filePath).ToLower();

                                if (ext == ".html") ctx.Response.ContentType = "text/html; charset=utf-8";
                                else if (ext == ".js") ctx.Response.ContentType = "application/javascript; charset=utf-8";
                                else if (ext == ".css") ctx.Response.ContentType = "text/css; charset=utf-8";
                                else if (ext == ".svg") ctx.Response.ContentType = "image/svg+xml";
                                else if (ext == ".png") ctx.Response.ContentType = "image/png";
                                else if (ext == ".ico") ctx.Response.ContentType = "image/x-icon";
                                else if (ext == ".json") ctx.Response.ContentType = "application/json; charset=utf-8";
                                else ctx.Response.ContentType = "application/octet-stream";

                                ctx.Response.ContentLength64 = bytes.Length;
                                ctx.Response.OutputStream.Write(bytes, 0, bytes.Length);
                            } else {
                                ctx.Response.StatusCode = 404;
                            }
                        } catch {}
                        finally {
                            try { ctx.Response.OutputStream.Close(); } catch {}
                        }
                    }, context);
                }
            } catch {}
        });

        serverThread.IsBackground = true;
        serverThread.Start();

        // 建立系統匣圖示 (System Tray)
        Icon appIcon = null;
        try {
            string icoPath = Path.Combine(appDir, "app.ico");
            if (File.Exists(icoPath)) {
                appIcon = new Icon(icoPath);
            } else {
                appIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
        } catch {
            appIcon = SystemIcons.Application;
        }

        trayIcon = new NotifyIcon();
        trayIcon.Icon = appIcon;
        trayIcon.Text = "阻抗等高線分析工具 (運行中)";
        trayIcon.Visible = true;

        ContextMenu contextMenu = new ContextMenu();
        contextMenu.MenuItems.Add(new MenuItem("🌐 開啟 阻抗等高線分析", (s, e) => {
            try { Process.Start(serverUrl); } catch {}
        }));
        contextMenu.MenuItems.Add(new MenuItem("📁 開啟資料夾", (s, e) => {
            try { Process.Start(appDir); } catch {}
        }));
        contextMenu.MenuItems.Add("-");
        contextMenu.MenuItems.Add(new MenuItem("❌ 結束程式", (s, e) => {
            trayIcon.Visible = false;
            try { if (listener != null) listener.Stop(); } catch {}
            Application.Exit();
        }));

        trayIcon.ContextMenu = contextMenu;
        trayIcon.DoubleClick += (s, e) => {
            try { Process.Start(serverUrl); } catch {}
        };

        // 自動喚起預設瀏覽器開啟工具
        try {
            Process.Start(serverUrl);
        } catch {}

        Application.Run();
    }
}
