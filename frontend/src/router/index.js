import { createRouter, createWebHistory } from 'vue-router'
<<<<<<< HEAD
import { useAuthStore } from '@/stores/auth'
import LoginView from '@/views/LoginView.vue'
=======
import HomeView from '../views/HomeView.vue'
>>>>>>> 1248e0acbeb7847ddd41678e923e753c890644a0

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
<<<<<<< HEAD
      path: '/login',
      name: 'login',
      component: LoginView,
      meta: { publica: true }
    },
    {
      path: '/operador',
      name: 'operador',
      component: () => import('@/views/OperadorHomeView.vue'),
      meta: { perfis: ['operador', 'supervisor'] }
    },
    {
      path: '/operador/formulario',
      name: 'operador-formulario',
      component: () => import('@/views/FormularioOperadorView.vue'),
      meta: { perfis: ['operador', 'supervisor'] }
    },
    {
      path: '/supervisor',
      name: 'supervisor',
      component: () => import('@/views/PainelSupervisorView.vue'),
      meta: { perfis: ['supervisor'] }
    },
    {
      path: '/',
      redirect: '/login'
    }
  ]
})

// Guard de autenticação
router.beforeEach(async (to) => {
  const auth = useAuthStore()

  // Tenta restaurar sessão do IndexedDB se não logado
  if (!auth.logado) {
    await auth.restaurarSessao()
  }

  // Rotas públicas
  if (to.meta.publica) {
    // Se já logado, redireciona
    if (auth.logado) {
      return auth.isSupervisor ? '/supervisor' : '/operador'
    }
    return true
  }

  // Rotas protegidas
  if (!auth.logado) {
    return '/login'
  }

  // Verifica perfil
  if (to.meta.perfis && !to.meta.perfis.includes(auth.perfil)) {
    return auth.isSupervisor ? '/supervisor' : '/operador'
  }

  return true
=======
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/AboutView.vue'),
    },
  ],
>>>>>>> 1248e0acbeb7847ddd41678e923e753c890644a0
})

export default router
