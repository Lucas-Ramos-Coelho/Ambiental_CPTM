<template>
  <div class="field-group">
    <div class="field-group-meta">
      <div class="field-group-label-row">
        <label class="field-group-label">
          {{ label }}
        </label>
        <CortinaAmarela v-if="tooltip">
          {{ tooltip }}
        </CortinaAmarela>
      </div>
      <p class="field-group-hint" :class="{ 'field-group-hint-empty': !hint }">
        {{ hint || ' ' }}
      </p>
    </div>
    <slot />
  </div>
</template>

<script setup>
import CortinaAmarela from '@/components/CortinaAmarela.vue'

defineProps({
  label: { type: String, required: true },
  tooltip: { type: String, default: '' },
  hint: { type: String, default: '' }
})
</script>

<style scoped>
.field-group {
  display: flex;
  flex-direction: column;
  gap: 0.35rem;
  height: 100%;
}

.field-group-meta {
  display: flex;
  flex-direction: column;
  justify-content: flex-start;
  min-height: 3.75rem;
}

.field-group-label-row {
  display: flex;
  align-items: center;
  gap: 0.35rem;
  flex-wrap: wrap;
}

.field-group-label {
  font-size: 0.75rem;
  font-weight: 600;
  color: var(--cptm-text);
  line-height: 1.35;
}

.field-group-hint {
  margin: 0;
  font-size: 0.7rem;
  line-height: 1.35;
  color: var(--cptm-text-muted);
}

.field-group-hint-empty {
  visibility: hidden;
}

@media (max-width: 640px) {
  .field-group-meta {
    min-height: auto;
  }
}
</style>
