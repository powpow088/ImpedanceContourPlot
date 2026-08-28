using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

class IconGenerator {
    static void Main() {
        int size = 256;
        using (Bitmap bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb))
        using (Graphics g = Graphics.FromImage(bmp)) {
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.Clear(Color.Transparent);

            // 圓角深色背景
            Rectangle rect = new Rectangle(12, 12, size - 24, size - 24);
            using (GraphicsPath path = RoundedRect(rect, 48)) {
                using (LinearGradientBrush bgBrush = new LinearGradientBrush(
                    new Point(0, 0), new Point(size, size),
                    Color.FromArgb(255, 10, 14, 23),
                    Color.FromArgb(255, 17, 24, 39))) {
                    g.FillPath(bgBrush, path);
                }
                using (Pen borderPen = new Pen(Color.FromArgb(180, 0, 242, 254), 6)) {
                    g.DrawPath(borderPen, path);
                }
            }

            // 繪製等高線波動線條 (Contour Wave Lines)
            using (Pen wavePen1 = new Pen(Color.FromArgb(80, 0, 242, 254), 4))
            using (Pen wavePen2 = new Pen(Color.FromArgb(120, 59, 130, 246), 4))
            using (Pen wavePen3 = new Pen(Color.FromArgb(160, 245, 158, 11), 3)) {
                wavePen1.DashStyle = DashStyle.Dash;
                g.DrawArc(wavePen1, 35, 35, 186, 186, 190, 160);
                g.DrawArc(wavePen2, 55, 55, 146, 146, 200, 140);
                g.DrawArc(wavePen3, 75, 75, 106, 106, 210, 120);
            }

            // 繪製中央核心文字 "Z0" 或 "Ω"
            using (Font font = new Font("Segoe UI", 78, FontStyle.Bold, GraphicsUnit.Pixel))
            using (LinearGradientBrush textBrush = new LinearGradientBrush(
                new Point(0, 70), new Point(0, 180),
                Color.FromArgb(255, 0, 242, 254),
                Color.FromArgb(255, 79, 172, 254))) {
                StringFormat sf = new StringFormat {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                g.DrawString("Z₀", font, textBrush, new RectangleF(0, 8, size, size), sf);
            }

            // 儲存為 PNG
            bmp.Save("app.png", ImageFormat.Png);

            // 轉換為標準 Windows .ico 格式
            SaveAsIcon(bmp, "app.ico");
        }
        Console.WriteLine("Icon generated successfully: app.ico");
    }

    static GraphicsPath RoundedRect(Rectangle bounds, int radius) {
        int diameter = radius * 2;
        Size size = new Size(diameter, diameter);
        Rectangle arc = new Rectangle(bounds.Location, size);
        GraphicsPath path = new GraphicsPath();

        path.AddArc(arc, 180, 90);
        arc.X = bounds.Right - diameter;
        path.AddArc(arc, 270, 90);
        arc.Y = bounds.Bottom - diameter;
        path.AddArc(arc, 0, 90);
        arc.X = bounds.Left;
        path.AddArc(arc, 90, 90);
        path.CloseFigure();
        return path;
    }

    static void SaveAsIcon(Bitmap srcBmp, string filePath) {
        using (FileStream fs = new FileStream(filePath, FileMode.Create))
        using (BinaryWriter bw = new BinaryWriter(fs)) {
            // ICONHEADER
            bw.Write((short)0); // Reserved
            bw.Write((short)1); // Type 1 = ICO
            bw.Write((short)1); // 1 Image

            // ICONDIRENTRY
            bw.Write((byte)0); // Width 256 -> 0
            bw.Write((byte)0); // Height 256 -> 0
            bw.Write((byte)0); // Colors
            bw.Write((byte)0); // Reserved
            bw.Write((short)1); // Color planes
            bw.Write((short)32); // Bits per pixel

            using (MemoryStream ms = new MemoryStream()) {
                srcBmp.Save(ms, ImageFormat.Png);
                byte[] pngBytes = ms.ToArray();
                bw.Write((int)pngBytes.Length); // Image size
                bw.Write((int)22); // Image offset

                // Image Data (PNG payload in modern ICO)
                bw.Write(pngBytes);
            }
        }
    }
}
