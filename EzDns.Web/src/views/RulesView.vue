<template>
  <div class="rules-manager">
    <h2>DNS 规则管理</h2>
    
    <form @submit.prevent="addRule" class="rule-form">
      <input v-model="newRule.pattern" placeholder="域名模式 (如 *.example.cn)" required />
      <select v-model.number="newRule.type">
        <option :value="1">A</option>
        <option :value="28">AAAA</option>
        <option :value="15">MX</option>
        <option :value="2">NS</option>
        <option :value="16">TXT</option>
      </select>
      <select v-model="newRule.mode">
        <option value="fixed">固定</option>
        <option value="auto">自动</option>
      </select>
      <input v-model="newRule.value" placeholder="IP地址 (固定模式)" />
      <input v-model="newRule.ipBase" placeholder="IP前缀 (自动模式, 如 192.168.0.)" />
      <input v-model.number="newRule.priority" type="number" placeholder="优先级" />
      <button type="submit">添加规则</button>
    </form>

    <table class="rules-table">
      <thead>
        <tr>
          <th>域名模式</th>
          <th>类型</th>
          <th>模式</th>
          <th>值/IP前缀</th>
          <th>状态</th>
          <th>操作</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="rule in rules" :key="rule.pattern">
          <td>{{ rule.pattern }}</td>
          <td>{{ recordTypeToString(rule.type) }}</td>
          <td>{{ rule.mode === 'auto' ? '自动' : '固定' }}</td>
          <td>{{ rule.mode === 'auto' ? rule.ipBase : rule.value }}</td>
          <td>
            <button @click="toggleRule(rule)">
              {{ rule.isEnabled ? '禁用' : '启用' }}
            </button>
          </td>
          <td>
            <button @click="deleteRule(rule.pattern)">删除</button>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script lang="ts">
import { defineComponent, ref, onMounted } from 'vue'
import axios from 'axios'
import type { DnsRule } from '../types'

export default defineComponent({
  setup() {
    const rules = ref<DnsRule[]>([])
    const newRule = ref({
      pattern: '',
      type: 1,
      mode: 'fixed',
      value: '',
      isEnabled: true,
      priority: 0,
      ipBase: ''
    })

    const fetchRules = async () => {
      const response = await axios.get<DnsRule[]>('/api/rules')
      rules.value = response.data
    }

    const addRule = async () => {
      await axios.post('/api/rules', newRule.value)
      newRule.value = {
        pattern: '',
        type: 1,
        mode: 'fixed',
        value: '',
        isEnabled: true,
        priority: 0,
        ipBase: ''
      }
      await fetchRules()
    }

    const deleteRule = async (pattern: string) => {
      await axios.delete('/api/rules', { params: { pattern } })
      await fetchRules()
    }

    const toggleRule = async (rule: DnsRule) => {
      const updated = { ...rule, isEnabled: !rule.isEnabled }
      await axios.put(`/api/rules/${encodeURIComponent(rule.pattern)}`, updated)
      await fetchRules()
    }

    const recordTypeToString = (type: number): string => {
      const types: Record<number, string> = {
        1: 'A', 2: 'NS', 15: 'MX', 28: 'AAAA', 16: 'TXT'
      }
      return types[type] || `Type${type}`
    }

    onMounted(fetchRules)

    return {
      rules,
      newRule,
      addRule,
      deleteRule,
      toggleRule,
      recordTypeToString
    }
  }
})
</script>

<style scoped>
.rules-manager {
  margin-top: 20px;
}
.rule-form {
  display: flex;
  gap: 10px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}
.rule-form input, .rule-form select {
  padding: 8px;
  border: 1px solid #ccc;
  border-radius: 4px;
}
.rule-form button {
  padding: 8px 16px;
  background: #007bff;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
}
.rules-table {
  width: 100%;
  border-collapse: collapse;
}
.rules-table th, .rules-table td {
  border: 1px solid #ddd;
  padding: 8px;
  text-align: left;
}
.rules-table th {
  background: #f4f4f4;
}
.rules-table button {
  padding: 4px 8px;
  cursor: pointer;
}
</style>