<template>
  <div class="relative" ref="wrapper">
    <div class="field-input flex items-center cursor-pointer" @click="aberto = !aberto">
      <span v-if="textoSelecionado" :style="{ color: 'var(--cptm-text)' }">{{ textoSelecionado }}</span>
      <span v-else :style="{ color: 'var(--cptm-text-muted)' }">{{ placeholder }}</span>
      <ChevronDownIcon class="w-4 h-4 ml-auto shrink-0" :style="{ color: 'var(--cptm-text-muted)' }" />
    </div>

    <Transition name="dropdown">
      <div v-if="aberto" class="absolute z-50 mt-1 w-full rounded-lg shadow-lg border max-h-48 overflow-auto"
           :style="{ backgroundColor: 'var(--cptm-surface)', borderColor: 'var(--cptm-border)' }">
        <!-- Campo de busca -->
        <div class="sticky top-0 p-2" :style="{ backgroundColor: 'var(--cptm-surface)' }">
          <div class="relative">
            <SearchIcon class="absolute left-2.5 top-1/2 -translate-y-1/2 w-3.5 h-3.5" :style="{ color: 'var(--cptm-text-muted)' }" />
            <input
              v-model="busca"
              ref="inputBusca"
              class="field-input pl-8 text-xs"
              placeholder="Buscar..."
              @click.stop
            />
          </div>
        </div>

        <!-- Opções -->
        <div class="px-1 pb-1">
          <div
            v-for="op in opcoesFiltradas"
            :key="op.valor"
            class="px-3 py-2 text-sm rounded-md cursor-pointer transition-colors"
            :style="{ 
              color: 'var(--cptm-text)',
              backgroundColor: modelValue === op.valor ? 'rgba(22,163,74,0.1)' : 'transparent'
            }"
            @mouseenter="$event.target.style.backgroundColor = 'var(--cptm-surface-alt)'"
            @mouseleave="$event.target.style.backgroundColor = modelValue === op.valor ? 'rgba(22,163,74,0.1)' : 'transparent'"
            @click="selecionar(op.valor)"
          >
            {{ op.texto }}
          </div>
          <div v-if="opcoesFiltradas.length === 0" class="px-3 py-2 text-xs" :style="{ color: 'var(--cptm-text-muted)' }">
            Nenhum resultado.
          </div>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup>
import { ref, computed, watch, nextTick, onMounted, onUnmounted } from 'vue'
import { ChevronDown as ChevronDownIcon, Search as SearchIcon } from 'lucide-vue-next'

const props = defineProps({
  modelValue: { type: String, default: '' },
  opcoes: { type: Array, default: () => [] },
  placeholder: { type: String, default: 'Selecione...' }
})

const emit = defineEmits(['update:modelValue'])

const aberto = ref(false)
const busca = ref('')
const wrapper = ref(null)
const inputBusca = ref(null)

const textoSelecionado = computed(() =>
  props.opcoes.find(o => o.valor === props.modelValue)?.texto || ''
)

const opcoesFiltradas = computed(() => {
  if (!busca.value) return props.opcoes
  const termo = busca.value.toLowerCase()
  return props.opcoes.filter(o => o.texto.toLowerCase().includes(termo))
})

function selecionar(valor) {
  emit('update:modelValue', valor)
  aberto.value = false
  busca.value = ''
}

watch(aberto, async (val) => {
  if (val) {
    await nextTick()
    inputBusca.value?.focus()
  }
})

function fecharExterno(e) {
  if (wrapper.value && !wrapper.value.contains(e.target)) {
    aberto.value = false
  }
}

onMounted(() => document.addEventListener('click', fecharExterno))
onUnmounted(() => document.removeEventListener('click', fecharExterno))
</script>

<style scoped>
.dropdown-enter-active, .dropdown-leave-active { transition: all 0.15s ease; }
.dropdown-enter-from, .dropdown-leave-to { opacity: 0; transform: translateY(-4px); }
</style>
