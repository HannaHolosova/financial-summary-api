using System.Text.Json.Serialization;

namespace FinancialSummaryApi.V1.Domain
{

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TypeOfStatement
    {
        Quaterly,
        Yearly
    }
}
