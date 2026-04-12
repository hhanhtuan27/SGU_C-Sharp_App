using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using VinhKhanhGuide.Controls;
using VinhKhanhGuide.Models;
using VinhKhanhGuide.Services;

namespace VinhKhanhGuide.Forms
{
    public partial class MainForm : Form
    {
        // ---------- Services ----------
        private readonly PoiRepository     _repo;
        private readonly GeofenceService   _geofence;
        private readonly NarrationService  _narration;
        private readonly GpsSimulator      _simulator;
        private List<PointOfInterest>      _pois;

        // ---------- Theme ----------
        private static readonly Color ColBg         = Color.FromArgb(22,  24,  28);
        private static readonly Color ColPanel      = Color.FromArgb(30,  32,  38);
        private static readonly Color ColPanelAlt   = Color.FromArgb(37,  40,  47);
        private static readonly Color ColBorder     = Color.FromArgb(55,  60,  70);
        private static readonly Color ColAccent     = Color.FromArgb(255, 140, 66);  // neon orange
        private static readonly Color ColText       = Color.FromArgb(235, 237, 240);
        private static readonly Color ColTextMuted  = Color.FromArgb(160, 165, 175);

        // ---------- Map layers ----------
        private GMapControl       _map;
        private GMapOverlay       _poiOverlay;
        private GMapOverlay       _userOverlay;
        private GMapOverlay       _radiusOverlay;
        private GMapMarker        _userMarker;

        // ---------- Left pane ----------
        private PoiListBox _poiList;
        private TextBox    _searchBox;
        private Label      _lblStatus;
        private Button     _btnPlay;
        private Button     _btnLang;

        // ---------- Status strip ----------
        private Label _lblCoords;
        private Label _lblZoom;
        private Label _lblSelection;

        // ---------- State ----------
        private bool   _tracking;
        private string _language = "VN";
        private readonly Timer _tickTimer = new Timer { Interval = 1500 };

        public MainForm()
        {
            InitializeComponent();

            // Connection string is read from App.config but we also pass a default
            // so students can run the app the first time without touching config.
            var connStr = System.Configuration.ConfigurationManager
                             .ConnectionStrings["VinhKhanhDb"]?.ConnectionString
                         ?? @"Server=(local);Database=VinhKhanhGuide;Integrated Security=True;";

            _repo       = new PoiRepository(connStr);
            _pois       = _repo.LoadAll();
            _geofence   = new GeofenceService(_pois);
            _narration  = new NarrationService();
            _simulator  = new GpsSimulator();

            WireEvents();
            LoadMap();
            PopulatePoiList();
            UpdateStatus(_language == "VN" ? "Sẵn sàng" : "Ready");
        }

        // ================================================================
        // Layout (all dark-themed, no designer file needed)
        // ================================================================
        private void InitializeComponent()
        {
            SuspendLayout();
            Text          = "POI Navigator — Vĩnh Khánh, Ho Chi Minh City";
            Size          = new Size(1360, 780);
            MinimumSize   = new Size(1024, 640);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColBg;
            ForeColor     = ColText;
            Font          = new Font("Segoe UI", 9f);

            // --- Split container ---
            var split = new SplitContainer
            {
                Dock             = DockStyle.Fill,
                SplitterDistance = 320,
                SplitterWidth    = 1,
                BackColor        = ColBorder,
                FixedPanel       = FixedPanel.Panel1,
                IsSplitterFixed  = false
            };
            split.Panel1.BackColor = ColPanel;
            split.Panel2.BackColor = ColBg;

            BuildLeftPane(split.Panel1);
            BuildRightPane(split.Panel2);

            Controls.Add(split);
            Controls.Add(BuildStatusStrip());
            Controls.Add(BuildMenuStrip());
            MainMenuStrip = (MenuStrip)Controls[Controls.Count - 1];

            ResumeLayout(false);
            PerformLayout();
        }

        private MenuStrip BuildMenuStrip()
        {
            var menu = new MenuStrip
            {
                BackColor  = ColPanel,
                ForeColor  = ColText,
                Renderer   = new DarkMenuRenderer()
            };
            foreach (var label in new[] { "File", "View", "Map", "Tools", "Help" })
                menu.Items.Add(new ToolStripMenuItem(label) { ForeColor = ColText });
            return menu;
        }

        private void BuildLeftPane(Control host)
        {
            // Header
            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 42,
                BackColor = Color.FromArgb(58, 50, 90)
            };
            header.Paint += (s, e) =>
            {
                using (var f = new Font("Segoe UI Semibold", 10.5f, FontStyle.Bold))
                using (var b = new SolidBrush(ColText))
                    e.Graphics.DrawString("📍  Points of Interest", f, b, 14, 11);
            };

            // Search box with rounded border
            var searchPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = ColPanel,
                Padding   = new Padding(12, 8, 12, 8)
            };
            _searchBox = new TextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = ColPanelAlt,
                ForeColor   = ColText,
                BorderStyle = BorderStyle.FixedSingle,
                Font        = new Font("Segoe UI", 9.5f)
            };
            _searchBox.Text = "Search POIs...";
            _searchBox.GotFocus  += (s, e) => { if (_searchBox.Text == "Search POIs...") _searchBox.Text = ""; };
            _searchBox.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(_searchBox.Text)) _searchBox.Text = "Search POIs..."; };
            _searchBox.TextChanged += (s, e) => ApplyFilter();
            searchPanel.Controls.Add(_searchBox);

            // Bottom controls panel
            var controlsPanel = new GroupBox
            {
                Dock      = DockStyle.Bottom,
                Height    = 110,
                Text      = "  Controls  ",
                ForeColor = ColTextMuted,
                Padding   = new Padding(10)
            };

            _btnPlay = MakeFlatButton("▶  Play\nNarration", ColAccent);
            _btnPlay.Dock  = DockStyle.Left;
            _btnPlay.Width = 130;
            _btnPlay.Click += BtnPlay_Click;

            _btnLang = MakeFlatButton("🌐  VN\nVN / EN", Color.FromArgb(80, 140, 200));
            _btnLang.Dock  = DockStyle.Left;
            _btnLang.Width = 130;
            _btnLang.Click += BtnLang_Click;

            var spacer = new Panel { Dock = DockStyle.Left, Width = 8, BackColor = Color.Transparent };

            _lblStatus = new Label
            {
                Dock      = DockStyle.Bottom,
                Height    = 22,
                Text      = "⏹ Stopped",
                ForeColor = ColTextMuted,
                BackColor = ColPanelAlt,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(8, 0, 0, 0)
            };

            controlsPanel.Controls.Add(_btnLang);
            controlsPanel.Controls.Add(spacer);
            controlsPanel.Controls.Add(_btnPlay);
            controlsPanel.Controls.Add(_lblStatus);

            // The POI list fills the remaining space
            _poiList = new PoiListBox { Dock = DockStyle.Fill };
            _poiList.SelectedIndexChanged += PoiList_SelectedIndexChanged;
            _poiList.DoubleClick          += (s, e) => PlaySelectedPoi();

            host.Controls.Add(_poiList);
            host.Controls.Add(controlsPanel);
            host.Controls.Add(searchPanel);
            host.Controls.Add(header);
        }

        private void BuildRightPane(Control host)
        {
            // Toolbar
            var toolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 42,
                BackColor = ColPanel
            };
            toolbar.Paint += (s, e) =>
            {
                using (var pen = new Pen(ColBorder))
                    e.Graphics.DrawLine(pen, 0, toolbar.Height - 1, toolbar.Width, toolbar.Height - 1);
            };

            int x = 8;
            foreach (var (label, handler) in new (string, EventHandler)[]
            {
                ("🔍  Zoom In",  (s,e)=>{ if(_map!=null) _map.Zoom += 1; UpdateZoomLabel(); }),
                ("🔎  Zoom Out", (s,e)=>{ if(_map!=null) _map.Zoom -= 1; UpdateZoomLabel(); }),
                ("◎  Centre",    (s,e)=> CenterOnPois()),
            })
            {
                var b = MakeToolbarButton(label);
                b.Location = new Point(x, 6);
                b.Click += handler;
                toolbar.Controls.Add(b);
                x += b.Width + 6;
            }
            var lblZoomTag = new Label
            {
                Text      = "Zoom: 15",
                ForeColor = ColText,
                Location  = new Point(x + 4, 13),
                AutoSize  = true
            };
            toolbar.Controls.Add(lblZoomTag);
            _lblZoom = lblZoomTag;

            // Map
            _map = new GMapControl
            {
                Dock          = DockStyle.Fill,
                MapProvider   = GMapProviders.OpenStreetMap,
                MinZoom       = 2,
                MaxZoom       = 20,
                Zoom          = 16,
                DragButton    = MouseButtons.Left,
                ShowCenter    = false,
                Position      = new PointLatLng(10.7607, 106.7035),
                BackColor     = ColBg
            };
            _map.MouseDoubleClick += Map_MouseDoubleClick;
            _map.OnMapZoomChanged += () => UpdateZoomLabel();

            host.Controls.Add(_map);
            host.Controls.Add(toolbar);
        }

        private StatusStrip BuildStatusStrip()
        {
            var strip = new StatusStrip
            {
                BackColor    = ColPanel,
                SizingGrip   = false,
                Renderer     = new DarkMenuRenderer()
            };
            strip.Items.Add(new ToolStripStatusLabel("Status: Ready") { ForeColor = ColText });
            strip.Items.Add(new ToolStripStatusLabel() { Spring = true });
            var lblCoords = new ToolStripStatusLabel("Lat: 10.7607°  Lon: 106.7035°") { ForeColor = ColTextMuted };
            var lblSel    = new ToolStripStatusLabel("No selection")                  { ForeColor = ColTextMuted };
            var lblStop   = new ToolStripStatusLabel("■ Stopped")                      { ForeColor = ColTextMuted };
            strip.Items.Add(lblCoords);
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(lblSel);
            strip.Items.Add(new ToolStripSeparator());
            strip.Items.Add(lblStop);

            _lblCoords    = new Label(); // placeholder (we update the strip items directly)
            _lblSelection = new Label();
            // Expose update helpers via closures
            _updateCoords    = (lat, lon) => lblCoords.Text = $"Lat: {lat:F4}°  Lon: {lon:F4}°";
            _updateSelection = text       => lblSel.Text    = text;
            _updateRunState  = running    => lblStop.Text   = running ? "▶ Tracking" : "■ Stopped";
            return strip;
        }

        private Action<double, double> _updateCoords;
        private Action<string>         _updateSelection;
        private Action<bool>           _updateRunState;

        private Button MakeFlatButton(string text, Color accent)
        {
            var b = new Button
            {
                Text       = text,
                ForeColor  = ColText,
                BackColor  = Color.FromArgb(45, 48, 56),
                FlatStyle  = FlatStyle.Flat,
                Font       = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                TextAlign  = ContentAlignment.MiddleCenter
            };
            b.FlatAppearance.BorderColor = accent;
            b.FlatAppearance.BorderSize  = 1;
            b.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 65, 75);
            return b;
        }

        private Button MakeToolbarButton(string text)
        {
            var b = new Button
            {
                Text       = text,
                ForeColor  = ColText,
                BackColor  = ColPanelAlt,
                FlatStyle  = FlatStyle.Flat,
                AutoSize   = true,
                Padding    = new Padding(8, 2, 8, 2)
            };
            b.FlatAppearance.BorderColor = ColBorder;
            b.FlatAppearance.BorderSize  = 1;
            return b;
        }

        // ================================================================
        // Services wiring
        // ================================================================
        private void WireEvents()
        {
            _geofence.GeofenceEntered += Geofence_Entered;
            _narration.SpeakingStarted   += (s, text) => BeginInvoke((Action)(() => { _lblStatus.Text = "🔊 " + (text ?? ""); }));
            _narration.SpeakingCompleted += (s, e)    => BeginInvoke((Action)(() =>
            {
                _lblStatus.Text = _tracking ? "▶ Listening..." : "⏹ Stopped";
                _poiList.NowPlayingPoiId = -1;
                _poiList.Invalidate();
            }));
            _simulator.LocationChanged += Simulator_LocationChanged;
            _tickTimer.Tick += (s, e) => _geofence.UpdateLocation(
                _simulator.CurrentLatitude, _simulator.CurrentLongitude);
        }

        private void LoadMap()
        {
            GMaps.Instance.Mode = AccessMode.ServerAndCache;

            _poiOverlay    = new GMapOverlay("pois");
            _userOverlay   = new GMapOverlay("user");
            _radiusOverlay = new GMapOverlay("radius");
            _map.Overlays.Add(_radiusOverlay);
            _map.Overlays.Add(_poiOverlay);
            _map.Overlays.Add(_userOverlay);

            foreach (var poi in _pois)
            {
                var color = poi.Category == PoiCategory.CaPhe
                    ? GMarkerGoogleType.blue_small
                    : GMarkerGoogleType.red_small;
                var m = new GMarkerGoogle(new PointLatLng(poi.Latitude, poi.Longitude), color)
                {
                    ToolTipText = poi.Name,
                    ToolTipMode = MarkerTooltipMode.OnMouseOver,
                    Tag         = poi
                };
                _poiOverlay.Markers.Add(m);
            }

            _userMarker = new GMarkerGoogle(
                new PointLatLng(10.760719, 106.703297),
                GMarkerGoogleType.green);
            _userMarker.ToolTipText = "You";
            _userOverlay.Markers.Add(_userMarker);

            CenterOnPois();
            UpdateZoomLabel();
        }

        private void PopulatePoiList()
        {
            _poiList.Language = _language;
            _poiList.SetSource(_pois);
        }

        private void ApplyFilter()
        {
            var q = _searchBox.Text?.Trim();
            if (string.IsNullOrEmpty(q) || q == "Search POIs...")
            {
                _poiList.SetSource(_pois);
                return;
            }
            _poiList.SetSource(_pois.Where(p =>
                p.Name.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0));
        }

        // ================================================================
        // Event handlers
        // ================================================================
        private void PoiList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_poiList.SelectedItem is PointOfInterest poi)
            {
                _map.Position = new PointLatLng(poi.Latitude, poi.Longitude);
                _map.Zoom     = 18;
                UpdateZoomLabel();
                _updateSelection?.Invoke(poi.Name);
            }
        }

        private void PlaySelectedPoi()
        {
            if (_poiList.SelectedItem is PointOfInterest poi)
            {
                _poiList.NowPlayingPoiId = poi.Id;
                _poiList.Invalidate();
                _narration.Speak(poi);
            }
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            _tracking = !_tracking;
            if (_tracking)
            {
                _tickTimer.Start();
                // Demo route: walks along Vinh Khanh street so the geofence
                // fires on several POIs in sequence.
                _simulator.StartRoute(new[]
                {
                    (10.760600, 106.703200),
                    (10.760719, 106.703297),
                    (10.760836, 106.703505),
                    (10.760800, 106.703800),
                    (10.760780, 106.704200),
                    (10.760778, 106.704739),
                    (10.760846, 106.704983),
                    (10.761201, 106.706126),
                }, 2500);
                _btnPlay.Text = "⏸  Pause\nNarration";
                UpdateStatus(_language == "VN" ? "Đang theo dõi..." : "Tracking...");
            }
            else
            {
                _tickTimer.Stop();
                _simulator.StopRoute();
                _narration.Stop();
                _btnPlay.Text = "▶  Play\nNarration";
                UpdateStatus(_language == "VN" ? "Đã dừng" : "Stopped");
            }
            _updateRunState?.Invoke(_tracking);
        }

        private void BtnLang_Click(object sender, EventArgs e)
        {
            _language = _language == "VN" ? "EN" : "VN";
            _btnLang.Text = $"🌐  {_language}\nVN / EN";
            _narration.SetLanguage(_language);
            _poiList.Language = _language;
            _poiList.Invalidate();
        }

        private void Map_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // Double-click acts as manual GPS jump — lets the demo "walk"
            // the user anywhere on the map without a physical GPS.
            var p = _map.FromLocalToLatLng(e.X, e.Y);
            _simulator.SetLocation(p.Lat, p.Lng);
        }

        private void Simulator_LocationChanged(object sender, LocationChangedEventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                _userMarker.Position = new PointLatLng(e.Latitude, e.Longitude);
                _map.Position        = _userMarker.Position;
                _updateCoords?.Invoke(e.Latitude, e.Longitude);
                _geofence.UpdateLocation(e.Latitude, e.Longitude);
            }));
        }

        private void Geofence_Entered(object sender, GeofenceEnteredEventArgs e)
        {
            BeginInvoke((Action)(() =>
            {
                _poiList.NowPlayingPoiId = e.Poi.Id;
                _poiList.Invalidate();
                _updateSelection?.Invoke($"▶ {e.Poi.Name} ({e.DistanceMeters:F1} m)");
                _narration.Speak(e.Poi);
            }));
        }

        // ================================================================
        // Helpers
        // ================================================================
        private void CenterOnPois()
        {
            if (_pois == null || _pois.Count == 0) return;
            var pts = _pois.Select(p => new PointLatLng(p.Latitude, p.Longitude)).ToList();
            var rect = RectLatLngFromPoints(pts);
            _map.SetZoomToFitRect(rect);
            UpdateZoomLabel();
        }

        private static RectLatLng RectLatLngFromPoints(List<PointLatLng> pts)
        {
            double minLat = pts.Min(p => p.Lat), maxLat = pts.Max(p => p.Lat);
            double minLng = pts.Min(p => p.Lng), maxLng = pts.Max(p => p.Lng);
            return RectLatLng.FromLTRB(minLng, maxLat, maxLng, minLat);
        }

        private void UpdateZoomLabel()
        {
            if (_map != null && _lblZoom != null)
                _lblZoom.Text = $"Zoom: {(int)_map.Zoom}";
        }

        private void UpdateStatus(string text)
        {
            _lblStatus.Text = text;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _tickTimer?.Stop();
            _simulator?.StopRoute();
            _narration?.Dispose();
            base.OnFormClosing(e);
        }

        // --- Custom renderer so MenuStrip/StatusStrip follow the dark theme ---
        private class DarkMenuRenderer : ToolStripProfessionalRenderer
        {
            public DarkMenuRenderer() : base(new DarkColors()) { }
        }
        private class DarkColors : ProfessionalColorTable
        {
            public override Color MenuItemSelected            => ColPanelAlt;
            public override Color MenuItemSelectedGradientBegin => ColPanelAlt;
            public override Color MenuItemSelectedGradientEnd   => ColPanelAlt;
            public override Color ToolStripDropDownBackground   => ColPanel;
            public override Color ImageMarginGradientBegin      => ColPanel;
            public override Color ImageMarginGradientMiddle     => ColPanel;
            public override Color ImageMarginGradientEnd        => ColPanel;
            public override Color MenuBorder                    => ColBorder;
            public override Color MenuItemBorder                => ColAccent;
            public override Color ToolStripBorder               => ColBorder;
            public override Color StatusStripGradientBegin      => ColPanel;
            public override Color StatusStripGradientEnd        => ColPanel;
        }
    }
}
