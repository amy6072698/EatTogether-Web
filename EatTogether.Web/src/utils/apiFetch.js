const BASE = import.meta.env.VITE_API_BASE_URL;

async function apiFetch(path, options = {}){
  const res = await fetch(`${BASE}${path}`, {
    headers: {'Content-Type': 'application/json', ...options.headers},
    credentials: 'include',
    ...options
  });

  // ── 登入驗證完成後取消以下註解 ──────────────────
  // if (res.status === 401) {
  //   const refreshed = await fetch(`${BASE}/auth/refresh`, {
  //     method: 'POST',
  //     credentials: 'include'
  //   })
  //   if (refreshed.ok) {
  //     return fetch(`${BASE}${path}`, {
  //       headers: { 'Content-Type': 'application/json', ...options.headers },
  //       credentials: 'include',
  //       ...options
  //     })
  //   }
  //   window.location.href = '/login'
  //   return
  // }

  return res
}

export default apiFetch;