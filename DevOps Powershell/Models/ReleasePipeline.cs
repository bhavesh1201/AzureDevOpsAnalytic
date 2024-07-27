namespace DevOps_Powershell.Models
{
    public class ReleasePipeline
    

        {
    public string ProjectName { get; set; }
        public string PipelineName { get; set; }
        public string LastReleaseName { get; set; }
        public DateTime? LastBuildDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastReleaseRange { get; set; }
    
}
}
