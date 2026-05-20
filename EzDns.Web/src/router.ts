import { createRouter, createWebHistory } from 'vue-router'
import RulesView from './views/RulesView.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/rules' },
    { path: '/rules', component: RulesView }
  ]
})

export default router