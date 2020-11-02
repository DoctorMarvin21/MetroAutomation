using LiteDB;
using MetroAutomation.ExpressionEvaluation;
using System;

namespace MetroAutomation.Calibration
{
    [Serializable]
    public class RangeInfo
    {
        [NonSerialized]
        private Evaluator evaluator;

        [NonSerialized]
        private bool expressionValid = true;

        public string Output { get; set; }

        public string Alias { get; set; }

        public string ErrorExpression { get; set; }

        [BsonIgnore]
        public Evaluator Evaluator
        {
            get
            {
                if (evaluator == null && expressionValid && !string.IsNullOrWhiteSpace(ErrorExpression))
                {
                    try
                    {
                        evaluator = new Evaluator(ErrorExpression);
                    }
                    catch
                    {
                        expressionValid = false;
                    }
                }

                return evaluator;
            }
        }

        public BaseValueInfo Range { get; set; }

        public ValueRange[] ComponentsRanges { get; set; }
    }
}
