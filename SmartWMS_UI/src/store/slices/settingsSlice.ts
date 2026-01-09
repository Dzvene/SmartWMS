import { createSlice, PayloadAction } from '@reduxjs/toolkit';
import type { SupportedLocale } from '@/localization';

interface SettingsState {
  locale: SupportedLocale;
  sidebarCollapsed: boolean;
}

const getInitialLocale = (): SupportedLocale => {
  const saved = localStorage.getItem('smartwms_locale');
  if (saved === 'en') {
    return saved;
  }
  return 'en';
};

const initialState: SettingsState = {
  locale: getInitialLocale(),
  sidebarCollapsed: localStorage.getItem('smartwms_sidebar_collapsed') === 'true',
};

const settingsSlice = createSlice({
  name: 'settings',
  initialState,
  reducers: {
    setLocale: (state, action: PayloadAction<SupportedLocale>) => {
      state.locale = action.payload;
      localStorage.setItem('smartwms_locale', action.payload);
    },
    setSidebarCollapsed: (state, action: PayloadAction<boolean>) => {
      state.sidebarCollapsed = action.payload;
      localStorage.setItem('smartwms_sidebar_collapsed', String(action.payload));
    },
    toggleSidebar: (state) => {
      state.sidebarCollapsed = !state.sidebarCollapsed;
      localStorage.setItem('smartwms_sidebar_collapsed', String(state.sidebarCollapsed));
    },
  },
});

export const { setLocale, setSidebarCollapsed, toggleSidebar } = settingsSlice.actions;
export default settingsSlice.reducer;
