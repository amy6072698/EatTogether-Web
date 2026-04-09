import { createApp } from 'vue'
import { createPinia } from 'pinia'
import App from './App.vue'
import router from './router'

// Bootstrap
import 'bootstrap'
import 'bootstrap/dist/css/bootstrap.min.css'

// Vue Datepicker
import { VueDatePicker } from '@vuepic/vue-datepicker'
import '@vuepic/vue-datepicker/dist/main.css'

// Design System css (順序必須在 bootstrap 之後)
import './assets/css/main.css'

const app = createApp(App)
app.use(createPinia())
app.use(router)
app.component('VueDatePicker', VueDatePicker)
app.mount('#app')
