import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// The app host hands the API's address over by service discovery, so no address sits in the code.
const api = process.env.services__api__http__0 ?? process.env.services__api__https__0
const endpoints = ['/ingest', '/ask']

export default defineConfig({
  plugins: [react()],
  server: {
    port: Number(process.env.PORT ?? 5173),
    strictPort: true,
    proxy: api === undefined
      ? undefined
      : Object.fromEntries(endpoints.map((path) => [path, { target: api, changeOrigin: true, secure: false }])),
  },
  test: {
    environment: 'jsdom',
  },
})
