import { db } from '@/database'
import axios from 'axios'
import { checkConnectivity } from '@/services/connectivityService'

/**
 * Serviço de sincronização offline → online.
 * Busca formulários com status 'completo' no IndexedDB e envia para a API.
 */
export async function sincronizarPendentes() {
  if (!(await checkConnectivity())) return { enviados: 0, erros: 0 }

  const pendentes = await db.formularios.where('status').equals('completo').toArray()
  let enviados = 0
  let erros = 0

  for (const form of pendentes) {
    try {
      // Busca fotos vinculadas
      const fotos = await db.fotos.where('formularioId').equals(form.id).toArray()

      // Monta o payload multipart — envia campos flat (sem wrapper "campos")
      const formData = new FormData()
      formData.append('dados', JSON.stringify(form.campos))

      fotos.forEach((foto, i) => {
        const blob = new Blob([foto.blob], { type: 'image/jpeg' })
        formData.append(`fotos`, blob, `foto_${i + 1}.jpg`)
      })

      await axios.post('/api/formularios', formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      })

      // Marca como sincronizado
      await db.formularios.update(form.id, {
        status: 'sincronizado',
        sincronizadoEm: new Date().toISOString()
      })
      enviados++
    } catch (e) {
      console.error(`Erro ao sincronizar formulário ${form.id}:`, e)
      await db.formularios.update(form.id, { status: 'erro' })
      erros++
    }
  }

  return { enviados, erros }
}

/**
 * Carrega as listas de referência (linhas, estações, etc.) e salva no cache local.
 */
export async function carregarCacheReferencias() {
  if (!(await checkConnectivity())) return

  try {
    const [linhas, estacoes, naturezas] = await Promise.all([
      axios.get('/api/referencia/linhas'),
      axios.get('/api/referencia/estacoes'),
      axios.get('/api/referencia/naturezas')
    ])

    await db.cache.bulkPut([
      { chave: 'linhas', dados: linhas.data, atualizadoEm: new Date().toISOString() },
      { chave: 'estacoes', dados: estacoes.data, atualizadoEm: new Date().toISOString() },
      { chave: 'naturezas', dados: naturezas.data, atualizadoEm: new Date().toISOString() }
    ])
  } catch (e) {
    console.warn('Não foi possível atualizar cache de referências:', e)
  }
}

/**
 * Lê uma lista de referência do cache local.
 */
export async function obterCacheReferencia(chave) {
  const item = await db.cache.get(chave)
  return item?.dados || []
}
