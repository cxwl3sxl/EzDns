import { ref } from 'vue'

/** 
 * Shared refresh counter incremented by App.vue's refresh button.
 * Downstream views (RulesView) watch this key to trigger re-fetch.
 */
const _refreshKey = ref(0)

export function useRefreshKey() {
  function triggerRefresh() {
    _refreshKey.value++
  }

  return {
    refreshKey: _refreshKey,
    triggerRefresh,
  }
}
