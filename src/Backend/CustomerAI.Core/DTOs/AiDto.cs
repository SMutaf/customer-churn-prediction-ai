using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace CustomerAI.Core.DTOs
{
    public class AiRequestDto
    {
        public int customer_id { get; set; }
        public string sector { get; set; }
        public double total_spend { get; set; }
        public int membership_days { get; set; }
        public int recency_days { get; set; }
        public int order_count { get; set; }
        public double average_order_value { get; set; }
        public double average_order_gap_days { get; set; }
        public double purchase_frequency { get; set; }
        public float last_interaction_score { get; set; }
        public float average_sentiment_score { get; set; }
        public int interaction_count { get; set; }
        public int complaint_count { get; set; }
        public double spend_last_30_days { get; set; }
        public double spend_last_90_days { get; set; }
        public double spend_drop_rate { get; set; }
    }

    public class AiResponseDto
    {
        public int customer_id { get; set; }
        public double churn_probability { get; set; }
        public int predicted_class { get; set; }
        public double confidence_score { get; set; }
        public ModelExplanationDto? model_explanations { get; set; }
        public JsonElement top_feature_impacts { get; set; }
        public string model_version { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public string message { get; set; } = string.Empty;

        // Backward-compatible fields for existing dashboard/API consumers.
        public double churn_risk_score { get; set; }
        public string segment { get; set; }
        public string ai_advice { get; set; }
        public string main_reason { get; set; }
    }

    public class ModelExplanationDto
    {
        [JsonPropertyName("method")]
        public string Method { get; set; } = string.Empty;

        [JsonPropertyName("base_value")]
        public double? BaseValue { get; set; }

        [JsonPropertyName("prediction_value")]
        public double? PredictionValue { get; set; }

        [JsonPropertyName("top_positive_factors")]
        public List<ModelExplanationFactorDto> TopPositiveFactors { get; set; } = new();

        [JsonPropertyName("top_negative_factors")]
        public List<ModelExplanationFactorDto> TopNegativeFactors { get; set; } = new();

        [JsonPropertyName("raw_shap_values")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Dictionary<string, double>? RawShapValues { get; set; }

        [JsonPropertyName("error")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Error { get; set; }

        [JsonPropertyName("top_feature_impacts")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public JsonElement TopFeatureImpacts { get; set; }
    }

    public class ModelExplanationFactorDto
    {
        [JsonPropertyName("feature")]
        public string Feature { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public double Value { get; set; }

        [JsonPropertyName("shap_value")]
        public double ShapValue { get; set; }

        [JsonPropertyName("impact_direction")]
        public string ImpactDirection { get; set; } = string.Empty;

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; } = string.Empty;
    }
}
