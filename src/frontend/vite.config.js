import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react-swc';

const target =
  process.env.services__storage__https__0 ||
  process.env.services__storage__http__0;


const port = process.env.PORT;

export default defineConfig({
  plugins: [react()],
  server: {
    port: port,
    proxy: {
      '/api': { target, changeOrigin: true, secure: false },
    },
  },
  build: { outDir: 'build' },
});


