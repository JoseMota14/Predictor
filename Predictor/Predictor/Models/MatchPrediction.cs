using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Predictor.Models
{
    public class MatchPrediction
    {
        [ColumnName("Score")]
        public float[] Score { get; set; }

        [ColumnName("PredictedLabel")]
        public string PredictedResult { get; set; }
    }
}
