using Oracle.ManagedDataAccess.Client;

var builder = WebApplication.CreateBuilder(args);

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