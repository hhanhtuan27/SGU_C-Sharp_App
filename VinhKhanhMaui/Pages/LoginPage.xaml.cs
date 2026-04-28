namespace VinhKhanhMaui.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BtnLogin.Clicked += OnLoginClicked;
        BtnRegister.Clicked += OnRegisterClicked;
        BtnGuest.Clicked += OnGuestClicked;
    }

    private async void OnLoginClicked(object? sender, EventArgs e)
    {
        string user = TxtUsername.Text?.Trim() ?? "";
        string pass = TxtPassword.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(user) || string.IsNullOrWhiteSpace(pass))
        {
            await this.DisplayAlert("Thiếu thông tin", "Vui lòng điền đầy đủ", "OK");
            return;
        }

        // Check admin mặc định
        if (user == "admin" && pass == "1234")
        {
            Preferences.Set("logged_in", true);
            Preferences.Set("username", "admin");
            Preferences.Set("display_name", "Quản trị viên");
            App.GoToMain();
            return;
        }

        // Check user đã đăng ký (local storage)
        string saved = Preferences.Get($"user_{user}", "");
        if (!string.IsNullOrEmpty(saved))
        {
            var parts = saved.Split('|');
            if (parts.Length >= 1 && parts[0] == pass)
            {
                Preferences.Set("logged_in", true);
                Preferences.Set("username", user);
                Preferences.Set("display_name", parts.Length >= 2 ? parts[1] : user);
                App.GoToMain();
                return;
            }
        }

        await this.DisplayAlert("Lỗi", "Sai tên đăng nhập hoặc mật khẩu", "OK");
    }

    private void OnRegisterClicked(object? sender, EventArgs e)
    {
        Application.Current!.Windows[0].Page = new RegisterPage();
    }

    private void OnGuestClicked(object? sender, EventArgs e)
    {
        Preferences.Set("logged_in", true);
        Preferences.Set("username", "guest");
        Preferences.Set("display_name", "Khách");
        App.GoToMain();
    }
}