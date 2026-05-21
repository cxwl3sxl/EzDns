import { ref, computed } from 'vue'

const TOKEN_KEY = 'ezdns_token'
const USER_KEY  = 'ezdns_user'

const token    = ref<string | null>(localStorage.getItem(TOKEN_KEY))
const username = ref<string | null>(localStorage.getItem(USER_KEY))

export function useAuth()
{
    const isLoggedIn = computed(() => !!token.value)

    function login(tokenString: string, user: string)
    {
        token.value    = tokenString
        username.value = user
        localStorage.setItem(TOKEN_KEY, tokenString)
        localStorage.setItem(USER_KEY,  user)
    }

    function logout()
    {
        token.value    = null
        username.value = null
        localStorage.removeItem(TOKEN_KEY)
        localStorage.removeItem(USER_KEY)
    }

    return { token, username, isLoggedIn, login, logout }
}
