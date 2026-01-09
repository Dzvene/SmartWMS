import { createTheme } from '@mui/material/styles';

/**
 * SmartWMS Dark Theme
 *
 * Custom dark theme matching the CSS variables system.
 * Primary colors use Electric Blue (#3A7BFF) for brand consistency.
 */
export const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#3A7BFF',
      light: '#5590FF',
      dark: '#2861CC',
      contrastText: '#ffffff',
    },
    secondary: {
      main: '#6BA4FF',
      light: '#8DBDFF',
      dark: '#4A7ACC',
      contrastText: '#ffffff',
    },
    success: {
      main: '#4AC28B',
      light: '#6ED0A3',
      dark: '#3A9B6F',
      contrastText: '#ffffff',
    },
    warning: {
      main: '#F5A623',
      light: '#F7B84F',
      dark: '#C4851C',
      contrastText: '#000000',
    },
    error: {
      main: '#F45C5C',
      light: '#F77D7D',
      dark: '#C34A4A',
      contrastText: '#ffffff',
    },
    background: {
      default: '#0F0F12',
      paper: '#1A1A1F',
    },
    text: {
      primary: '#F2F2F5',
      secondary: '#C5C7D0',
      disabled: '#6C6F78',
    },
    divider: '#2D2E35',
    action: {
      active: '#D0D3DB',
      hover: 'rgba(255, 255, 255, 0.08)',
      selected: 'rgba(58, 123, 255, 0.16)',
      disabled: '#51535A',
      disabledBackground: 'rgba(255, 255, 255, 0.04)',
    },
  },
  typography: {
    fontFamily: '"Inter Variable", "IBM Plex Sans", system-ui, sans-serif',
    h1: {
      fontSize: '2.5rem',
      fontWeight: 600,
      lineHeight: 1.2,
    },
    h2: {
      fontSize: '2rem',
      fontWeight: 600,
      lineHeight: 1.25,
    },
    h3: {
      fontSize: '1.5rem',
      fontWeight: 600,
      lineHeight: 1.33,
    },
    h4: {
      fontSize: '1.125rem',
      fontWeight: 600,
      lineHeight: 1.4,
    },
    h5: {
      fontSize: '0.9375rem',
      fontWeight: 600,
      lineHeight: 1.5,
    },
    h6: {
      fontSize: '0.875rem',
      fontWeight: 600,
      lineHeight: 1.5,
    },
    body1: {
      fontSize: '0.9375rem',
      lineHeight: 1.6,
    },
    body2: {
      fontSize: '0.875rem',
      lineHeight: 1.5,
    },
    caption: {
      fontSize: '0.75rem',
      lineHeight: 1.4,
    },
    button: {
      textTransform: 'none',
      fontWeight: 500,
    },
  },
  shape: {
    borderRadius: 6,
  },
  spacing: 8,
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          scrollbarWidth: 'thin',
          scrollbarColor: 'rgba(255, 255, 255, 0.08) transparent',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 6,
          padding: '8px 16px',
          fontWeight: 500,
        },
        contained: {
          boxShadow: 'none',
          '&:hover': {
            boxShadow: 'none',
          },
        },
      },
      defaultProps: {
        disableElevation: true,
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          borderRadius: 8,
          backgroundImage: 'none',
          border: '1px solid #3A3B43',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
        },
        outlined: {
          borderColor: '#3A3B43',
        },
      },
    },
    MuiTextField: {
      defaultProps: {
        size: 'small',
        variant: 'outlined',
      },
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            backgroundColor: '#1A1A20',
            '& fieldset': {
              borderColor: '#2A2B30',
            },
            '&:hover fieldset': {
              borderColor: '#3B3C42',
            },
            '&.Mui-focused fieldset': {
              borderColor: '#3A7BFF',
            },
          },
        },
      },
    },
    MuiSelect: {
      defaultProps: {
        size: 'small',
      },
      styleOverrides: {
        root: {
          backgroundColor: '#1A1A20',
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        root: {
          borderColor: '#2D2E35',
        },
        head: {
          fontWeight: 600,
          backgroundColor: '#15151A',
          color: '#C5C7D0',
        },
      },
    },
    MuiTableRow: {
      styleOverrides: {
        root: {
          '&:hover': {
            backgroundColor: 'rgba(255, 255, 255, 0.04)',
          },
        },
      },
    },
    MuiDialog: {
      styleOverrides: {
        paper: {
          backgroundColor: '#22222A',
          borderRadius: 12,
          border: '1px solid rgba(255, 255, 255, 0.06)',
        },
      },
    },
    MuiDialogTitle: {
      styleOverrides: {
        root: {
          padding: '20px 24px',
          fontSize: '1.125rem',
          fontWeight: 600,
        },
      },
    },
    MuiDialogContent: {
      styleOverrides: {
        root: {
          padding: '16px 24px',
        },
      },
    },
    MuiDialogActions: {
      styleOverrides: {
        root: {
          padding: '16px 24px',
        },
      },
    },
    MuiTooltip: {
      styleOverrides: {
        tooltip: {
          backgroundColor: '#22222A',
          color: '#F2F2F5',
          fontSize: '0.8125rem',
          border: '1px solid #3A3B43',
          borderRadius: 6,
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 9999,
        },
        outlined: {
          borderColor: '#3A3B43',
        },
      },
    },
    MuiDivider: {
      styleOverrides: {
        root: {
          borderColor: '#2D2E35',
        },
      },
    },
    MuiIconButton: {
      styleOverrides: {
        root: {
          color: '#8D9098',
          '&:hover': {
            color: '#ffffff',
            backgroundColor: 'rgba(255, 255, 255, 0.08)',
          },
        },
      },
    },
    MuiTabs: {
      styleOverrides: {
        indicator: {
          backgroundColor: '#3A7BFF',
        },
      },
    },
    MuiTab: {
      styleOverrides: {
        root: {
          textTransform: 'none',
          fontWeight: 500,
          color: '#9A9DA6',
          '&.Mui-selected': {
            color: '#F2F2F5',
          },
        },
      },
    },
  },
});
