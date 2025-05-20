using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predictor.Models
{
    internal class HeadToHeadStats
    {
        public string Team1 { get; private set; }
        public string Team2 { get; private set; }
        public int TotalMatches { get; set; }
        public int Team1Wins { get; set; }
        public int Team2Wins { get; set; }
        public int Draws { get; set; }
        public int Team1Goals { get; set; }
        public int Team2Goals { get; set; }

        public HeadToHeadStats(string team1, string team2)
        {
            Team1 = team1;
            Team2 = team2;
            TotalMatches = 0;
            Team1Wins = 0;
            Team2Wins = 0;
            Draws = 0;
            Team1Goals = 0;
            Team2Goals = 0;
        }

        // Calcula a taxa de vitória do time 1 contra o time 2
        public double Team1WinRate => TotalMatches > 0 ? (double)Team1Wins / TotalMatches : 0;

        // Calcula a taxa de vitória do time 2 contra o time 1
        public double Team2WinRate => TotalMatches > 0 ? (double)Team2Wins / TotalMatches : 0;

        // Retorna o time dominante nos confrontos diretos
        public string DominantTeam
        {
            get
            {
                if (Team1Wins > Team2Wins)
                    return Team1;
                else if (Team2Wins > Team1Wins)
                    return Team2;
                else
                    return "Nenhum (equilibrado)";
            }
        }

        public override string ToString()
        {
            return $"Confrontos diretos - {Team1} vs {Team2}:\n" +
                   $"Total de jogos: {TotalMatches}\n" +
                   $"Vitórias de {Team1}: {Team1Wins} ({Team1WinRate:P1})\n" +
                   $"Vitórias de {Team2}: {Team2Wins} ({Team2WinRate:P1})\n" +
                   $"Empates: {Draws} ({(double)Draws / TotalMatches:P1})\n" +
                   $"Gols de {Team1}: {Team1Goals} (média: {(double)Team1Goals / TotalMatches:F1})\n" +
                   $"Gols de {Team2}: {Team2Goals} (média: {(double)Team2Goals / TotalMatches:F1})";
        }
    }
}
