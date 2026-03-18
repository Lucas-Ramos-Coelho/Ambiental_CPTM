<template>
  <div class="form-container">
    <div v-if="!isOnline" class="offline-banner">
      Você está Offline. Os dados serão salvos no dispositivo. 💾
    </div>

    <form @submit.prevent="salvarOcorrencia">
      <input v-model="form.titulo" placeholder="Título da Ocorrência" required />
      <textarea v-model="form.descricao" placeholder="Descrição detalhada"></textarea>
      
      <button type="submit" :disabled="loading">
        {{ isOnline ? 'Enviar para CPTM' : 'Salvar Localmente' }}
      </button>
    </form>
  </div>
</template>

<script setup>
import { ref } from 'vue';
import { db } from '../database'; // Importando o Dexie que você criou
import axios from 'axios';

const form = ref({ titulo: '', descricao: '', status: 'pendente' });
const loading = ref(false);

const salvarOcorrencia = async () => {
  loading.value = true;
  
  // 1. Sempre salva no IndexedDB primeiro (Garantia de segurança dos dados)
  const idLocal = await db.ocorrencias.add({
    ...form.value,
    dataCriacao: new Date(),
    status: 'pendente'
  });

  // 2. Tenta enviar para o Back-end se houver internet
  if (navigator.onLine) {
    try {
      await axios.post('http://localhost:5217/ocorrencias', form.value);
      // Se deu certo, atualiza o status no banco local para 'sincronizado'
      await db.ocorrencias.update(idLocal, { status: 'sincronizado' });
      alert("Enviado com sucesso ao Oracle!");
    } catch (error) {
      console.error("Erro ao enviar, ficará pendente no celular:", error);
    }
  }

  loading.value = false;
  form.value = { titulo: '', descricao: '', status: 'pendente' }; // Limpa form
};
</script>
