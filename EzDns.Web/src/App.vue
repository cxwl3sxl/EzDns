<template>
  <div class="layout">
    <aside class="sidebar" v-if="showLayout">
      <div class="sidebar-brand">
        <div class="brand-icon">
          <svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <line x1="2" y1="12" x2="22" y2="12"/>
            <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/>
          </svg>
        </div>
        <span class="brand-name">EzDns</span>
        <span class="brand-badge">v1.0</span>
      </div>

      <nav class="sidebar-nav">
        <router-link to="/rules" class="nav-item" active-class="active">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
          </svg>
          <span>DNS 规则</span>
          <span class="nav-badge" v-if="stats">{{ rulesCount }}</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <div class="sidebar-status">
          <span class="status-dot"></span>
          <span>服务运行中</span>
        </div>
      </div>
    </aside>

    <main class="main-content">
      <header class="topbar" v-if="showLayout">
        <div class="topbar-title">
          <h1>DNS 规则管理</h1>
          <p class="topbar-subtitle">配置和管理 DNS 解析规则</p>
        </div>
        <div class="topbar-actions">
          <button class="btn-primary" @click="$emit('refresh')">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" :class="{ spinning: loading }">
              <polyline points="23 4 23 10 17 10"/>
              <polyline points="1 20 1 14 7 14"/>
              <path d="M3.51 9a9 9 0 0 1 14.85-3.36L23 10M1 14l4.64 4.36A9 9 0 0 0 20.49 15"/>
            </svg>
            <span>刷新</span>
          </button>
          <span class="topbar-user" v-if="auth.isLoggedIn">
            <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            {{ auth.username }}
          </span>
          <button class="btn-logout" v-if="auth.isLoggedIn" @click="onLogout" title="退出登录">
            <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
            <span>退出</span>
          </button>
        </div>
      </header>

      <div class="content">
        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </div>
    </main>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRoute }   from 'vue-router'
import { useAuth } from './composables/useAuth'

const route  = useRoute()
const auth   = useAuth()

const showLayout = computed(() =>
    auth.isLoggedIn.value && route.path !== '/login'
)

defineProps<{ loading?: boolean }>()
defineEmits<{ refresh: [] }>()

function onLogout()
{
    auth.logout()
    window.location.href = '/login'
}
</script>

<style scoped>
.layout {
  display: flex;
  min-height: 100vh;
}

.sidebar {
  width: 260px;
  background: var(--bg-surface);
  border-right: 1px solid var(--border);
  display: flex;
  flex-direction: column;
  padding: 28px 16px;
  position: fixed;
  top: 0;
  left: 0;
  bottom: 0;
  z-index: 10;
}

.sidebar-brand {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 0 12px;
  margin-bottom: 40px;
}

.brand-icon {
  width: 40px;
  height: 40px;
  background: var(--accent-dim);
  border: 1px solid rgba(0, 209, 193, 0.2);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--accent);
  flex-shrink: 0;
}

.brand-name {
  font-family: var(--font-display);
  font-weight: 700;
  font-size: 1.25rem;
  letter-spacing: -0.02em;
  color: var(--text-primary);
}

.brand-badge {
  font-family: var(--font-mono);
  font-size: 0.65rem;
  font-weight: 500;
  color: var(--text-muted);
  background: rgba(255,255,255,0.04);
  border: 1px solid var(--border);
  border-radius: 4px;
  padding: 2px 6px;
  margin-left: 4px;
}

.sidebar-nav {
  flex: 1;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 11px 14px;
  border-radius: var(--radius-md);
  color: var(--text-secondary);
  text-decoration: none;
  font-size: 0.93rem;
  font-weight: 500;
  transition: all 0.2s ease;
  margin-bottom: 2px;
  position: relative;
}

.nav-item:hover {
  background: var(--bg-hover);
  color: var(--text-primary);
}

.nav-item.active {
  background: var(--accent-dim);
  color: var(--accent);
  border: 1px solid rgba(0, 209, 193, 0.15);
}

.nav-item svg {
  flex-shrink: 0;
  opacity: 0.7;
}

.nav-item.active svg {
  opacity: 1;
}

.nav-badge {
  margin-left: auto;
  font-family: var(--font-mono);
  font-size: 0.7rem;
  font-weight: 600;
  background: var(--accent-dim);
  color: var(--accent);
  border-radius: 20px;
  padding: 2px 8px;
}

.sidebar-footer {
  padding-top: 20px;
  border-top: 1px solid var(--border);
}

.sidebar-status {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 0.78rem;
  color: var(--text-muted);
  padding: 0 12px;
}

.status-dot {
  width: 7px;
  height: 7px;
  border-radius: 50%;
  background: var(--success);
  box-shadow: 0 0 8px rgba(61, 214, 140, 0.5);
  animation: pulse 2.5s ease-in-out infinite;
}

@keyframes pulse {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.4; }
}

.main-content {
  flex: 1;
  margin-left: 260px;
  min-height: 100vh;
}

.topbar {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 28px 36px;
  border-bottom: 1px solid var(--border);
  background: rgba(18, 22, 31, 0.6);
  backdrop-filter: blur(12px);
  position: sticky;
  top: 0;
  z-index: 5;
}

.topbar-title h1 {
  font-family: var(--font-display);
  font-weight: 600;
  font-size: 1.35rem;
  letter-spacing: -0.025em;
  color: var(--text-primary);
}

.topbar-subtitle {
  font-size: 0.82rem;
  color: var(--text-muted);
  margin-top: 4px;
}

.topbar-actions {
  display: flex;
  gap: 10px;
}

.btn-primary {
  display: inline-flex;
  align-items: center;
  gap: 8px;
  padding: 9px 18px;
  background: var(--accent-dim);
  color: var(--accent);
  border: 1px solid rgba(0, 209, 193, 0.2);
  border-radius: var(--radius-md);
  font-family: var(--font-body);
  font-size: 0.85rem;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.2s ease;
}

.btn-primary:hover {
  background: rgba(0, 209, 193, 0.18);
  border-color: rgba(0, 209, 193, 0.35);
  box-shadow: var(--shadow-glow);
}

.spinning {
  animation: spin 1s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.topbar-user {
  display: inline-flex;
  align-items: center;
  gap: 7px;
  padding: 7px 14px;
  font-size: 0.82rem;
  color: var(--text-secondary);
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
}

.btn-logout {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 8px 16px;
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--danger);
  background: rgba(242, 87, 87, 0.08);
  border: 1px solid rgba(242, 87, 87, 0.2);
  border-radius: var(--radius-md);
  cursor: pointer;
  transition: all 0.2s;
}

.btn-logout:hover {
  background: rgba(242, 87, 87, 0.15);
  border-color: rgba(242, 87, 87, 0.35);
}

.content {
  padding: 36px;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.25s ease, transform 0.25s ease;
}

.fade-enter-from {
  opacity: 0;
  transform: translateY(8px);
}

.fade-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}

@media (max-width: 900px) {
  .sidebar {
    width: 64px;
    padding: 20px 10px;
    align-items: center;
  }

  .sidebar-brand {
    justify-content: center;
    margin-bottom: 32px;
  }

  .brand-name,
  .brand-badge,
  .nav-item span,
  .sidebar-status span {
    display: none;
  }

  .nav-item {
    justify-content: center;
    padding: 12px;
  }

  .nav-badge {
    display: none;
  }

  .main-content {
    margin-left: 64px;
  }

  .content {
    padding: 24px 20px;
  }
}
</style>
