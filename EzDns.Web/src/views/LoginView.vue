<template>
  <div class="login-page">
    <div class="login-accent" />
    <div class="login-card">
      <div class="login-brand">
        <div class="brand-mark">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
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
        doLogin(resp.data.token, form.value.username)
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
  position: fixed;
  inset: 0;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
  overflow: hidden;
  background:
    radial-gradient(ellipse at 15% 50%, rgba(0, 209, 193, 0.10) 0%, transparent 55%),
    radial-gradient(ellipse at 85% 20%, rgba(94, 171, 240, 0.08) 0%, transparent 50%),
    var(--bg-base, var(--bg-root));
}

[data-theme="light"] .login-page {
  background:
    radial-gradient(ellipse at 15% 50%, rgba(0, 168, 153, 0.06) 0%, transparent 55%),
    radial-gradient(ellipse at 85% 20%, rgba(36, 113, 163, 0.05) 0%, transparent 50%),
    var(--bg-base, var(--bg-root));
}

.login-accent {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at 75% 30%, rgba(0, 209, 193, 0.055) 0%, transparent 45%),
    radial-gradient(circle at 20% 80%, rgba(94, 171, 240, 0.04) 0%, transparent 40%);
  pointer-events: none;
}

.login-card {
  position: relative;
  z-index: 1;
  width: 100%;
  max-width: 400px;
  background: rgba(18, 22, 31, 0.88);
  border: 1px solid rgba(255, 255, 255, 0.08);
  border-radius: 20px;
  padding: 48px 40px;
  box-shadow:
    0 0 0 1px rgba(0, 209, 193, 0.06),
    0 24px 80px rgba(0, 0, 0, 0.55),
    inset 0 1px 0 rgba(255, 255, 255, 0.06);
  backdrop-filter: blur(24px);
  -webkit-backdrop-filter: blur(24px);
  animation: cardIn 0.55s cubic-bezier(0.16, 1, 0.3, 1) both;
}

/* ── Bright / Light mode ──────────────────────────────── */
[data-theme="light"] .login-card {
  background: #ffffff;
  border: 1px solid rgba(0, 0, 0, 0.06);
  border-radius: 20px;
  padding: 48px 40px;
  box-shadow:
    0 1px 3px rgba(0, 0, 0, 0.04),
    0 20px 60px rgba(0, 0, 0, 0.08);
  backdrop-filter: none;
  -webkit-backdrop-filter: none;
}

[data-theme="light"] .login-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 40px;
  right: 40px;
  height: 3px;
  background: linear-gradient(90deg, rgba(0, 168, 153, 0.4), rgba(0, 168, 153, 0.8), rgba(0, 168, 153, 0.4));
  border-radius: 0 0 3px 3px;
  pointer-events: none;
}

[data-theme="light"] .login-accent {
  opacity: 0.5;
}

[data-theme="light"] .field-label {
  color: #4a5060;
}

[data-theme="light"] .login-hint {
  color: #aab0bc;
}

@keyframes cardIn {
  from { opacity: 0; transform: translateY(24px) scale(0.96); }
  to   { opacity: 1; transform: translateY(0) scale(1); }
}

.login-brand {
  display: flex;
  align-items: center;
  gap: 16px;
  margin-bottom: 40px;
}

.brand-mark {
  width: 52px;
  height: 52px;
  background: rgba(0, 209, 193, 0.10);
  border: 1px solid rgba(0, 209, 193, 0.22);
  border-radius: 14px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--accent);
  flex-shrink: 0;
  box-shadow: 0 0 20px rgba(0, 209, 193, 0.12);
}

[data-theme="light"] .brand-mark {
  background: rgba(0, 168, 153, 0.08);
  border: 1px solid rgba(0, 168, 153, 0.18);
  box-shadow: 0 0 16px rgba(0, 168, 153, 0.08);
}

.brand-text h1 {
  font-family: var(--font-display);
  font-weight: 700;
  font-size: 1.6rem;
  color: var(--text-primary);
  letter-spacing: -0.03em;
  line-height: 1.1;
}

.brand-text p {
  font-size: 0.78rem;
  color: var(--text-muted);
  margin-top: 4px;
}

.login-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.login-error {
  background: rgba(242, 87, 87, 0.09);
  border: 1px solid rgba(242, 87, 87, 0.28);
  border-radius: var(--radius-sm);
  color: var(--danger);
  font-size: 0.83rem;
  padding: 10px 14px;
  animation: shake 0.4s ease both;
}

[data-theme="light"] .login-error {
  background: rgba(208, 57, 57, 0.06);
  border: 1px solid rgba(208, 57, 57, 0.20);
}

@keyframes shake {
  0%, 100% { transform: translateX(0); }
  20%      { transform: translateX(-7px); }
  40%      { transform: translateX(7px); }
  60%      { transform: translateX(-5px); }
  80%      { transform: translateX(5px); }
}

.field-group {
  display: flex;
  flex-direction: column;
  gap: 7px;
}

.field-label {
  font-size: 0.76rem;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.05em;
}

.field-input {
  width: 100%;
  padding: 14px 16px;
  background: rgba(255, 255, 255, 0.04);
  border: 1px solid rgba(255, 255, 255, 0.10);
  border-radius: var(--radius-sm);
  font-family: var(--font-body);
  font-size: 0.95rem;
  color: var(--text-primary);
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s, background 0.2s;
  box-sizing: border-box;
}

.field-input::placeholder { color: var(--text-muted); }

.field-input:focus {
  background: rgba(255, 255, 255, 0.06);
  border-color: rgba(0, 209, 193, 0.45);
  box-shadow: 0 0 0 3px rgba(0, 209, 193, 0.10);
}

[data-theme="light"] .field-input {
  background: #f0f2f5;
  border: 1px solid rgba(0, 0, 0, 0.10);
  color: #1a1d23;
}

[data-theme="light"] .field-input::placeholder {
  color: #aab0bc;
}

[data-theme="light"] .field-input:focus {
  background: #ffffff;
  border-color: rgba(0, 168, 153, 0.5);
  box-shadow: 0 0 0 3px rgba(0, 168, 153, 0.12);
}

.login-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 13px;
  background: var(--accent);
  color: #0b0e14;
  border: none;
  border-radius: var(--radius-sm);
  font-family: var(--font-body);
  font-size: 0.95rem;
  font-weight: 700;
  cursor: pointer;
  transition: all 0.2s;
  box-shadow: 0 2px 18px rgba(0, 209, 193, 0.28);
  letter-spacing: 0.04em;
  margin-top: 6px;
}

.login-btn:hover:not(:disabled) {
  background: #00d6be;
  box-shadow: 0 6px 28px rgba(0, 209, 193, 0.40);
  transform: translateY(-1px);
}

.login-btn:active:not(:disabled) {
  transform: translateY(0);
}

.login-btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

[data-theme="light"] .login-btn {
  background: #00a899;
  color: #ffffff;
  box-shadow: 0 4px 16px rgba(0, 168, 153, 0.30);
}

[data-theme="light"] .login-btn:hover:not(:disabled) {
  background: #009688;
  box-shadow: 0 8px 24px rgba(0, 168, 153, 0.40);
  transform: translateY(-1px);
}

[data-theme="light"] .login-btn:active:not(:disabled) {
  transform: translateY(0);
}

[data-theme="light"] .login-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
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

[data-theme="light"] .spinner-mini {
  border-top-color: #ffffff;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.login-hint {
  margin-top: 26px;
  text-align: center;
  font-size: 0.73rem;
  color: var(--text-muted);
}
</style>
