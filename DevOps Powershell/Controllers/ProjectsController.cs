using DevOps_Powershell.Interfaces;
using DevOps_Powershell.Models;
using DevOps_Powershell.Models.Input_Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace DevOps_Powershell.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly ISaveProjectExcel saveProjectExcel;

        public ProjectsController(IProjectService projectService, ISaveProjectExcel saveProjectExcel)
        {
            _projectService = projectService;
            this.saveProjectExcel = saveProjectExcel;
        }



        [HttpGet("GetProjects")]
        public async Task<ActionResult<IEnumerable<Project>>> GetProjectsA([FromQuery] GetProjectIn getProjectIn)
        {
            try
            {
                IEnumerable<Project> projects = await _projectService.GetProjectsAsync(getProjectIn);
               // var temp = saveProjectExcel.SaveObjectsToExcelAsync(projects);

                return Ok(projects);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetAllUser")]
        public async Task<ActionResult<IEnumerable<Project>>> GetAllUser([FromQuery] GetProjectIn getProjectIn)
        {
            try
            {
              var User = await _projectService.GetUserEntitlementsAsync(getProjectIn);



                var userEntitlementObjects = User.Select(entitlement =>
                {
                    return new User
                    {
                        LastAccessedDate = (DateTime)entitlement["LastAccessedDate"],
                        DateCreated = (DateTime)entitlement["DateCreated"],
                        LicenseDisplayName = (string)entitlement["LicenseDisplayName"],
                        MailAddress = (string)entitlement["MailAddress"],
                        UserName = (string)entitlement["UserName"],
                        SubjectKind = (string)entitlement["SubjectKind"],
                        PrincipalName = (string)entitlement["PrincipalName"],
                        Range = (string)entitlement["Range"]
                    };
                }).ToList();

                //var temp = saveProjectExcel.SaveObjectsToExcelAsync(userEntitlementObjects);
             return Ok(User);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }


        [HttpGet("GetRelease")]
        public async Task<ActionResult<IEnumerable<Project>>> GetRelease([FromQuery] GetProjectIn getProjectIn)
        {
            try
            {
                var projects = await _projectService.GetPipelineInfoAsync(getProjectIn);
                // var temp = saveProjectExcel.SaveObjectsToExcelAsync(projects);

                return Ok(projects);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetBuilds")]
        public async Task<ActionResult<IEnumerable<Project>>> GetBuilds([FromQuery] GetProjectIn getProjectIn)
        {
            try
            {
                var projects = await _projectService.GetBuildPipelineInfoAsync(getProjectIn);
                // var temp = saveProjectExcel.SaveObjectsToExcelAsync(projects);

                return Ok(projects);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("GetRepository")]
        public async Task<ActionResult<IEnumerable<Project>>> GetRepository([FromQuery] GetProjectIn getProjectIn)
        {
            try
            {
                var projects = await _projectService.GetRepoCommitInfoAsync(getProjectIn);
                // var temp = saveProjectExcel.SaveObjectsToExcelAsync(projects);

                return Ok(projects);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

    }
}
