using NGMHS.Models;

namespace NGMHS.Services;

public interface IFormTemplateService
{
    IReadOnlyList<FormTemplateDefinition> GetTemplates();
    FormTemplateDefinition? GetTemplateByCode(string code);
}
