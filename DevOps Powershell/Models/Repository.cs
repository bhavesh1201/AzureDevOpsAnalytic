namespace DevOps_Powershell.Models
{
    public class Repository
    {
        public string ProjectName { get; set; }
        public string RepoName { get; set; }
        public DateTime? CommitDate { get; set; }
        public string CommitRange { get; set; }
    }
}
