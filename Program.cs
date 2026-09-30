using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using NeuroSync.Models;
using QuestPDF.Infrastructure;


QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);



var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString) || connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
{
    connectionString = "Data Source=neurosync.db";
}
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));


builder.Services.AddControllersWithViews();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
    });

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");


InicializarBancoDeDados(app);

app.Run();








static void InicializarBancoDeDados(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    
    db.Database.EnsureCreated();

    
    db.Database.ExecuteSqlRaw(@"
        CREATE TABLE IF NOT EXISTS parecer_tecnico (
            id_parecer INTEGER PRIMARY KEY AUTOINCREMENT,
            id_paciente INTEGER NOT NULL,
            titulo TEXT NOT NULL,
            motivo_avaliacao TEXT,
            procedimentos_recursos TEXT,
            analise_avaliativa TEXT,
            sintese_avaliativa TEXT,
            recomendacoes_finais TEXT,
            data_emissao TEXT NOT NULL,
            profissional_nome TEXT NOT NULL,
            registro_profissional TEXT,
            FOREIGN KEY (id_paciente) REFERENCES paciente (id_paciente) ON DELETE CASCADE
        );

        CREATE TABLE IF NOT EXISTS usuario (
            id_usuario INTEGER PRIMARY KEY AUTOINCREMENT,
            nome TEXT NOT NULL,
            email TEXT NOT NULL,
            senha TEXT NOT NULL,
            criado_em TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS despesa (
            id_despesa INTEGER PRIMARY KEY AUTOINCREMENT,
            descricao TEXT NOT NULL,
            categoria TEXT NOT NULL,
            valor REAL NOT NULL,
            data_vencimento TEXT NOT NULL,
            data_pagamento TEXT,
            status TEXT NOT NULL,
            observacoes TEXT
        );
    ");

    
    VerificarColunasEvolucao(db);

    
    if (!db.Usuarios.Any())
    {
        db.Usuarios.Add(new Usuario
        {
            Nome = "Mariana Silva",
            Email = "admin",
            Senha = "admin123",
            CriadoEm = DateTime.Now
        });
        db.SaveChanges();
    }

    
    if (!db.Despesas.Any())
    {
        PopularDespesasIniciais(db);
    }
}




static void VerificarColunasEvolucao(AppDbContext db)
{
    var conn = db.Database.GetDbConnection();
    if (conn.State != System.Data.ConnectionState.Open) conn.Open();

    using var cmd = conn.CreateCommand();
    cmd.CommandText = "PRAGMA table_info(evolucao);";

    var colunasExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    using (var reader = cmd.ExecuteReader())
    {
        while (reader.Read())
            colunasExistentes.Add(reader.GetString(1));
    }

    if (!colunasExistentes.Contains("tipo_evolucao"))
    {
        cmd.CommandText = "ALTER TABLE evolucao ADD COLUMN tipo_evolucao TEXT DEFAULT 'Sessão Terapêutica';";
        cmd.ExecuteNonQuery();
    }

    if (!colunasExistentes.Contains("profissional_nome"))
    {
        cmd.CommandText = "ALTER TABLE evolucao ADD COLUMN profissional_nome TEXT;";
        cmd.ExecuteNonQuery();
    }
}




static void PopularDespesasIniciais(AppDbContext db)
{
    var hoje = DateTime.Today;
    int ano = hoje.Year, mes = hoje.Month;
    DateTime DataNoMes(int dia) => new(ano, mes, Math.Min(dia, DateTime.DaysInMonth(ano, mes)));

    db.Despesas.AddRange([
        new Despesa
        {
            Descricao = "Energia Elétrica - Clínica",
            Categoria = "Energia Elétrica",
            Valor = 380.00m,
            DataVencimento = DataNoMes(10),
            DataPagamento = DataNoMes(9),
            Status = "Pago",
            Observacoes = "CEMIG - Débito em conta"
        },
        new Despesa
        {
            Descricao = "Água e Saneamento",
            Categoria = "Água",
            Valor = 95.50m,
            DataVencimento = DataNoMes(15),
            DataPagamento = DataNoMes(14),
            Status = "Pago",
            Observacoes = "Copasa"
        },
        new Despesa
        {
            Descricao = "Internet Fibra Dedicada 500MB",
            Categoria = "Internet",
            Valor = 149.90m,
            DataVencimento = DataNoMes(20),
            Status = "Pendente",
            Observacoes = "Vivo Fibra Empresarial"
        },
        new Despesa
        {
            Descricao = "Telefonia Celular / WhatsApp Clínico",
            Categoria = "Telefonia",
            Valor = 89.90m,
            DataVencimento = DataNoMes(12),
            DataPagamento = DataNoMes(11),
            Status = "Pago",
            Observacoes = "Linha exclusiva para agendamentos"
        },
        new Despesa
        {
            Descricao = "Assinatura Softwares & Sistemas",
            Categoria = "Sistemas",
            Valor = 249.00m,
            DataVencimento = DataNoMes(5),
            DataPagamento = DataNoMes(5),
            Status = "Pago",
            Observacoes = "Hospedagem em nuvem e licenças clínicas"
        },
        new Despesa
        {
            Descricao = "Papelaria e Materiais Clínicos",
            Categoria = "Material de Escritório",
            Valor = 320.00m,
            DataVencimento = DataNoMes(25),
            Status = "Pendente",
            Observacoes = "Folhas de aplicação e jogos pedagógicos"
        },
        new Despesa
        {
            Descricao = "Aluguel e Condomínio Sala Clínica",
            Categoria = "Aluguel",
            Valor = 1850.00m,
            DataVencimento = DataNoMes(8),
            DataPagamento = DataNoMes(7),
            Status = "Pago",
            Observacoes = "Imobiliária Central"
        },
        new Despesa
        {
            Descricao = "Serviço de Limpeza e Higienização",
            Categoria = "Limpeza",
            Valor = 400.00m,
            DataVencimento = DataNoMes(28),
            Status = "Pendente",
            Observacoes = "Diarista quinzenal"
        }
    ]);

    db.SaveChanges();
}