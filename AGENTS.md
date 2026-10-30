# AGENTS.md

Instruções para agentes de IA (Claude Code, Codex, Antigravity etc.) trabalhando neste repositório.

## O que é

**Treinos**: API para registrar e analisar treinos (musculação, peso corporal, exercícios por tempo, cardio) e
peso corporal. Produto independente do Planner, só backend, consumido pela API (scripts, a ALICE, apps futuros).
Pensado para muitos usuários, embora hoje só um use.

> Status: **versão mínima no ar na VPS** — registra treinos (exercícios e séries) e peso corporal, lista o
> catálogo e acha exercícios por nome ou apelido. Ficou de fora, de propósito: concorrência otimista
> (If-Match), exercícios e apelidos criados pelo usuário, reordenação, análises, login com Google e testes de
> integração (Testcontainers exige Docker). A ALICE já registra e corrige treinos e grava o peso pelo Telegram
> (`src/treinos.py` no repositório dela).
>
> **Banco: o Postgres da VPS, sem banco local.** Em desenvolvimento, a API roda no PC e acessa a VPS por túnel
> SSH, no banco separado `treinos_dev` (nunca nos dados reais). Em produção, roda na VPS como serviço systemd,
> como o Planner, no schema `treinos` do banco `planner`, com o usuário `treinos` (que não lê as tabelas do
> Planner).

## Decisões planejadas

- Mesma stack e convenções do Planner (repositório `briel0/planner`): C# / ASP.NET Core, Clean Architecture
  enxuta, EF Core + PostgreSQL, OpenAPI, xUnit.
- Mesmo servidor Postgres do Planner na VM, mas **schema `treinos` próprio** e **usuário do banco próprio**;
  nenhuma API lê as tabelas da outra. Identidade compartilhada pelo login com Google, não por tabela.
- Modelagem pelo método clássico (minimundo → conceitual → lógico → físico) em `docs/data-modeling/`, textos em
  inglês. O minimundo (`01-miniworld.md`) é a fonte de verdade dos requisitos de dados.

## Revisitar as decisões durante a implementação (regra do dono)

As decisões de modelagem foram tomadas uma a uma, comparando caminhos, **antes** de existir código. Ao
implementar algo que dependa de uma delas, **relembre o dono da decisão e reexplique**: qual caminho foi
escolhido, quais eram as alternativas e os prós e contras. A implementação pode mostrar que outro caminho é
melhor; nesse caso, proponha a mudança antes de seguir, em vez de implementar em silêncio o que foi decidido.

Decisões a revisitar (detalhes nos documentos de `docs/data-modeling/`):

| # | Decisão | Escolha | Onde |
|---|---|---|---|
| 1 | Chaves primárias | UUID v7 em tudo; exceções: `muscle_groups` com código legível (`chest`) e `exercise_muscle_groups` com chave composta | 03, "Primary keys" |
| 2 | Chave de entradas e séries (entidades fracas) | UUID próprio; ordem (`sort_order`) como restrição única dentro do dono | 03, "Primary keys" |
| 3 | Onde ficam as medidas das séries | uma tabela `workout_sets` com uma coluna opcional por medida, preenchidas conforme o tipo do exercício | 03, "Set measurements" |
| 4 | Carga adicional e assistência (peso corporal) | duas colunas não negativas (`load_kg`, `assistance_kg`), nunca as duas juntas; nada de número com sinal | 03, "Set measurements" |
| 5 | Unidade no nome da coluna | sim: `load_kg`, `distance_m`, `duration_sec` | 03, "Naming" |
| 6 | Mudar o tipo de medição de um exercício já usado | travado após o primeiro uso; conversão explícita pode vir depois | 03, restrição 6 |
| 7 | "Um termo leva a um exercício só" | unicidade dentro de cada escopo (sistema / usuário) + precedência na busca (o termo do usuário vence); exercício do usuário não pode ter o nome de um do sistema | 03, "Search terms" |
| 8 | Banco conferir as medidas da série pelo tipo do exercício | sim, sem trigger: `measurement_type` copiado em `workout_exercises` e `workout_sets`, com FKs compostas `(id, measurement_type)` garantindo a cópia e `CHECK` por linha; as mesmas FKs travam a mudança de tipo (decisão 6). A API valida também, para dar mensagens claras | 04 |
| 9 | Listas de valores fixos (`measurement_type`, `role`, `set_type`, `side`) | `text` com `CHECK`; `enum` no C# com `HasConversion<string>()`. Tabela de consulta só quando o valor tem atributos próprios (`muscle_groups`) | 04 |
| 10 | Busca ignorando maiúsculas e acentos | collation ICU não determinística (nível 1) nas colunas `exercises.name` e `exercise_aliases.term`; sem coluna normalizada. Busca aproximada (`pg_trgm`) pode exigir uma coluna normalizada depois | 04 |
| 11 | Grupos musculares | manter os 11 (`chest`, `back`, `shoulders`, `biceps`, `triceps`, `forearms`, `quadriceps`, `hamstrings`, `glutes`, `calves`, `abs`), sem detalhar as costas (dorsal, trapézio, lombar) | 01, 04 "Seed data" |
| 12 | Catálogo inicial de exercícios | curadoria própria (~80–120 exercícios comuns), com apelidos em pt-BR e inglês feitos à mão. Fontes consideradas: free-exercise-db (Unlicense, sem obrigações; útil como referência) e wger (CC-BY-SA: copiar dados exige atribuição e compartilhamento pela mesma licença; evitar copiar) | 04 "Seed data" |
| 13 | Grupos musculares do cardio | exercícios `distance_time` do sistema não têm grupo muscular (exceção na restrição 5), para não inflar as análises por grupo | 01, 02, 03 restrição 5 |
| 14 | Regras que cruzam entidades, no domínio | exercício visível para o dono do treino e lado só em exercício unilateral ficam no domínio: `Workout.AddExercise` recebe o `Exercise` e confere (previstas antes como "só na API") | `Workout`, `WorkoutSet` |
| 15 | Ordem de exercícios e séries | vem da posição na lista; o domínio numera `SortOrder` (0, 1, 2…) e renumera ao remover ou reordenar; o cliente nunca envia a ordem | `Workout` |
| 16 | Zero repetições | só numa série marcada como falhada (o banco aceita ≥ 0; o domínio é mais estrito) | `WorkoutSet` |
| 17 | Nome do usuário do banco | `treinos` (não `treinos_app`): igual ao usuário de sistema do serviço, para entrar pelo socket com autenticação peer, sem senha, como o Planner | 04, "Schema and database role" |
| 18 | FKs compostas no EF Core | o EF Core declara as FKs `(id, measurement_type)` como chaves alternativas (o tipo nunca muda); as FKs simples extras do modelo físico não existem. Nas séries, a cópia do tipo é uma propriedade oculta preenchida pelo EF | 04, "Foreign keys" |
| 19 | Restrições de ordem | `DEFERRABLE INITIALLY DEFERRED` (conferidas só no commit), porque o EF Core não ordena os UPDATEs ao renumerar | 04, "Uniqueness" |
| 20 | Peso corporal na API | identificado pelo dia: `PUT /api/body-weights/{data}` cria ou substitui, sem id do cliente | `BodyWeightUseCases` |

Ao tomar uma decisão nova, acrescente-a nesta tabela.

## Estrutura

Só backend, então a solução fica na raiz (sem a pasta `api/` do Planner):

```
treinos/
├── Treinos.slnx, global.json, Directory.*.props, .editorconfig   # mesma configuração do Planner
├── docs/data-modeling/          # modelagem do banco, etapa por etapa
├── docs/exercise-catalog.json   # catálogo inicial (fonte da migration SeedExerciseCatalog)
├── deploy/treinos-api.service   # unit do systemd na VPS
├── src/Treinos.Domain/          # entidades e regras: Users, Exercises, Workouts, BodyWeights
├── src/Treinos.Application/     # casos de uso, respostas e interfaces de repositório/consulta
├── src/Treinos.Infrastructure/  # EF Core + Npgsql: mapeamento, migrations, repositórios, consultas
├── src/Treinos.Api/             # controllers REST, autenticação de desenvolvimento, Problem Details
└── tests/Treinos.Domain.Tests/
```

## API

Tudo sob `/api`, JSON em camelCase, listas de valores em snake_case (`weight_reps`), erros em Problem Details
com `code`. Ids de criação vêm do cliente (UUID v7): repetir o pedido não duplica. OpenAPI em `/openapi/v1.json`.

- `GET /api/exercises`, `GET /api/exercises/{id}`, `GET /api/exercises/resolve?term=paralela` (404 se nada bater).
- `GET /api/workouts?since=&until=`, `GET|PATCH|DELETE /api/workouts/{id}`, `POST /api/workouts` (com
  exercícios e séries, ou vazio), `POST /api/workouts/{id}/exercises`, `DELETE /api/workouts/{id}/exercises/{id}`.
- `GET /api/body-weights?since=&until=`, `PUT|DELETE /api/body-weights/{2026-10-30}`.

## Comandos (na raiz)

- `dotnet build` — compila; qualquer aviso (incluindo formatação e nulos) quebra o build.
- `dotnet test` — roda os testes (xUnit v3 sobre a Microsoft.Testing.Platform).
- `dotnet format` — corrige a formatação; `dotnet format --verify-no-changes` só verifica.
- Versões de pacotes NuGet ficam só em `Directory.Packages.props`.
- Migrations (`dotnet tool restore` uma vez):
  - criar: `dotnet ef migrations add <Nome> --project src/Treinos.Infrastructure --startup-project src/Treinos.Infrastructure --output-dir Persistence/Migrations`;
  - aplicar no `treinos_dev` (com o túnel aberto): `dotnet ef database update --project src/Treinos.Infrastructure --startup-project src/Treinos.Infrastructure --connection "<string dos User Secrets>;Search Path=treinos"`. Sem o `Search Path`, a collation não é achada.
- Rodar a API: `ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/Treinos.Api` (porta 5265). Fora de
  Development ela se recusa a subir: só existe o usuário dev.

## Ambiente de desenvolvimento

- Windows 10 com o .NET 10 SDK (winget `Microsoft.DotNet.SDK.10`). Sem Docker e sem Postgres local.
- Banco: `treinos_dev` na VPS. Túnel: `ssh -N -L 15432:127.0.0.1:5432 alice-vm`. A string de conexão fica nos User
  Secrets (`ConnectionStrings:Treinos`, com `Port=15432`, usuário e senha `treinos_dev`), nunca no Git.

## Deploy na VPS

Mesmo molde do Planner: executável publicado (self-contained, linux-x64) em `/opt/treinos/releases/<versão>`,
`/opt/treinos/current` aponta para a versão ativa, usuário de sistema `treinos`, serviço `treinos-api` (ver
`deploy/treinos-api.service`) escutando só em `127.0.0.1:5265`, em Development.

1. `dotnet publish src/Treinos.Api -c Release -r linux-x64 --self-contained -o <pasta>` e copiar para a VPS.
2. Migrations: `dotnet ef migrations script --idempotent ...` com `SET search_path = treinos;` na primeira linha,
   aplicado com `sudo -u treinos psql -v ON_ERROR_STOP=1 -d planner -f script.sql`.
3. Trocar o link `current` e `sudo systemctl restart treinos-api`.

Do PC, a API de produção é acessível por túnel: `ssh -N -L 8781:127.0.0.1:5265 alice-vm` → `http://localhost:8781/api`.

## Backlog

1. Dashboard (ou algo do tipo) com os dados dos treinos: progressão de carga por exercício, volume por grupo
   muscular, frequência, peso corporal ao longo do tempo. Nada decidido: página própria, consultas na API,
   ferramenta pronta lendo o banco etc. As análises ficaram fora da versão mínima de propósito.
2. Leitura pela ALICE (o que foi feito num dia, histórico de um exercício, peso corporal): decisão adiada para
   depois do teste em produção; detalhes no backlog da ALICE.
3. Exercícios por tempo no catálogo (prancha etc.): hoje não há nenhum.

## Convenções

- Commits em inglês, no padrão Conventional Commits, como no Planner.
- Nunca commitar sem aprovação explícita do dono do projeto.
- Dependências novas: conferir a licença antes de recomendar ou instalar (preferir MIT/Apache/BSD).
