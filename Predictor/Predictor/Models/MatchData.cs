using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predictor.Models
{
    public class MatchData
    {
        [LoadColumn(0)]
        public string Season { get; set; }

        [LoadColumn(1)]
        public string Date { get; set; }

        [LoadColumn(2)]
        public string HomeTeam { get; set; }

        [LoadColumn(3)]
        public string AwayTeam { get; set; }

        [LoadColumn(4)]
        public float HomeTeamGoals { get; set; }

        [LoadColumn(5)]
        public float AwayTeamGoals { get; set; }

        [LoadColumn(6)]
        public string Result { get; set; }

        // Estatísticas adicionais que podem ser usadas
        [LoadColumn(7)]
        public float HomeTeamShots { get; set; }

        [LoadColumn(8)]
        public float AwayTeamShots { get; set; }

        [LoadColumn(9)]
        public float HomeTeamShotsOnTarget { get; set; }

        [LoadColumn(10)]
        public float AwayTeamShotsOnTarget { get; set; }

        [LoadColumn(11)]
        public float HomeTeamCorners { get; set; }

        [LoadColumn(12)]
        public float AwayTeamCorners { get; set; }

        [LoadColumn(13)]
        public float HomeTeamFouls { get; set; }

        [LoadColumn(14)]
        public float AwayTeamFouls { get; set; }

        [LoadColumn(15)]
        public float HomeTeamYellowCards { get; set; }

        [LoadColumn(16)]
        public float AwayTeamYellowCards { get; set; }

        [LoadColumn(17)]
        public float HomeTeamRedCards { get; set; }

        [LoadColumn(18)]
        public float AwayTeamRedCards { get; set; }

        // Métricas de confrontos diretos
        [LoadColumn(19)]
        public float H2HHomeWinRate { get; set; }

        [LoadColumn(20)]
        public float H2HAwayWinRate { get; set; }

        [LoadColumn(21)]
        public float H2HDrawRate { get; set; }

        [LoadColumn(22)]
        public float H2HHomeGoalsAvg { get; set; }

        [LoadColumn(23)]
        public float H2HAwayGoalsAvg { get; set; }
    }
}
