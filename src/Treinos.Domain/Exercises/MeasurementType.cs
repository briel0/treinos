namespace Treinos.Domain.Exercises;

/// <summary>Como as séries de um exercício são medidas; decide quais medidas cada série preenche.</summary>
public enum MeasurementType {
    /// <summary>Carga e repetições (supino, rosca).</summary>
    WeightReps,

    /// <summary>Peso corporal: repetições, com carga adicional ou assistência opcionais (barra fixa, paralela).</summary>
    Bodyweight,

    /// <summary>Só duração (prancha).</summary>
    Time,

    /// <summary>Distância e duração: é como o cardio entra (corrida, bike, remo).</summary>
    DistanceTime,
}
