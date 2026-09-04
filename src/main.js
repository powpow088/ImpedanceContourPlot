import { createApp } from 'vue'
import Plotly from 'plotly.js-dist-min'
import './style.css'
import App from './App.vue'

window.Plotly = Plotly

createApp(App).mount('#app')
