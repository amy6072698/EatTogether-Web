import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'

import 'bootstrap'
import 'bootstrap/dist/css/bootstrap.min.css'

// Design System css (順序必須在 bootstrap 之後)
import './assets/css/variables.css'
import './assets/css/base.css'
import './assets/css/components.css'

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.mount('#app')
