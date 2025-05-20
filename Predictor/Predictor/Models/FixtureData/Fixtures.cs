using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predictor.Models.FixtureData
{
    public class Fixtures
    {
        public static List<Fixture> LoadFixtures() => new List<Fixture> // Placeholder
        {
            new Fixture { HomeTeam = "TeamA", AwayTeam = "TeamB" },
            new Fixture { HomeTeam = "TeamC", AwayTeam = "TeamD" }
        };
    }
}
