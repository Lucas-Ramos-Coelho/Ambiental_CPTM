import Dexie from 'dexie';

export const db = new Dexie('CptmAmbientalDB');
db.version(1).stores({
  ocorrencias: '++id, titulo, descricao, latitude, longitude, status' 
  // status pode ser: 'pendente' (offline) ou 'sincronizado' (online)
});
