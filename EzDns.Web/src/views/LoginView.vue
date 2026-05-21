<template>
  <div class="login-page">
    <div class="login-card">
      <div class="login-brand">
        <div class="brand-mark">
          <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <line x1="2" y1="12" x2="22" y2="12"/>
            <path d="M12 2a15.3 15.3 0 0 1 4 10 15.3 15.3 0 0 1-4 10 15.3 15.3 0 0 1-4-10 15.3 15.3 0 0 1 4-10z"/>
          </svg>
        </div>
        <div class="brand-text">
          <h1>EzDns</h1>
          <p>DNS 规则管理后台</p>
        </div>
      </div>

      <form @submit.prevent="onLogin" class="login-form" autocomplete="off">
        <div class="login-error" v-if="error">{{ error }}</div>

        <div class="field-group">
          <label class="field-label" for="username">用户名</label>
          <input
            id="username"
            v-model="form.username"
            class="field-input"
            placeholder="请输入用户名"
            autoComplete="username"
            required
            autofocus
          />
        </div>

        <div class="field-group">
          <label class="field-label" for="password">密码</label>
          <input
            id="password"
            v-model="form.password"
            class="field-input"
            type="password"
            placeholder="请输入密码"
            autoComplete="current-password"
            required
          />
        </div>

        <button type="submit" class="login-btn" :disabled="submitting">
          <span v-if="!submitting">登 录</span>
          <span v-else class="spinner-mini"></span>
        </button>
      </form>

      <p class="login-hint">已登录账号将自动续期，关闭浏览器需重新登录。</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import axios from 'axios'
import { useAuth } from '../composables/useAuth'

const router    = useRouter()
const { login: doLogin } = useAuth()

const form      = ref({ username: '', password: '' })
const submitting = ref(false)
const error      = ref('')

async function onLogin()
{
    if (submitting.value || !form.value.username || !form.value.password) return
    submitting.value = true
    error.value      = ''
    try
    {
        const resp = await axios.post<{ token: string; expiresAt: string }>(
            '/api/auth/login',
            { username: form.value.username, password: form.value.password }
        )
        doLogin(resp.data.token)
        router.replace('/rules')
    }
    catch (e: unknown)
    {
        const msg = axios.isAxiosError(e) && e.response?.data
            ? String(e.response.data)
            : '登录失败，请检查网络'
        error.value = msg
    }
    finally
    {
        submitting.value = false
    }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background:
    radial-gradient(ellipse at 20% 50%, rgba(0, 209, 193, 0.07) 0%, transparent 60%),
    radial-gradient(ellipse at 80% 20%, rgba(94, 171, 240, 0.06) 0%, transparent 55%),
    var(--bg-base);
  padding: 24px;
}

.login-card {
  width: 100%;
  max-width: 400px;
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  padding: 40px 36px;
  box-shadow: 0 12px 48px rgba(0, 0, 0, 0.35);
  animation: cardIn 0.5s ease both;
}

@keyframes cardIn {
  from { opacity: 0; transform: translateY(18px) scale(0.97); }
  to   { opacity: 1; transform: translateY(0) scale(1); }
}

.login-brand {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 36px;
}

.brand-mark {
  width: 50px;
  height: 50px;
  background: var(--accent-dim);
  border: 1px solid rgba(0, 209, 193, 0.2);
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--accent);
  flex-shrink: 0;
}

.brand-text h1 {
  font-family: var(--font-display);
  font-weight: 700;
  font-size: 1.5rem;
  color: var(--text-primary);
  letter-spacing: -0.025em;
  line-height: 1;
}

.brand-text p {
  font-size: 0.78rem;
  color: var(--text-muted);
  margin-top: 4px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.login-error {
  background: rgba(242, 87, 87, 0.08);
  border: 1px solid rgba(242, 87, 87, 0.25);
  border-radius: var(--radius-sm);
  color: var(--danger);
  font-size: 0.83rem;
  padding: 10px 14px;
  animation: shake 0.35s ease both;
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  20%      { transform: translateX(-6px); }
  40%      { transform: translateX(6px); }
  60%      { transform: translateX(-4px); }
  80%      { transform: translateX(4px); }
}

.field-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.field-label {
  font-size: 0.76rem;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.field-input {
  width: 100%;
  padding: 12px 14px;
  background: var(--bg-input);
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  font-family: var(--font-body);
  font-size: 0.92rem;
  color: var(--text-primary);
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
  box-sizing: border-box;
}

.field-input:focus {
  border-color: var(--border-active);
  box-shadow: 0 0 0 3px var(--accent-dim);
}

.login-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 12px;
  background: linear-gradient(135deg, var(--accent), #00b5a4);
  color: #0b0e14;
  border: none;
  border-radius: var(--radius-sm);
  font-family: var(--font-body);
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 2px 14px rgba(0, 209, 193, 0.25);
  letter-spacing: 0.04em;
  margin-top: 4px;
}

.login-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: 0 6px 22px rgba(0, 209, 193, 0.38);
}

.login-btn:active:not(:disabled) {
  transform: translateY(0);
}

.login-btn:disabled {
  opacity: 0.65;
  cursor: default;
  transform: none;
}

.spinner-mini {
  width: 16px;
  height: 16px;
  border: 2.5px solid transparent;
  border-top-color: #0b0e14;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.login-hint {
  margin-top: 22px;
  text-align: center;
  font-size: 0.75rem;
  color: var(--text-muted);
}
</style>
