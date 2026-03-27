<template>
  <div class="map-card">
    <div ref="mapElement" class="map-canvas"></div>

    <div class="map-toolbar">
      <div class="map-coords">
        <p class="map-label">GPS</p>
        <p class="map-value">
          <strong>{{ latitude || '—' }}</strong>,
          <strong>{{ longitude || '—' }}</strong>
        </p>
        <p class="map-hint">Toque no mapa para ajustar o ponto manualmente.</p>
      </div>

      <button type="button" class="map-action" @click="capturarCoordenadas">
        Usar localização atual
      </button>
    </div>
  </div>
</template>

<script setup>
import { computed, nextTick, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import L from 'leaflet'

const props = defineProps({
  latitude: { type: [String, Number, null], default: null },
  longitude: { type: [String, Number, null], default: null }
})

const emit = defineEmits(['update:latitude', 'update:longitude'])

const mapElement = ref(null)

let map = null
let marker = null
let tileLayer = null

const pinIcon = L.divIcon({
  className: 'map-pin-wrapper',
  html: '<span class="map-pin"></span>',
  iconSize: [24, 34],
  iconAnchor: [12, 32],
  popupAnchor: [0, -28]
})

const parsedLatitude = computed(() => {
  const value = Number(props.latitude)
  return Number.isFinite(value) ? value : null
})

const parsedLongitude = computed(() => {
  const value = Number(props.longitude)
  return Number.isFinite(value) ? value : null
})

function definirCoordenadas(lat, lng, center = true) {
  emit('update:latitude', lat.toFixed(6))
  emit('update:longitude', lng.toFixed(6))

  if (!map) {
    return
  }

  const latLng = [lat, lng]
  if (!marker) {
    marker = L.marker(latLng, { icon: pinIcon, keyboard: false }).addTo(map)
  } else {
    marker.setLatLng(latLng)
  }

  if (center) {
    map.setView(latLng, Math.max(map.getZoom(), 16))
  }
}

function sincronizarMarcador() {
  if (!map) {
    return
  }

  if (parsedLatitude.value === null || parsedLongitude.value === null) {
    if (marker) {
      map.removeLayer(marker)
      marker = null
    }
    return
  }

  definirCoordenadas(parsedLatitude.value, parsedLongitude.value, false)
}

function reajustarMapa(center = false) {
  if (!map) {
    return
  }

  nextTick(() => {
    window.requestAnimationFrame(() => {
      map.invalidateSize()

      if (parsedLatitude.value !== null && parsedLongitude.value !== null) {
        definirCoordenadas(parsedLatitude.value, parsedLongitude.value, center)
        return
      }

      if (center) {
        map.setView([-23.5489, -46.6388], 11)
      }
    })
  })
}

function capturarCoordenadas() {
  if (!navigator.geolocation) {
    alert('Geolocalização não suportada neste dispositivo.')
    return
  }

  navigator.geolocation.getCurrentPosition(
    ({ coords }) => {
      definirCoordenadas(coords.latitude, coords.longitude)
    },
    (err) => {
      alert(`Erro ao capturar GPS: ${err.message}`)
    },
    { enableHighAccuracy: true, timeout: 15000, maximumAge: 0 }
  )
}

onMounted(() => {
  const initialCenter = parsedLatitude.value !== null && parsedLongitude.value !== null
    ? [parsedLatitude.value, parsedLongitude.value]
    : [-23.5489, -46.6388]

  map = L.map(mapElement.value, { zoomControl: false }).setView(initialCenter, parsedLatitude.value !== null ? 16 : 11)

  L.control.zoom({ position: 'bottomright' }).addTo(map)

  tileLayer = L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
    maxZoom: 19,
    attribution: '&copy; OpenStreetMap contributors'
  }).addTo(map)

  map.on('click', (event) => {
    definirCoordenadas(event.latlng.lat, event.latlng.lng, true)
  })

  sincronizarMarcador()
  reajustarMapa(true)
  window.addEventListener('resize', reajustarMapa)
})

watch(() => [props.latitude, props.longitude], () => {
  sincronizarMarcador()
})

onBeforeUnmount(() => {
  window.removeEventListener('resize', reajustarMapa)

  if (tileLayer) {
    tileLayer.remove()
    tileLayer = null
  }

  if (map) {
    map.remove()
    map = null
    marker = null
  }
})
</script>

<style scoped>
.map-card {
  position: relative;
  isolation: isolate;
  border: 1px solid var(--cptm-border);
  border-radius: 1rem;
  overflow: hidden;
  background: var(--cptm-surface);
}

.map-canvas {
  height: 19rem;
  width: 100%;
}

.map-toolbar {
  display: flex;
  justify-content: space-between;
  gap: 1rem;
  align-items: center;
  padding: 0.9rem 1rem 1rem;
  background: var(--cptm-surface);
}

.map-coords {
  min-width: 0;
}

.map-label {
  margin: 0;
  font-size: 0.72rem;
  font-weight: 700;
  color: var(--cptm-text);
}

.map-value {
  margin: 0.2rem 0 0;
  font-size: 0.82rem;
  color: var(--cptm-text);
}

.map-hint {
  margin: 0.28rem 0 0;
  font-size: 0.72rem;
  color: var(--cptm-text-muted);
}

.map-action {
  border: none;
  border-radius: 0.8rem;
  background: var(--cptm-primary);
  color: #fff;
  padding: 0.85rem 1rem;
  font-size: 0.82rem;
  font-weight: 700;
  cursor: pointer;
  white-space: nowrap;
}

.map-action:hover {
  background: var(--cptm-primary-hover);
}

:deep(.leaflet-control-zoom a) {
  color: #111;
}

:deep(.leaflet-container),
:deep(.leaflet-pane),
:deep(.leaflet-top),
:deep(.leaflet-bottom),
:deep(.leaflet-control) {
  z-index: 1 !important;
}

:deep(.map-pin-wrapper) {
  background: transparent;
  border: none;
}

:deep(.map-pin) {
  position: relative;
  display: block;
  width: 1.1rem;
  height: 1.1rem;
  background: #d92d20;
  border: 2px solid #fff;
  border-radius: 999px 999px 999px 0;
  box-shadow: 0 8px 18px rgba(0, 0, 0, 0.28);
  transform: rotate(-45deg);
}

:deep(.map-pin::after) {
  content: '';
  position: absolute;
  inset: 0.2rem;
  border-radius: 999px;
  background: #fff;
}

@media (max-width: 640px) {
  .map-canvas {
    height: 13.5rem;
  }

  .map-toolbar {
    flex-direction: column;
    align-items: stretch;
    padding: 0.85rem;
  }

  .map-action {
    width: 100%;
  }
}
</style>