using VinhKhanhMaui.Services;

namespace VinhKhanhMaui;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Loaded += (s, e) =>
        {
            string name = Preferences.Get("display_name", "Khách");
            LblUserInfo.Text = $"Xin chào, {name}!";
        };
    }

    private void OnPoiListClicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        EventBus.SendMenuAction("poilist");
    }

    private void OnNearbyClicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        EventBus.SendMenuAction("nearby");
    }

    private void OnSimulatorClicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        EventBus.SendMenuAction("simulator");
    }

    private void OnRefreshClicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        EventBus.SendMenuAction("refresh");
    }

    private void OnHelpClicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        EventBus.SendMenuAction("help");
    }

    private void OnLogoutClicked(object sender, EventArgs e)
    {
        FlyoutIsPresented = false;
        App.GoToLogin();
    }
}