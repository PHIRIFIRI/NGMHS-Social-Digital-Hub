using NGMHS.Models;

namespace NGMHS.Services;

public class FormTemplateService : IFormTemplateService
{
    // Pre-built templates requested for social work reporting.
    private static readonly List<FormTemplateDefinition> Templates =
    [
        new FormTemplateDefinition(
            "FORM2",
            "Form 2 - Intake Assessment",
            "1. Client background\n2. Presenting concern\n3. Initial intervention\n4. Referral needs\n5. Social worker signature"),

        new FormTemplateDefinition(
            "FORM7",
            "Form 7 - Home Visit Report",
            "1. Visit date and location\n2. Household composition\n3. Safety and risk observations\n4. Immediate action taken\n5. Follow-up plan"),

        new FormTemplateDefinition(
            "FORM33",
            "Form 33 - Court Preparation",
            "1. Case summary for magistrate\n2. Evidence and attachments\n3. Child/family welfare observations\n4. Recommendation to court\n5. Social worker declaration"),

        new FormTemplateDefinition(
            "FORM56",
            "Form 56 - Department Submission",
            "1. Department receiving report (DOH/DBE/DSD)\n2. Beneficiary details\n3. Services rendered\n4. Outcome targets\n5. Supporting documents"),

        new FormTemplateDefinition(
            "FORM77",
            "Form 77 - Case Closure",
            "1. Final progress summary\n2. Closure reason\n3. Long-term recommendations\n4. Stakeholder notifications\n5. Closure approval"),
    ];

    public IReadOnlyList<FormTemplateDefinition> GetTemplates() => Templates;

    public FormTemplateDefinition? GetTemplateByCode(string code)
    {
        return Templates.FirstOrDefault(t =>
            string.Equals(t.Code, code, StringComparison.OrdinalIgnoreCase));
    }
}
