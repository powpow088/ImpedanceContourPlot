import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { viteSingleFile } from 'vite-plugin-singlefile'

export default defineConfig({
  base: './',
  plugins: [vue()],
  build: {
    emptyOutDir: false
  },
  server: {
    open: true
  }
})
