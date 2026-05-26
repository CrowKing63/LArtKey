(() => {
  const storageKey = 'lartkey-docs-theme';
  const root = document.documentElement;
  const select = document.getElementById('theme-select');
  const themeColorMeta = document.querySelector('meta[name="theme-color"]');
  const darkMedia = window.matchMedia('(prefers-color-scheme: dark)');
  const forcedColorsMedia = window.matchMedia('(forced-colors: active)');

  const themeColors = {
    light: '#f7f3ff',
    dark: '#071818',
    'high-contrast': '#000000'
  };

  function resolveTheme(theme) {
    if (theme === 'light' || theme === 'dark' || theme === 'high-contrast') {
      return theme;
    }

    if (forcedColorsMedia.matches) {
      return 'high-contrast';
    }

    return darkMedia.matches ? 'dark' : 'light';
  }

  function updateThemeColor(theme) {
    if (!themeColorMeta) {
      return;
    }

    themeColorMeta.setAttribute('content', themeColors[theme] || themeColors.light);
  }

  function applyTheme(preference, persist) {
    const resolvedTheme = resolveTheme(preference);
    root.dataset.theme = resolvedTheme;
    root.dataset.themePreference = preference;

    if (select) {
      select.value = preference;
    }

    updateThemeColor(resolvedTheme);

    if (persist) {
      localStorage.setItem(storageKey, preference);
    }
  }

  const initialPreference = root.dataset.themePreference || localStorage.getItem(storageKey) || 'system';
  applyTheme(initialPreference, false);

  if (select) {
    select.addEventListener('change', (event) => {
      applyTheme(event.target.value, true);
    });
  }

  function handleSystemThemeChange() {
    if ((root.dataset.themePreference || 'system') === 'system') {
      applyTheme('system', false);
    }
  }

  darkMedia.addEventListener('change', handleSystemThemeChange);
  forcedColorsMedia.addEventListener('change', handleSystemThemeChange);
})();
