import { createRouter, createWebHistory } from 'vue-router'
import { useAuth }   from './composables/useAuth'
import RulesView     from './views/RulesView.vue'
import LoginView     from './views/LoginView.vue'

const routes = [
    { path: '/',       redirect: '/rules' },
    { path: '/login',  component: LoginView, meta: { public: true } },
    { path: '/rules',  component: RulesView, meta: { public: false } },
]

const router = createRouter({
    history: createWebHistory(),
    routes,
})

// ── Navigation guard ────────────────────────────────────────────────────
router.beforeEach((to) =>
{
    const { isLoggedIn } = useAuth()

    if (to.meta.public === false && !isLoggedIn.value)
    {
        return { path: '/login', query: { redirect: to.fullPath } }
    }

    // Already logged in but navigating to the login page — redirect back.
    if (to.path === '/login' && isLoggedIn.value)
    {
        return { path: '/' }
    }

    return true
})
// ─────────────────────────────────────────────────────────────────────────

export default router
