import React from 'react';
import ReactDOM from 'react-dom/client';
import { Provider, useSelector } from 'react-redux';
import { PersistGate } from 'redux-persist/integration/react';
import { BrowserRouter } from 'react-router-dom';
import { IntlProvider } from 'react-intl';
import { LocalizationProvider } from '@mui/x-date-pickers';
import { AdapterLuxon } from '@mui/x-date-pickers/AdapterLuxon';
import { ThemeProvider, CssBaseline } from '@mui/material';

import { store, persistor } from './store';
import type { RootState } from './store';
import { theme } from './styles/theme';
import { getMessages, defaultLocale } from './localization';
import App from './modules/@core/App';

import './styles/main.scss';

function IntlWrapper({ children }: { children: React.ReactNode }) {
  const locale = useSelector((state: RootState) => state.settings.locale);
  const messages = getMessages(locale);

  return (
    <IntlProvider messages={messages} locale={locale} defaultLocale={defaultLocale}>
      {children}
    </IntlProvider>
  );
}

const root = ReactDOM.createRoot(
  document.getElementById('root') as HTMLElement
);

root.render(
  <React.StrictMode>
    <Provider store={store}>
      <PersistGate loading={null} persistor={persistor}>
        <IntlWrapper>
          <LocalizationProvider dateAdapter={AdapterLuxon}>
            <ThemeProvider theme={theme}>
              <CssBaseline />
              <BrowserRouter>
                <App />
              </BrowserRouter>
            </ThemeProvider>
          </LocalizationProvider>
        </IntlWrapper>
      </PersistGate>
    </Provider>
  </React.StrictMode>
);
