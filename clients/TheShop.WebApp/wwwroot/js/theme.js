export function applyTheme(isDark) {
    const theme = isDark ? 'dark' : 'light';
    const themeClass = isDark ? 'theme-dark' : 'theme-light';
    
    document.body.classList.remove('theme-dark', 'theme-light');
    document.body.classList.add(themeClass);
    
    // Store in both localStorage and cookie for server-side access
    localStorage.setItem('theme', theme);
    document.cookie = `theme=${theme};path=/;max-age=31536000;SameSite=Lax`;
}

export function getStoredTheme() {
    return localStorage.getItem('theme');
}

export function setStoredTheme(theme) {
    localStorage.setItem('theme', theme);
    document.cookie = `theme=${theme};path=/;max-age=31536000;SameSite=Lax`;
}

