import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import axios from 'axios'

const app = createApp(App)
app.use(router)

// ── Axios JWT interceptor ───────────────────────────────────────────────
const tokenKey = 'ezdns_token'
axios.interceptors.request.use(
    (config) =>
    {
        const t = localStorage.getItem(tokenKey)
        if (t) config.headers.Authorization = `Bearer ${t}`
        return config
    },
    (err) => Promise.reject(err)
)

axios.interceptors.response.use(
    (resp) => resp,
    (err) =>
    {
        if (err.response?.status === 401)
        {
            localStorage.removeItem(tokenKey)
            if (window.location.pathname !== '/login')
                window.location.href = '/login'
        }
        return Promise.reject(err)
    }
)
// ─────────────────────────────────────────────────────────────────────────

app.mount('#app')