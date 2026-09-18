using Microsoft.AspNetCore.Authentication.Cookies; 
using Microsoft.EntityFrameworkCore;
using NeuroSync.Data;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; 
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

// Garante que todas as tabelas (inclusive parecer_tecnico) existam no SQLite
using (var scope = app.Services.CreateScope())
{
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
    ");

    var conn = db.Database.GetDbConnection();
    if (conn.State != System.Data.ConnectionState.Open) conn.Open();
    using (var cmd = conn.CreateCommand())
    {
        cmd.CommandText = "PRAGMA table_info(evolucao);";
        var colunasExistentes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var reader = cmd.ExecuteReader())
        {
            while (reader.Read())
            {
                colunasExistentes.Add(reader.GetString(1));
            }
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

    if (!db.Usuarios.Any())
    {
        db.Usuarios.Add(new NeuroSync.Models.Usuario
        {
            Nome = "Mariana Silva",
            Email = "admin",
            Senha = "admin123",
            CriadoEm = DateTime.Now
        });
        db.SaveChanges();
    }
}

app.Run();