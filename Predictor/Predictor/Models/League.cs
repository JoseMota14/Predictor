using Predictor.Models.TeamData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predictor.Models
{
    internal class League
    {
        private Dictionary<string, Team> _teamStats;

        public League()
        {
            _teamStats = new Dictionary<string, Team>();
        }

        public void AddTeam(string teamName)
        {
            if (!_teamStats.ContainsKey(teamName))
                _teamStats[teamName] = new Team(teamName);
        }

        public void AddMatch(string homeTeam, string awayTeam, int homeGoals, int awayGoals)
        {
            AddTeam(homeTeam);
            AddTeam(awayTeam);

            _teamStats[homeTeam].UpdateStats(homeGoals, awayGoals);
            _teamStats[awayTeam].UpdateStats(awayGoals, homeGoals);
        }

        public List<Team> GetTable()
        {
            return _teamStats.Values
                .OrderByDescending(t => t.Points)
                .ThenByDescending(t => t.GoalDifference)
                .ThenByDescending(t => t.GoalsFor)
                .ThenBy(t => t.TeamName)
                .ToList();
        }

        public void DisplayTable()
        {
            List<Team> table = GetTable();

            Console.WriteLine("\nTabela da Liga Portuguesa:");
            Console.WriteLine("Pos | Equipe             | J  | V  | E  | D  | GM | GS | GD | Pts");
            Console.WriteLine("----+--------------------+----+----+----+----+----+----+----+----");

            for (int i = 0; i < table.Count; i++)
            {
                Team team = table[i];
                Console.WriteLine($"{i + 1,3} | {team.TeamName,-18} | {team.GamesPlayed,2} | {team.Wins,2} | {team.Draws,2} | {team.Losses,2} | {team.GoalsFor,2} | {team.GoalsAgainst,2} | {team.GoalDifference,3} | {team.Points,3}");
            }
        }
    }
}
