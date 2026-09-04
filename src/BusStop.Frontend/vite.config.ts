/// <reference types="vitest/config" />
import path from 'path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { tanstackRouter } from '@tanstack/router-plugin/vite'
import { playwright } from '@vitest/browser-playwright'
import { keycloakify } from 'keycloakify/vite-plugin'

// https://vite.dev/config/
export default defineConfig({
  plugins: [
    keycloakify({
      accountThemeImplementation: 'none',
      themeName: 'busstop',
      extraThemeProperties: ['parent=keycloak'],
    }),
    tanstackRouter({
      target: 'react',
      autoCodeSplitting: true,
    }),
    react(),
    tailwindcss(),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  test: {
    silent: 'passed-only',
    unstubEnvs: true,
    browser: {
      enabled: true,
      provider: playwright(),
      instances: [
        {
          browser: 'chromium',
          viewport: { width: 1280, height: 800 },
        },
      ],
    },
    coverage: {
      provider: 'v8',
      include: ['src/**/*.{js,jsx,ts,tsx}'],
      reporter: [
        'text-summary',
        'html',
        // Match the repository-root paths used by the .NET monorepo scanner.
        ['lcovonly', { projectRoot: path.resolve(__dirname, '../..') }],
      ],
      exclude: [
        '**/*.d.ts',
        '**/*.json*',
        '**/*.{test,spec}.{js,jsx,ts,tsx}',
        'src/kc.gen.tsx',
        'src/components/ui/**',
        'src/assets/**',
        'src/tanstack-table.d.ts',
        'src/routeTree.gen.ts',
        'src/test-utils/**',
        'src/routes/**',
      ],
    },
  },
})
