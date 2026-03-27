<<<<<<< HEAD
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Cptm.Api.DTOs;
using Cptm.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
=======
>>>>>>> 1248e0acbeb7847ddd41678e923e753c890644a0
using Oracle.ManagedDataAccess.Client;

var builder = WebApplication.CreateBuilder(args);

<<<<<<< HEAD
// ====== JWT Authentication ======
var jwtKey = builder.Configuration["Jwt:SecretKey"]
    ?? throw new InvalidOperationException("Jwt:SecretKey não configurada.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CptmAmbiental";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

// ====== CORS (Restrito ao frontend) ======
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:4173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ====== Serviços ======
builder.Services.AddSingleton<JwtService>();
builder.Services.AddAntiforgery();

var app = builder.Build();

// ====== Middleware (ordem importa) ======
app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

// Limitar tamanho do request body (proteção contra upload abusivo)
app.Use(async (context, next) =>
{
    // Segurança: Header para prevenir XSS, clickjacking
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});

var connectionString = builder.Configuration.GetConnectionString("OracleConnection")
    ?? throw new InvalidOperationException("Connection string OracleConnection não encontrada.");

// ============================================================
// ENDPOINTS
// ============================================================

// ====== Health Check ======
app.MapGet("/", () => Results.Ok(new { status = "API CPTM Ambiental rodando", versao = "2.0" }));

// ====== AUTH: Login ======
app.MapPost("/api/auth/login", async (LoginRequest request) =>
{
    // Validação de input
    if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Senha))
        return Results.BadRequest(new { mensagem = "E-mail e senha são obrigatórios." });

    // Sanitiza o e-mail (previne XSS)
    var emailLimpo = request.Email.Trim().ToLowerInvariant();
    if (emailLimpo.Length > 200)
        return Results.BadRequest(new { mensagem = "E-mail inválido." });

    using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ID, NOME, EMAIL, SENHA_HASH, PERFIL FROM USUARIOS WHERE LOWER(EMAIL) = :email AND ATIVO = 1";
    cmd.Parameters.Add(new OracleParameter("email", emailLimpo));

    using var reader = await cmd.ExecuteReaderAsync();
    if (!await reader.ReadAsync())
        return Results.Unauthorized(); // Mensagem genérica (OWASP: não revelar se é email ou senha)

    var id = Convert.ToInt32(reader["ID"]);
    var nome = reader["NOME"]?.ToString() ?? "";
    var email = reader["EMAIL"]?.ToString() ?? "";
    var senhaHash = reader["SENHA_HASH"]?.ToString() ?? "";
    var perfil = reader["PERFIL"]?.ToString() ?? "operador";

    // Verificar senha com SHA256 (em produção usar BCrypt)
    var senhaHashInput = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Senha))).ToLowerInvariant();
    if (senhaHash != senhaHashInput)
        return Results.Unauthorized();

    // Gerar JWT
    var jwtService = app.Services.GetRequiredService<JwtService>();
    var (jwtToken, expiraEm) = jwtService.GerarToken(id, email, perfil);

    // Registrar sessão no Oracle
    using var cmdSessao = conn.CreateCommand();
    cmdSessao.CommandText = "INSERT INTO SESSOES (USUARIO_ID, TOKEN, CRIADO_EM, EXPIRA_EM) VALUES (:p_usuario_id, :p_token, SYSTIMESTAMP, :p_expira_em)";
    cmdSessao.Parameters.Add(new OracleParameter("p_usuario_id", id));
    cmdSessao.Parameters.Add(new OracleParameter("p_token", jwtToken));
    cmdSessao.Parameters.Add(new OracleParameter("p_expira_em", expiraEm));
    await cmdSessao.ExecuteNonQueryAsync();

    return Results.Ok(new LoginResponse
    {
        Id = id,
        Nome = nome,
        Email = email,
        Perfil = perfil,
        Token = jwtToken,
        HashOffline = senhaHashInput, // Para validação offline no IndexedDB
        ExpiraEm = expiraEm
    });
});

// ====== FORMULÁRIOS: Criar (Operador + Supervisor) — grava na PT_EFLUENTE ======
app.MapPost("/api/formularios", [Authorize] async (HttpContext ctx) =>
{
    var userIdClaim = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var userEmail = ctx.User.FindFirst(ClaimTypes.Email)?.Value ?? "";
    if (string.IsNullOrEmpty(userIdClaim))
        return Results.Unauthorized();

    var form = await ctx.Request.ReadFormAsync();
    var dadosJson = form["dados"].FirstOrDefault();
    if (string.IsNullOrWhiteSpace(dadosJson))
        return Results.BadRequest(new { mensagem = "Dados do formulário são obrigatórios." });

    if (dadosJson.Length > 50_000)
        return Results.BadRequest(new { mensagem = "Payload excede o tamanho máximo permitido." });

    FormularioRequest? req;
    try
    {
        req = JsonSerializer.Deserialize<FormularioRequest>(dadosJson, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }
    catch
    {
        return Results.BadRequest(new { mensagem = "JSON de dados inválido." });
    }

    if (req == null)
        return Results.BadRequest(new { mensagem = "Dados do formulário inválidos." });

    // Validar fotos: mínimo 4, se arvore: 6
    var fotos = form.Files.Where(f => f.Name == "fotos").ToList();
    var tipo = (req.Natureza ?? "efluente").ToLowerInvariant();
    var minFotos = tipo == "arvore" ? 6 : 4;
    if (fotos.Count < minFotos)
        return Results.BadRequest(new { mensagem = $"Tipo '{tipo}' exige no mínimo {minFotos} fotos. Enviadas: {fotos.Count}" });

    // Validar extensões de fotos (OWASP: prevenir upload malicioso)
    var extensoesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
    var maxTamanhoFoto = 10 * 1024 * 1024;
    foreach (var foto in fotos)
    {
        var ext = Path.GetExtension(foto.FileName).ToLowerInvariant();
        if (!extensoesPermitidas.Contains(ext))
            return Results.BadRequest(new { mensagem = $"Extensão '{ext}' não permitida. Use: jpg, jpeg, png, webp." });
        if (foto.Length > maxTamanhoFoto)
            return Results.BadRequest(new { mensagem = "Foto excede 10MB." });
        if (foto.Length == 0)
            return Results.BadRequest(new { mensagem = "Foto com tamanho zero não é permitida." });
    }

    // Gerar PK — "EFL-{timestamp}-{random}"
    var pk = req.ChavePrimaria;
    if (string.IsNullOrWhiteSpace(pk))
        pk = $"EFL-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8]}";

    using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    // ── INSERT na PT_EFLUENTE ──
    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        INSERT INTO PT_EFLUENTE (
            PK_CD_MEIO_AMBIENTE_CPTM,
            TX_NATUREZA_DO_PGA,
            TX_TIPO_DE_FORMULARIO,
            DT_DATA_EMISSAO_FORMULARIO,
            NR_NUMERO_DE_FORMULARIO,
            TX_AUTOR_PF_DO_FORMULARIO,
            TX_SIGLA_DEPTO_MEIO_AMBIENTE,
            TX_NOME_PJ_DA_CONTRATADA,
            TX_NR_CONTRATO_CONTRATADA,
            TX_NOME_PJ_EXECUTORA,
            TX_NOME_PJ_DA_SUPERVISORA,
            TX_NM_RESPONSAVEL_CADASTRO,
            TX_RP_RESPONSAVEL_CADASTRO,
            TX_DRT_RESPONSAVEL_CADASTRO,
            TX_NR_ELEMENTO_MONITORAMENTO,
            TX_NM_ELEMENTO_MONITORAMENTO,
            TX_MUNICIPIO,
            TX_LINHA_CPTM,
            TX_ESTACAO_CPTM,
            TX_VIA_CPTM,
            TX_TRECHO_E_SENTIDO_CPTM,
            TX_KM_POSTE,
            NR_LAT_GRAU_DECIMAL_WGS84,
            NR_LONG_GRAU_DECIMAL_WGS84,
            GEOM,
            TX_TIPO_ATIVIDADE_LISTADA,
            TX_TIPO_ATIVIDADE_N_LISTADA,
            TX_TIPO_DRA_LISTADO,
            TX_ID_DRA,
            DT_VALIDADE_DRA,
            TX_TIPO_ATIVIDADE_CPTM,
            TX_NM_LOCAL_ATIV,
            TX_NM_LOCAL_ATIV_COMPLEMENTO,
            TX_ORIGEM_EFLUENTE,
            TX_FONTE_GERADORA,
            TX_TIPO_DESTINACAO,
            NR_QUANTIDADE_L,
            TX_TIPO_VEICULO,
            TX_ID_VEICULO,
            TX_ID_GUIA_REMESSA,
            NR_DISTANCIA_DA_VIA_M,
            TX_OFERECE_RISCO_SISTEMA_CPTM,
            TX_OBS_CADASTRAMENTO,
            DT_DATA_DO_CADASTRAMENTO,
            HR_HORA_DO_CADASTRAMENTO,
            TX_AUTOR_PF_DO_CADASTRO,
            TX_STATUS_DO_REGISTRO_NO_BD,
            TX_NOME_FOTO_01,
            TX_NOME_FOTO_02,
            TX_NOME_FOTO_03,
            TX_NOME_FOTO_04
        ) VALUES (
            :pk, :natureza, :tipoDoc, TO_DATE(:dataForm, 'YYYY-MM-DD'), :numero,
            :autor, :sigla, :contratada, :numContrato, :empresa,
            :supervisor, :responsavel, :rt, :registro,
            :elemNum, :elemNome, :municipio, :linha, :estacao,
            :via, :trecho, :km,
            :lat, :lng,
            SDO_GEOMETRY(2001, 4326, SDO_POINT_TYPE(:lng2, :lat2, NULL), NULL, NULL),
            :tipoAtiv, :ativNaoList, :tipoDra, :codDra,
            TO_DATE(:dataValDra, 'YYYY-MM-DD'),
            :tipoAtivCptm, :nomeEdif, :edifCompl, :origemEfl,
            :fonteGer, :destinacao, :qtdLitros,
            :veicTipo, :veicPlaca, :guia, :distancia,
            :commodities, :obs,
            TO_DATE(:dataCad, 'YYYY-MM-DD'), :horaCad,
            :autorCad, 'ativo',
            :foto01, :foto02, :foto03, :foto04
        )";

    cmd.Parameters.Add(new OracleParameter("pk", pk));
    cmd.Parameters.Add(new OracleParameter("natureza", (object?)req.Natureza ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("tipoDoc", (object?)req.TipoDocumento ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("dataForm", (object?)req.Data ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("numero", req.Numero != null && double.TryParse(req.Numero, out var nrForm) ? (object)nrForm : DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("autor", (object?)req.Autor ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("sigla", (object?)req.SiglaMeioAmbiente ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("contratada", (object?)req.Contratada ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("numContrato", (object?)req.NumContrato ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("empresa", (object?)req.Empresa ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("supervisor", (object?)req.Supervisor ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("responsavel", (object?)req.Autor ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("rt", (object?)req.Rt ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("registro", (object?)req.RegistroProfissional ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("elemNum", (object?)req.ElementoNumero ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("elemNome", (object?)req.ElementoNome ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("municipio", (object?)req.Municipio ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("linha", (object?)req.LinhaCptm ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("estacao", (object?)req.Estacao ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("via", (object?)req.Via ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("trecho", (object?)req.TrechoSentido ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("km", (object?)req.KmPoste ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("lat", req.Latitude.HasValue ? (object)req.Latitude.Value : DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("lng", req.Longitude.HasValue ? (object)req.Longitude.Value : DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("lng2", req.Longitude ?? 0));
    cmd.Parameters.Add(new OracleParameter("lat2", req.Latitude ?? 0));
    cmd.Parameters.Add(new OracleParameter("tipoAtiv", (object?)req.TipoAtividade ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("ativNaoList", (object?)req.AtividadeNaoListada ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("tipoDra", (object?)req.TipoDra ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("codDra", (object?)req.CodigoDra ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("dataValDra", (object?)req.DataValidadeDra ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("tipoAtivCptm", (object?)req.TipoAtividadeCptm ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("nomeEdif", (object?)req.NomeEdificacao ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("edifCompl", (object?)req.EdificacaoComplemento ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("origemEfl", (object?)req.OrigemEfluente ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("fonteGer", (object?)req.QtdComplemento ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("destinacao", (object?)req.Destinacao ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("qtdLitros", req.QuantidadeLitros.HasValue ? (object)req.QuantidadeLitros.Value : DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("veicTipo", (object?)req.VeiculoTipo ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("veicPlaca", (object?)req.VeiculoPlaca ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("guia", (object?)req.GuiaRemocao ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("distancia", req.DistanciaVia.HasValue ? (object)req.DistanciaVia.Value : DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("commodities", (object?)req.Commodities ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("obs", (object?)req.Observacoes ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("dataCad", req.DataColeta ?? DateTime.UtcNow.ToString("yyyy-MM-dd")));
    cmd.Parameters.Add(new OracleParameter("horaCad", (object?)req.HoraColeta ?? DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("autorCad", (object?)userEmail ?? DBNull.Value));

    // Nomes das fotos nas colunas TX_NOME_FOTO_01..04
    cmd.Parameters.Add(new OracleParameter("foto01", fotos.Count > 0 ? (object)fotos[0].FileName : DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("foto02", fotos.Count > 1 ? (object)fotos[1].FileName : DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("foto03", fotos.Count > 2 ? (object)fotos[2].FileName : DBNull.Value));
    cmd.Parameters.Add(new OracleParameter("foto04", fotos.Count > 3 ? (object)fotos[3].FileName : DBNull.Value));

    await cmd.ExecuteNonQueryAsync();

    // ── INSERT fotos como BLOB na RT_EFLUENTE ──
    for (int i = 0; i < fotos.Count; i++)
    {
        var foto = fotos[i];
        // Gerar ATTACHMENTID sequencial
        using var cmdSeq = conn.CreateCommand();
        cmdSeq.CommandText = "SELECT COALESCE(MAX(ATTACHMENTID), 0) + 1 FROM RT_EFLUENTE";
        var nextId = Convert.ToInt32(await cmdSeq.ExecuteScalarAsync());

        using var ms = new MemoryStream();
        await foto.CopyToAsync(ms);
        var bytes = ms.ToArray();

        using var cmdFoto = conn.CreateCommand();
        cmdFoto.CommandText = @"
            INSERT INTO RT_EFLUENTE (ATTACHMENTID, REL_OBJECTID, CONTENT_TYPE, ATT_NAME, DATA_SIZE, DATA)
            VALUES (:attId, :relId, :ct, :attName, :dataSize, :blobData)";
        cmdFoto.Parameters.Add(new OracleParameter("attId", nextId));
        cmdFoto.Parameters.Add(new OracleParameter("relId", pk));
        cmdFoto.Parameters.Add(new OracleParameter("ct", foto.ContentType));
        cmdFoto.Parameters.Add(new OracleParameter("attName", foto.FileName));
        cmdFoto.Parameters.Add(new OracleParameter("dataSize", bytes.Length));
        cmdFoto.Parameters.Add(new OracleParameter("blobData", OracleDbType.Blob) { Value = bytes });
        await cmdFoto.ExecuteNonQueryAsync();
    }

    return Results.Created($"/api/formularios/{pk}", new { pk, mensagem = "Formulário salvo com sucesso." });
});

// ====== FORMULÁRIOS: Listar (Autenticado) — lê da PT_EFLUENTE ======
app.MapGet("/api/formularios", [Authorize] async (HttpContext ctx, string? tipo, string? dataInicio, string? dataFim) =>
{
    var perfil = ctx.User.FindFirst(ClaimTypes.Role)?.Value ?? "operador";
    var userEmail = ctx.User.FindFirst(ClaimTypes.Email)?.Value ?? "";

    using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    var sql = new StringBuilder(@"
        SELECT p.PK_CD_MEIO_AMBIENTE_CPTM,
               p.TX_NATUREZA_DO_PGA,
               p.TX_MUNICIPIO,
               p.TX_LINHA_CPTM,
               p.TX_ESTACAO_CPTM,
               p.TX_AUTOR_PF_DO_CADASTRO,
               p.NR_LAT_GRAU_DECIMAL_WGS84,
               p.NR_LONG_GRAU_DECIMAL_WGS84,
               p.TX_STATUS_DO_REGISTRO_NO_BD,
               p.DT_DATA_DO_CADASTRAMENTO,
               p.TX_NOME_PJ_DA_CONTRATADA,
               p.TX_ORIGEM_EFLUENTE,
               p.NR_QUANTIDADE_L,
               (SELECT COUNT(*) FROM RT_EFLUENTE r WHERE r.REL_OBJECTID = p.PK_CD_MEIO_AMBIENTE_CPTM) AS QTD_FOTOS
        FROM PT_EFLUENTE p
        WHERE (p.TX_STATUS_DO_REGISTRO_NO_BD IS NULL OR p.TX_STATUS_DO_REGISTRO_NO_BD != 'excluido')");

    var parametros = new List<OracleParameter>();

    // Operador só vê os próprios registros
    if (perfil != "supervisor")
    {
        sql.Append(" AND LOWER(p.TX_AUTOR_PF_DO_CADASTRO) = :email");
        parametros.Add(new OracleParameter("email", userEmail.ToLowerInvariant()));
    }

    if (!string.IsNullOrWhiteSpace(tipo))
    {
        sql.Append(" AND LOWER(p.TX_NATUREZA_DO_PGA) = :tipo");
        parametros.Add(new OracleParameter("tipo", tipo.Trim().ToLowerInvariant()));
    }

    sql.Append(" ORDER BY p.DT_DATA_DO_CADASTRAMENTO DESC NULLS LAST");

    using var cmd = conn.CreateCommand();
    cmd.CommandText = sql.ToString();
    foreach (var p in parametros) cmd.Parameters.Add(p);

    var resultados = new List<FormularioResponse>();
    using var reader = await cmd.ExecuteReaderAsync();
    while (await reader.ReadAsync())
    {
        resultados.Add(new FormularioResponse
        {
            Pk = reader["PK_CD_MEIO_AMBIENTE_CPTM"]?.ToString() ?? "",
            Natureza = reader["TX_NATUREZA_DO_PGA"]?.ToString(),
            Municipio = reader["TX_MUNICIPIO"]?.ToString(),
            LinhaCptm = reader["TX_LINHA_CPTM"]?.ToString(),
            Estacao = reader["TX_ESTACAO_CPTM"]?.ToString(),
            Autor = reader["TX_AUTOR_PF_DO_CADASTRO"]?.ToString(),
            Latitude = reader["NR_LAT_GRAU_DECIMAL_WGS84"] == DBNull.Value ? null : Convert.ToDouble(reader["NR_LAT_GRAU_DECIMAL_WGS84"]),
            Longitude = reader["NR_LONG_GRAU_DECIMAL_WGS84"] == DBNull.Value ? null : Convert.ToDouble(reader["NR_LONG_GRAU_DECIMAL_WGS84"]),
            Status = reader["TX_STATUS_DO_REGISTRO_NO_BD"]?.ToString() ?? "ativo",
            DataCadastro = reader["DT_DATA_DO_CADASTRAMENTO"] == DBNull.Value ? null : Convert.ToDateTime(reader["DT_DATA_DO_CADASTRAMENTO"]),
            QtdFotos = Convert.ToInt32(reader["QTD_FOTOS"]),
            Contratada = reader["TX_NOME_PJ_DA_CONTRATADA"]?.ToString(),
            OrigemEfluente = reader["TX_ORIGEM_EFLUENTE"]?.ToString(),
            QuantidadeLitros = reader["NR_QUANTIDADE_L"] == DBNull.Value ? null : Convert.ToDouble(reader["NR_QUANTIDADE_L"])
        });
    }

    return Results.Ok(resultados);
});

// ====== FORMULÁRIOS: Excluir (APENAS Supervisor) — soft delete na PT_EFLUENTE ======
app.MapDelete("/api/formularios/{pk}", [Authorize(Roles = "supervisor")] async (string pk, HttpContext ctx) =>
{
    if (string.IsNullOrWhiteSpace(pk) || pk.Length > 255)
        return Results.BadRequest(new { mensagem = "PK inválida." });

    using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = conn.CreateCommand();
    cmd.CommandText = @"
        UPDATE PT_EFLUENTE SET TX_STATUS_DO_REGISTRO_NO_BD = 'excluido'
        WHERE PK_CD_MEIO_AMBIENTE_CPTM = :pk AND (TX_STATUS_DO_REGISTRO_NO_BD IS NULL OR TX_STATUS_DO_REGISTRO_NO_BD != 'excluido')";
    cmd.Parameters.Add(new OracleParameter("pk", pk));

    var rows = await cmd.ExecuteNonQueryAsync();
    return rows > 0
        ? Results.Ok(new { mensagem = "Registro excluído com sucesso." })
        : Results.NotFound(new { mensagem = "Registro não encontrado ou já excluído." });
});

// ====== REFERÊNCIAS: Listas para cache no front ======
app.MapGet("/api/referencia/linhas", [Authorize] async () =>
{
    // Dados de referência das linhas CPTM
    return Results.Ok(new[]
    {
        new { Id = 7, Nome = "Linha 07 - Rubi", Cor = "#CA016B" },
        new { Id = 8, Nome = "Linha 08 - Diamante", Cor = "#97999B" },
        new { Id = 9, Nome = "Linha 09 - Esmeralda", Cor = "#01A9A7" },
        new { Id = 10, Nome = "Linha 10 - Turquesa", Cor = "#008DA5" },
        new { Id = 11, Nome = "Linha 11 - Coral", Cor = "#F68B1F" },
        new { Id = 12, Nome = "Linha 12 - Safira", Cor = "#083E8D" },
        new { Id = 13, Nome = "Linha 13 - Jade", Cor = "#00843D" }
    });
});

app.MapGet("/api/referencia/estacoes", [Authorize] async () =>
{
    return Results.Ok(new[]
    {
        new { Id = 1, Nome = "Luz", LinhaId = 7 },
        new { Id = 2, Nome = "Palmeiras-Barra Funda", LinhaId = 7 },
        new { Id = 3, Nome = "Água Branca", LinhaId = 7 },
        new { Id = 4, Nome = "Lapa", LinhaId = 7 },
        new { Id = 5, Nome = "Piqueri", LinhaId = 7 },
        new { Id = 6, Nome = "Pirituba", LinhaId = 7 },
        new { Id = 7, Nome = "Vila Clarice", LinhaId = 7 },
        new { Id = 8, Nome = "Jaraguá", LinhaId = 7 },
        new { Id = 9, Nome = "Vila Aurora", LinhaId = 7 },
        new { Id = 10, Nome = "Perus", LinhaId = 7 },
        new { Id = 11, Nome = "Caieiras", LinhaId = 7 },
        new { Id = 12, Nome = "Franco da Rocha", LinhaId = 7 },
        new { Id = 13, Nome = "Baltazar Fidélis", LinhaId = 7 },
        new { Id = 14, Nome = "Francisco Morato", LinhaId = 7 },
        new { Id = 15, Nome = "Brás", LinhaId = 10 },
        new { Id = 16, Nome = "Mooca", LinhaId = 10 },
        new { Id = 17, Nome = "Ipiranga", LinhaId = 10 },
        new { Id = 18, Nome = "Tamanduateí", LinhaId = 10 },
        new { Id = 19, Nome = "São Caetano do Sul", LinhaId = 10 },
        new { Id = 20, Nome = "Santo André", LinhaId = 10 },
        new { Id = 21, Nome = "Mauá", LinhaId = 10 },
        new { Id = 22, Nome = "Ribeirão Pires", LinhaId = 10 },
        new { Id = 23, Nome = "Rio Grande da Serra", LinhaId = 10 },
        new { Id = 24, Nome = "Jardim Helena-V. Mara", LinhaId = 12 },
        new { Id = 25, Nome = "Itaim Paulista", LinhaId = 12 },
        new { Id = 26, Nome = "Comendador Ermelino", LinhaId = 12 },
        new { Id = 27, Nome = "São Miguel Paulista", LinhaId = 12 },
        new { Id = 28, Nome = "Engenheiro Goulart", LinhaId = 12 },
        new { Id = 29, Nome = "USP Leste", LinhaId = 12 },
        new { Id = 30, Nome = "Calmon Viana", LinhaId = 11 }
    });
});

app.MapGet("/api/referencia/naturezas", [Authorize] async () =>
{
    return Results.Ok(new[]
    {
        new { Id = 1, Nome = "Efluente", Tipo = "efluente" },
        new { Id = 2, Nome = "Vegetação / Árvore Isolada", Tipo = "arvore" },
        new { Id = 3, Nome = "Fauna", Tipo = "fauna" },
        new { Id = 4, Nome = "Erosão / Movimento de Massa", Tipo = "erosao" },
        new { Id = 5, Nome = "Resíduos Sólidos", Tipo = "residuo" }
    });
});

// ====== Endpoint legado da Semana 1 (pode remover depois) ======
app.MapGet("/api/nomes", async () =>
{
    var nomes = new List<object>();
    using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ID, NOME FROM TESTE_SEMANA1 ORDER BY ID";
    using var reader = await cmd.ExecuteReaderAsync();
=======
builder.Services.AddCors(options =>
{
    options.AddPolicy("liberado", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.MapGet("/teste-oracle", async (IConfiguration config) => {
    var connectionString = config.GetConnectionString("OracleConnection");
    using var connection = new Oracle.ManagedDataAccess.Client.OracleConnection(connectionString);
    try {
        await connection.OpenAsync();
        return Results.Ok(new { mensagem = "Conexão com Oracle Spatial: SUCESSO!", versao = connection.ServerVersion });
    }
    catch (Exception ex) {
        return Results.Problem($"Erro ao conectar no Oracle: {ex.Message}");
    }
});


app.UseCors("liberado");

string connectionString = builder.Configuration.GetConnectionString("OracleConnection")
    ?? throw new InvalidOperationException("Connection string não encontrada.");

app.MapGet("/", () => Results.Ok("API rodando"));

app.MapGet("/nomes", async () =>
{
    var nomes = new List<object>();

    using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT ID, NOME FROM TESTE_SEMANA1 ORDER BY ID";

    using var reader = await cmd.ExecuteReaderAsync();

>>>>>>> 1248e0acbeb7847ddd41678e923e753c890644a0
    while (await reader.ReadAsync())
    {
        nomes.Add(new
        {
            Id = reader["ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ID"]),
            Nome = reader["NOME"]?.ToString()
        });
    }
<<<<<<< HEAD
    return Results.Ok(nomes);
});

app.Run();
=======

    return Results.Ok(nomes);
});

app.MapPost("/nomes", async (NomeRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome))
        return Results.BadRequest(new { mensagem = "Nome é obrigatório." });

    using var conn = new OracleConnection(connectionString);
    await conn.OpenAsync();

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "INSERT INTO TESTE_SEMANA1 (NOME) VALUES (:nome)";
    cmd.Parameters.Add(new OracleParameter("nome", request.Nome));

    await cmd.ExecuteNonQueryAsync();

    using var cmdSelect = conn.CreateCommand();
    cmdSelect.CommandText = "SELECT ID, NOME FROM TESTE_SEMANA1 ORDER BY ID";

    var nomes = new List<object>();
    using var reader = await cmdSelect.ExecuteReaderAsync();

    while (await reader.ReadAsync())
    {
        nomes.Add(new
        {
            Id = reader["ID"] == DBNull.Value ? (int?)null : Convert.ToInt32(reader["ID"]),
            Nome = reader["NOME"]?.ToString()
        });
    }

    return Results.Ok(nomes);
});

app.Run();

record NomeRequest(string Nome);
>>>>>>> 1248e0acbeb7847ddd41678e923e753c890644a0
