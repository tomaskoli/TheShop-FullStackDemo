namespace TheShop.WebApp.Services;

public class ThemeService
{
    public bool IsDarkMode { get; private set; } = true;
    
    public event Action? OnThemeChanged;

    public void SetTheme(bool isDark)
    {
        IsDarkMode = isDark;
        OnThemeChanged?.Invoke();
    }

    public void ToggleTheme()
    {
        IsDarkMode = !IsDarkMode;
        OnThemeChanged?.Invoke();
    }
}

