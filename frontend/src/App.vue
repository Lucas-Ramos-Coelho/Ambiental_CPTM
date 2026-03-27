<script setup>
import { RouterView } from 'vue-router'
import { onMounted, watch } from 'vue'
import { sincronizarPendentes, carregarCacheReferencias } from '@/services/syncService'
import { useThemeStore } from '@/stores/theme'
import { useConnectivityStatus } from '@/services/connectivityService'

const themeStore = useThemeStore()
const { isOnline, checkConnectivity } = useConnectivityStatus()

const onOnline = async () => {
  console.log('[CPTM] Internet voltou - iniciando sincronizacao...')
  const resultado = await sincronizarPendentes()
  console.log(`[CPTM] Sync: ${resultado.enviados} enviados, ${resultado.erros} erros`)
  await carregarCacheReferencias()
}

onMounted(() => {
  themeStore.aplicar()
  checkConnectivity()
})

watch(isOnline, (online, previous) => {
  if (online && previous === false) {
    onOnline()
  }
})
</script>

<template>
  <RouterView />
</template>
