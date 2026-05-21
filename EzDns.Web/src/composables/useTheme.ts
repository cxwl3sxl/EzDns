import { ref, watch, watchEffect, onMounted } from 'vue'

const STORAGE_KEY = 'ezdns_theme'
type Theme = 'dark' | 'light'

// Seed from localStorage immediately (synchronously) so that even before
// onMounted vows fire, <html data-theme="…"> is already correct.
// This prevents the hydration-vs-HMR mismatch that causes a refresh reset.
const savedTheme: string | null = localStorage.getItem(STORAGE_KEY)
export const theme = ref<Theme>(
  savedTheme === 'dark' || savedTheme === 'light' ? savedTheme : 'dark'
)

const THEME_MAP: Record<Theme, string> = {
  dark: 'dark',
  light: 'light',
}

// Apply theme to <html> by reading theme.value directly (reactive proxy
// is stable across HMR patches, avoiding "stale-ref" silent failures).
function applyTheme()
{
  document.documentElement.setAttribute('data-theme', THEME_MAP[theme.value])
}

// ── 1. Pre-render: set <html data-theme> immediately at module scope ───
//    This runs before onMounted / watch / any HMR patch, so there is
//    zero gap between module execution and DOM being correct.
applyTheme()

// ── 2. Persist every mutation ────────────────────────────────────────────
watch(() => theme.value, (val) =>
{
  localStorage.setItem(STORAGE_KEY, val)
  applyTheme()
})

// ── 3. Fall back to OS preference when no saved value exists ─────────────
onMounted(() =>
{
  const saved: string | null = localStorage.getItem(STORAGE_KEY)
  if (saved === 'dark' || saved === 'light')
  {
    // Explicit save wins — re-assert once to close any HMR race.
    applyTheme()
  }
  else
  {
    // No saved value → derive from OS, then persist
    const prefersLight = window.matchMedia('(prefers-color-scheme: light)').matches
    theme.value = prefersLight ? 'light' : 'dark'
  }
})

export function toggleTheme()
{
  theme.value = theme.value === 'dark' ? 'light' : 'dark'
}
