using Microsoft.ML;
using Predictor.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Predictor.Data
{
    internal class DataProcessor
    {

        private readonly MLContext _mlContext;
        public List<MatchData> HistoricalMatches { get; private set; }
        private Dictionary<string, Dictionary<string, List<MatchData>>> HeadToHeadMatches { get; set; }

        public DataProcessor()
        {
            _mlContext = new MLContext(seed: 0);
            HistoricalMatches = new List<MatchData>();
            HeadToHeadMatches = new Dictionary<string, Dictionary<string, List<MatchData>>>();
        }

        public async Task LoadDataFromApi(string apiUrl, string apiKey)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // Adicionar header de autenticação se necessário
                    if (!string.IsNullOrEmpty(apiKey))
                    {
                        client.DefaultRequestHeaders.Add("X-Auth-Token", apiKey);
                    }

                    Console.WriteLine($"Conectando à API em: {apiUrl}");
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();
                        ProcessApiData(jsonResponse);
                        Console.WriteLine($"Dados obtidos com sucesso da API. Total de jogos: {HistoricalMatches.Count}");
                    }
                    else
                    {
                        Console.WriteLine($"Falha ao obter dados da API. Status: {response.StatusCode}");
                        throw new Exception($"Falha na API. Status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar dados da API: {ex.Message}");
                throw;
            }
        }

        private void ProcessApiData(string jsonData)
        {
            try
            {
                // Usando System.Text.Json para processar o JSON
                using (JsonDocument document = JsonDocument.Parse(jsonData))
                {
                    JsonElement root = document.RootElement;

                    // Este é apenas um exemplo - a estrutura real dependerá da API que você está usando
                    if (root.TryGetProperty("matches", out JsonElement matches))
                    {
                        foreach (JsonElement match in matches.EnumerateArray())
                        {
                            MatchData matchData = new MatchData
                            {
                                Season = GetStringProperty(match, "season"),
                                Date = GetStringProperty(match, "utcDate").Split('T')[0], // Converter para apenas a data
                                HomeTeam = GetNestedStringProperty(match, "homeTeam", "name"),
                                AwayTeam = GetNestedStringProperty(match, "awayTeam", "name"),
                                HomeTeamGoals = GetNestedIntProperty(match, "score", "fullTime", "homeTeam"),
                                AwayTeamGoals = GetNestedIntProperty(match, "score", "fullTime", "awayTeam")
                            };

                            // Determinar o resultado com base nos gols
                            matchData.Result = DetermineResult(matchData.HomeTeamGoals, matchData.AwayTeamGoals);

                            // Tentar extrair estatísticas adicionais se disponíveis
                            if (match.TryGetProperty("stats", out JsonElement stats))
                            {
                                // Estatísticas da equipe da casa
                                JsonElement homeStats = stats.GetProperty("home");
                                matchData.HomeTeamShots = GetFloatProperty(homeStats, "shots", 0);
                                matchData.HomeTeamShotsOnTarget = GetFloatProperty(homeStats, "shotsOnTarget", 0);
                                matchData.HomeTeamCorners = GetFloatProperty(homeStats, "corners", 0);
                                matchData.HomeTeamFouls = GetFloatProperty(homeStats, "fouls", 0);
                                matchData.HomeTeamYellowCards = GetFloatProperty(homeStats, "yellowCards", 0);
                                matchData.HomeTeamRedCards = GetFloatProperty(homeStats, "redCards", 0);

                                // Estatísticas da equipe visitante
                                JsonElement awayStats = stats.GetProperty("away");
                                matchData.AwayTeamShots = GetFloatProperty(awayStats, "shots", 0);
                                matchData.AwayTeamShotsOnTarget = GetFloatProperty(awayStats, "shotsOnTarget", 0);
                                matchData.AwayTeamCorners = GetFloatProperty(awayStats, "corners", 0);
                                matchData.AwayTeamFouls = GetFloatProperty(awayStats, "fouls", 0);
                                matchData.AwayTeamYellowCards = GetFloatProperty(awayStats, "yellowCards", 0);
                                matchData.AwayTeamRedCards = GetFloatProperty(awayStats, "redCards", 0);
                            }

                            HistoricalMatches.Add(matchData);
                        }
                    }
                }

                // Após carregar todos os jogos, processamos os confrontos diretos
                ProcessHeadToHeadMatches();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar dados JSON: {ex.Message}");
                throw;
            }
        }

        private string GetStringProperty(JsonElement element, string propertyName, string defaultValue = "")
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind != JsonValueKind.Null)
            {
                return property.GetString() ?? defaultValue;
            }
            return defaultValue;
        }

        private string GetNestedStringProperty(JsonElement element, params string[] propertyPath)
        {
            JsonElement current = element;
            foreach (string prop in propertyPath)
            {
                if (!current.TryGetProperty(prop, out current) || current.ValueKind == JsonValueKind.Null)
                {
                    return "";
                }
            }
            return current.GetString() ?? "";
        }

        private float GetNestedIntProperty(JsonElement element, params string[] propertyPath)
        {
            JsonElement current = element;
            foreach (string prop in propertyPath)
            {
                if (!current.TryGetProperty(prop, out current) || current.ValueKind == JsonValueKind.Null)
                {
                    return 0;
                }
            }
            return current.ValueKind == JsonValueKind.Number ? current.GetInt32() : 0;
        }

        private float GetFloatProperty(JsonElement element, string propertyName, float defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind != JsonValueKind.Null)
            {
                return property.ValueKind == JsonValueKind.Number ? property.GetSingle() : defaultValue;
            }
            return defaultValue;
        }

        public void LoadMatchData(string filePath)
        {
            try
            {
                string[] lines = File.ReadAllLines(filePath);

                // Pular cabeçalho
                for (int i = 1; i < lines.Length; i++)
                {
                    string[] parts = lines[i].Split(',');
                    if (parts.Length >= 19)
                    {
                        HistoricalMatches.Add(new MatchData
                        {
                            Season = parts[0],
                            Date = parts[1],
                            HomeTeam = parts[2],
                            AwayTeam = parts[3],
                            HomeTeamGoals = float.Parse(parts[4]),
                            AwayTeamGoals = float.Parse(parts[5]),
                            Result = DetermineResult(float.Parse(parts[4]), float.Parse(parts[5])),
                            HomeTeamShots = float.TryParse(parts[7], out float hShots) ? hShots : 0,
                            AwayTeamShots = float.TryParse(parts[8], out float aShots) ? aShots : 0,
                            HomeTeamShotsOnTarget = float.TryParse(parts[9], out float hShotsTarget) ? hShotsTarget : 0,
                            AwayTeamShotsOnTarget = float.TryParse(parts[10], out float aShotsTarget) ? aShotsTarget : 0,
                            HomeTeamCorners = float.TryParse(parts[11], out float hCorners) ? hCorners : 0,
                            AwayTeamCorners = float.TryParse(parts[12], out float aCorners) ? aCorners : 0,
                            HomeTeamFouls = float.TryParse(parts[13], out float hFouls) ? hFouls : 0,
                            AwayTeamFouls = float.TryParse(parts[14], out float aFouls) ? aFouls : 0,
                            HomeTeamYellowCards = float.TryParse(parts[15], out float hYCards) ? hYCards : 0,
                            AwayTeamYellowCards = float.TryParse(parts[16], out float aYCards) ? aYCards : 0,
                            HomeTeamRedCards = float.TryParse(parts[17], out float hRCards) ? hRCards : 0,
                            AwayTeamRedCards = float.TryParse(parts[18], out float aRCards) ? aRCards : 0
                        });
                    }
                }

                Console.WriteLine($"Carregados {HistoricalMatches.Count} jogos históricos do arquivo.");

                // Processar confrontos diretos após carregar todos os jogos
                ProcessHeadToHeadMatches();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao carregar dados do arquivo: {ex.Message}");
            }
        }

        private void ProcessHeadToHeadMatches()
        {
            foreach (var match in HistoricalMatches)
            {
                // Garantir que os dicionários para as equipes existam
                if (!HeadToHeadMatches.ContainsKey(match.HomeTeam))
                {
                    HeadToHeadMatches[match.HomeTeam] = new Dictionary<string, List<MatchData>>();
                }

                if (!HeadToHeadMatches.ContainsKey(match.AwayTeam))
                {
                    HeadToHeadMatches[match.AwayTeam] = new Dictionary<string, List<MatchData>>();
                }

                // Adicionar o jogo nos confrontos diretos para ambas as equipes
                if (!HeadToHeadMatches[match.HomeTeam].ContainsKey(match.AwayTeam))
                {
                    HeadToHeadMatches[match.HomeTeam][match.AwayTeam] = new List<MatchData>();
                }

                if (!HeadToHeadMatches[match.AwayTeam].ContainsKey(match.HomeTeam))
                {
                    HeadToHeadMatches[match.AwayTeam][match.HomeTeam] = new List<MatchData>();
                }

                HeadToHeadMatches[match.HomeTeam][match.AwayTeam].Add(match);
                HeadToHeadMatches[match.AwayTeam][match.HomeTeam].Add(match);
            }
        }

        // Obtém histórico de confrontos diretos entre duas equipes
        public List<MatchData> GetHeadToHeadMatches(string team1, string team2)
        {
            if (HeadToHeadMatches.ContainsKey(team1) && HeadToHeadMatches[team1].ContainsKey(team2))
            {
                return HeadToHeadMatches[team1][team2];
            }
            return new List<MatchData>();
        }

        // Calcula estatísticas de confrontos diretos
        public HeadToHeadStats GetHeadToHeadStats(string team1, string team2)
        {
            List<MatchData> h2hMatches = GetHeadToHeadMatches(team1, team2);
            HeadToHeadStats stats = new HeadToHeadStats(team1, team2);

            foreach (var match in h2hMatches)
            {
                if (match.HomeTeam == team1)
                {
                    if (match.HomeTeamGoals > match.AwayTeamGoals)
                        stats.Team1Wins++;
                    else if (match.HomeTeamGoals < match.AwayTeamGoals)
                        stats.Team2Wins++;
                    else
                        stats.Draws++;

                    stats.Team1Goals += (int)match.HomeTeamGoals;
                    stats.Team2Goals += (int)match.AwayTeamGoals;
                }
                else // match.HomeTeam == team2
                {
                    if (match.HomeTeamGoals > match.AwayTeamGoals)
                        stats.Team2Wins++;
                    else if (match.HomeTeamGoals < match.AwayTeamGoals)
                        stats.Team1Wins++;
                    else
                        stats.Draws++;

                    stats.Team1Goals += (int)match.AwayTeamGoals;
                    stats.Team2Goals += (int)match.HomeTeamGoals;
                }

                stats.TotalMatches++;
            }

            return stats;
        }

        private string DetermineResult(float homeGoals, float awayGoals)
        {
            if (homeGoals > awayGoals)
                return "H"; // Home win
            else if (homeGoals < awayGoals)
                return "A"; // Away win
            else
                return "D"; // Draw
        }

        public IEnumerable<string> GetAllTeams()
        {
            HashSet<string> teams = new HashSet<string>();

            foreach (var match in HistoricalMatches)
            {
                teams.Add(match.HomeTeam);
                teams.Add(match.AwayTeam);
            }

            return teams;
        }
    }
}
