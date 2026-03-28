<<<<<<< HEAD
<<<<<<< HEAD
import './assets/app.css'
import 'leaflet/dist/leaflet.css'
=======
import './assets/main.css'
>>>>>>> 1248e0acbeb7847ddd41678e923e753c890644a0
=======
import './assets/app.css'
import 'leaflet/dist/leaflet.css'
>>>>>>> efea528 (correções accept changes)

import { createApp } from 'vue'
import { createPinia } from 'pinia'

import App from './App.vue'
import router from './router'

const app = createApp(App)

app.use(createPinia())
app.use(router)

app.mount('#app')
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> efea528 (correções accept changes)

// Registrar Service Worker da PWA
import { registerSW } from 'virtual:pwa-register'
registerSW({
  onNeedRefresh() {
    if (confirm('Nova versão disponível. Atualizar agora?')) {
      location.reload()
    }
  }
})
<<<<<<< HEAD
=======
>>>>>>> 1248e0acbeb7847ddd41678e923e753c890644a0
=======
>>>>>>> efea528 (correções accept changes)
