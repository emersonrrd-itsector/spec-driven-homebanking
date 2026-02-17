import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react-swc'
import { webcrypto } from 'crypto'

// Polyfill crypto for Node.js < 18
if (!globalThis.crypto) {
  globalThis.crypto = webcrypto as Crypto
}

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/tests/setup.ts',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'node_modules/',
        'src/tests/setup.ts',
      ],
    },
  },
})
