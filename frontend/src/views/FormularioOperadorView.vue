<template>
  <div class="min-h-dvh operator-form-page" :style="{ backgroundColor: 'var(--cptm-bg)' }">
    <!-- ====== HEADER ====== -->
    <header class="sticky top-0 z-40 px-4 py-3 shadow-sm border-b"
            :style="{ backgroundColor: 'var(--cptm-header)', color: 'var(--cptm-header-text)', borderColor: 'var(--cptm-border)' }">
      <div class="max-w-5xl mx-auto flex items-center justify-between gap-3">
        <div class="flex items-center gap-3 min-w-0">
          <img :src="cptmLogo" alt="CPTM" class="w-10 h-10 object-contain rounded-lg bg-white p-1" />
        <div>
            <p class="text-sm font-bold leading-tight">Novo formulário de campo</p>
            <p class="text-[11px] opacity-70">Registro operacional para agentes em campo</p>
          </div>
        </div>

        <div class="flex items-center gap-2 shrink-0">
          <span class="hidden sm:flex items-center gap-1 text-[10px] font-medium px-2 py-1 rounded-full"
                :class="isOnline ? 'bg-green-500/20 text-green-400' : 'bg-amber-500/20 text-amber-400'">
            <span class="w-1.5 h-1.5 rounded-full" :class="isOnline ? 'bg-green-400' : 'bg-amber-400'"></span>
            {{ isOnline ? 'Online' : 'Offline' }}
          </span>
          <button @click="router.push('/operador')" class="px-3 py-2 rounded-lg cursor-pointer text-xs font-semibold border"
                  :style="{ color: 'var(--cptm-header-text)', borderColor: 'rgba(255,255,255,0.15)' }">
            Início
          </button>
          <button @click="themeStore.toggle()" class="p-2 rounded-lg cursor-pointer border"
                  :style="{ color: 'var(--cptm-header-text)', borderColor: 'rgba(255,255,255,0.15)' }">
            <SunIcon v-if="themeStore.isDark" class="w-4 h-4" />
            <MoonIcon v-else class="w-4 h-4" />
          </button>
        </div>
      </div>
    </header>

    <!-- ====== OFFLINE BANNER ====== -->
    <div v-if="!isOnline" class="offline-banner">
      Sem internet — dados serão salvos localmente
    </div>

    <!-- ====== STEPPER ====== -->
    <div class="px-4 py-4">
      <div class="max-w-3xl mx-auto mb-3">
        <div class="operator-form-draft-banner">
          <div>
            <p class="operator-form-draft-title">Formulário de efluente</p>
            <p class="operator-form-draft-text">Campos operacionais salvos automaticamente neste dispositivo.</p>
          </div>
          <span class="operator-form-draft-status">{{ statusRascunho }}</span>
        </div>
      </div>
      <div class="flex items-center justify-between max-w-md mx-auto">
        <template v-for="(step, i) in steps" :key="i">
          <div class="flex flex-col items-center gap-1">
            <div class="stepper-dot" :class="stepClass(i)">
              <CheckIcon v-if="i < etapaAtual" class="w-4 h-4" />
              <span v-else>{{ i + 1 }}</span>
            </div>
            <span class="text-[10px] font-medium" :style="{ color: i <= etapaAtual ? 'var(--cptm-primary)' : 'var(--cptm-text-muted)' }">
              {{ step }}
            </span>
          </div>
          <div v-if="i < steps.length - 1" class="flex-1 h-0.5 mx-2 rounded-full"
               :style="{ backgroundColor: i < etapaAtual ? 'var(--cptm-primary)' : 'var(--cptm-border)' }"></div>
        </template>
      </div>
    </div>

    <!-- ====== FORM CONTENT ====== -->
    <div class="operator-form-content px-4 pb-44 md:pb-36 max-w-3xl mx-auto w-full">

      <!-- ETAPA 1: Identificação -->
      <div v-show="etapaAtual === 0" class="space-y-4 operator-form-step">
        <div class="operator-form-cover-card">
          <img :src="cptmLogo" alt="CPTM" class="operator-form-cover-logo" />
          <div>
            <p class="operator-form-cover-title">Companhia Paulista de Trens Metropolitanos - CPTM</p>
            <p class="operator-form-cover-text">Gerência de Meio Ambiente - GEA</p>
            <p class="operator-form-cover-text">Formulário de Cadastramento/Caracterização - FDC</p>
            <p class="operator-form-cover-text">Programa Ambiental: Efluentes e Emissões Atmosféricas - EEA</p>
            <p class="operator-form-cover-text"><strong>Natureza:</strong> Efluentes - EF</p>
          </div>
        </div>

        <div class="operator-form-glossary-card">
          <p class="operator-form-glossary-title">Glossário</p>
          <p class="operator-form-glossary-text">E.M. = Elemento de Monitoramento · PF = Pessoa Física · PJ = Pessoa Jurídica · DRA = Documento de Regularidade Ambiental</p>
        </div>

        <h2 class="operator-form-section-title">1. Premissas Institucionais / Cabeçalho</h2>

        <FieldGroup label="Nome (PJ) da Contratada" tooltip="Inserir o nome da empresa contratada responsável pela atividade." hint="Use a razão social conforme cadastro ou documento de referência.">
          <input v-model="form.contratada" @blur="form.contratada = sanitizeTrim(form.contratada)" class="field-input" placeholder="CPTM" maxlength="200" />
        </FieldGroup>

        <FieldGroup label="Nº do Contrato (da Contratada)" tooltip="Inserir o identificador do contrato da contratada, quando aplicável." hint="Preencha exatamente como no documento. Evite espaços extras no início ou no fim.">
          <input v-model="form.numContrato" @blur="form.numContrato = sanitizeTrim(form.numContrato)" class="field-input" placeholder="Ex: ARP 01823" maxlength="50" />
        </FieldGroup>

        <FieldGroup label="Local do Escopo Contratual (Pseudônimo)" tooltip="Indicar o nome genérico do local, área ou trecho relacionado ao serviço em campo." hint="Preencha com o nome adotado pela operação, sem espaços excedentes.">
          <input v-model="form.empresa" @blur="form.empresa = sanitizeTrim(form.empresa)" class="field-input" placeholder="Ex: Pátio Capuava" maxlength="200" />
        </FieldGroup>

        <FieldGroup label="Sigla da Área de Meio Ambiente" tooltip="Sigla da gerência de Meio Ambiente responsável pelo cadastro." hint="Campo institucional fixo, preenchido automaticamente.">
          <input v-model="form.siglaMeioAmbiente" class="field-input" value="GEA.OEXE" readonly />
        </FieldGroup>

        <FieldGroup label="Nome (PJ) da Supervisora Ambiental" tooltip="Inserir o nome da empresa supervisora ambiental vinculada à atividade." hint="Use a razão social sem abreviações desnecessárias.">
          <input v-model="form.supervisor" @blur="form.supervisor = sanitizeTrim(form.supervisor)" class="field-input" placeholder="Empresa de Supervisão Ambiental" maxlength="200" />
        </FieldGroup>

        <h2 class="operator-form-section-title pt-4">2. Identificação do Cadastrador e Responsável Técnico</h2>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Autor(a) (PF) do Cadastramento" tooltip="Nome completo da pessoa que realizou o cadastramento da informação." hint="Informar nome completo, sem apelidos.">
            <input v-model="form.autor" @blur="form.autor = sanitizeTrim(form.autor)" class="field-input" maxlength="200" />
          </FieldGroup>
          <FieldGroup label="Responsável Técnico - RT" tooltip="Nome completo do responsável técnico pelo cadastramento e caracterização." hint="Preencha com o nome completo, sem abreviações desnecessárias.">
            <input v-model="form.rt" @blur="form.rt = sanitizeTrim(form.rt)" class="field-input" maxlength="50" />
          </FieldGroup>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Registro Profissional (do RT)" tooltip="Registro profissional do responsável técnico." hint="Informar somente o código do registro, sem espaços.">
            <input v-model="form.registroProfissional" @input="form.registroProfissional = sanitizeCodigo(form.registroProfissional)" class="field-input" maxlength="50" inputmode="text" autocapitalize="characters" />
          </FieldGroup>
          <FieldGroup label="Documento de Responsabilidade Técnica" tooltip="Documento de responsabilidade técnica do responsável técnico." hint="Copiar o identificador do documento sem espaços extras.">
            <input v-model="form.documentoRt" @input="form.documentoRt = sanitizeCodigo(form.documentoRt)" class="field-input" maxlength="50" inputmode="text" autocapitalize="characters" />
          </FieldGroup>
        </div>

        <h2 class="operator-form-section-title pt-4">3. Identificação do Formulário</h2>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Natureza do PGA" tooltip="Para operadores desta frente, a natureza do formulário é sempre efluente." hint="Campo definido automaticamente pelo fluxo do operador.">
            <input v-model="form.natureza" class="field-input" readonly />
          </FieldGroup>
          <FieldGroup label="Tipo de Formulário" tooltip="Tipo do documento de campo utilizado pela equipe." hint="Campo institucional fixo.">
            <input v-model="form.tipoDocumento" class="field-input" maxlength="20" readonly />
          </FieldGroup>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Data de Emissão do Formulário" tooltip="Data de emissão do formulário em campo." hint="Selecionar a data oficial do registro.">
            <input v-model="form.data" type="date" class="field-input" />
          </FieldGroup>
          <FieldGroup label="Número do Formulário" tooltip="Número sequencial do formulário." hint="Use o padrão do formulário físico. Não inserir espaços.">
            <input v-model="form.numero" @input="form.numero = formatNumeroFormulario(form.numero)" class="field-input" placeholder="001 ou FDC-001" maxlength="20" inputmode="text" autocapitalize="characters" />
          </FieldGroup>
        </div>
      </div>

      <!-- ETAPA 2: Local e Data -->
      <div v-show="etapaAtual === 1" class="space-y-4 operator-form-step">
        <h2 class="operator-form-section-title">4. Data e Hora do Cadastramento do E.M.</h2>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Data do Cadastramento" tooltip="Data do cadastramento da informação em campo." hint="Registrar a data efetiva da vistoria/cadastro.">
            <input v-model="form.dataColeta" type="date" class="field-input" />
          </FieldGroup>
          <FieldGroup label="Hora do Cadastramento" tooltip="Horário do cadastramento da informação." hint="Informar o horário efetivo do registro.">
            <input v-model="form.horaColeta" type="time" class="field-input" />
          </FieldGroup>
        </div>

        <h2 class="operator-form-section-title pt-4">5. Identificação do E.M.</h2>

        <FieldGroup label="Chave Primária - Meio Ambiente" tooltip="Chave primária do elemento de monitoramento na base de dados." hint="Informar o identificador sem espaços.">
          <input v-model="form.chavePrimaria" @input="form.chavePrimaria = sanitizeCodigo(form.chavePrimaria)" class="field-input" maxlength="100" inputmode="text" autocapitalize="characters" />
        </FieldGroup>

        <FieldGroup label="Elemento de Monitoramento - Número" tooltip="Número do elemento de monitoramento." hint="Preencha o código conforme cadastro interno, sem espaços.">
          <input v-model="form.elementoNumero" @input="form.elementoNumero = sanitizeCodigo(form.elementoNumero)" class="field-input" maxlength="50" inputmode="text" autocapitalize="characters" />
        </FieldGroup>

        <FieldGroup label="Elemento de Monitoramento - Nome" tooltip="Nome descritivo do elemento monitorado." hint="Usar a denominação operacional adotada pela equipe.">
          <input v-model="form.elementoNome" @blur="form.elementoNome = sanitizeTrim(form.elementoNome)" class="field-input" placeholder="Ex: Caçamba A" maxlength="200" />
        </FieldGroup>

        <h2 class="operator-form-section-title pt-4">6. Localização do E.M.</h2>

        <FieldGroup label="Município" tooltip="Município onde a atividade está sendo realizada." hint="Selecione um item da lista oficial.">
          <SearchSelect
            v-model="form.municipio"
            :opcoes="municipios"
            placeholder="Buscar município..."
          />
        </FieldGroup>

        <!-- Lógica Condicional: Na Estação ou Entre Estações -->
        <FieldGroup label="Local" tooltip="Selecione se a atividade é na estação ou entre estações." hint="Escolha uma única opção para liberar os campos corretos abaixo.">
          <select v-model="form.localTipo" class="field-input">
            <option value="">Selecione</option>
            <option value="estacao">Na Estação</option>
            <option value="entre_estacoes">Entre Estações</option>
          </select>
        </FieldGroup>

        <!-- Campos de Estação (condicional) -->
        <div v-if="form.localTipo === 'estacao'" class="space-y-4">
          <FieldGroup label="Linha CPTM" tooltip="Selecione a linha ferroviária." hint="Selecionar a linha correspondente ao local do registro.">
            <SearchSelect v-model="form.linhaCptm" :opcoes="linhasCptm" placeholder="Buscar linha..." />
          </FieldGroup>
          <FieldGroup label="Estação" tooltip="Estação onde ocorre a atividade." hint="Selecionar a estação oficial da CPTM.">
            <SearchSelect v-model="form.estacao" :opcoes="estacoesFiltradas" placeholder="Buscar estação..." />
          </FieldGroup>
        </div>

        <!-- Campos de Entre Estações (condicional) -->
        <div v-if="form.localTipo === 'entre_estacoes'" class="space-y-4">
          <FieldGroup label="Linha CPTM" tooltip="Selecione a linha ferroviária." hint="Selecionar a linha correspondente ao trecho informado.">
            <SearchSelect v-model="form.linhaCptm" :opcoes="linhasCptm" placeholder="Buscar linha..." />
          </FieldGroup>
          <FieldGroup label="Número da Via da Linha CPTM" tooltip="Selecionar ou informar a via associada ao elemento monitorado." hint="Preencher conforme a identificação operacional da via.">
            <input v-model="form.via" @blur="form.via = sanitizeTrim(form.via)" class="field-input" placeholder="Via 03 - Trecho 2" maxlength="50" />
          </FieldGroup>
          <FieldGroup label="Trecho e Sentido da Linha CPTM" tooltip="Trecho e sentido associados ao elemento monitorado." hint="Descrever o trecho com o sentido operacional adotado.">
            <input v-model="form.trechoSentido" @blur="form.trechoSentido = sanitizeTrim(form.trechoSentido)" class="field-input" maxlength="100" />
          </FieldGroup>
          <FieldGroup label="Número do Quilômetro e Poste" tooltip="Quilometragem e poste mais próximos do elemento monitorado." hint="Informar conforme o padrão de campo, sem espaços.">
            <input v-model="form.kmPoste" @input="form.kmPoste = formatKmPoste(form.kmPoste)" class="field-input" placeholder="51/02" maxlength="20" inputmode="text" autocapitalize="characters" />
          </FieldGroup>
        </div>

        <!-- GPS -->
        <h2 class="operator-form-section-title pt-4">Latitude e Longitude (Datum: WGS84)</h2>
        <OperatorMapCard
          v-if="etapaAtual === 1"
          v-model:latitude="form.latitude"
          v-model:longitude="form.longitude"
        />
      </div>

      <!-- ETAPA 3: Caracterização (DRA) -->
      <div v-show="etapaAtual === 2" class="space-y-4 operator-form-step">
        <h2 class="operator-form-section-title">7. Caracterização do E.M.</h2>
        <h3 class="operator-form-subsection-title">7.1 Regulamentação Ambiental</h3>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Tipo de Atividade (Listada)" tooltip="Selecionar o tipo de atividade relacionada ao elemento de monitoramento." hint="Escolha apenas uma opção listada. Se não existir, use o campo abaixo.">
            <select v-model="form.tipoAtividade" class="field-input">
              <option value="">Selecione</option>
              <option value="transporte">Transporte</option>
              <option value="manutencao">Manutenção</option>
              <option value="obra">Obra</option>
            </select>
          </FieldGroup>
          <FieldGroup label="Tipo de DRA (Listado)" tooltip="Tipo do Documento de Regularidade Ambiental." hint="Informar a sigla ou tipo oficial do documento.">
            <input v-model="form.tipoDra" @blur="form.tipoDra = sanitizeTrim(form.tipoDra)" class="field-input" placeholder="Outro" maxlength="50" />
          </FieldGroup>
        </div>

        <FieldGroup label="Tipo de Atividade (Não Listada)" tooltip="Descrever a atividade quando ela não constar entre as opções listadas." hint="Preencher somente se a atividade não estiver na lista anterior.">
          <input v-model="form.atividadeNaoListada" @blur="form.atividadeNaoListada = sanitizeTrim(form.atividadeNaoListada)" class="field-input" maxlength="200" />
        </FieldGroup>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Código Identificador do DRA" tooltip="Código identificador do documento de regularidade ambiental." hint="Copiar exatamente do documento, sem espaços adicionais.">
            <input v-model="form.codigoDra" @input="form.codigoDra = sanitizeCodigoDocumento(form.codigoDra)" class="field-input" placeholder="Nº 1.285.456" maxlength="50" inputmode="text" autocapitalize="characters" />
          </FieldGroup>
          <FieldGroup label="Data Validade DRA" tooltip="Data de validade do documento." hint="Selecionar a data de validade informada no documento.">
            <input v-model="form.dataValidadeDra" type="date" class="field-input" />
          </FieldGroup>
        </div>

        <h3 class="operator-form-subsection-title pt-4">7.2 Detalhamento</h3>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Tipo de Atividade na CPTM" tooltip="Tipo de atividade associada à CPTM." hint="Selecionar a categoria institucional correspondente.">
            <select v-model="form.tipoAtividadeCptm" class="field-input">
              <option value="">Selecione</option>
              <option value="empreendimento">Empreendimento</option>
              <option value="operacao">Operação</option>
              <option value="manutencao">Manutenção</option>
            </select>
          </FieldGroup>
          <FieldGroup label="Nome Edificação/Local da CPTM" tooltip="Nome da edificação ou local associado ao registro." hint="Selecionar a tipologia que mais se aproxima do local.">
            <select v-model="form.nomeEdificacao" class="field-input">
              <option value="">Selecione</option>
              <option value="estacao">Estação</option>
              <option value="patio">Pátio</option>
              <option value="oficina">Oficina</option>
            </select>
          </FieldGroup>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Origem Efluente" tooltip="De onde o efluente é originado." hint="Selecionar a origem predominante do efluente.">
            <select v-model="form.origemEfluente" class="field-input">
              <option value="">Selecione</option>
              <option value="industrial">Industrial</option>
              <option value="domestico">Doméstico</option>
              <option value="pluvial">Pluvial</option>
            </select>
          </FieldGroup>
          <FieldGroup label="Nome Edificação/Local (Complemento)" tooltip="Complemento do nome da edificação ou local." hint="Usar apenas quando necessário para detalhar o local.">
            <select v-model="form.edificacaoComplemento" class="field-input">
              <option value="">Selecione</option>
              <option value="banheiro_quimico">Banheiro químico</option>
              <option value="caixa_separadora">Caixa separadora</option>
              <option value="fossa">Fossa</option>
            </select>
          </FieldGroup>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Tipo de Destinação do Efluente" tooltip="Selecionar o tipo de destinação do efluente." hint="Selecionar a forma real de encaminhamento do efluente.">
            <select v-model="form.destinacao" class="field-input">
              <option value="">Selecione</option>
              <option value="coleta_licenciada">Coleta por empresa licenciada</option>
              <option value="rede_publica">Rede pública de esgoto</option>
              <option value="tratamento_local">Sistema de tratamento local</option>
            </select>
          </FieldGroup>
          <FieldGroup label="Fonte Geradora do Efluente" tooltip="Selecionar a fonte geradora do efluente." hint="Descrever a origem objetiva do efluente, sem texto excessivo.">
            <input v-model="form.qtdComplemento" @blur="form.qtdComplemento = sanitizeTrim(form.qtdComplemento)" class="field-input" placeholder="Ex: Sanitário, caixa separadora, lavagem" maxlength="120" />
          </FieldGroup>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Código Identificador da Guia de Remessa" tooltip="Identificador da guia de remessa, quando aplicável." hint="Preencher exatamente como na guia. Não inserir espaços.">
            <input v-model="form.codigoGuiaRemessa" @input="form.codigoGuiaRemessa = formatGuiaRemessa(form.codigoGuiaRemessa)" class="field-input" placeholder="Ex: GR-2026-00125" maxlength="80" inputmode="text" autocapitalize="characters" />
          </FieldGroup>
          <FieldGroup label="Quantidade (Litros)" tooltip="Quantidade de efluente em litros." hint="Usar apenas números. Casas decimais são permitidas.">
            <input v-model="form.quantidadeLitros" @input="form.quantidadeLitros = sanitizeDecimal(form.quantidadeLitros)" type="text" inputmode="decimal" class="field-input" placeholder="25.78" maxlength="12" />
          </FieldGroup>
        </div>

        <h3 class="operator-form-subsection-title pt-4">7.3 Transporte e Encaminhamento</h3>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Tipo de Veículo" tooltip="Tipo de veículo utilizado no transporte do efluente." hint="Selecionar o veículo efetivamente utilizado no transporte.">
            <select v-model="form.veiculoTipo" class="field-input">
              <option value="">Selecione</option>
              <option value="caminhao">Caminhão</option>
              <option value="van">Van</option>
              <option value="pickup">Pick-up</option>
            </select>
          </FieldGroup>
          <FieldGroup label="Identificador/Placa do Veículo" tooltip="Identificador ou placa do veículo transportador." hint="Usar o padrão da placa/identificador, sem espaços excedentes.">
            <input v-model="form.veiculoPlaca" @input="form.veiculoPlaca = formatPlaca(form.veiculoPlaca)" class="field-input" placeholder="ABC-1234 ou ABC1D23" maxlength="10" inputmode="text" autocapitalize="characters" />
          </FieldGroup>
        </div>

        <div class="grid grid-cols-2 gap-3">
          <FieldGroup label="Guia de Remessa / Remoção" tooltip="Número da guia de remessa ou remoção, quando aplicável." hint="Informar o número exatamente como emitido, sem espaços excedentes.">
            <input v-model="form.guiaRemocao" @input="form.guiaRemocao = formatGuiaRemessa(form.guiaRemocao)" class="field-input" maxlength="50" inputmode="text" autocapitalize="characters" placeholder="Ex: GR-2026-00125" />
          </FieldGroup>
          <FieldGroup label="Distância da Via CPTM (Metros)" tooltip="Distância da via mais próxima em relação ao efluente." hint="Informar apenas o valor numérico em metros.">
            <input v-model="form.distanciaVia" @input="form.distanciaVia = sanitizeInteiro(form.distanciaVia)" type="text" inputmode="numeric" class="field-input" maxlength="6" />
          </FieldGroup>
        </div>

        <FieldGroup label="Observações Gerais do Cadastramento" tooltip="Inserir observações relevantes do cadastramento ou da vistoria." hint="Registrar apenas informações complementares objetivas.">
          <textarea v-model="form.observacoesGerais" @blur="form.observacoesGerais = sanitizeTrim(form.observacoesGerais)" class="field-input min-h-[80px]" maxlength="2000" placeholder="Descreva..."></textarea>
        </FieldGroup>
      </div>

      <!-- ETAPA 4: Fotos / Evidências -->
      <div v-show="etapaAtual === 3" class="space-y-4 operator-form-step">
        <h2 class="operator-form-section-title">Registro Fotográfico</h2>
        <p class="text-sm" :style="{ color: 'var(--cptm-text-muted)' }">
          Inserir fotos (3x4 Paisagem) — Mínimo:
          <strong :style="{ color: 'var(--cptm-primary)' }">{{ minFotos }} fotos</strong>
        </p>

        <!-- Indicador de fotos -->
        <div class="flex items-center gap-2 px-3 py-2 rounded-lg text-sm"
             :class="fotos.length >= minFotos ? 'bg-green-500/10 text-green-600' : 'bg-amber-500/10 text-amber-600'">
          <CameraIcon class="w-4 h-4" />
          {{ fotos.length }} / {{ minFotos }} fotos
          <span v-if="fotos.length >= minFotos"> — OK</span>
        </div>

        <!-- Grid de fotos -->
        <div class="grid grid-cols-2 gap-3">
          <div v-for="(foto, i) in fotoSlots" :key="i" class="space-y-2">
            <p class="text-xs font-medium" :style="{ color: 'var(--cptm-text)' }">Fotografia {{ i + 1 }}</p>
            <div class="photo-slot" @click="abrirInputFoto(i)">
              <img v-if="foto.preview" :src="foto.preview" alt="Foto" />
              <template v-else>
                <CameraIcon class="w-8 h-8" :style="{ color: 'var(--cptm-text-muted)' }" />
                <span class="text-[10px] mt-1" :style="{ color: 'var(--cptm-text-muted)' }">Adicionar Foto</span>
              </template>
              <!-- Botão remover -->
              <button v-if="foto.preview" @click.stop="removerFoto(i)"
                      class="absolute top-1 right-1 w-6 h-6 rounded-full bg-red-500 text-white flex items-center justify-center text-xs cursor-pointer">
                <XIcon class="w-3 h-3" />
              </button>
            </div>
          </div>
        </div>

        <input ref="inputFoto" type="file" accept="image/jpeg,image/png,image/webp" capture="environment"
               class="hidden" @change="onFotoSelecionada" />

        <!-- Summary Review -->
        <div class="mt-6 p-4 rounded-xl" :style="{ backgroundColor: 'var(--cptm-surface)' }">
          <h3 class="text-sm font-bold mb-2" :style="{ color: 'var(--cptm-text)' }">Resumo</h3>
          <ul class="text-xs space-y-1" :style="{ color: 'var(--cptm-text-muted)' }">
            <li>Registro Fotográfico: {{ fotos.length >= minFotos ? 'Completo' : 'Incompleto' }}</li>
            <li>Mín fotos (3x4 Paisagem): {{ minFotos }}</li>
            <li>Fotografias inseridas: {{ fotos.length }}</li>
          </ul>
        </div>
      </div>
    </div>

    <!-- ====== FOOTER NAVIGATION ====== -->
    <div class="operator-form-footer fixed bottom-0 left-0 right-0 px-4 py-4 z-[950] shadow-[0_-4px_12px_rgba(0,0,0,0.1)]"
         :style="{ backgroundColor: 'var(--cptm-surface)' }">
      <div class="flex gap-3 max-w-lg mx-auto">
        <button v-if="etapaAtual > 0" @click="etapaAnterior"
                class="flex-1 py-3 rounded-xl font-semibold text-sm border cursor-pointer"
                :style="{ borderColor: 'var(--cptm-border)', color: 'var(--cptm-text)' }">
          Voltar
        </button>
        <button v-if="etapaAtual < steps.length - 1" @click="proximaEtapa"
                class="flex-1 py-3 rounded-xl text-white font-semibold text-sm cursor-pointer"
                :style="{ backgroundColor: 'var(--cptm-primary)' }">
          Próximo
        </button>
        <button v-if="etapaAtual === steps.length - 1" @click="finalizarFormulario"
                :disabled="!podeEnviar"
                class="flex-1 py-3 rounded-xl text-white font-semibold text-sm cursor-pointer disabled:opacity-40"
                :style="{ backgroundColor: 'var(--cptm-primary)' }">
          {{ isOnline ? 'Enviar Formulário' : 'Salvar Localmente' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, onBeforeUnmount } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore } from '@/stores/theme'
import { db } from '@/database'
import { useConnectivityStatus } from '@/services/connectivityService'
import { sincronizarPendentes } from '@/services/syncService'
import cptmLogo from '@/assets/Logo_CPTM.png'
import FieldGroup from '@/components/FieldGroup.vue'
import OperatorMapCard from '@/components/OperatorMapCard.vue'
import SearchSelect from '@/components/SearchSelect.vue'
import {
  Sun as SunIcon, Moon as MoonIcon, Check as CheckIcon, Camera as CameraIcon,
  X as XIcon
} from 'lucide-vue-next'

const router = useRouter()
const authStore = useAuthStore()
const themeStore = useThemeStore()
const { isOnline, checkConnectivity } = useConnectivityStatus()

const statusRascunho = ref('Rascunho vazio')

// ====== Stepper ======
const steps = ['Identificação', 'Local', 'Caracterização', 'Fotos']
const etapaAtual = ref(0)

function stepClass(i) {
  if (i < etapaAtual.value) return 'completed'
  if (i === etapaAtual.value) return 'active'
  return 'inactive'
}

function proximaEtapa() { if (etapaAtual.value < steps.length - 1) etapaAtual.value++ }
function etapaAnterior() { if (etapaAtual.value > 0) etapaAtual.value-- }

function sanitizeTrim(value) {
  if (typeof value !== 'string') {
    return value
  }

  return value.replace(/\s+/g, ' ').trim()
}

function sanitizeSemEspacos(value, upperCase = false) {
  if (typeof value !== 'string') {
    return value
  }

  const normalized = value.replace(/\s+/g, '')
  return upperCase ? normalized.toUpperCase() : normalized
}

function sanitizeCodigo(value, upperCase = false) {
  if (typeof value !== 'string') {
    return value
  }

  const normalized = value.replace(/\s+/g, '').replace(/[^a-zA-Z0-9./_-]/g, '')
  return upperCase ? normalized.toUpperCase() : normalized
}

function formatNumeroFormulario(value) {
  if (typeof value !== 'string') {
    return value
  }

  const cleaned = value.replace(/\s+/g, '').replace(/[^a-zA-Z0-9_-]/g, '').toUpperCase()
  const parts = cleaned.split('-').filter(Boolean)

  if (parts.length <= 1) {
    return cleaned
  }

  return `${parts[0]}-${parts.slice(1).join('')}`
}

function sanitizeCodigoDocumento(value) {
  if (typeof value !== 'string') {
    return value
  }

  return value
    .replace(/[^a-zA-Z0-9./_\-º°\s]/g, '')
    .replace(/\s+/g, ' ')
    .trimStart()
}

function sanitizePlaca(value) {
  if (typeof value !== 'string') {
    return value
  }

  return value.replace(/\s+/g, '').replace(/[^a-zA-Z0-9-]/g, '').toUpperCase()
}

function formatPlaca(value) {
  const cleaned = sanitizePlaca(value)
  const semHifen = cleaned.replace(/-/g, '')

  if (/^[A-Z]{3}\d[A-Z]\d{0,2}$/.test(semHifen)) {
    return semHifen
  }

  if (/^[A-Z]{3}\d{1,4}$/.test(semHifen)) {
    return `${semHifen.slice(0, 3)}-${semHifen.slice(3)}`
  }

  return cleaned
}

function formatGuiaRemessa(value) {
  if (typeof value !== 'string') {
    return value
  }

  const cleaned = value.replace(/\s+/g, '').replace(/[^a-zA-Z0-9/_-]/g, '').toUpperCase()
  const parts = cleaned.split('-').filter(Boolean)

  if (parts.length >= 3) {
    return `${parts[0]}-${parts[1]}-${parts.slice(2).join('')}`
  }

  if (parts.length === 2) {
    return `${parts[0]}-${parts[1]}`
  }

  if (/^[A-Z]{2}\d{5,}$/.test(cleaned)) {
    return `${cleaned.slice(0, 2)}-${cleaned.slice(2, 6)}-${cleaned.slice(6)}`
  }

  return cleaned
}

function formatKmPoste(value) {
  if (typeof value !== 'string') {
    return value
  }

  const cleaned = value.replace(/\s+/g, '').replace(/[^0-9/]/g, '')
  const [km, ...restante] = cleaned.split('/')

  if (restante.length) {
    return `${km}/${restante.join('').slice(0, 4)}`
  }

  if (km.length <= 2) {
    return km
  }

  return `${km.slice(0, -2)}/${km.slice(-2)}`
}

function sanitizeInteiro(value) {
  if (typeof value !== 'string') {
    return value
  }

  return value.replace(/\D+/g, '')
}

function sanitizeDecimal(value) {
  if (typeof value !== 'string') {
    return value
  }

  const normalized = value.replace(',', '.').replace(/[^0-9.]/g, '')
  const [inteiro, ...restante] = normalized.split('.')
  if (!restante.length) {
    return inteiro
  }

  return `${inteiro}.${restante.join('')}`
}

// ====== Dados do formulário ======
function createDefaultForm() {
  return {
  // Etapa 1 - Identificação
  contratada: 'CPTM',
  numContrato: '',
  empresa: '',
  siglaMeioAmbiente: 'GEA.OEXE',
  supervisor: '',
  autor: authStore.usuario?.nome || '',
  rt: '',
  registroProfissional: '',
  documentoRt: '',
  natureza: 'efluente',
  tipoDocumento: 'FDC',
  data: new Date().toISOString().split('T')[0],
  numero: '',

  // Etapa 2 - Local
  dataColeta: new Date().toISOString().split('T')[0],
  horaColeta: new Date().toTimeString().slice(0, 5),
  chavePrimaria: '',
  elementoNumero: '',
  elementoNome: '',
  municipio: '',
  localTipo: '', // 'estacao' ou 'entre_estacoes'
  linhaCptm: '',
  estacao: '',
  via: '',
  trechoSentido: '',
  kmPoste: '',
  latitude: null,
  longitude: null,

  // Etapa 3 - Caracterização
  tipoAtividade: '',
  tipoDra: '',
  atividadeNaoListada: '',
  codigoDra: '',
  dataValidadeDra: '',
  tipoAtividadeCptm: '',
  nomeEdificacao: '',
  origemEfluente: '',
  edificacaoComplemento: '',
  destinacao: '',
  qtdComplemento: '',
  codigoGuiaRemessa: '',
  quantidadeLitros: '',
  veiculoTipo: '',
  veiculoPlaca: '',
  guiaRemocao: '',
  distanciaVia: '',
  observacoesGerais: ''
  }
}

const form = ref(createDefaultForm())

// ====== Listas de referência ======
const linhasCptm = ref([
  { valor: '7', texto: 'Linha 07 - Rubi' },
  { valor: '8', texto: 'Linha 08 - Diamante' },
  { valor: '9', texto: 'Linha 09 - Esmeralda' },
  { valor: '10', texto: 'Linha 10 - Turquesa' },
  { valor: '11', texto: 'Linha 11 - Coral' },
  { valor: '12', texto: 'Linha 12 - Safira' },
  { valor: '13', texto: 'Linha 13 - Jade' }
])

const estacoes = ref([
  { valor: 'luz', texto: 'Luz', linha: '7' },
  { valor: 'barra_funda', texto: 'Palmeiras-Barra Funda', linha: '7' },
  { valor: 'lapa', texto: 'Lapa', linha: '7' },
  { valor: 'pirituba', texto: 'Pirituba', linha: '7' },
  { valor: 'perus', texto: 'Perus', linha: '7' },
  { valor: 'caieiras', texto: 'Caieiras', linha: '7' },
  { valor: 'franco_rocha', texto: 'Franco da Rocha', linha: '7' },
  { valor: 'francisco_morato', texto: 'Francisco Morato', linha: '7' },
  { valor: 'bras', texto: 'Brás', linha: '10' },
  { valor: 'mooca', texto: 'Mooca', linha: '10' },
  { valor: 'ipiranga', texto: 'Ipiranga', linha: '10' },
  { valor: 'santo_andre', texto: 'Santo André', linha: '10' },
  { valor: 'maua', texto: 'Mauá', linha: '10' },
  { valor: 'jardim_helena', texto: 'Jardim Helena-V. Mara', linha: '12' },
])

const estacoesFiltradas = computed(() =>
  form.value.linhaCptm ? estacoes.value.filter(e => e.linha === form.value.linhaCptm) : estacoes.value
)

const municipios = ref([
  { valor: 'sao_paulo', texto: 'São Paulo' },
  { valor: 'osasco', texto: 'Osasco' },
  { valor: 'barueri', texto: 'Barueri' },
  { valor: 'carapicuiba', texto: 'Carapicuíba' },
  { valor: 'santo_andre', texto: 'Santo André' },
  { valor: 'sao_bernardo', texto: 'São Bernardo do Campo' },
  { valor: 'maua', texto: 'Mauá' },
  { valor: 'franco_rocha', texto: 'Franco da Rocha' },
  { valor: 'caieiras', texto: 'Caieiras' },
  { valor: 'francisco_morato', texto: 'Francisco Morato' },
  { valor: 'rio_grande_serra', texto: 'Rio Grande da Serra' },
  { valor: 'ribeirao_pires', texto: 'Ribeirão Pires' },
])

function onNaturezaChange() {}

// ====== Fotos ======
const fotos = ref([])
const inputFoto = ref(null)
let fotoIndexAlvo = 0
let draftTimer = null

const minFotos = computed(() => 4)
const maxFotos = computed(() => Math.max(minFotos.value, 6))

function getDraftKey() {
  return `rascunho_operador_${authStore.usuario?.id || 'anon'}_efluente`
}

async function salvarRascunhoAgora() {
  const fotosSerializadas = await Promise.all(
    fotos.value.map(async (foto) => ({
      preview: foto.preview,
      blob: foto.blob ? await foto.blob.arrayBuffer() : null,
      type: foto.type || foto.blob?.type || 'image/jpeg',
      descricao: foto.descricao || ''
    }))
  )

  await db.cache.put({
    chave: getDraftKey(),
    dados: {
      etapaAtual: etapaAtual.value,
      form: JSON.parse(JSON.stringify(form.value)),
      fotos: fotosSerializadas,
      atualizadoEm: new Date().toISOString()
    },
    atualizadoEm: new Date().toISOString()
  })

  statusRascunho.value = 'Rascunho salvo automaticamente'
}

function agendarSalvamentoRascunho() {
  statusRascunho.value = 'Salvando rascunho...'
  clearTimeout(draftTimer)
  draftTimer = setTimeout(() => {
    salvarRascunhoAgora()
  }, 500)
}

async function restaurarRascunho() {
  const draft = await db.cache.get(getDraftKey())
  if (!draft?.dados) {
    statusRascunho.value = 'Novo formulário pronto'
    return
  }

  form.value = { ...createDefaultForm(), ...draft.dados.form }
  etapaAtual.value = draft.dados.etapaAtual ?? 0
  fotos.value = (draft.dados.fotos || []).map((foto) => ({
    preview: foto.preview,
    blob: foto.blob ? new Blob([foto.blob], { type: foto.type || 'image/jpeg' }) : null,
    type: foto.type || 'image/jpeg',
    descricao: foto.descricao || ''
  }))
  statusRascunho.value = 'Rascunho restaurado'
}

const fotoSlots = computed(() => {
  const slots = []
  for (let i = 0; i < maxFotos.value; i++) {
    slots.push(fotos.value[i] || { preview: null, blob: null })
  }
  return slots
})

function abrirInputFoto(index) {
  fotoIndexAlvo = index
  inputFoto.value?.click()
}

function onFotoSelecionada(event) {
  const file = event.target.files[0]
  if (!file) return

  // Validar extensão
  const extPermitidas = ['image/jpeg', 'image/png', 'image/webp']
  if (!extPermitidas.includes(file.type)) {
    alert('Formato não permitido. Use: JPG, PNG ou WebP.')
    return
  }

  // Validar tamanho (10MB)
  if (file.size > 10 * 1024 * 1024) {
    alert('Foto excede 10MB.')
    return
  }

  const reader = new FileReader()
  reader.onload = (e) => {
    const fotoObj = { preview: e.target.result, blob: file, type: file.type, descricao: `Foto ${fotoIndexAlvo + 1}` }
    if (fotoIndexAlvo < fotos.value.length) {
      fotos.value[fotoIndexAlvo] = fotoObj
    } else {
      fotos.value.push(fotoObj)
    }
    agendarSalvamentoRascunho()
  }
  reader.readAsDataURL(file)
  event.target.value = '' // Reset input
}

function removerFoto(index) {
  fotos.value.splice(index, 1)
  agendarSalvamentoRascunho()
}

// ====== Finalizar ======
const podeEnviar = computed(() => fotos.value.length >= minFotos.value)

async function finalizarFormulario() {
  if (!podeEnviar.value) {
    alert(`Mínimo de ${minFotos.value} fotos obrigatórias.`)
    return
  }

  // Salvar no IndexedDB (regra "Tudo ou Nada" - só salva completo)
  const formularioId = await db.formularios.add({
    tipo: form.value.natureza,
    campos: { ...form.value },
    status: 'completo',
    operadorId: authStore.usuario?.id || 0,
    criadoEm: new Date().toISOString()
  })

  // Salvar fotos no IndexedDB
  for (const foto of fotos.value) {
    if (foto.blob) {
      const arrayBuffer = await foto.blob.arrayBuffer()
      await db.fotos.add({
        formularioId,
        blob: arrayBuffer,
        descricao: foto.descricao,
        criadoEm: new Date().toISOString()
      })
    }
  }

  // Se online, tenta sincronizar imediatamente
  if (await checkConnectivity()) {
    const resultado = await sincronizarPendentes()
    if (resultado.enviados > 0) {
      alert('Formulário enviado com sucesso ao Oracle!')
    } else {
      alert('Salvo localmente. Será enviado quando a conexão voltar.')
    }
  } else {
    alert('Salvo localmente. Será enviado quando a conexão voltar.')
  }

  await db.cache.delete(getDraftKey())

  // Limpar formulário
  etapaAtual.value = 0
  fotos.value = []
  form.value = createDefaultForm()
  statusRascunho.value = 'Novo formulário pronto'
  router.push('/operador')
}

watch(form, () => {
  agendarSalvamentoRascunho()
}, { deep: true })

watch(etapaAtual, () => {
  agendarSalvamentoRascunho()
})

onMounted(async () => {
  await checkConnectivity()
  await restaurarRascunho()
})

onBeforeUnmount(() => {
  clearTimeout(draftTimer)
  salvarRascunhoAgora()
})
</script>

<style scoped>
.operator-form-page {
  background:
    linear-gradient(180deg, color-mix(in srgb, var(--cptm-bg) 82%, #ffffff 18%) 0%, var(--cptm-bg) 100%);
}

.operator-form-content {
  position: relative;
  z-index: 1;
}

.operator-form-draft-banner {
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 1rem;
  padding: 0.95rem 1rem;
  border-radius: 1rem;
  border: 1px solid var(--cptm-border);
  background: var(--cptm-surface);
}

.operator-form-draft-title {
  margin: 0;
  font-size: 0.86rem;
  font-weight: 800;
  color: var(--cptm-text);
}

.operator-form-draft-text {
  margin: 0.25rem 0 0;
  font-size: 0.76rem;
  color: var(--cptm-text-muted);
}

.operator-form-draft-status {
  white-space: nowrap;
  font-size: 0.76rem;
  font-weight: 700;
  color: var(--cptm-primary);
}

.operator-form-step {
  border: 1px solid var(--cptm-border);
  border-radius: 1.25rem;
  background: var(--cptm-surface);
  padding: 1.25rem;
  box-shadow: 0 18px 36px rgba(65, 54, 35, 0.08), 0 2px 6px rgba(0, 0, 0, 0.04);
}

.operator-form-step > .grid {
  grid-template-columns: minmax(0, 1fr);
}

.operator-form-cover-card {
  display: grid;
  grid-template-columns: auto 1fr;
  gap: 1rem;
  align-items: center;
  padding: 1rem;
  border: 1px solid color-mix(in srgb, var(--cptm-primary) 18%, var(--cptm-border) 82%);
  border-radius: 1rem;
  background:
    linear-gradient(135deg, color-mix(in srgb, var(--cptm-primary) 8%, var(--cptm-surface) 92%) 0%, var(--cptm-surface) 100%);
}

.operator-form-cover-logo {
  width: 4rem;
  height: 4rem;
  object-fit: contain;
  border-radius: 1rem;
  background: #fff;
  padding: 0.5rem;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.08);
}

.operator-form-cover-title {
  margin: 0;
  font-size: 0.94rem;
  font-weight: 800;
  color: var(--cptm-text);
}

.operator-form-cover-text {
  margin: 0.18rem 0 0;
  font-size: 0.78rem;
  color: var(--cptm-text-muted);
}

.operator-form-glossary-card {
  padding: 0.95rem 1rem;
  border-radius: 1rem;
  border: 1px dashed color-mix(in srgb, var(--cptm-primary) 30%, var(--cptm-border) 70%);
  background: color-mix(in srgb, var(--cptm-primary) 5%, var(--cptm-surface) 95%);
}

.operator-form-glossary-title {
  margin: 0;
  font-size: 0.78rem;
  font-weight: 800;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  color: var(--cptm-primary);
}

.operator-form-glossary-text {
  margin: 0.35rem 0 0;
  font-size: 0.78rem;
  line-height: 1.5;
  color: var(--cptm-text-muted);
}

.operator-form-section-title {
  display: flex;
  align-items: center;
  gap: 0.75rem;
  margin: 0 0 0.7rem;
  min-height: 1.9rem;
  font-size: 1rem;
  font-weight: 800;
  line-height: 1.3;
  color: var(--cptm-text);
}

.operator-form-section-title::before {
  content: '';
  width: 4px;
  height: 1.55rem;
  border-radius: 999px;
  background: var(--cptm-primary);
  flex: 0 0 auto;
}

.operator-form-subsection-title {
  margin: 0 0 0.45rem;
  font-size: 0.88rem;
  font-weight: 800;
  line-height: 1.35;
  color: var(--cptm-text);
}

.operator-form-footer {
  border-top: 1px solid color-mix(in srgb, var(--cptm-border) 82%, #ffffff 18%);
  backdrop-filter: blur(14px);
}

@media (max-width: 640px) {
  .operator-form-draft-banner {
    flex-direction: column;
    align-items: flex-start;
  }

  .operator-form-cover-card {
    grid-template-columns: 1fr;
  }

  .operator-form-cover-logo {
    width: 3.5rem;
    height: 3.5rem;
  }

  .operator-form-step {
    padding: 1rem;
    border-radius: 1rem;
  }

  .operator-form-step > .grid {
    gap: 0.85rem;
  }
}

@media (min-width: 640px) {
  .operator-form-step > .grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}
</style>
