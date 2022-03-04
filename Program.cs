using Tarefas.db;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Conexão
builder.Services.AddDbContext<tarefasContext>(opt =>
{
    string connectionString = builder.Configuration.GetConnectionString("tarefasConnection");
    var serverVersion = ServerVersion.AutoDetect(connectionString);
    opt.UseMySql(connectionString, serverVersion);
});

// OpenAPI (Swagger)
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // OpenAPI (Swagger)
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Arquivos estáticos
app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoints da API
app.MapGet("/api/tarefas", ([FromServices] tarefasContext _db) =>
{
    return Results.Ok(_db.Tarefa.ToList<Tarefa>());
});

app.MapGet("/api/tarefas/{id}", ([FromServices]tarefasContext _db, [FromRoute] int id) =>
{
    var tarefa = _db.Tarefa.Find(id);

    if (tarefa == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(tarefa);
});

app.MapPost("/api/tarefas",([FromServices] tarefasContext _db, [FromBody] Tarefa novaTarefa)=>
{
    if(String.IsNullOrEmpty(novaTarefa.Descricao))
   {
       return Results.BadRequest(new{mensagem = "Não é possivel cadastrar um nova tarefa sem descrição"});
   }

   if(novaTarefa.Concluida)
   {
       return Results.BadRequest(new{mensagem = "Não é possivel cadastrar uma nova tarefa, se a mesma já estiver concluida."});
   }

    var tarefa = new Tarefa
    {
        Descricao = novaTarefa.Descricao,
        Concluida = novaTarefa.Concluida,
    };
    _db.Tarefa.Add(tarefa);
    _db.SaveChanges();

    string urlTarefaCriada = $"/api/tarefas/{tarefa.Id}";

    return Results.Created(urlTarefaCriada, tarefa);

});

app.Run();
