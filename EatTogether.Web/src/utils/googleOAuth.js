const SESSION_KEY = 'oauth_state'
const REDIRECT_KEY = 'oauth_redirect'

export function generateState(redirectPath = '/') {
    const state = crypto.randomUUID()
    sessionStorage.setItem(SESSION_KEY, state)
    sessionStorage.setItem(REDIRECT_KEY, redirectPath)
    return state
}

export function buildGoogleOAuthUrl(state) {
    const clientId = import.meta.env.VITE_GOOGLE_CLIENT_ID
    const redirectUri = import.meta.env.VITE_GOOGLE_REDIRECT_URI

    const params = new URLSearchParams({
        client_id: clientId,
        redirect_uri: redirectUri,
        response_type: 'code',
        scope: 'openid email profile',
        state,
    })

    return `https://accounts.google.com/o/oauth2/v2/auth?${params.toString()}`
}

export function validateState(returnedState) {
    const storedState = sessionStorage.getItem(SESSION_KEY)
    if (!storedState || !returnedState) return false
    return storedState === returnedState
}

export function clearState() {
    sessionStorage.removeItem(SESSION_KEY)
    sessionStorage.removeItem(REDIRECT_KEY)
}

export function getRedirectPath() {
    return sessionStorage.getItem(REDIRECT_KEY) || '/'
}
