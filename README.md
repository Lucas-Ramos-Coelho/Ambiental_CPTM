# Projeto CPTM - Semana 1

## Objetivo da Semana 1
A primeira semana do projeto teve como foco a preparação do ambiente de desenvolvimento e a validação de uma conexão mínima com banco de dados Oracle, conforme o planejamento definido no cronograma do projeto. 

Menbros:

Hellen 
Pedro 
Davi
Lucas

## Escopo realizado
Nesta etapa, foram executadas as seguintes atividades:

- validação das ferramentas principais do ambiente
- instalação e teste do .NET SDK
- criação de um projeto back-end mínimo em ASP.NET Core
- configuração de uma API básica com rotas de teste
- criação de uma interface front-end simples em HTML, CSS e JavaScript
- validação de consultas e gravação de dados em Oracle
- teste de fluxo mínimo entre front-end, back-end e banco de dados

## Tecnologias utilizadas
- .NET SDK
- ASP.NET Core Minimal API
- Oracle.ManagedDataAccess.Core
- HTML
- CSS
- JavaScript
- Oracle FreeSQL

## Estrutura implementada
O projeto foi organizado com uma estrutura mínima para demonstrar o funcionamento inicial da solução:

- `backend/Cptm.Api` → API em .NET
- `frontend/index.html` → interface simples para cadastro de nomes
- `docs` → documentação da etapa
- `evidencias/semana1` → prints e comprovações de funcionamento

## Funcionalidade implementada
Foi desenvolvido um fluxo mínimo funcional com o objetivo de validar a arquitetura inicial da aplicação:

1. o usuário digita um nome na interface
2. o front-end envia a informação para a API .NET
3. a API realiza a gravação no banco Oracle
4. os dados cadastrados são listados em tela

Esse fluxo foi implementado como prova de conceito da integração inicial entre as camadas da aplicação.

## Banco de dados Oracle
Para esta primeira semana, foi utilizada uma estrutura simples de tabela para testes:

- tabela de nomes
- inserção de registros
- consulta dos dados cadastrados

## Motivo do uso do Oracle no navegador neste momento
Durante a execução da Semana 1, optou-se por utilizar o Oracle em ambiente de navegador, por meio do Oracle FreeSQL, em vez de uma instalação local completa do Oracle Database.

Essa decisão foi tomada pelos seguintes motivos:

- o cronograma do projeto exige uso de Oracle, portanto a tecnologia principal foi mantida
- a instalação local do Oracle XE apresentou alto consumo de recursos na máquina utilizada
- também ocorreram falhas técnicas durante a tentativa de instalação local
- para não comprometer o andamento da entrega da Semana 1, foi adotada uma alternativa oficial e mais leve, utilizando Oracle em ambiente browser-based
- essa abordagem permitiu validar o uso do Oracle, executar consultas, criar tabela de teste, inserir dados e comprovar a integração mínima necessária para a etapa

Dessa forma, a stack definida no projeto foi preservada, sem substituição por outro banco, mantendo aderência ao planejamento original.

## Justificativa técnica
A utilização do Oracle em navegador nesta fase foi adequada porque o objetivo principal da Semana 1 não era a construção completa do sistema, mas sim:

- configurar o ambiente
- validar ferramentas
- garantir uma conexão mínima com Oracle
- comprovar que o projeto já possui base funcional para evolução nas próximas semanas

Assim, a solução adotada permitiu cumprir o objetivo da etapa sem comprometer a continuidade do projeto.

## Resultados obtidos
Ao final da Semana 1, foi possível validar que:

- o ambiente .NET está funcional
- o back-end mínimo está criado e executando
- o front-end mínimo está criado
- o Oracle está acessível e operacional
- a API consegue consultar e gravar dados
- a aplicação já demonstra um fluxo básico entre interface, serviço e banco

## Próximos passos
Nas próximas semanas, o projeto será evoluído de acordo com o cronograma:

- ampliação da API em .NET Core
- evolução do front-end para Vue.js
- implementação das funcionalidades de coleta offline
- sincronização de dados
- autenticação e controle de acesso
- ajustes finais e consultas Oracle

## Evidências
As evidências desta etapa estão disponíveis na pasta:

`evidencias/semana1`

## Conclusão
A Semana 1 foi concluída com sucesso, atendendo ao objetivo de fundação do ambiente e teste inicial com Oracle. Mesmo utilizando o Oracle em navegador nesta fase, foi possível manter aderência à tecnologia definida no projeto e demonstrar uma integração mínima funcional entre front-end, back-end e banco de dados.