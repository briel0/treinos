namespace Treinos.Domain.Exercises;

/// <summary>Papel de um grupo muscular num exercício.</summary>
public enum MuscleRole {
    Primary,
    Secondary,
}

/// <summary>Um grupo muscular trabalhado por um exercício, com o seu papel.</summary>
public sealed record ExerciseMuscle(MuscleGroup MuscleGroup, MuscleRole Role);
