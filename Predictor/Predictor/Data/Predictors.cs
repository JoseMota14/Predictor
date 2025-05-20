using Microsoft.ML;
using Predictor.Models;

namespace Predictor.Data
{
    internal class Predictors
    {
        private readonly MLContext _mlContext;
        private readonly ITransformer _model;
        private readonly PredictionEngine<MatchData, MatchPrediction> _predictionEngine;

        public Predictors(ITransformer model)
        {
            _mlContext = new MLContext(seed: 0);
            _model = model;
            _predictionEngine = _mlContext.Model.CreatePredictionEngine<MatchData, MatchPrediction>(model);
        }

        public MatchPrediction PredictMatch(MatchData matchData)
        {
            return _predictionEngine.Predict(matchData);
        }

        // Simula uma temporada completa do campeonato
        public League SimulateSeason(List<string> teams)
        {
            League table = new League();

            // Para cada par de equipes, simula partidas de ida e volta
            for (int i = 0; i < teams.Count; i++)
            {
                for (int j = 0; j < teams.Count; j++)
                {
                    if (i == j) continue; // Uma equipe não joga contra si mesma

                    string homeTeam = teams[i];
                    string awayTeam = teams[j];

                    // Dados de jogo para prever (podemos usar médias ou valores típicos para estatísticas que não temos)
                    var matchData = new MatchData
                    {
                        HomeTeam = homeTeam,
                        AwayTeam = awayTeam,
                        HomeTeamShots = 12, // Valores médios como placeholder
                        AwayTeamShots = 10,
                        HomeTeamShotsOnTarget = 5,
                        AwayTeamShotsOnTarget = 4,
                        HomeTeamCorners = 6,
                        AwayTeamCorners = 5,
                        HomeTeamFouls = 12,
                        AwayTeamFouls = 14,
                        HomeTeamYellowCards = 2,
                        AwayTeamYellowCards = 3,
                        HomeTeamRedCards = 0.1f,
                        AwayTeamRedCards = 0.2f
                    };

                    MatchPrediction prediction = PredictMatch(matchData);

                    // Converter resultado previsto em gols (simplificado)
                    int homeGoals = 0;
                    int awayGoals = 0;

                    switch (prediction.PredictedResult)
                    {
                        case "H": // Home win
                            homeGoals = new Random().Next(1, 4);
                            awayGoals = new Random().Next(0, homeGoals);
                            break;
                        case "A": // Away win
                            awayGoals = new Random().Next(1, 4);
                            homeGoals = new Random().Next(0, awayGoals);
                            break;
                        case "D": // Draw
                            homeGoals = new Random().Next(0, 3);
                            awayGoals = homeGoals;
                            break;
                    }

                    // Adicionar os resultados à tabela
                    table.AddMatch(homeTeam, awayTeam, homeGoals, awayGoals);

                    Console.WriteLine($"{homeTeam} {homeGoals} x {awayGoals} {awayTeam} - Previsão: {prediction.PredictedResult}");
                }
            }

            return table;
        }
    }
}
