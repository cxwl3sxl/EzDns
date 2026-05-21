<template>
  <div class="rules-view">
    <!-- Stats Cards -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon total">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
          </svg>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ stats.total }}</span>
          <span class="stat-label">全部规则</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon enabled">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="20 6 9 17 4 12"/>
          </svg>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ stats.enabled }}</span>
          <span class="stat-label">已启用</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon disabled">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <circle cx="12" cy="12" r="10"/>
            <line x1="4.93" y1="4.93" x2="19.07" y2="19.07"/>
          </svg>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ stats.disabled }}</span>
          <span class="stat-label">已禁用</span>
        </div>
      </div>
      <div class="stat-card">
        <div class="stat-icon types">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
            <polyline points="22 12 18 12 15 21 9 3 6 12 2 12"/>
          </svg>
        </div>
        <div class="stat-info">
          <span class="stat-value">{{ stats.typeCount }}</span>
          <span class="stat-label">记录类型</span>
        </div>
      </div>
    </div>

    <!-- Add Rule Form -->
    <div class="form-section">
      <div class="form-header">
        <h3>添加新规则</h3>
        <span class="form-hint">配置一条新的 DNS 解析规则</span>
      </div>
      <!-- Rule definition help -->
      <details class="help-details">
        <summary class="help-summary">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
          <span>规则说明《domain patterns, IP addressing, and priority explained》</span>
          <svg class="help-chevron" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="6 9 12 15 18 9"/></svg>
        </summary>
        <div class="help-body">
          <div class="help-grid">
            <!-- Pattern format -->
            <div class="help-card">
              <h4><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/></svg> 域名模式</h4>
              <p>支持两种模式，均不区分大小写。</p>
              <div class="help-item">
                <code class="help-tag">固定</code>
                <p>精确匹配域名，直接返回指定 IP。<br>示例：<code>b.pj.cn</code> → <code>192.168.0.11</code></p>
              </div>
              <div class="help-item">
                <code class="help-tag">通配符</code>
                <p>格式为 <code>*.{后缀}</code>，自动匹配该后缀的所有子域名。<br>示例：<code>*.84.pj.cn</code> 匹配 <code>a.84.pj.cn</code>、<code>b.84.pj.cn</code> 等</p>
              </div>
            </div>
            <!-- Auto IP addressing -->
            <div class="help-card">
              <h4><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg> 自动 IP 生成</h4>
              <p>通配符模式从<strong>子域名的最后一段</strong>提取数字，拼接 <code class="help-code">IpBase</code> 生成 IP。</p>
              <div class="help-examples">
                <p><code>a.84.pj.cn</code><br>选 <code>84</code> → <code>192.168.0.84</code></p>
                <p><code>a.3.pj.cn</code><br>选 <code>3</code> → <code>192.168.0.3</code></p>
              </div>
              <p class="help-note">仅最后一串数字被解析为 IP 尾段；非数字段（如"a"）无法生成有效 IP，需确保子域名末段为数字。</p>
            </div>
            <!-- Priority -->
            <div class="help-card">
              <h4><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M12 20V10"/><path d="M18 20V4"/><path d="M6 20v-4"/></svg> 优先级</h4>
              <p>数字越大越先匹配；<strong>优先级相同时，更具体的规则优先</strong>。</p>
              <div class="help-examples">
                <p><code>60</code> → <code>固定: b.pj.cn</code>（总是先匹配）</p>
                <p><code>50</code> → <code>通配符: *.84.pj.cn</code>（高优先级通配）</p>
                <p><code>10</code> → <code>通配符: *.pj.cn</code>（低优先级通配，不点亮 a.84.pj.cn）</p>
              </div>
              <p class="help-note">避免通用规则与具体规则同名放在同优先级，防止冲突。</p>
            </div>
            <!-- Rule type & disabled -->
            <div class="help-card">
              <h4><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20.59 13.41l-7.17 7.17a2 2 0 0 1-2.83 0L2 12V2h10l8.59 8.59a2 2 0 0 1 0 2.82z"/><line x1="7" y1="7" x2="7.01" y2="7"/></svg> 记录类型与状态</h4>
              <p class="help-examples">
                <span>A</span><span>IPv4 正向查询</span>
                <span>AAAA</span><span>IPv6 正向查询</span>
                <span>MX</span><span>邮件交换</span>
              </p>
              <p>禁用规则不被匹配，建议修改前先禁用旧规则，确认后再删除。</p>
            </div>
          </div>
        </div>
      </details>
      <form @submit.prevent="addRule" class="rule-form">
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">域名模式</label>
            <input
              v-model="newRule.pattern"
              class="form-input"
               placeholder="例如 *.2.example.cn"
              required
            />
          </div>
          <div class="form-group">
            <label class="form-label">记录类型</label>
            <select v-model.number="newRule.type" class="form-select">
              <option :value="1">A (IPv4)</option>
              <option :value="28">AAAA (IPv6)</option>
              <option :value="15">MX</option>
              <option :value="2">NS</option>
              <option :value="16">TXT</option>
            </select>
          </div>
          <div class="form-group">
            <label class="form-label">模式</label>
            <div class="toggle-group">
              <button
                type="button"
                class="toggle-btn"
                :class="{ active: newRule.mode === 'fixed' }"
                @click="newRule.mode = 'fixed'"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="3"/><path d="M12 1v2M12 21v2M4.22 4.22l1.42 1.42M18.36 18.36l1.42 1.42M1 12h2M21 12h2M4.22 19.78l1.42-1.42M18.36 5.64l1.42-1.42"/></svg>
                固定
              </button>
              <button
                type="button"
                class="toggle-btn"
                :class="{ active: newRule.mode === 'auto' }"
                @click="newRule.mode = 'auto'"
              >
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="17 1 21 5 17 9"/><path d="M3 11V9a4 4 0 0 1 4-4h14"/><polyline points="7 23 3 19 7 15"/><path d="M21 13v2a4 4 0 0 1-4 4H3"/></svg>
                自动
              </button>
            </div>
          </div>
        </div>

        <div class="form-row form-row-secondary">
          <div class="form-group flex-fill" v-if="newRule.mode === 'fixed'">
            <label class="form-label">IP 地址</label>
            <input
              v-model="newRule.value"
              class="form-input"
              placeholder="例如 192.168.1.100"
            />
          </div>
          <div class="form-group flex-fill" v-else>
            <label class="form-label">IP 前缀</label>
            <input
              v-model="newRule.ipBase"
              class="form-input"
              placeholder="例如 192.168.0."
            />
          </div>
          <div class="form-group">
            <label class="form-label">优先级</label>
            <input
              v-model.number="newRule.priority"
              class="form-input form-input-sm"
              type="number"
              placeholder="0"
              min="0"
            />
          </div>
          <div class="form-group form-group-action">
            <button type="submit" class="btn-add" :disabled="submitting">
              <svg v-if="!submitting" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
              <span v-else class="spinner-mini"></span>
              <span>{{ submitting ? '添加中...' : '添加规则' }}</span>
            </button>
          </div>
        </div>
      </form>
    </div>

    <!-- Rules Table -->
    <div class="table-section">
      <div class="table-header">
        <h3>规则列表</h3>
        <div class="table-header-right">
          <div class="search-box">
            <svg class="search-icon" width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
            </svg>
            <input
              class="search-input"
              type="text"
              placeholder="搜索域名..."
              :value="searchQuery"
              @input="onSearchInput"
            />
          </div>
          <span class="table-count">{{ filteredRules.length }} 条规则</span>
        </div>
      </div>

      <!-- Empty state -->
      <div v-if="!loading && filteredRules.length === 0" class="empty-state">
        <div class="empty-icon">
          <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" stroke-linecap="round" stroke-linejoin="round">
            <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
          </svg>
        </div>
        <p class="empty-title">{{ searchQuery ? '未找到匹配的规则' : '暂无规则' }}</p>
        <p class="empty-desc">{{ searchQuery ? '尝试修改搜索关键词' : '添加第一条 DNS 规则开始配置' }}</p>
      </div>

      <!-- Table -->
      <div v-else class="table-wrapper">
        <table class="rules-table">
          <thead>
            <tr>
              <th>域名模式</th>
              <th>类型</th>
              <th>模式</th>
              <th>值 / IP 前缀</th>
              <th>优先级</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="rule in paginatedRules" :key="rule.pattern + rule.type" :class="{ disabled: !rule.isEnabled }">
              <td class="cell-pattern">
                <code>{{ rule.pattern }}</code>
              </td>
              <td>
                <span class="type-badge" :class="`type-${recordTypeClass(rule.type)}`">
                  {{ recordTypeToString(rule.type) }}
                </span>
              </td>
              <td>
                <span class="mode-badge" :class="rule.mode">
                  {{ rule.mode === 'auto' ? '自动' : '固定' }}
                </span>
              </td>
              <td class="cell-value">
                <code>{{ rule.mode === 'auto' ? rule.ipBase : rule.value }}</code>
              </td>
              <td>
                <span class="priority-badge">{{ rule.priority }}</span>
              </td>
              <td>
                <button
                  class="toggle-switch"
                  :class="{ on: rule.isEnabled }"
                  @click="toggleRule(rule)"
                  :title="rule.isEnabled ? '禁用规则' : '启用规则'"
                >
                  <span class="toggle-knob"></span>
                </button>
              </td>
              <td>
                <button class="btn-delete" @click="deleteRule(rule.pattern)" title="删除规则">
                  <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                    <polyline points="3 6 5 6 21 6"/>
                    <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/>
                    <line x1="10" y1="11" x2="10" y2="17"/>
                    <line x1="14" y1="11" x2="14" y2="17"/>
                  </svg>
                </button>
              </td>
            </tr>
          </tbody>
        </table>

        <!-- Pagination -->
        <div v-if="totalPages > 1" class="pagination">
          <span class="pagination-info">显示 {{ pageStart }} – {{ pageEnd }} 条，共 {{ filteredRules.length }} 条</span>
          <div class="pagination-controls">
            <button
              class="page-btn"
              :disabled="currentPage <= 1"
              @click="goToPage(currentPage - 1)"
              title="上一页"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="15 18 9 12 15 6"/></svg>
            </button>
            <button
              v-for="p in visiblePages"
              :key="p"
              class="page-num"
              :class="{ active: p === currentPage, ellipsis: p === -1 }"
              :disabled="p === -1"
              @click="p !== -1 && goToPage(p)"
            >
              {{ p === -1 ? '…' : p }}
            </button>
            <button
              class="page-btn"
              :disabled="currentPage >= totalPages"
              @click="goToPage(currentPage + 1)"
              title="下一页"
            >
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="9 18 15 12 9 6"/></svg>
            </button>
          </div>
        </div>
      </div>

      <!-- Loading state -->
      <div v-if="loading" class="loading-overlay">
        <div class="spinner"></div>
        <span>加载规则中...</span>
      </div>
    </div>

    <!-- Toast notifications -->
    <div class="toast-container">
      <transition-group name="toast">
        <div v-for="toast in toasts" :key="toast.id"
          :class="['toast', toast.type]" :style="{ animationDelay: toast.delay + 'ms' }">
          <span class="toast-icon">
            <svg v-if="toast.type === 'success'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"/></svg>
            <svg v-else-if="toast.type === 'error'" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>
            <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
          </span>
          {{ toast.message }}
        </div>
      </transition-group>
    </div>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, computed, onMounted, watch } from 'vue'
import axios from 'axios'
import type { DnsRule } from '../types'
import { useRefreshKey } from '../composables/useRefreshKey'

let toastId = 0

export default defineComponent({
  setup() {
    const rules = ref<DnsRule[]>([])
    const loading = ref(false)
    const submitting = ref(false)
    const toasts = ref<{ id: number; type: 'success' | 'error' | 'info'; message: string; delay: number }[]>([])

    // ── Search & Pagination ──────────────────────────────
    const searchQuery = ref('')
    const currentPage = ref(1)
    const pageSize = ref(15)

    const filteredRules = computed(() => {
      const q = searchQuery.value.trim().toLowerCase()
      const sorted = [...rules.value].sort((a, b) => b.priority - a.priority)
      if (!q) return sorted
      return sorted.filter(r => r.pattern.toLowerCase().includes(q))
    })

    const totalPages = computed(() =>
      Math.max(1, Math.ceil(filteredRules.value.length / pageSize.value))
    )

    const paginatedRules = computed(() => {
      const start = (currentPage.value - 1) * pageSize.value
      return filteredRules.value.slice(start, start + pageSize.value)
    })

    const pageStart = computed(() =>
      filteredRules.value.length === 0 ? 0 : (currentPage.value - 1) * pageSize.value + 1
    )
    const pageEnd = computed(() =>
      Math.min(currentPage.value * pageSize.value, filteredRules.value.length)
    )

    /** Page numbers to show in paginator (max 7) */
    const visiblePages = computed(() => {
      const total = totalPages.value
      const cur = currentPage.value
      if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1)
      if (cur <= 4) return [1, 2, 3, 4, 5, -1, total]
      if (cur >= total - 3) return [1, -1, total - 4, total - 3, total - 2, total - 1, total]
      return [1, -1, cur - 1, cur, cur + 1, -1, total]
    })

    function goToPage(page: number) {
      if (page >= 1 && page <= totalPages.value) currentPage.value = page
    }

    function onSearchInput(e: Event) {
      const val = (e.target as HTMLInputElement).value
      searchQuery.value = val
      currentPage.value = 1
    }
    // ─────────────────────────────────────────────────────

    const newRule = ref({
      pattern: '',
      type: 1,
      mode: 'fixed',
      value: '',
      isEnabled: true,
      priority: 0,
      ipBase: ''
    })

    const stats = computed(() => {
      const total = rules.value.length
      const enabled = rules.value.filter(r => r.isEnabled).length
      const types = new Set(rules.value.map(r => r.type))
      return { total, enabled, disabled: total - enabled, typeCount: types.size }
    })

    const showToast = (type: 'success' | 'error' | 'info', message: string) => {
      const id = toastId++
      toasts.value.push({ id, type, message, delay: 0 })
      setTimeout(() => {
        toasts.value = toasts.value.filter(t => t.id !== id)
      }, 3500)
    }

    const fetchRules = async () => {
      loading.value = true
      try {
        const response = await axios.get<DnsRule[]>('/api/rules')
        rules.value = response.data
        currentPage.value = 1
      }
      catch (e) {
        showToast('error', '获取规则列表失败')
      }
      finally {
        loading.value = false
      }
    }

    const addRule = async () => {
      submitting.value = true
      try {
        await axios.post('/api/rules', newRule.value)
        newRule.value = { pattern: '', type: 1, mode: 'fixed', value: '', isEnabled: true, priority: 0, ipBase: '' }
        await fetchRules()
        showToast('success', '规则添加成功')
      }
      catch (e: unknown) {
        const msg = axios.isAxiosError(e) && e.response?.data ? String(e.response.data) : '添加规则失败'
        showToast('error', msg)
      }
      finally {
        submitting.value = false
      }
    }

    const deleteRule = async (pattern: string) => {
      try {
        await axios.delete('/api/rules', { params: { pattern } })
        await fetchRules()
        showToast('success', '规则已删除')
      }
      catch {
        showToast('error', '删除规则失败')
      }
    }

    const toggleRule = async (rule: DnsRule) => {
      const updated = { ...rule, isEnabled: !rule.isEnabled }
      try {
        await axios.put(`/api/rules/${encodeURIComponent(rule.pattern)}`, updated)
        await fetchRules()
      }
      catch {
        showToast('error', '更新规则状态失败')
      }
    }

    const recordTypeToString = (type: number): string => {
      const types: Record<number, string> = { 1: 'A', 2: 'NS', 15: 'MX', 28: 'AAAA', 16: 'TXT' }
      return types[type] || `Type${type}`
    }

    const recordTypeClass = (type: number): string => {
      return { 1: 'a', 2: 'ns', 15: 'mx', 28: 'aaaa', 16: 'txt' }[type] || 'default'
    }

    // ── Refresh key — re-fetch when App.vue triggers ──────
    const { refreshKey } = useRefreshKey()
    watch(refreshKey, () => fetchRules())
    // ──────────────────────────────────────────────────────

    onMounted(fetchRules)

    return {
      rules,
      newRule,
      stats,
      loading,
      submitting,
      toasts,
      addRule,
      deleteRule,
      toggleRule,
      recordTypeToString,
      recordTypeClass,
      // search & pagination
      searchQuery,
      currentPage,
      pageSize,
      totalPages,
      paginatedRules,
      filteredRules,
      pageStart,
      pageEnd,
      visiblePages,
      goToPage,
      onSearchInput
    }
  }
})
</script>

<style scoped>
.rules-view {
  position: relative;
}

/* ── Stats ── */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 16px;
  margin-bottom: 28px;
  animation: fadeInUp 0.5s ease both;
}

.stat-card {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  padding: 22px 24px;
  display: flex;
  align-items: center;
  gap: 18px;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.stat-card:hover {
  border-color: rgba(255, 255, 255, 0.1);
  box-shadow: var(--shadow-glow);
}

.stat-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-md);
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.stat-icon.total {
  background: var(--accent-dim);
  color: var(--accent);
  border: 1px solid rgba(0, 209, 193, 0.15);
}

.stat-icon.enabled {
  background: var(--success-dim);
  color: var(--success);
  border: 1px solid rgba(61, 214, 140, 0.15);
}

.stat-icon.disabled {
  background: var(--accent-secondary-dim);
  color: var(--accent-secondary);
  border: 1px solid rgba(240, 140, 90, 0.15);
}

.stat-icon.types {
  background: var(--info-dim);
  color: var(--info);
  border: 1px solid rgba(94, 171, 240, 0.15);
}

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-value {
  font-family: var(--font-display);
  font-weight: 700;
  font-size: 1.6rem;
  letter-spacing: -0.03em;
  line-height: 1;
  color: var(--text-primary);
}

.stat-label {
  font-size: 0.78rem;
  font-weight: 500;
  color: var(--text-muted);
  margin-top: 4px;
}

/* ── Form ── */
.form-section {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  padding: 24px;
  margin-bottom: 28px;
  animation: fadeInUp 0.5s ease 0.1s both;
}

.form-header {
  margin-bottom: 20px;
}

.form-header h3 {
  font-family: var(--font-display);
  font-weight: 600;
  font-size: 1rem;
  color: var(--text-primary);
}

.form-hint {
  font-size: 0.78rem;
  color: var(--text-muted);
  margin-top: 4px;
}

/* ── Rule Definition Help Panel ── */
.help-details {
  margin-top: 16px;
  border: 1px solid var(--border);
  border-radius: var(--radius-md);
  overflow: hidden;
  background: rgba(94, 171, 240, 0.03);
  animation: fadeInUp 0.5s ease 0.15s both;
}

.help-summary {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 16px;
  cursor: pointer;
  font-family: var(--font-body);
  font-size: 0.82rem;
  font-weight: 600;
  color: var(--accent);
  user-select: none;
  -webkit-user-select: none;
  list-style: none;
  transition: background 0.2s;
  border: none;
  width: 100%;
  background: transparent;
  text-align: left;
}

.help-summary::-webkit-details-marker { display: none; }
.help-summary::marker { display: none; content: ''; }

.help-summary:hover {
  background: rgba(94, 171, 240, 0.06);
}

.help-chevron {
  margin-left: auto;
  transition: transform 0.25s ease;
  flex-shrink: 0;
  color: var(--text-muted);
}

.help-details[open] .help-chevron {
  transform: rotate(180deg);
}

.help-body {
  padding: 0 16px 16px;
  animation: fadeIn 0.2s ease both;
}

@keyframes fadeIn {
  from { opacity: 0; transform: translateY(-4px); }
  to   { opacity: 1; transform: translateY(0); }
}

.help-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 12px;
}

.help-card {
  background: var(--bg-input);
  border: 1px solid rgba(255, 255, 255, 0.04);
  border-radius: var(--radius-sm);
  padding: 14px 16px;
}

.help-card h4 {
  font-family: var(--font-display);
  font-size: 0.85rem;
  font-weight: 600;
  color: var(--text-primary);
  margin-bottom: 8px;
  display: flex;
  align-items: center;
  gap: 6px;
}

.help-card p {
  font-size: 0.78rem;
  color: var(--text-secondary);
  line-height: 1.6;
  margin: 0;
}

.help-card p + p {
  margin-top: 8px;
}

.help-item {
  margin-top: 10px;
}

.help-tag {
  font-family: var(--font-mono);
  font-size: 0.72rem;
  font-weight: 700;
  padding: 2px 8px;
  border-radius: 4px;
  background: rgba(94, 171, 240, 0.1);
  color: var(--accent);
  border: 1px solid rgba(94, 171, 240, 0.2);
}

code.help-code,
.help-card code {
  font-family: var(--font-mono);
  font-size: 0.78rem;
  padding: 1px 5px;
  border-radius: 3px;
  background: rgba(255, 255, 255, 0.05);
  color: var(--accent);
  border: 1px solid rgba(0, 209, 193, 0.1);
  white-space: nowrap;
}

.help-examples {
  display: flex;
  flex-direction: column;
  gap: 4px;
  margin-top: 8px;
}

.help-examples p,
.help-examples span {
  font-size: 0.78rem;
  color: var(--text-secondary);
  line-height: 1.5;
  margin: 0;
}

.help-examples code {
  font-family: var(--font-mono);
  font-size: 0.76rem;
  padding: 0 4px;
}

.help-examples span {
  font-weight: 500;
  color: var(--accent);
  margin-right: 8px;
}

.help-note {
  margin-top: 8px !important;
  font-size: 0.74rem !important;
  color: var(--warning) !important;
  opacity: 0.9;
}

.rule-form {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.form-row {
  display: flex;
  align-items: flex-end;
  gap: 14px;
  flex-wrap: wrap;
}

.form-row-secondary {
  padding-top: 4px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 6px;
  min-width: 160px;
}

.form-group.flex-fill {
  flex: 1;
  min-width: 200px;
}

.form-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--text-secondary);
  text-transform: uppercase;
  letter-spacing: 0.04em;
}

.form-input,
.form-select {
  width: 100%;
  padding: 10px 14px;
  background: var(--bg-input);
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  font-family: var(--font-body);
  font-size: 0.88rem;
  color: var(--text-primary);
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
  appearance: none;
  -webkit-appearance: none;
}

.form-input::placeholder {
  color: var(--text-muted);
}

.form-input:focus,
.form-select:focus {
  border-color: var(--border-active);
  box-shadow: 0 0 0 3px var(--accent-dim);
}

.form-input-sm {
  width: 90px;
}

/* Toggle buttons for mode */
.toggle-group {
  display: flex;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  overflow: hidden;
}

.toggle-btn {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 9px 14px;
  background: var(--bg-input);
  border: none;
  color: var(--text-muted);
  font-family: var(--font-body);
  font-size: 0.82rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  flex: 1;
}

.toggle-btn:first-child {
  border-right: 1px solid var(--border);
}

.toggle-btn.active {
  background: var(--accent-dim);
  color: var(--accent);
}

.toggle-btn svg {
  flex-shrink: 0;
}

.form-select {
  background-image: url("data:image/svg+xml,%3Csvg width='10' height='6' viewBox='0 0 10 6' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M1 1l4 4 4-4' stroke='%238b92a5' stroke-width='1.5' fill='none' stroke-linecap='round' stroke-linejoin='round'/%3E%3C/svg%3E");
  background-repeat: no-repeat;
  background-position: right 12px center;
  padding-right: 32px;
  cursor: pointer;
}

.btn-add {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 10px 22px;
  background: linear-gradient(135deg, var(--accent), #00b5a4);
  color: #0b0e14;
  border: none;
  border-radius: var(--radius-sm);
  font-family: var(--font-body);
  font-size: 0.88rem;
  font-weight: 700;
  cursor: pointer;
  white-space: nowrap;
  transition: all 0.2s;
  box-shadow: 0 2px 12px rgba(0, 209, 193, 0.2);
}

.btn-add:hover {
  transform: translateY(-1px);
  box-shadow: 0 4px 20px rgba(0, 209, 193, 0.35);
}

.btn-add:active {
  transform: translateY(0);
}

.btn-add:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.spinner-mini {
  width: 14px;
  height: 14px;
  border: 2px solid transparent;
  border-top-color: #0b0e14;
  border-radius: 50%;
  animation: spin 0.6s linear infinite;
}

/* ── Table ── */
.table-section {
  background: var(--bg-surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  overflow: hidden;
  animation: fadeInUp 0.5s ease 0.2s both;
  position: relative;
}

.table-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid var(--border);
}

.table-header h3 {
  font-family: var(--font-display);
  font-weight: 600;
  font-size: 1rem;
  color: var(--text-primary);
}

.table-count {
  font-family: var(--font-mono);
  font-size: 0.75rem;
  font-weight: 500;
  color: var(--text-muted);
  background: rgba(255, 255, 255, 0.03);
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  padding: 4px 10px;
}

.table-wrapper {
  overflow-x: auto;
}

.rules-table {
  width: 100%;
  border-collapse: collapse;
}

.rules-table thead th {
  font-family: var(--font-body);
  font-size: 0.72rem;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  color: var(--text-muted);
  padding: 12px 16px;
  text-align: left;
  background: rgba(255, 255, 255, 0.02);
  border-bottom: 1px solid var(--border);
  white-space: nowrap;
}

.rules-table tbody tr {
  border-bottom: 1px solid rgba(255, 255, 255, 0.04);
  transition: background 0.15s;
}

.rules-table tbody tr:last-child {
  border-bottom: none;
}

.rules-table tbody tr:hover {
  background: var(--bg-hover);
}

.rules-table tbody tr.disabled {
  opacity: 0.55;
}

.rules-table tbody td {
  padding: 14px 16px;
  vertical-align: middle;
  font-size: 0.88rem;
  color: var(--text-primary);
}

.cell-pattern code,
.cell-value code {
  font-family: var(--font-mono);
  font-size: 0.82rem;
  font-weight: 500;
  color: var(--accent);
  background: var(--accent-dim);
  border: 1px solid rgba(0, 209, 193, 0.1);
  border-radius: 4px;
  padding: 2px 8px;
}

.cell-value code {
  color: var(--text-secondary);
  background: rgba(255, 255, 255, 0.04);
  border-color: var(--border);
}

/* Type badges */
.type-badge {
  display: inline-block;
  font-family: var(--font-mono);
  font-size: 0.72rem;
  font-weight: 700;
  letter-spacing: 0.03em;
  border-radius: 4px;
  padding: 3px 9px;
}

.type-a {
  background: rgba(61, 214, 140, 0.1);
  color: var(--success);
  border: 1px solid rgba(61, 214, 140, 0.2);
}

.type-aaaa {
  background: var(--info-dim);
  color: var(--info);
  border: 1px solid rgba(94, 171, 240, 0.2);
}

.type-mx {
  background: rgba(240, 196, 25, 0.1);
  color: var(--warning);
  border: 1px solid rgba(240, 196, 25, 0.2);
}

.type-ns {
  background: var(--accent-secondary-dim);
  color: var(--accent-secondary);
  border: 1px solid rgba(240, 140, 90, 0.2);
}

.type-txt {
  background: rgba(200, 160, 255, 0.1);
  color: #c8a0ff;
  border: 1px solid rgba(200, 160, 255, 0.2);
}

.type-default {
  background: rgba(255, 255, 255, 0.05);
  color: var(--text-secondary);
  border: 1px solid var(--border);
}

/* Mode badges */
.mode-badge {
  font-family: var(--font-mono);
  font-size: 0.72rem;
  font-weight: 500;
  border-radius: 4px;
  padding: 3px 9px;
}

.mode-badge.auto {
  background: var(--accent-dim);
  color: var(--accent);
  border: 1px solid rgba(0, 209, 193, 0.15);
}

.mode-badge.fixed {
  background: rgba(255, 255, 255, 0.04);
  color: var(--text-secondary);
  border: 1px solid var(--border);
}

/* Priority badge */
.priority-badge {
  font-family: var(--font-mono);
  font-size: 0.8rem;
  font-weight: 600;
  color: var(--text-secondary);
  opacity: 0.8;
}

/* Toggle switch */
.toggle-switch {
  position: relative;
  width: 44px;
  height: 24px;
  border-radius: 12px;
  border: 2px solid var(--text-muted);
  background: transparent;
  cursor: pointer;
  transition: all 0.3s;
  padding: 0;
  flex-shrink: 0;
}

.toggle-switch.on {
  border-color: var(--accent);
  background: var(--accent-dim);
}

.toggle-knob {
  position: absolute;
  top: 2px;
  left: 2px;
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: var(--text-muted);
  transition: all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1);
}

.toggle-switch.on .toggle-knob {
  left: 22px;
  background: var(--accent);
  box-shadow: 0 0 10px rgba(0, 209, 193, 0.4);
}

/* Delete button */
.btn-delete {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 32px;
  height: 32px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-muted);
  cursor: pointer;
  transition: all 0.2s;
}

.btn-delete:hover {
  background: var(--danger-dim);
  border-color: rgba(242, 87, 87, 0.3);
  color: var(--danger);
}

/* ── Empty State ── */
.empty-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 72px 20px;
  text-align: center;
}

.empty-icon {
  color: var(--text-muted);
  margin-bottom: 20px;
  opacity: 0.5;
}

.empty-title {
  font-family: var(--font-display);
  font-weight: 600;
  font-size: 1.1rem;
  color: var(--text-secondary);
  margin-bottom: 8px;
}

.empty-desc {
  font-size: 0.85rem;
  color: var(--text-muted);
}

/* ── Loading overlay ── */
.loading-overlay {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 16px;
  background: rgba(18, 22, 31, 0.78);
  backdrop-filter: blur(6px);
  z-index: 5;
  color: var(--text-secondary);
  font-size: 0.88rem;
}

.spinner {
  width: 32px;
  height: 32px;
  border: 3px solid var(--border);
  border-top-color: var(--accent);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

/* ── Toast ── */
.toast-container {
  position: fixed;
  bottom: 28px;
  right: 28px;
  display: flex;
  flex-direction: column;
  gap: 10px;
  z-index: 100;
}

.toast {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 12px 18px;
  border-radius: var(--radius-md);
  font-size: 0.88rem;
  font-weight: 500;
  border: 1px solid;
  backdrop-filter: blur(12px);
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.4);
}

.toast.success {
  background: rgba(61, 214, 140, 0.1);
  border-color: rgba(61, 214, 140, 0.25);
  color: var(--success);
}

.toast.error {
  background: rgba(242, 87, 87, 0.1);
  border-color: rgba(242, 87, 87, 0.25);
  color: var(--danger);
}

.toast.info {
  background: var(--accent-dim);
  border-color: rgba(0, 209, 193, 0.25);
  color: var(--accent);
}

.toast-icon {
  display: flex;
  align-items: center;
  flex-shrink: 0;
}

/* ── Animations ── */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(14px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.toast-enter-active {
  animation: toastIn 0.35s ease both;
}

.toast-leave-active {
  animation: toastOut 0.3s ease both;
}

@keyframes toastIn {
  from {
    opacity: 0;
    transform: translateY(16px) scale(0.96);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

@keyframes toastOut {
  from {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
  to {
    opacity: 0;
    transform: translateY(8px) scale(0.96);
  }
}

/* ── Search ── */
.table-header-right {
  display: flex;
  align-items: center;
  gap: 12px;
}

.search-box {
  position: relative;
  display: flex;
  align-items: center;
}

.search-icon {
  position: absolute;
  left: 12px;
  color: var(--text-muted);
  pointer-events: none;
  flex-shrink: 0;
}

.search-input {
  width: 220px;
  padding: 8px 12px 8px 34px;
  background: var(--bg-input);
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  font-family: var(--font-body);
  font-size: 0.82rem;
  color: var(--text-primary);
  outline: none;
  transition: border-color 0.2s, box-shadow 0.2s;
}

.search-input::placeholder {
  color: var(--text-muted);
}

.search-input:focus {
  border-color: var(--border-active);
  box-shadow: 0 0 0 3px var(--accent-dim);
}

/* ── Pagination ── */
.pagination {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 16px 24px;
  border-top: 1px solid var(--border);
}

.pagination-info {
  font-size: 0.78rem;
  color: var(--text-muted);
}

.pagination-controls {
  display: flex;
  align-items: center;
  gap: 4px;
}

.page-btn,
.page-num {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 32px;
  height: 32px;
  padding: 0 8px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  background: transparent;
  color: var(--text-secondary);
  font-family: var(--font-body);
  font-size: 0.82rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.15s;
  user-select: none;
}

.page-btn:hover:not(:disabled),
.page-num:hover:not(:disabled):not(.ellipsis) {
  background: var(--bg-hover);
  border-color: var(--border-active);
  color: var(--text-primary);
}

.page-btn:disabled {
  opacity: 0.35;
  cursor: not-allowed;
}

.page-num.active {
  background: var(--accent-dim);
  border-color: var(--accent);
  color: var(--accent);
  font-weight: 700;
}

.page-num.ellipsis {
  border: none;
  cursor: default;
  color: var(--text-muted);
  min-width: 20px;
  padding: 0;
}

/* ── Responsive ── */
@media (max-width: 960px) {
  .stats-grid {
    grid-template-columns: repeat(2, 1fr);
  }
  .form-row {
    flex-direction: column;
    align-items: stretch;
  }
  .form-group {
    width: 100%;
  }
  .form-row-secondary {
    flex-direction: column;
  }
  .table-header-right {
    flex-direction: column;
    align-items: stretch;
    gap: 8px;
  }
  .search-input {
    width: 100%;
  }
  .pagination {
    flex-direction: column;
    gap: 12px;
  }
}

@media (max-width: 640px) {
  .stats-grid {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .rules-table thead {
    display: none;
  }
  .rules-table tbody tr {
    display: flex;
    flex-wrap: wrap;
    gap: 6px;
    padding: 16px;
    border-radius: var(--radius-md);
    margin: 8px;
    border: 1px solid var(--border);
  }
  .rules-table tbody td {
    padding: 6px 8px;
    font-size: 0.84rem;
  }
  .cell-pattern {
    width: 100%;
    order: -2;
  }
  .cell-value {
    width: 100%;
    order: -1;
    color: var(--text-muted);
  }
  .pagination-info {
    font-size: 0.72rem;
    text-align: center;
  }
  .page-num:not(.active):not(.ellipsis) {
    display: none;
  }
  .pagination-controls {
    gap: 2px;
  }
}
</style>
