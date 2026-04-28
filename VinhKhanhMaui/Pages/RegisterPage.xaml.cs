namespace VinhKhanhMaui.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
        BtnRegister.Clicked += OnRegisterClicked;
        BtnBackLogin.Clicked += OnBackClicked;
    }

    private async void OnRegisterClicked(object? sender, EventArgs e)
    {
        string fullName = TxtFullName.Text?.Trim() ?? "";
        string username = TxtUsername.Text?.Trim() ?? "";
        string password = TxtPassword.Text?.Trim() ?? "";
        string confirm = TxtConfirm.Text?.Trim() ?? "";

        // Validation
        if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(username))
        {
            await this.DisplayAlert("Thiếu thông tin",
                "Vui lòng điền họ tên và tên đăng nhập", "OK");
            return;
        }

        if (password.Length < 4)
        {
            await this.DisplayAlert("Mật khẩu yếu",
                "Mật khẩu phải có ít nhất 4 ký tự", "OK");
            return;
        }

        if (password != confirm)
        {
            await this.DisplayAlert("Không khớp",
                "Xác nhận mật khẩu không khớp", "OK");
            return;
        }

        // Check trùng username (local storage)
        string existing = Preferences.Get($"user_{username}", "");
        if (!string.IsNullOrEmpty(existing))
        {
            await this.DisplayAlert("Đã tồn tại",
                "Tên đăng nhập đã được sử dụng", "OK");
            return;
        }

        // Save user (local, sau này thay bằng API call tới web)
        // Format: "password|fullname|email"
        string email = TxtEmail.Text?.Trim() ?? "";
        Preferences.Set($"user_{username}", $"{password}|{fullName}|{email}");

        await this.DisplayAlert("Thành công",
            $"Tài khoản '{username}' đã được tạo. Hãy đăng nhập.", "OK");

        // Quay về login
        Application.Current!.Windows[0].Page = new LoginPage();
    }

    private void OnBackClicked(object? sender, EventArgs e)
    {
        Application.Current!.Windows[0].Page = new LoginPage();
    }
}