import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import svgr from 'vite-plugin-svgr';
import { resolve } from 'path';

/**
 * Vite Configuration
 * Based on architecture decisions from ARCHITECTURE_DECISION_RECORD.md
 *
 * Key features:
 * - Lightning-fast HMR for development
 * - Optimized production builds with code splitting
 * - Path aliases for clean imports
 * - SVG as React components support
 */
export default defineConfig({
  plugins: [
    react(),
    svgr({
      include: '**/*.svg',
      svgrOptions: {
        exportType: 'default',
        icon: true,
      },
    }),
  ],

  resolve: {
    alias: {
      '@': resolve(__dirname, 'src'),
      '@core': resolve(__dirname, 'src/modules/@core'),
      '@operations': resolve(__dirname, 'src/modules/@operations'),
      '@inventory': resolve(__dirname, 'src/modules/@inventory'),
      '@orders': resolve(__dirname, 'src/modules/@orders'),
      '@delivery': resolve(__dirname, 'src/modules/@delivery'),
      '@equipment': resolve(__dirname, 'src/modules/@equipment'),
      '@configuration': resolve(__dirname, 'src/modules/@configuration'),
      '@sync': resolve(__dirname, 'src/modules/@sync'),
      '@reports': resolve(__dirname, 'src/modules/@reports'),
    },
  },

  server: {
    port: 3000,
    open: true,
    host: true,
  },

  build: {
    outDir: 'dist',
    sourcemap: true,
    rollupOptions: {
      output: {
        manualChunks: {
          vendor: ['react', 'react-dom', 'react-router-dom'],
          mui: ['@mui/material', '@mui/icons-material'],
          redux: ['@reduxjs/toolkit', 'react-redux'],
        },
      },
    },
  },

  css: {
    preprocessorOptions: {
      scss: {
        api: 'modern-compiler',
      },
    },
  },

  envPrefix: 'VITE_',
});
