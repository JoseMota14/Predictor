using Microsoft.ML;
using System;

namespace FootballPredictor.ML
{
    internal class Trainer
    {
        private readonly MLContext _mlContext;
        private DataViewSchema _trainSchema;

        public Trainer()
        {
            _mlContext = new MLContext(seed: 0); // Garantir resultados reprodutíveis
        }

        public ITransformer TrainMatchResultModel(IDataView trainingData)
        {
            // Store the schema for later use when saving the model
            _trainSchema = trainingData.Schema;

            var pipeline = _mlContext.Transforms.Concatenate("Features",
                    "HomeRanking", "AwayRanking", "HomeGoalsAvg", "AwayGoalsAvg", "Rivalry")
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("HomeTeamEncoded", "HomeTeam"))
                .Append(_mlContext.Transforms.Categorical.OneHotEncoding("AwayTeamEncoded", "AwayTeam"))
                .Append(_mlContext.Transforms.Concatenate("AllFeatures", "Features", "HomeTeamEncoded", "AwayTeamEncoded"))
                .Append(_mlContext.Transforms.Conversion.MapValueToKey("Label", "Result")) // Chave para treino
                .Append(_mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Label", "AllFeatures"))
                .Append(_mlContext.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            Console.WriteLine("Treinando o modelo...");
            var model = pipeline.Fit(trainingData);
            Console.WriteLine("Modelo treinado com sucesso.");
            return model;
        }

        public void EvaluateModel(ITransformer model, IDataView testData)
        {
            var predictions = model.Transform(testData);
            var metrics = _mlContext.MulticlassClassification.Evaluate(predictions, labelColumnName: "Label", scoreColumnName: "Score");

            Console.WriteLine("Avaliação do Modelo:");
            Console.WriteLine($"Micro Accuracy: {metrics.MicroAccuracy:F2}");
            Console.WriteLine($"Macro Accuracy: {metrics.MacroAccuracy:F2}");
            Console.WriteLine($"Log Loss: {metrics.LogLoss:F2}");
            Console.WriteLine($"Log Loss Reduction: {metrics.LogLossReduction:F2}");
        }

        public void SaveModel(ITransformer model, string modelPath)
        {
            if (_trainSchema == null)
            {
                throw new InvalidOperationException("Model must be trained before saving. Train the model first by calling TrainMatchResultModel.");
            }

            _mlContext.Model.Save(model, _trainSchema, modelPath);
            Console.WriteLine($"Modelo salvo em: {modelPath}");
        }

        public ITransformer LoadModel(string modelPath, out DataViewSchema schema)
        {
            var model = _mlContext.Model.Load(modelPath, out schema);
            Console.WriteLine($"Modelo carregado de: {modelPath}");
            return model;
        }
    }
}
