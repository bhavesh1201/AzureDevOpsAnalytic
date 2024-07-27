namespace DevOps_Powershell.Models
{
    public class BuildPipeline
    {
        
            public string ProjectName { get; set; }
            public string PipelineName { get; set; }
            public DateTime? LastBuildOn { get; set; }
            public string LastBuildStatus { get; set; }
            public string LastBuildResult { get; set; }
            public string LastBuildRange { get; set; }
            public DateTime LastEditDate { get; set; }
            public bool IsHostedAgent { get; set; }
       
    }
}
