<script setup>
import { RouterView } from 'vue-router'
import { ref, onMounted, onUnmounted } from 'vue';

const isOnline = ref(navigator.onLine);

const updateOnlineStatus = () => {
  isOnline.value = navigator.onLine;
  if (isOnline.value) {
    console.log("Internet conectada! Sincronização ativada.");
  }
};

onMounted(() => {
  window.addEventListener('online', updateOnlineStatus);
  window.addEventListener('offline', updateOnlineStatus);
});

onUnmounted(() => {
  window.removeEventListener('online', updateOnlineStatus);
  window.removeEventListener('offline', updateOnlineStatus);
});
</script>

<template>
  <div v-if="!isOnline" class="offline-alert">
    Você está em modo Offline. Os dados serão salvos no dispositivo. 💾
  </div>

  <RouterView />
</template>

<style>
body {
  margin: 0;
  padding: 0;
  font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
  background-color: #f4f4f4;
  color: #333;
}

.offline-alert {
  background-color: #ff9800;
  color: white;
  text-align: center;
  padding: 10px;
  font-size: 14px;
  font-weight: bold;
  position: sticky;
  top: 0;
  z-index: 1000;
}
</style>
