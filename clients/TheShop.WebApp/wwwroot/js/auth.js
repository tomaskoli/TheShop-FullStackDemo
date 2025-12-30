const ACCESS_COOKIE = "theshop_access_token";
const REFRESH_COOKIE = "theshop_refresh_token";

function setCookie(name, value) {
  document.cookie = `${name}=${encodeURIComponent(value)}; path=/; samesite=lax`;
}

function deleteCookie(name) {
  document.cookie = `${name}=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; samesite=lax`;
}

export function setAuthCookies(accessToken, refreshToken) {
  if (accessToken) {
    setCookie(ACCESS_COOKIE, accessToken);
  }
  if (refreshToken) {
    setCookie(REFRESH_COOKIE, refreshToken);
  }
}

export function clearAuthCookies() {
  deleteCookie(ACCESS_COOKIE);
  deleteCookie(REFRESH_COOKIE);
}


