import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// If VITE_API_BASE is not set, we rely on same-origin calls.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173
  }
})
