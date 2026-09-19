# NeuroSync 🧠✨

> **Sistema de Gestão Clínica & Prontuário Eletrônico do Paciente (PEP)**  
> Desenvolvido especificamente para a prática de **Neuropsicopedagogia Clínica e Institucional**.

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/dotnet/csharp/)
[![SQLite](https://img.shields.io/badge/SQLite-003B57?style=for-the-badge&logo=sqlite&logoColor=white)](https://www.sqlite.org/)
[![Bootstrap 5](https://img.shields.io/badge/Bootstrap-5.3-7952B3?style=for-the-badge&logo=bootstrap&logoColor=white)](https://getbootstrap.com/)
[![QuestPDF](https://img.shields.io/badge/PDF-QuestPDF-E11D48?style=for-the-badge)](https://www.questpdf.com/)

---

## 📖 Visão Geral

O **NeuroSync** é uma plataforma concebida para otimizar o fluxo de trabalho de profissionais de neuropsicopedagogia, unificando desde a recepção e o acolhimento do paciente até a avaliação neurocognitiva, intervenção clínica, emissão de relatórios timbrados e controle financeiro do consultório.

Construído com base nos padrões modernos do ecossistema **ASP.NET Core (.NET 8)** e **C# 12**, o sistema alia alta performance, código didático e uma experiência de usuário (UX) acolhedora e responsiva.

---

## 🌟 Principais Módulos e Recursos

### 1. 📊 Dashboard Executivo & Tela de Boas-Vindas
* **Panorama Diário**: Saudações personalizadas e dinâmicas conforme o horário e a usuária conectada.
* **Atendimentos de Hoje**: Lista de sessões programadas, status em tempo real (*Confirmado*, *Em atendimento*, *Aguardando*).
* **Gráfico Donut de Eficiência (Chart.js)**: Distribuição visual das sessões do mês com badge central de total e percentual de realização.
* **Ritmo Semanal**: Acompanhamento da distribuição de atendimentos de segunda a sexta-feira.
* **Aniversariantes do Mês**: Alertas proativos para fortalecimento do vínculo terapêutico.

### 2. 📅 Agenda Clínica Integrada
* **FullCalendar Interativo**: Visualização flexível por Dia, Semana e Mês.
* **Busca Rápida de Pacientes (Select2)**: Campo com digitação assistida em tempo real, permitindo localizar rapidamente qualquer paciente em bases extensas.
* **Sessões Recorrentes**: Opção de agendamento em lote com repetição semanal (1, 4, 12 ou 24 semanas).
* **Preenchimento Inteligente**: Sugestão automática de data e do próximo horário de expediente disponível.
* **Geração Automática de Cobrança**: Integração opcional para gerar lançamentos financeiros vinculados à sessão criada.

### 3. 🗂️ Prontuário Eletrônico do Paciente (PEP)
* **Cadastro Completo**: Dados demográficos, endereço com busca automática por CEP (ViaCEP), filiação (dados do pai e da mãe) e contexto escolar (escola, série, professoras e psicólogas).
* **Anamnese e Linha Terapêutica**: Registro de queixa principal, diagnóstico/hipótese diagnóstica, medicações de uso contínuo e metas de curto/longo prazo.
* **Evolução Clínica Diária**: Notas detalhadas de cada atendimento, categorizadas por tipo (Intervenção, Avaliação, Devolutiva, Orientação) com carimbo de data, hora e responsável.
* **Parecer Técnico & Relatório de Avaliação**: Estruturação completa de documentos clínicos formais com geração de **PDF Timbrado em alta definição via QuestPDF**.
* **Gestão de Anexos**: Armazenamento e catalogação segura de laudos, avaliações neuropsicológicas e exames complementares.

### 4. 💰 Gestão Financeira Descomplicada
* **Aba Resumo**: Comparativo visual consolidado de *Receitas vs. Despesas* mês a mês, saldo líquido e gráfico donut de custos operacionais.
* **Aba Receitas**: Controle de cobranças avulsas ou de mensalidades com status (*Pendente*, *Pago*, *Atrasado*), emissão de recibos e rotina de baixa/reabertura.
* **Aba Despesas**: Lançamento categorizado dos custos da clínica (Energia Elétrica, Água, Internet, Telefonia, Aluguel, Limpeza, Materiais, Softwares).
* **Exportação em PDF**: Relatório gerencial com resumo de faturamento e extrato filtrável por paciente ou período.

### 5. 📈 Relatórios & Indicadores de Desempenho
* **Indicadores Clínicos**: Taxa de assiduidade/presença, novas avaliações iniciadas e total de pacientes ativos com comparativo percentual em relação ao período anterior.
* **Foco Clínico Neuropsicopedagógico**: Gráfico de distribuição do tempo entre Intervenções Cognitivas, Avaliações, Devolutivas com Pais e Orientações Escolares.
* **Evolução Temporal**: Curva analítica de atendimentos dos últimos 5 meses.
* **Auditoria de Prontuário**: Monitoramento em tempo real de quais sessões já possuem nota clínica registrada.
* **Exportação Excel (CSV)**: Exportação compatível com Excel usando separador `;` e encoding `UTF-8 com BOM`.

### 6. ⚙️ Perfil e Segurança
* **Configurações da Conta**: Edição de dados cadastrais e alteração segura de credenciais de acesso.
* **Autenticação Segura**: Gerenciamento de sessão baseado em cookies criptografados (`CookieAuthentication`) com proteção contra CSRF.

---

## 🛠️ Tecnologias e Bibliotecas

| Camada | Tecnologia | Descrição |
| :--- | :--- | :--- |
| **Linguagem** | **C# 12** | Idiomas modernos: Primary Constructors, Collection Expressions (`[]`), File-Scoped Namespaces |
| **Framework** | **ASP.NET Core 8.0 MVC** | Arquitetura Model-View-Controller robusta e escalável |
| **ORM / Banco** | **EF Core 8 + SQLite** | Banco relacional local leve, sem dependência de serviços externos |
| **Geração de PDF**| **QuestPDF 2024** | Motor de renderização declarativo de documentos clínicos timbrados |
| **Front-end** | **Bootstrap 5.3 + Razor** | Layout responsivo com paleta executiva e moderna |
| **Componentes** | **FullCalendar 6.1** | Calendário interativo com suporte a eventos clínicos |
| **Seleção/Busca** | **Select2 4.1** | Menus com filtragem dinâmica por digitação |
| **Gráficos** | **Chart.js** | Visualizações interativas de barras, linhas e donuts |
| **Mapas** | **MapLibre GL + OpenFreeMap** | Localização do consultório na tela de login sem custos de API |

---

## 📁 Estrutura do Projeto

```text
NeuroSyncV1/
├── Controllers/         # Controladores MVC com Primary Constructors e anotações didáticas
│   ├── AgendaController.cs
│   ├── ConfiguracoesController.cs
│   ├── FinanceiroController.cs
│   ├── HomeController.cs
│   ├── LoginController.cs
│   ├── PacientesController.cs
│   ├── ProntuariosController.cs
│   └── RelatoriosController.cs
├── Data/                # Contexto do EF Core e inicialização do SQLite
│   └── AppDbContext.cs
├── Models/              # Entidades do banco de dados e ViewModels tipados
│   ├── Agendamento.cs
│   ├── Cobranca.cs
│   ├── Despesa.cs
│   ├── Evolucao.cs
│   ├── Paciente.cs
│   ├── ParecerTecnico.cs
│   ├── Usuario.cs
│   └── ...
├── Views/               # Telas em Razor (.cshtml) organizadas por módulo
│   ├── Agenda/
│   ├── Configuracoes/
│   ├── Financeiro/
│   ├── Home/
│   ├── Login/
│   ├── Pacientes/
│   ├── Prontuarios/
│   ├── Relatorios/
│   └── Shared/
├── wwwroot/             # Assets estáticos (CSS, JS, Logos, Imagens e Uploads)
├── appsettings.json     # Configurações do ambiente e Connection String SQLite
├── Program.cs           # Inicialização da aplicação, injeção de dependência e migrações
└── neurosync.db         # Banco de dados local SQLite
```

---

## 🚀 Como Executar o Projeto

### Pré-requisitos
* [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.
* Editor de código (Visual Studio 2022, VS Code com C# Dev Kit ou JetBrains Rider).

### Passo a Passo

1. **Clone o repositório:**
   ```bash
   git clone https://github.com/PatrickRodrigues021/NeuroSyncV1.git
   cd NeuroSyncV1
   ```

2. **Restaure as dependências e compile:**
   ```bash
   dotnet restore
   dotnet build
   ```

3. **Execute a aplicação:**
   ```bash
   dotnet run
   ```

4. **Acesse no navegador:**
   Abra o navegador no endereço exibido no terminal (normalmente `http://localhost:5125` ou `https://localhost:7125`).

### 🔑 Credenciais de Primeiro Acesso

O sistema já inicializa automaticamente com uma conta padrão para acesso inicial:
* **Usuário / E-mail**: `admin`
* **Senha**: `admin123`

*(Você pode alterar essas credenciais a qualquer momento na aba **Configurações**).*

---

## 📄 Licença e Direitos

Projeto desenvolvido para uso clínico profissional. Todos os direitos reservados © 2026 **NeuroSync**.
