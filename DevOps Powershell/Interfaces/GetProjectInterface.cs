using DevOps_Powershell.Models;
using DevOps_Powershell.Models.Input_Model;

namespace DevOps_Powershell.Interfaces
{
   
        public interface IProjectService
        {
            Task<IEnumerable<Project>> GetAllProjectsAsync();
  
            Task<IEnumerable<Project>> GetProjectsAsync(GetProjectIn getProjectIn);

            Task<List<Dictionary<string, object>>> GetUserEntitlementsAsync(GetProjectIn getProjectIn);

            Task<List<ReleasePipeline>> GetPipelineInfoAsync(GetProjectIn getProjectIn);

            Task<List<BuildPipeline>> GetBuildPipelineInfoAsync(GetProjectIn getProjectIn);

        Task<List<Repository>> GetRepoCommitInfoAsync(GetProjectIn getProjectIn);


    }

    
}
