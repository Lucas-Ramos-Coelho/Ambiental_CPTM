<template>
  <div class="operator-home">
    <header class="operator-home__header">
      <div class="operator-home__brand">
        <img :src="cptmLogo" alt="CPTM" class="operator-home__logo" />
        <div>
          <p class="operator-home__title">CPTM Efluentes</p>
          <p class="operator-home__subtitle">Tela do operador de campo</p>
        </div>
      </div>

      <div class="operator-home__header-actions">
        <span class="operator-home__status" :class="isOnline ? 'operator-home__status--online' : 'operator-home__status--offline'">
          {{ isOnline ? 'Com conexão' : 'Sem conexão' }}
        </span>
        <button type="button" class="operator-home__icon-button" @click="themeStore.toggle()">
          <SunIcon v-if="themeStore.isDark" class="w-4 h-4" />
          <MoonIcon v-else class="w-4 h-4" />
        </button>
        <button type="button" class="operator-home__exit-button" @click="encerrarSessao">
          Sair
        </button>
      </div>
    </header>

    <main class="operator-home__main">
      <section class="operator-home__welcome-card">
        <p class="operator-home__welcome-label">Operação de efluentes</p>
        <h1 class="operator-home__welcome-title">Olá, {{ authStore.usuario?.nome || 'Operador' }}</h1>
        <p class="operator-home__welcome-text">
          Aqui você cria o formulário de efluente, continua um rascunho salvo e acompanha o envio automático quando a internet voltar.
        </p>

        <div class="operator-home__primary-actions">
          <button type="button" class="operator-home__primary-button" @click="iniciarFormulario">
            <ClipboardPlusIcon class="w-5 h-5" />
            Criar formulário de efluente
          </button>

          <button v-if="temRascunho" type="button" class="operator-home__secondary-button" @click="continuarRascunho">
            <FilePenLineIcon class="w-5 h-5" />
            Continuar preenchimento
          </button>
        </div>
      </section>

      <section class="operator-home__grid">
        <article class="operator-home__panel">
          <h2 class="operator-home__panel-title">Resumo rápido</h2>
          <div class="operator-home__summary-list">
            <div class="operator-home__summary-item">
              <span>Pendentes de envio</span>
              <strong>{{ resumo.pendentes }}</strong>
            </div>
            <div class="operator-home__summary-item">
              <span>Sincronizados</span>
              <strong>{{ resumo.sincronizados }}</strong>
            </div>
            <div class="operator-home__summary-item">
              <span>Com erro</span>
              <strong>{{ resumo.erro }}</strong>
            </div>
          </div>
        </article>

        <article class="operator-home__panel">
          <h2 class="operator-home__panel-title">Situação do dispositivo</h2>
          <p class="operator-home__panel-text">
            <template v-if="temRascunho">
              Existe um rascunho salvo neste aparelho. Se o aplicativo fechar sem querer, o preenchimento continuará de onde parou.
            </template>
            <template v-else>
              Nenhum rascunho em andamento neste dispositivo.
            </template>
          </p>

          <button type="button" class="operator-home__secondary-button operator-home__secondary-button--compact" @click="sincronizarAgora" :disabled="sincronizando">
            <RefreshCwIcon class="w-4 h-4" :class="sincronizando ? 'animate-spin' : ''" />
            {{ sincronizando ? 'Sincronizando...' : 'Sincronizar agora' }}
          </button>

          <p v-if="mensagem" class="operator-home__feedback">{{ mensagem }}</p>
        </article>
      </section>
    </main>
  </div>
</template>

<script setup>
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { db } from '@/database'
import { sincronizarPendentes } from '@/services/syncService'
import { useConnectivityStatus } from '@/services/connectivityService'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'
import cptmLogo from '@/assets/Logo_CPTM.png'
import {
  ClipboardPlus as ClipboardPlusIcon,
  FilePenLine as FilePenLineIcon,
  Moon as MoonIcon,
  RefreshCw as RefreshCwIcon,
  Sun as SunIcon,
} from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()
const themeStore = useThemeStore()
const { isOnline, checkConnectivity } = useConnectivityStatus()

const sincronizando = ref(false)
const mensagem = ref('')
const temRascunho = ref(false)
const resumo = ref({ pendentes: 0, sincronizados: 0, erro: 0 })

function getDraftKey() {
  return `rascunho_operador_${authStore.usuario?.id || 'anon'}_efluente`
}

async function carregarEstado() {
  const draft = await db.cache.get(getDraftKey())
  temRascunho.value = Boolean(draft?.dados)

  const [pendentes, sincronizados, erro] = await Promise.all([
    db.formularios.where('status').equals('completo').count(),
    db.formularios.where('status').equals('sincronizado').count(),
    db.formularios.where('status').equals('erro').count(),
  ])

  resumo.value = { pendentes, sincronizados, erro }
}

function iniciarFormulario() {
  router.push('/operador/formulario')
}

function continuarRascunho() {
  router.push('/operador/formulario')
}

async function sincronizarAgora() {
  if (!(await checkConnectivity())) {
    mensagem.value = 'Sem internet no momento. Os registros continuam salvos localmente.'
    return
  }

  sincronizando.value = true
  mensagem.value = ''

  try {
    const resultado = await sincronizarPendentes()
    mensagem.value = `Sincronização concluída: ${resultado.enviados} enviados, ${resultado.erros} com erro.`
    await carregarEstado()
  } finally {
    sincronizando.value = false
  }
}

async function encerrarSessao() {
  await authStore.logout()
  await router.push('/login')
}

onMounted(async () => {
  await checkConnectivity()
  await carregarEstado()
})
</script>

<style scoped>
.operator-home {
  min-height: 100dvh;
  background: var(--cptm-bg);
}

.operator-home__header,
.operator-home__main {
  max-width: 980px;
  margin: 0 auto;
}

.operator-home__header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  padding: 1.1rem 1.25rem;
}

.operator-home__brand {
  display: flex;
  align-items: center;
  gap: 0.85rem;
}

.operator-home__logo {
  width: 48px;
  height: 48px;
  object-fit: contain;
}

.operator-home__title {
  margin: 0;
  font-size: 1.1rem;
  font-weight: 800;
  color: var(--cptm-text);
}

.operator-home__subtitle {
  margin: 0.15rem 0 0;
  color: var(--cptm-text-muted);
  font-size: 0.8rem;
}

.operator-home__header-actions {
  display: flex;
  align-items: center;
  gap: 0.65rem;
}

.operator-home__status,
.operator-home__icon-button,
.operator-home__exit-button,
.operator-home__secondary-button,
.operator-home__primary-button {
  border-radius: 0.9rem;
  font-weight: 700;
}

.operator-home__status {
  padding: 0.55rem 0.85rem;
  font-size: 0.75rem;
}

.operator-home__status--online {
  background: rgba(34, 197, 94, 0.14);
  color: #15803d;
}

.operator-home__status--offline {
  background: rgba(245, 158, 11, 0.18);
  color: #b45309;
}

.operator-home__icon-button,
.operator-home__exit-button {
  border: 1px solid var(--cptm-border);
  background: var(--cptm-surface);
  color: var(--cptm-text);
}

.operator-home__icon-button {
  width: 2.5rem;
  height: 2.5rem;
  display: grid;
  place-items: center;
  cursor: pointer;
}

.operator-home__exit-button {
  padding: 0.7rem 1rem;
  cursor: pointer;
}

.operator-home__main {
  padding: 0 1.25rem 2rem;
}

.operator-home__welcome-card,
.operator-home__panel {
  border: 1px solid var(--cptm-border);
  border-radius: 1.25rem;
  background: var(--cptm-surface);
}

.operator-home__welcome-card {
  padding: 1.5rem;
}

.operator-home__welcome-label {
  margin: 0 0 0.4rem;
  font-size: 0.78rem;
  font-weight: 800;
  letter-spacing: 0.05em;
  text-transform: uppercase;
  color: #4c6246;
}

.operator-home__welcome-title {
  margin: 0;
  font-size: 1.85rem;
  color: var(--cptm-text);
}

.operator-home__welcome-text,
.operator-home__panel-text,
.operator-home__feedback {
  margin: 0.7rem 0 0;
  color: var(--cptm-text-muted);
  line-height: 1.5;
}

.operator-home__primary-actions {
  display: flex;
  flex-wrap: wrap;
  gap: 0.8rem;
  margin-top: 1.25rem;
}

.operator-home__primary-button,
.operator-home__secondary-button {
  display: inline-flex;
  align-items: center;
  gap: 0.6rem;
  padding: 1rem 1.1rem;
  cursor: pointer;
}

.operator-home__primary-button {
  border: none;
  background: var(--cptm-primary);
  color: #fff;
}

.operator-home__secondary-button {
  border: 1px solid var(--cptm-border);
  background: var(--cptm-surface-alt);
  color: var(--cptm-text);
}

.operator-home__secondary-button--compact {
  margin-top: 1rem;
}

.operator-home__grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 1rem;
  margin-top: 1rem;
}

.operator-home__panel {
  padding: 1.25rem;
}

.operator-home__panel-title {
  margin: 0;
  color: var(--cptm-text);
  font-size: 1rem;
}

.operator-home__summary-list {
  display: grid;
  gap: 0.7rem;
  margin-top: 1rem;
}

.operator-home__summary-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-radius: 0.9rem;
  background: var(--cptm-surface-alt);
  border: 1px solid var(--cptm-border);
  padding: 0.85rem 1rem;
  color: var(--cptm-text);
}

@media (max-width: 720px) {
  .operator-home__header {
    flex-direction: column;
    align-items: center;
    text-align: center;
  }

  .operator-home__brand {
    width: 100%;
    justify-content: center;
    text-align: center;
  }

  .operator-home__header-actions {
    display: grid;
    grid-template-columns: 1fr auto auto;
    width: 100%;
  }

  .operator-home__grid {
    grid-template-columns: 1fr;
  }

  .operator-home__primary-actions {
    display: grid;
  }
}

.operator-primary-action,
.operator-secondary-action {
  border-radius: 1rem;
  padding: 0.95rem 1.1rem;
  font-size: 0.92rem;
  font-weight: 700;
  display: inline-flex;
  align-items: center;
  gap: 0.55rem;
  cursor: pointer;
}

.operator-primary-action {
  border: none;
  background: #4C6246;
  color: #fff;
}

.operator-secondary-action {
  border: 1px solid var(--cptm-border);
  background: var(--cptm-surface);
  color: var(--cptm-text);
}

.operator-metrics-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 1rem;
  margin-top: 1.25rem;
}

.operator-metric-card,
.operator-panel {
  border: 1px solid var(--cptm-border);
  background: var(--cptm-surface);
  border-radius: 1.25rem;
}

.operator-metric-card {
  display: flex;
  align-items: center;
  gap: 1rem;
  padding: 1rem;
}

.operator-metric-icon {
  width: 2.9rem;
  height: 2.9rem;
  border-radius: 1rem;
  display: grid;
  place-items: center;
}

.operator-metric-icon--green { background: rgba(34, 197, 94, 0.12); color: #16a34a; }
.operator-metric-icon--blue { background: rgba(59, 130, 246, 0.12); color: #2563eb; }
.operator-metric-icon--amber { background: rgba(245, 158, 11, 0.14); color: #d97706; }
.operator-metric-icon--slate { background: rgba(100, 116, 139, 0.14); color: #475569; }

.operator-metric-label {
  margin: 0;
  font-size: 0.8rem;
  color: var(--cptm-text-muted);
}

.operator-metric-value {
  margin: 0.2rem 0 0;
  font-size: 1.6rem;
  font-weight: 800;
  color: var(--cptm-text);
}

.operator-panels-grid {
  display: grid;
  grid-template-columns: 1.2fr 0.9fr;
  gap: 1rem;
  margin-top: 1rem;
}

.operator-panel {
  padding: 1.25rem;
}

.operator-panel-head {
  margin-bottom: 1rem;
}

.operator-panel-title {
  margin: 0;
  font-size: 1rem;
  color: var(--cptm-text);
}

.operator-panel-subtitle {
  margin: 0.3rem 0 0;
  font-size: 0.8rem;
  color: var(--cptm-text-muted);
}

.operator-action-list {
  display: grid;
  gap: 0.75rem;
}

.operator-action-card {
  border: 1px solid var(--cptm-border);
  border-radius: 1rem;
  background: var(--cptm-surface-alt);
  color: var(--cptm-text);
  padding: 1rem;
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: center;
  text-align: left;
  cursor: pointer;
}

.operator-action-title {
  margin: 0;
  font-size: 0.92rem;
  font-weight: 700;
}

.operator-action-text,
.operator-activity-text,
.operator-feedback {
  margin: 0.3rem 0 0;
  color: var(--cptm-text-muted);
  font-size: 0.8rem;
}

.operator-activity-box {
  border-radius: 1rem;
  border: 1px dashed var(--cptm-border);
  background: var(--cptm-surface-alt);
  padding: 1rem;
}

.operator-activity-label {
  margin: 0;
  font-size: 0.74rem;
  font-weight: 700;
  text-transform: uppercase;
  letter-spacing: 0.04em;
  color: var(--cptm-text-muted);
}

.operator-activity-title {
  margin: 0.45rem 0 0;
  font-size: 1rem;
  font-weight: 700;
  color: var(--cptm-text);
}

@media (max-width: 960px) {
  .operator-metrics-grid,
  .operator-panels-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }

  .operator-hero {
    flex-direction: column;
    align-items: stretch;
  }
}

@media (max-width: 640px) {
  .operator-shell-inner,
  .operator-shell-main {
    padding-left: 1rem;
    padding-right: 1rem;
  }

  .operator-shell-inner,
  .operator-brand,
  .operator-shell-actions,
  .operator-hero-actions,
  .operator-metrics-grid,
  .operator-panels-grid {
    grid-template-columns: 1fr;
  }

  .operator-shell-inner {
    flex-direction: column;
    align-items: stretch;
  }

  .operator-shell-actions {
    justify-content: flex-end;
  }

  .operator-metrics-grid,
  .operator-panels-grid {
    display: grid;
  }
}
</style>