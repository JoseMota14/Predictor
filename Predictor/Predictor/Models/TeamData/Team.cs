using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predictor.Models.TeamData
{
    public class Team
    {
        public string TeamName { get; set; }
        public int GamesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int Points => Wins * 3 + Draws;
        public int GoalDifference => GoalsFor - GoalsAgainst;

        public Team(string teamName)
        {
            TeamName = teamName;
            GamesPlayed = 0;
            Wins = 0;
            Draws = 0;
            Losses = 0;
            GoalsFor = 0;
            GoalsAgainst = 0;
        }

        public void UpdateStats(int goalsFor, int goalsAgainst)
        {
            GamesPlayed++;
            GoalsFor += goalsFor;
            GoalsAgainst += goalsAgainst;

            if (goalsFor > goalsAgainst)
                Wins++;
            else if (goalsFor == goalsAgainst)
                Draws++;
            else
                Losses++;
        }
    }
}
