# 01 — Miniworld

First step of the classic database design process (requirements gathering).
Describes, in plain language, the slice of reality the database represents.
The following steps (conceptual, logical and physical models) are derived from this text.

## Description

The system records the workouts of **many users** (a single one for now, but the model must not assume that). Each user has a **name** and an **email**, and only sees their own data.

Workouts are made of **exercises**. Exercises come from two places: a **catalog defined by the system**, visible to every user and not editable by them, and **exercises created by each user**, visible only to their creator. Every exercise has a **name** — unique among the system exercises, and unique among the exercises of the same user — and a **measurement type**, which decides what is recorded in each of its sets:

- **Weight and reps** (bench press, curl): repetitions and load.
- **Bodyweight** (pull-up, dip): repetitions, plus an optional **added load** or **assistance**, both in kg.
- **Time** (plank, isometric holds): duration.
- **Distance and time** (running, cycling, rowing): distance and duration. This is how cardio is recorded: a run is a workout with a single running exercise.

An exercise may be marked as **unilateral**, meaning each side is trained separately.

An exercise works **zero or more muscle groups**, each with a **role**: **primary** or **secondary** (a dip works chest and triceps as primary and shoulders as secondary). The list of **muscle groups is fixed and defined by the system** (chest, back, shoulders, biceps, triceps, forearms, quadriceps, hamstrings, glutes, calves, abs); users pick from it but cannot create new ones. System exercises always list their muscle groups, except cardio (distance and time), which works no muscle group so it never inflates per-muscle analyses; for exercises created by users it is optional, and an exercise without them simply does not appear in per-muscle analyses. A muscle group appears at most once per exercise.

Exercises can be found by **aliases** besides their name, so the same exercise can be referred to in different ways ("dip", "parallel bars"). An alias belongs to **exactly one exercise** and is either **defined by the system** (visible to every user) or **created by a user** (visible only to them); a user may add aliases to system exercises as well as to their own. Searching ignores case and accents. Within the system's terms, and within each user's terms, a name or alias leads to **at most one exercise**; when a user's term and a system term are the same, **the user's term wins**, so a search is never ambiguous and new system aliases never break what users already use. A user's exercise cannot have the same name as a system exercise. When an exercise is deleted, its aliases are deleted with it.

A **workout** belongs to **exactly one user** and has a **start date and time**, an optional **duration** and an optional **note**. There are no workout plans or templates: the system only records what was done.

A workout contains **exercise entries** in the **order** they were performed. Each entry refers to **exactly one exercise**, which must be a system exercise or one created by the workout's owner. The same exercise may appear more than once in the same workout.

Each entry has one or more **sets**, also in **order**. Every set has a **type** — **warm-up** or **working** — and may be marked as **failed**, meaning a repetition was attempted and not completed. Depending on the exercise's measurement type, a set records **repetitions**, **load**, **duration** and/or **distance**; fields that do not apply to the exercise stay empty, and fields that do apply cannot be negative. For unilateral exercises, a set may record the **side** (left or right).

Units are fixed: load in **kilograms**, distance in **meters**, duration in **seconds**. Converting to pounds, kilometers or minutes is a display concern.

Each user also records their **body weight** by **date**, at most once per day. With it, the real load of bodyweight exercises can be calculated for any workout date.

Values derived from the records are **never stored**: estimated one-rep max, volume (sets × reps × load), personal records and running pace all come from queries.

The system records **when** each user, exercise, workout and body weight entry was **created** and **last updated**, so other tools (including AI agents) can find out what changed since a given moment.

When a workout is deleted, **all of its entries and sets are deleted with it**. An exercise created by a user **cannot be deleted while any workout uses it**. System exercises are never deleted by users.

## Out of scope for now

- Workout plans, templates and routines.
- Rest time between sets and repetition tempo: recording them by hand is tedious; they can be added later without breaking the model.
- Subjective effort (RPE, RIR).
- Stretching and mobility work, and physical assessments (e.g. straight-leg raise): only training is recorded for now.
- Cardio metrics beyond distance and duration (heart rate, calories, elevation, GPS track): they can be added later as optional set attributes.
- Changing the measurement type of an exercise already used in workouts: for now it is fixed after the first use; converting an exercise (and its sets) may become an explicit operation later.
- Per-muscle activation percentages: the primary/secondary role is enough; how much each role counts (e.g. 1 set for primary, 0.5 for secondary) is decided in queries, not stored.
- Password and login (depend on the authentication decision; the plan is to sign in with Google).
- Sharing workouts between users.
- A frontend: the project is backend only, consumed through its API (scripts, AI agents, future apps).
