using System.Collections.Generic;
using System.Linq;

namespace MAAME.DROMO.PARTOGRAPH.APP.Droid.Services
{
    public enum ValidationSeverity
    {
        Info,       // Informational message
        Warning,    // Allows save but shows warning
        Error       // Blocks save
    }

    public class ValidationMessage
    {
        public ValidationSeverity Severity { get; set; }
        public string Field { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Suggestion { get; set; } = string.Empty;

        public string SeverityIcon => Severity switch
        {
            ValidationSeverity.Error => "❌",
            ValidationSeverity.Warning => "⚠️",
            ValidationSeverity.Info => "ℹ️",
            _ => "•"
        };

        public string SeverityColor => Severity switch
        {
            ValidationSeverity.Error => "#EF5350",
            ValidationSeverity.Warning => "#FF9800",
            ValidationSeverity.Info => "#2196F3",
            _ => "#9E9E9E"
        };
    }

    public class ValidationResult
    {
        public bool IsValid => !Messages.Any(m => m.Severity == ValidationSeverity.Error);
        public bool HasWarnings => Messages.Any(m => m.Severity == ValidationSeverity.Warning);
        public List<ValidationMessage> Messages { get; set; } = new List<ValidationMessage>();

        public List<ValidationMessage> Errors => Messages.Where(m => m.Severity == ValidationSeverity.Error).ToList();
        public List<ValidationMessage> Warnings => Messages.Where(m => m.Severity == ValidationSeverity.Warning).ToList();
        public List<ValidationMessage> Infos => Messages.Where(m => m.Severity == ValidationSeverity.Info).ToList();

        public void AddError(string field, string message, string suggestion = "")
        {
            Messages.Add(new ValidationMessage
            {
                Severity = ValidationSeverity.Error,
                Field = field,
                Message = message,
                Suggestion = suggestion
            });
        }

        public void AddWarning(string field, string message, string suggestion = "")
        {
            Messages.Add(new ValidationMessage
            {
                Severity = ValidationSeverity.Warning,
                Field = field,
                Message = message,
                Suggestion = suggestion
            });
        }

        public void AddInfo(string field, string message, string suggestion = "")
        {
            Messages.Add(new ValidationMessage
            {
                Severity = ValidationSeverity.Info,
                Field = field,
                Message = message,
                Suggestion = suggestion
            });
        }

        public string GetSummary()
        {
            if (IsValid && !HasWarnings)
                return "All validations passed";

            var summary = new List<string>();
            var errorCount = Errors.Count;
            var warningCount = Warnings.Count;

            if (errorCount > 0)
                summary.Add($"{errorCount} error{(errorCount > 1 ? "s" : "")}");
            if (warningCount > 0)
                summary.Add($"{warningCount} warning{(warningCount > 1 ? "s" : "")}");

            return string.Join(", ", summary);
        }
    }
}
