namespace NGMHS.ViewModels;

public class DashboardViewModel
{
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;

    public int ActiveForms { get; set; }
    public int CompletedForms { get; set; }
    public int SubmittedForms { get; set; }

    public int OpenExternalQueries { get; set; }
    public int DraftOutreachLetters { get; set; }

    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);
}
