<template>
  <div class="min-h-dvh" :style="{ backgroundColor: 'var(--cptm-bg)' }">
    <!-- Header -->
    <header class="px-6 py-4 flex items-center justify-between shadow-sm"
            :style="{ backgroundColor: 'var(--cptm-header)', color: 'var(--cptm-header-text)' }">
      <div class="flex items-center gap-3">
        <div class="w-10 h-10 rounded-xl flex items-center justify-center" style="background-color: var(--cptm-primary);">
          <ShieldIcon class="w-5 h-5 text-white" />
        </div>
        <div>
          <h1 class="text-lg font-bold">Central do Supervisor</h1>
          <p class="text-xs opacity-70">Sistema CCA - CPTM Ambiental</p>
        </div>
      </div>
      <div class="flex items-center gap-4">
        <span class="text-sm opacity-70">{{ authStore.usuario?.nome }}</span>
        <button @click="themeStore.toggle()" class="p-2 rounded-lg cursor-pointer" style="color: var(--cptm-header-text);">
          <SunIcon v-if="themeStore.isDark" class="w-5 h-5" />
          <MoonIcon v-else class="w-5 h-5" />
        </button>
        <button @click="fazerLogout" class="text-xs px-3 py-1.5 rounded-lg border cursor-pointer"
                style="border-color: rgba(255,255,255,0.2); color: var(--cptm-header-text);">
          Sair
        </button>
      </div>
    </header>

    <div class="p-6 max-w-7xl mx-auto">
      <!-- Dashboard Cards -->
      <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
        <DashboardCard titulo="Total de Registros" :valor="registros.length" icone="clipboard" />
        <DashboardCard titulo="Efluentes" :valor="registros.filter(r => (r.natureza||'').toLowerCase() === 'efluente').length" icone="droplets" />
        <DashboardCard titulo="Árvores" :valor="registros.filter(r => (r.natureza||'').toLowerCase() === 'arvore').length" icone="tree" />
        <DashboardCard titulo="Esta Semana" :valor="registrosSemana" icone="calendar" />
      </div>

      <!-- Filtros -->
      <div class="rounded-xl p-4 mb-4 flex flex-wrap items-center gap-4"
           :style="{ backgroundColor: 'var(--cptm-surface)' }">
        <div class="flex items-center gap-2">
          <FilterIcon class="w-4 h-4" :style="{ color: 'var(--cptm-text-muted)' }" />
          <span class="text-sm font-medium" :style="{ color: 'var(--cptm-text)' }">Filtros:</span>
        </div>
        <select v-model="filtroTipo" class="field-input w-auto min-w-[150px]">
          <option value="">Todos os Tipos</option>
          <option value="efluente">Efluente</option>
          <option value="arvore">Árvore</option>
          <option value="fauna">Fauna</option>
          <option value="erosao">Erosão</option>
          <option value="residuo">Resíduo</option>
        </select>
        <input v-model="filtroOperador" class="field-input w-auto min-w-[200px]" placeholder="Buscar operador..." maxlength="200" />
        <input v-model="filtroDataInicio" type="date" class="field-input w-auto" />
        <input v-model="filtroDataFim" type="date" class="field-input w-auto" />
      </div>

      <!-- Tabela de Registros -->
      <div class="rounded-xl overflow-hidden border" :style="{ borderColor: 'var(--cptm-border)' }">
        <div class="overflow-x-auto">
          <table class="w-full text-sm">
            <thead>
              <tr :style="{ backgroundColor: 'var(--cptm-surface-alt)' }">
                <th class="text-left px-4 py-3 font-semibold" :style="{ color: 'var(--cptm-text)' }">ID</th>
                <th class="text-left px-4 py-3 font-semibold" :style="{ color: 'var(--cptm-text)' }">Tipo</th>
                <th class="text-left px-4 py-3 font-semibold" :style="{ color: 'var(--cptm-text)' }">Operador</th>
                <th class="text-left px-4 py-3 font-semibold" :style="{ color: 'var(--cptm-text)' }">Lat/Long</th>
                <th class="text-left px-4 py-3 font-semibold" :style="{ color: 'var(--cptm-text)' }">Fotos</th>
                <th class="text-left px-4 py-3 font-semibold" :style="{ color: 'var(--cptm-text)' }">Data</th>
                <th class="text-left px-4 py-3 font-semibold" :style="{ color: 'var(--cptm-text)' }">Ações</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="reg in registrosFiltrados" :key="reg.pk"
                  class="border-t transition-colors"
                  :style="{ borderColor: 'var(--cptm-border)' }"
                  @mouseenter="$event.currentTarget.style.backgroundColor = 'var(--cptm-surface-alt)'"
                  @mouseleave="$event.currentTarget.style.backgroundColor = 'transparent'">
                <td class="px-4 py-3 font-mono text-xs" :style="{ color: 'var(--cptm-text-muted)' }">{{ reg.pk }}</td>
                <td class="px-4 py-3">
                  <span class="inline-flex items-center gap-1 px-2 py-0.5 rounded-full text-xs font-medium"
                        :class="tipoBadgeClass((reg.natureza||'').toLowerCase())">
                    {{ reg.natureza }}
                  </span>
                </td>
                <td class="px-4 py-3" :style="{ color: 'var(--cptm-text)' }">{{ reg.autor }}</td>
                <td class="px-4 py-3">
                  <a :href="`https://www.google.com/maps?q=${reg.latitude},${reg.longitude}`"
                     target="_blank" rel="noopener noreferrer"
                     class="text-xs flex items-center gap-1 hover:underline"
                     :style="{ color: 'var(--cptm-info)' }">
                    <MapPinIcon class="w-3 h-3" />
                    {{ reg.latitude?.toFixed(4) }}, {{ reg.longitude?.toFixed(4) }}
                  </a>
                </td>
                <td class="px-4 py-3">
                  <span class="flex items-center gap-1 text-xs" :style="{ color: 'var(--cptm-text-muted)' }">
                    <CameraIcon class="w-3 h-3" /> {{ reg.qtdFotos }}
                  </span>
                </td>
                <td class="px-4 py-3 text-xs" :style="{ color: 'var(--cptm-text-muted)' }">
                  {{ reg.dataCadastro ? formatarData(reg.dataCadastro) : '—' }}
                </td>
                <td class="px-4 py-3">
                  <button @click="confirmarExclusao(reg)"
                          class="inline-flex items-center gap-1 px-2.5 py-1.5 rounded-lg text-xs font-medium text-white cursor-pointer"
                          style="background-color: var(--cptm-danger);">
                    <Trash2Icon class="w-3 h-3" />
                    Excluir
                  </button>
                </td>
              </tr>
              <tr v-if="registrosFiltrados.length === 0">
                <td colspan="7" class="px-4 py-8 text-center text-sm" :style="{ color: 'var(--cptm-text-muted)' }">
                  Nenhum registro encontrado.
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>

    <!-- Modal de confirmação de exclusão -->
    <Teleport to="body">
      <div v-if="registroParaExcluir" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 px-4">
        <div class="rounded-2xl p-6 max-w-sm w-full shadow-2xl" :style="{ backgroundColor: 'var(--cptm-surface)' }">
          <div class="flex items-center gap-3 mb-4">
            <div class="w-10 h-10 rounded-full bg-red-100 dark:bg-red-950/50 flex items-center justify-center">
              <AlertTriangleIcon class="w-5 h-5 text-red-500" />
            </div>
            <h3 class="text-base font-bold" :style="{ color: 'var(--cptm-text)' }">Confirmar Exclusão</h3>
          </div>
          <p class="text-sm mb-6" :style="{ color: 'var(--cptm-text-muted)' }">
            Deseja excluir permanentemente o registro <strong>{{ registroParaExcluir.pk }}</strong>
            ({{ registroParaExcluir.natureza }})? Esta ação não pode ser desfeita.
          </p>
          <div class="flex gap-3">
            <button @click="registroParaExcluir = null"
                    class="flex-1 py-2.5 rounded-xl border text-sm font-medium cursor-pointer"
                    :style="{ borderColor: 'var(--cptm-border)', color: 'var(--cptm-text)' }">
              Cancelar
            </button>
            <button @click="excluirRegistro"
                    class="flex-1 py-2.5 rounded-xl text-white text-sm font-medium cursor-pointer"
                    style="background-color: var(--cptm-danger);">
              Excluir
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'
import axios from 'axios'
import {
  Shield as ShieldIcon, Sun as SunIcon, Moon as MoonIcon,
  Filter as FilterIcon, MapPin as MapPinIcon, Camera as CameraIcon,
  Trash2 as Trash2Icon, AlertTriangle as AlertTriangleIcon
} from 'lucide-vue-next'
import DashboardCard from '@/components/DashboardCard.vue'

const router = useRouter()
const authStore = useAuthStore()
const themeStore = useThemeStore()

const registros = ref([])
const filtroTipo = ref('')
const filtroOperador = ref('')
const filtroDataInicio = ref('')
const filtroDataFim = ref('')
const registroParaExcluir = ref(null)

const registrosFiltrados = computed(() => {
  return registros.value.filter(r => {
    if (filtroTipo.value && (r.natureza||'').toLowerCase() !== filtroTipo.value) return false
    if (filtroOperador.value && !(r.autor||'').toLowerCase().includes(filtroOperador.value.toLowerCase())) return false
    return true
  })
})

const registrosSemana = computed(() => {
  const umaSemanaAtras = new Date()
  umaSemanaAtras.setDate(umaSemanaAtras.getDate() - 7)
  return registros.value.filter(r => r.dataCadastro && new Date(r.dataCadastro) >= umaSemanaAtras).length
})

function tipoBadgeClass(tipo) {
  const classes = {
    efluente: 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-300',
    arvore: 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-300',
    fauna: 'bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-300',
    erosao: 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-300',
    residuo: 'bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-300'
  }
  return classes[tipo] || 'bg-gray-100 text-gray-700'
}

function formatarData(data) {
  return new Date(data).toLocaleString('pt-BR', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' })
}

function confirmarExclusao(registro) {
  registroParaExcluir.value = registro
}

async function excluirRegistro() {
  if (!registroParaExcluir.value) return
  try {
    await axios.delete(`/api/formularios/${encodeURIComponent(registroParaExcluir.value.pk)}`)
    registros.value = registros.value.filter(r => r.pk !== registroParaExcluir.value.pk)
    registroParaExcluir.value = null
  } catch (e) {
    alert('Erro ao excluir: ' + (e.response?.data?.mensagem || e.message))
  }
}

async function carregarRegistros() {
  try {
    const res = await axios.get('/api/formularios')
    registros.value = res.data
  } catch (e) {
    console.error('Erro ao carregar registros:', e)
  }
}

async function fazerLogout() {
  await authStore.logout()
  router.push('/login')
}

onMounted(carregarRegistros)
</script>
