using FootballPredictor.ML;
using Microsoft.ML;
using Predictor.Data;
using Predictor.Models;

Console.WriteLine("Hello, World!");

Console.WriteLine("Sistema de Previsão da Liga Portuguesa de Futebol");
Console.WriteLine("=================================================");

// Inicializar processador de dados
DataProcessor dataProcessor = new DataProcessor();

// Carregar dados (substitua pelo caminho do seu arquivo CSV)
string dataPath = "portuguese_league_data.csv";

// Verificar se o arquivo existe
if (!File.Exists(dataPath))
{
    Console.WriteLine($"Arquivo de dados não encontrado: {dataPath}");
    Console.WriteLine("Gerando dados fictícios para demonstração...");

    // Dados fictícios para demonstração
    GenerateDummyData(dataPath);
    Console.WriteLine($"Arquivo de dados fictícios gerado: {dataPath}");
}

dataProcessor.LoadMatchData(dataPath);

// Obter todas as equipes do dataset
var allTeams = dataProcessor.GetAllTeams().ToList();
Console.WriteLine($"\nEquipes encontradas no conjunto de dados: {string.Join(", ", allTeams)}");

// Verificar se temos dados suficientes
if (dataProcessor.HistoricalMatches.Count < 10)
{
    Console.WriteLine("Não há dados suficientes para treinar o modelo.");
    return;
}

// Dividir dados em conjuntos de treino e teste
var mlContext = new MLContext(seed: 0);
IDataView dataView = mlContext.Data.LoadFromEnumerable(dataProcessor.HistoricalMatches);
var dataSplit = mlContext.Data.TrainTestSplit(dataView, testFraction: 0.2);

// Treinar modelo
Trainer modelTrainer = new Trainer();
var model = modelTrainer.TrainMatchResultModel(dataSplit.TrainSet);

// Avaliar modelo
modelTrainer.EvaluateModel(model, dataSplit.TestSet);

// Salvar modelo (opcional)
string modelPath = "portuguese_league_model.zip";
modelTrainer.SaveModel(model, modelPath);

// Fazer previsões
Predictors predictor = new Predictors(model);

// Simular o resto da temporada
Console.WriteLine("\nSimulando jogos da temporada...");
League projectedTable = predictor.SimulateSeason(allTeams);

// Exibir tabela projetada
projectedTable.DisplayTable();

Console.WriteLine("\nPressione qualquer tecla para sair...");
Console.ReadKey();

static void GenerateDummyData(string filePath)
{
    List<string> teams = new List<string>
    {
        "Benfica", "Porto", "Sporting", "Braga",
        "Vitória SC", "Famalicão", "Rio Ave", "Boavista",
        "Santa Clara", "Moreirense", "Paços Ferreira", "Marítimo",
        "Gil Vicente", "Belenenses", "Tondela", "Portimonense",
        "Nacional", "Farense"
    };

    Random random = new Random(0);
    List<string> lines = new List<string>
    {
        "Season,Date,HomeTeam,AwayTeam,HomeTeamGoals,AwayTeamGoals,Result,HomeTeamShots,AwayTeamShots,HomeTeamShotsOnTarget,AwayTeamShotsOnTarget,HomeTeamCorners,AwayTeamCorners,HomeTeamFouls,AwayTeamFouls,HomeTeamYellowCards,AwayTeamYellowCards,HomeTeamRedCards,AwayTeamRedCards"
    };

    // Gerar dados para as últimas três temporadas
    for (int season = 2022; season <= 2024; season++)
    {
        for (int i = 0; i < teams.Count; i++)
        {
            for (int j = 0; j < teams.Count; j++)
            {
                if (i == j) continue;

                string homeTeam = teams[i];
                string awayTeam = teams[j];

                // Definir uma tendência baseada na força relativa das equipes
                int homeStrength = teams.Count - i;
                int awayStrength = teams.Count - j;

                // Adicionar aleatoriedade aos resultados
                int homeGoals = Math.Max(0, random.Next(homeStrength / 3 + 1) + random.Next(-1, 2));
                int awayGoals = Math.Max(0, random.Next(awayStrength / 3 + 1) + random.Next(-1, 2));

                // Estatísticas do jogo
                int homeShots = homeGoals * 3 + random.Next(5, 15);
                int awayShots = awayGoals * 3 + random.Next(5, 15);
                int homeShotsOnTarget = Math.Min(homeShots, homeGoals + random.Next(1, 6));
                int awayShotsOnTarget = Math.Min(awayShots, awayGoals + random.Next(1, 6));
                int homeCorners = random.Next(3, 10);
                int awayCorners = random.Next(3, 10);
                int homeFouls = random.Next(8, 18);
                int awayFouls = random.Next(8, 18);
                int homeYellowCards = random.Next(0, 4);
                int awayYellowCards = random.Next(0, 4);
                int homeRedCards = random.Next(0, 100) < 10 ? 1 : 0;
                int awayRedCards = random.Next(0, 100) < 10 ? 1 : 0;

                // Data fictícia
                int month = random.Next(8, 13);
                if (month == 12 && random.Next(2) == 0) month = random.Next(1, 6);
                int day = random.Next(1, 29);
                string date = $"{season}-{month:D2}-{day:D2}";

                // Adicionar linha de dados
                lines.Add($"{season},{date},{homeTeam},{awayTeam},{homeGoals},{awayGoals},," +
                         $"{homeShots},{awayShots},{homeShotsOnTarget},{awayShotsOnTarget}," +
                         $"{homeCorners},{awayCorners},{homeFouls},{awayFouls}," +
                         $"{homeYellowCards},{awayYellowCards},{homeRedCards},{awayRedCards}");
            }
        }
    }

    File.WriteAllLines(filePath, lines);
}