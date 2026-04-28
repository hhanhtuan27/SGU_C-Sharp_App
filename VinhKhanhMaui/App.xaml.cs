namespace VinhKhanhMaui;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        bool onboardingDone = Preferences.Get("onboarding_done", false);
        bool loggedIn = Preferences.Get("logged_in", false);

        Page startPage;
        if (!onboardingDone)
            startPage = new Pages.OnboardingPage();
        else if (!loggedIn)
            startPage = new Pages.LoginPage();
        else
            startPage = new AppShell();

        return new Window(startPage);
    }

    public static void GoToLogin()
    {
        Preferences.Remove("logged_in");
        Preferences.Remove("username");
        Preferences.Remove("display_name");
        if (Current != null)
            Current.Windows[0].Page = new Pages.LoginPage();
    }

    public static void GoToMain()
    {
        if (Current != null)
            Current.Windows[0].Page = new AppShell();
    }
}