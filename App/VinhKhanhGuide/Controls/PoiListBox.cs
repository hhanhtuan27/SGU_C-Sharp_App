using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using VinhKhanhGuide.Models;

namespace VinhKhanhGuide.Controls
{
    /// <summary>
    /// A dark-themed, owner-drawn list of POIs. Each row has a colored
    /// category bar on the left, a bold name and a small category label.
    /// Tracks the "now playing" POI so it can highlight it distinctly.
    /// </summary>
    public class PoiListBox : ListBox
    {
        public int    NowPlayingPoiId { get; set; } = -1;
        public string Language         { get; set; } = "VN";

        private static readonly Color BgColor         = Color.FromArgb(30,  32,  38);
        private static readonly Color BgAlt           = Color.FromArgb(37,  40,  47);
        private static readonly Color SelectedBg      = Color.FromArgb(58,  63,  74);
        private static readonly Color NowPlayingBg    = Color.FromArgb(70,  45,  20);
        private static readonly Color PrimaryText     = Color.FromArgb(235, 237, 240);
        private static readonly Color SecondaryText   = Color.FromArgb(160, 165, 175);

        public PoiListBox()
        {
            DrawMode      = DrawMode.OwnerDrawFixed;
            ItemHeight    = 62;
            BorderStyle   = BorderStyle.None;
            BackColor     = BgColor;
            ForeColor     = PrimaryText;
            IntegralHeight = false;
            DoubleBuffered = true;
        }

        public void SetSource(IEnumerable<PointOfInterest> pois)
        {
            BeginUpdate();
            Items.Clear();
            foreach (var p in pois) Items.Add(p);
            EndUpdate();
        }

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= Items.Count) return;
            var poi = Items[e.Index] as PointOfInterest;
            if (poi == null) return;

            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            bool selected  = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool isPlaying = poi.Id == NowPlayingPoiId;

            // Background
            Color bg;
            if (isPlaying)    bg = NowPlayingBg;
            else if (selected) bg = SelectedBg;
            else               bg = (e.Index % 2 == 0) ? BgColor : BgAlt;

            using (var brush = new SolidBrush(bg))
                g.FillRectangle(brush, e.Bounds);

            // Category accent bar
            var accent = poi.Category.AccentColor();
            using (var b = new SolidBrush(accent))
                g.FillRectangle(b, e.Bounds.X, e.Bounds.Y + 6, 4, e.Bounds.Height - 12);

            // Circular category icon background
            int iconSize = 38;
            var iconRect = new Rectangle(e.Bounds.X + 14, e.Bounds.Y + 12, iconSize, iconSize);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(iconRect);
                using (var b = new SolidBrush(Color.FromArgb(45, accent)))
                    g.FillPath(b, path);
                using (var pen = new Pen(accent, 2f))
                    g.DrawPath(pen, path);
            }
            // Category letter inside circle
            string letter = poi.Category.ToString().Substring(0, 1);
            using (var f = new Font("Segoe UI Semibold", 14f, FontStyle.Bold))
            using (var b = new SolidBrush(accent))
            {
                var sz = g.MeasureString(letter, f);
                g.DrawString(letter, f, b,
                    iconRect.X + (iconSize - sz.Width)  / 2,
                    iconRect.Y + (iconSize - sz.Height) / 2);
            }

            // Title (POI name)
            int textX = iconRect.Right + 12;
            using (var f = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold))
            using (var b = new SolidBrush(PrimaryText))
            {
                var rect = new RectangleF(textX, e.Bounds.Y + 10,
                                          e.Bounds.Width - textX - 8, 22);
                g.DrawString(poi.Name, f, b, rect,
                    new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap });
            }

            // Category label
            using (var f = new Font("Segoe UI", 8.25f, FontStyle.Regular))
            using (var b = new SolidBrush(SecondaryText))
            {
                g.DrawString(poi.Category.DisplayName(Language), f, b,
                    textX, e.Bounds.Y + 33);
            }

            // "Now Playing" pill
            if (isPlaying)
            {
                string pill = Language == "VN" ? "ĐANG PHÁT" : "PLAYING";
                using (var f = new Font("Segoe UI Semibold", 7.5f, FontStyle.Bold))
                {
                    var sz = g.MeasureString(pill, f);
                    var pr = new RectangleF(e.Bounds.Right - sz.Width - 18,
                                            e.Bounds.Y + 34, sz.Width + 12, 16);
                    using (var path = RoundedRect(pr, 8))
                    using (var b    = new SolidBrush(Color.FromArgb(255, 140, 66)))
                        g.FillPath(b, path);
                    using (var b = new SolidBrush(Color.Black))
                        g.DrawString(pill, f, b, pr.X + 6, pr.Y + 1);
                }
            }

            // Bottom separator
            using (var pen = new Pen(Color.FromArgb(22, 24, 28)))
                g.DrawLine(pen, e.Bounds.X, e.Bounds.Bottom - 1,
                                e.Bounds.Right, e.Bounds.Bottom - 1);

            base.OnDrawItem(e);
        }

        private static GraphicsPath RoundedRect(RectangleF r, float radius)
        {
            var path = new GraphicsPath();
            float d = radius * 2;
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
