# 04 — Physical model

Last step of the design process. Turns the [logical model](03-logical-model.md) into PostgreSQL:
exact types, constraints and indexes. Targets **PostgreSQL 18**, the same server as the Planner, for native
`uuidv7()` and `LIKE` on nondeterministic collations.

## Schema and database role

- Every object lives in the **`treinos` schema**, including the EF Core migrations history
  (`__EFMigrationsHistory` in `treinos`), so this product never collides with another one sharing the server.
- The API connects with its **own role** (`treinos`, the owner of the schema), which has privileges on
  `treinos` only and cannot read any other schema. On the VPS the role has the same name as the system user
  that runs the service, so it logs in through the local socket with peer authentication, without a password.
  On another installation the product may have a database of its own without changing a line: the schema
  name is the only assumption.
- **The connection's `search_path` is `treinos`** (set by the API and required when running migrations), so
  unqualified names such as the collation resolve to this schema.

## Collation for search terms

```sql
CREATE COLLATION treinos.ignore_case_accents (
    provider = icu, locale = 'und-u-ks-level1', deterministic = false
);
```

- **`und-u-ks-level1`:** language-neutral ICU rules compared at strength level 1, which ignores case and
  accents: `'Supino' = 'supino' = 'SÚPINO'`.
- **Nondeterministic:** equality follows the rule above instead of comparing bytes, so `=`, `LIKE` and unique
  indexes on these columns all ignore case and accents, while the text is stored exactly as typed.
- **Used only on `exercises.name` and `exercise_aliases.term`.** Every other column keeps the default
  collation.
- **Leading, trailing and repeated spaces are not handled by the collation:** checks forbid leading and
  trailing spaces, and the API collapses repeated ones before writing.
- **Future fuzzy search** ("supno" finding "supino", `pg_trgm`) may need a normalized column next to these;
  it can be added without changing the existing ones.

## Types

| Table | Column | Type | Null | Default | Notes |
|---|---|---|---|---|---|
| `users` | `id` | `uuid` | no | `uuidv7()` | |
| | `name` | `text` | no | | 1–100 characters |
| | `email` | `text` | no | | up to 254 characters, stored in lowercase |
| | `created_at` | `timestamptz` | no | `now()` | |
| | `updated_at` | `timestamptz` | no | `now()` | |
| `muscle_groups` | `code` | `text` | no | | `^[a-z_]{1,30}$` |
| | `name` | `text` | no | | display name, 1–50 characters |
| `exercises` | `id` | `uuid` | no | `uuidv7()` | |
| | `user_id` | `uuid` | yes | | empty for system exercises |
| | `name` | `text` `COLLATE ignore_case_accents` | no | | 1–100 characters |
| | `measurement_type` | `text` | no | | `weight_reps`, `bodyweight`, `time`, `distance_time` |
| | `is_unilateral` | `boolean` | no | `false` | |
| | `created_at` | `timestamptz` | no | `now()` | |
| | `updated_at` | `timestamptz` | no | `now()` | |
| `exercise_muscle_groups` | `exercise_id` | `uuid` | no | | |
| | `muscle_group_code` | `text` | no | | |
| | `role` | `text` | no | | `primary`, `secondary` |
| `exercise_aliases` | `id` | `uuid` | no | `uuidv7()` | |
| | `exercise_id` | `uuid` | no | | |
| | `user_id` | `uuid` | yes | | empty for system aliases |
| | `term` | `text` `COLLATE ignore_case_accents` | no | | 1–100 characters |
| | `created_at` | `timestamptz` | no | `now()` | |
| | `updated_at` | `timestamptz` | no | `now()` | |
| `workouts` | `id` | `uuid` | no | `uuidv7()` | |
| | `user_id` | `uuid` | no | | |
| | `started_at` | `timestamptz` | no | | |
| | `duration_sec` | `integer` | yes | | > 0 |
| | `note` | `text` | yes | | up to 2000 characters |
| | `created_at` | `timestamptz` | no | `now()` | |
| | `updated_at` | `timestamptz` | no | `now()` | |
| `workout_exercises` | `id` | `uuid` | no | `uuidv7()` | |
| | `workout_id` | `uuid` | no | | |
| | `exercise_id` | `uuid` | no | | |
| | `measurement_type` | `text` | no | | copy of the exercise's type (see "Measurement type copies") |
| | `sort_order` | `integer` | no | | ≥ 0 |
| `workout_sets` | `id` | `uuid` | no | `uuidv7()` | |
| | `workout_exercise_id` | `uuid` | no | | |
| | `measurement_type` | `text` | no | | copy of the entry's type |
| | `sort_order` | `integer` | no | | ≥ 0 |
| | `set_type` | `text` | no | `'working'` | `warmup`, `working` |
| | `is_failed` | `boolean` | no | `false` | |
| | `reps` | `integer` | yes | | ≥ 0 (0 only makes sense in a failed set) |
| | `load_kg` | `numeric(6,2)` | yes | | ≥ 0 |
| | `assistance_kg` | `numeric(6,2)` | yes | | > 0 |
| | `duration_sec` | `integer` | yes | | > 0 |
| | `distance_m` | `integer` | yes | | > 0 |
| | `side` | `text` | yes | | `left`, `right` |
| `body_weights` | `id` | `uuid` | no | `uuidv7()` | |
| | `user_id` | `uuid` | no | | |
| | `measured_on` | `date` | no | | |
| | `weight_kg` | `numeric(5,2)` | no | | > 0 |
| | `created_at` | `timestamptz` | no | `now()` | |
| | `updated_at` | `timestamptz` | no | `now()` | |

### Rationale

- **`text` + length `CHECK` instead of `varchar(n)`:** same performance in PostgreSQL; the limit is an explicit,
  easily changed rule. As in the Planner, the same checks forbid empty or whitespace-only names.
- **Value lists as `text` + `CHECK`** (decision 9 in `AGENTS.md`): the database guarantees the values, changing a
  list is one line in a migration, and anyone querying the database reads `'warmup'` instead of a number. In C#
  they are `enum`s mapped with `HasConversion<string>()`. Rejected: PostgreSQL enums (removing or renaming a
  value means recreating the type), lookup tables (a table per two-value list), integers (unreadable, and
  reordering the C# enum would silently change the data's meaning).
- **`numeric` for loads and body weight:** exact decimals. Plates come in 1.25 kg, and with `real` or
  `double precision` a sum of 1.25s can come out as 2.4999999. Two decimal places cover any plate or scale.
  Up to 9,999.99 kg for loads (leg presses get heavy) and 999.99 kg for body weight.
- **`integer` for distance and duration:** a meter and a second are fine enough for training; integers sum and
  divide without surprises. Pace (`duration_sec / distance_m`) is computed in queries.
- **`assistance_kg` > 0, `load_kg` ≥ 0:** zero assistance is the same as no assistance, so it is left empty;
  a load of 0 is a real value (an unloaded machine, or "no added weight" written explicitly).
- **`timestamptz` for `started_at`:** an absolute moment. Which **calendar day** a workout belongs to depends on
  the user's time zone (23:30 in São Paulo is already the next day in UTC), so day- and week-based queries take
  the time zone as a parameter. Body weight uses `date` because "weighed on the 30th" is a calendar day,
  wherever the user is.

## Constraints

### Foreign keys and cascades

| Foreign key | References | On delete | On update |
|---|---|---|---|
| `exercises.user_id` | `users.id` | cascade | — |
| `exercise_muscle_groups.exercise_id` | `exercises.id` | cascade | — |
| `exercise_muscle_groups.muscle_group_code` | `muscle_groups.code` | no action | — |
| `exercise_aliases.exercise_id` | `exercises.id` | cascade | — |
| `exercise_aliases.user_id` | `users.id` | cascade | — |
| `workouts.user_id` | `users.id` | cascade | — |
| `workout_exercises.workout_id` | `workouts.id` | cascade | — |
| `workout_exercises.exercise_id` | `exercises.id` | no action | — |
| `workout_exercises (exercise_id, measurement_type)` | `exercises (id, measurement_type)` | no action | no action |
| `workout_sets.workout_exercise_id` | `workout_exercises.id` | cascade | — |
| `workout_sets (workout_exercise_id, measurement_type)` | `workout_exercises (id, measurement_type)` | cascade | no action |
| `body_weights.user_id` | `users.id` | cascade | — |

- **Deleting a workout** deletes its entries, which delete their sets.
- **An exercise in use cannot be deleted:** the entries' foreign key blocks it. It is `NO ACTION`, not
  `RESTRICT`, on purpose: `NO ACTION` is checked at the end of the statement, so deleting a user — which cascades
  to their workouts *and* their exercises in the same statement — still succeeds, while deleting a used
  exercise on its own fails.
- **Deleting a user** deletes everything they own: exercises, aliases, workouts (with entries and sets) and body
  weights.
- **Implementation note:** unlike the Planner, EF Core declares the composite keys itself (as alternate keys),
  because the measurement type never changes. So the plain `exercise_id` and `workout_exercise_id` foreign keys
  listed above are not created: the composite ones cover them.

### Measurement type copies

Decision 8 in `AGENTS.md`. The measurement type is copied from `exercises` to `workout_exercises` and from there
to `workout_sets`, and each copy is guarded by a composite foreign key:

- `exercises` and `workout_exercises` declare `UNIQUE (id, measurement_type)` — redundant for uniqueness, since
  `id` is already unique, but required so a foreign key can reference the pair.
- **The copy always matches:** an entry's `(exercise_id, measurement_type)` must exist as `(id, measurement_type)`
  of an exercise, and a set's pair must exist in its entry.
- **The type is frozen once used** (logical constraint 6), with no trigger: changing
  `exercises.measurement_type` while an entry points to it breaks the composite key (`ON UPDATE NO ACTION`), so
  PostgreSQL rejects it. The same holds for an entry with sets.
- **The set check can then read its own row** — see "Checks".
- **Normalization:** this is a deliberate redundancy (`workout_sets.id → workout_exercise_id → measurement_type`
  is a transitive dependency), protected by the foreign keys, so it can never disagree with the source. The
  Planner does the same with a card's category.

### Uniqueness

| Unique | Notes |
|---|---|
| `users (email)` | the API stores emails in lowercase |
| `exercises (user_id, name)` `NULLS NOT DISTINCT` | names unique within a scope; `NULLS NOT DISTINCT` makes all system exercises (empty `user_id`) one scope. Case and accents ignored by the collation |
| `exercise_aliases (user_id, term)` `NULLS NOT DISTINCT` | same, for aliases |
| `exercises (id, measurement_type)` | target of the composite foreign key |
| `workout_exercises (workout_id, sort_order)` `DEFERRABLE` | order unique within a workout |
| `workout_exercises (id, measurement_type)` | target of the composite foreign key |
| `workout_sets (workout_exercise_id, sort_order)` `DEFERRABLE` | order unique within an entry |
| `body_weights (user_id, measured_on)` | one weight per user per day |

- **Order constraints are `DEFERRABLE INITIALLY DEFERRED`:** removing an exercise and renumbering the next ones
  (or swapping two sets) passes through duplicates halfway, and EF Core does not order its updates to avoid
  them. Uniqueness is checked only at commit. Written by hand in the migration: EF Core cannot declare
  deferrable constraints.
- **Not in the database, checked by the API** (logical "Search terms"): a user's exercise cannot take the name of a
  system exercise, and a new name or alias cannot clash with another term of the same scope in the other table.
  Searches stay deterministic regardless, by the fixed precedence.

### Checks

- `users.name`, `muscle_groups.name`, `exercises.name`, `exercise_aliases.term`: not empty, no leading or trailing
  spaces (`x = btrim(x)`), within the length limits in the types table.
- `users.email`: at most 254 characters, equal to `lower(email)`.
- `muscle_groups.code`: matches `^[a-z_]{1,30}$`.
- Value lists: `measurement_type`, `role`, `set_type` and `side` within their values (in every table that has them).
- `workouts.note`: at most 2000 characters. `workouts.duration_sec` > 0.
- `workout_exercises.sort_order`, `workout_sets.sort_order` ≥ 0.
- `workout_sets` numeric ranges as in the types table, and **measurements by type**:

  ```sql
  CHECK (
      (measurement_type = 'weight_reps'
          AND reps IS NOT NULL AND load_kg IS NOT NULL
          AND assistance_kg IS NULL AND duration_sec IS NULL AND distance_m IS NULL)
   OR (measurement_type = 'bodyweight'
          AND reps IS NOT NULL
          AND NOT (load_kg IS NOT NULL AND assistance_kg IS NOT NULL)
          AND duration_sec IS NULL AND distance_m IS NULL)
   OR (measurement_type = 'time'
          AND duration_sec IS NOT NULL
          AND reps IS NULL AND load_kg IS NULL AND assistance_kg IS NULL AND distance_m IS NULL)
   OR (measurement_type = 'distance_time'
          AND distance_m IS NOT NULL AND duration_sec IS NOT NULL
          AND reps IS NULL AND load_kg IS NULL AND assistance_kg IS NULL)
  )
  ```
- `body_weights.weight_kg` > 0.

### Rules enforced only by the API

Each one crosses tables in a way no declarative constraint expresses, and none of them breaks the system if
violated (unlike the Planner's cycles, which would make recursive queries loop):

- **Visible exercises only** (logical constraint 3): an entry's exercise is a system one or belongs to the
  workout's owner; the same for a user's aliases. Checked in the use cases, which already load the exercise.
- **Side only on unilateral exercises** (constraint 2). `is_unilateral` is not frozen like the measurement type:
  if an exercise stops being unilateral, its old sets keep their side as history.
- **System exercises list a primary muscle group, except cardio** (constraint 5): system rows are written only by migrations,
  which are reviewed.
- **An entry has at least one set** (constraint 7): the API creates and replaces an entry together with its sets,
  in one transaction.

### Triggers

- **`updated_at`:** a trigger on every table that has the column sets `updated_at = now()` on each update, as in
  the Planner.
- **Workout touched by its children:** a trigger on `workout_exercises` and `workout_sets` sets the workout's
  `updated_at = now()` on any insert, update or delete, so "what changed since…" queries see edits to entries and
  sets without any writer remembering to touch the workout.

## Seed data

Created by migrations, never by users:

- **`muscle_groups`:** `chest`, `back`, `shoulders`, `biceps`, `triceps`, `forearms`, `quadriceps`, `hamstrings`,
  `glutes`, `calves`, `abs`, with English display names. Translating them is a presentation concern for clients.
- **System exercises and aliases:** a separate migration, to be written when the catalog is defined. Each system
  exercise lists at least one primary muscle group, except `distance_time` exercises, which list none. Source:
  `docs/exercise-catalog.json`.

## Indexes

Derived from the queries the system actually runs, following the Planner's rules: none "just in case", foreign
key columns covered explicitly (PostgreSQL does not index them automatically), and a composite index `(a, b)` also
serves queries filtering only by `a`.

| Index | Serves |
|---|---|
| `users (email)` — unique | lookup by email at login |
| `exercises (user_id, name)` — unique | name search per scope (`user_id = ?`, then `user_id IS NULL`); listing a user's or the system's exercises (by prefix); cascade from `users` |
| `exercises (id, measurement_type)` — unique | target of the composite foreign key |
| `exercise_muscle_groups (exercise_id, muscle_group_code)` — primary key | muscle groups of an exercise; cascade from `exercises` |
| `exercise_aliases (user_id, term)` — unique | alias search per scope; cascade from `users` |
| `exercise_aliases (exercise_id)` | aliases of an exercise; cascade from `exercises` |
| `workouts (user_id, started_at)` | a user's workouts in a period, newest first; cascade from `users` (by prefix) |
| `workout_exercises (workout_id, sort_order)` — unique | entries of a workout in order; cascade from `workouts` |
| `workout_exercises (exercise_id)` | progress of one exercise over time; the delete check on `exercises` and its composite foreign key (by prefix) |
| `workout_exercises (id, measurement_type)` — unique | target of the composite foreign key |
| `workout_sets (workout_exercise_id, sort_order)` — unique | sets of an entry in order; cascade and composite foreign key check (by prefix) |
| `body_weights (user_id, measured_on)` — unique | weight on a date or the latest before it (for the real load of bodyweight sets); cascade from `users` |

Not created yet, on purpose:

- **`exercise_muscle_groups (muscle_group_code)`:** per-muscle analyses reach muscle groups from the sets through
  `exercise_id`, which the primary key covers. Filtering the catalog by muscle group scans a small table.
- **`updated_at` indexes** for synchronization: added when another tool starts syncing.

Every index is validated with `EXPLAIN ANALYZE` once there is real data.
