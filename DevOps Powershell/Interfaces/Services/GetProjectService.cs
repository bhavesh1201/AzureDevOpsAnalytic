using DevOps_Powershell.Models;
using DevOps_Powershell.Models.Input_Model;
using System.Net.Http.Headers;
using System.Text.Json;
using Newtonsoft.Json.Linq;

namespace DevOps_Powershell.Interfaces.Services
{
    public class ProjectService : IProjectService
    {
        private readonly HttpClient _httpClient;
        private readonly List<Project> _projects = new List<Project>
        {
            new Project
            {
                id = "3c607037-249a-402e-8180-6cb689b79f09",
                name = "da-datasceince-ai-ml",
                description = "Existing ADO Project Name : es-analytics-scrumberger",
                url = "https://dev.azure.com/slb-it/_apis/projects/3c607037-249a-402e-8180-6cb689b79f09",
                state = "wellFormed",
                revision = 11277,
                visibility = "private",
                lastUpdateTime = new DateTime(2024, 3, 4, 10, 35, 51, 720)
            }
        };

        public ProjectService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }



        public Task<IEnumerable<Project>> GetAllProjectsAsync()
        {
            return Task.FromResult<IEnumerable<Project>>(_projects);
        }


        public async Task<IEnumerable<Project>> GetProjectsAsync(GetProjectIn getProjectIn)
        {
            string organization = getProjectIn.OrganizationName;  
            string apiUrl = $"https://dev.azure.com/{organization}/_apis/projects?api-version=7.1-preview.4";

            
            string personalAccessToken =getProjectIn.PAT_Token;

            // Add the PAT to the request headers
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{personalAccessToken}")));

            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
              


                var data = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(data);
                var projectsElement = jsonDocument.RootElement.GetProperty("value");

                var projects = JsonSerializer.Deserialize<IEnumerable<Project>>(projectsElement.GetRawText());
                return projects;


            }
            else
            {
                throw new HttpRequestException($"Error fetching projects: {response.ReasonPhrase}");
            }

        }

        public async Task<List<Dictionary<string, object>>> GetUserEntitlementsAsync(GetProjectIn getProjectIn)
        {
            string organizationUrl = "https://vsaex.dev.azure.com/";
            string org = getProjectIn.OrganizationName;
            string apiVersion = "5.0-preview.2";
            string apiUrl = $"{organizationUrl}{org}/_apis/userentitlements?top=1000000&api-version={apiVersion}";

            // Base64 encode the PAT
            var base64AuthInfo = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{getProjectIn.PAT_Token}"));

            // Headers for the REST API request
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64AuthInfo);

            // Invoke the REST API to get user entitlements data
            var response = await _httpClient.GetStringAsync(apiUrl);
            var jsonResponse = JObject.Parse(response);
            var members = jsonResponse["members"].ToObject<List<JObject>>();


            var userEntitlements = members.Select(m =>
            {
                var lastAccessedDate = DateTime.Parse(m["lastAccessedDate"].ToString());
                var currentDate = DateTime.Now;

                var monthsDifference = Math.Round((currentDate - lastAccessedDate).TotalDays / 30);

                string label;

                if (lastAccessedDate == DateTime.MinValue)
                {
                    var createdDate = DateTime.Parse(m["dateCreated"].ToString());
                    monthsDifference = Math.Round((currentDate - createdDate).TotalDays / 30);

                    label = monthsDifference <= 1 ? "0 - 30 Days" :
                            monthsDifference <= 2 ? "31 - 60 Days" :
                            monthsDifference <= 3 ? "61 - 90 Days" : "90+ Days";
                }
                else
                {
                    label = monthsDifference <= 1 ? "0 - 30 Days" :
                            monthsDifference <= 2 ? "31 - 60 Days" :
                            monthsDifference <= 3 ? "61 - 90 Days" : "90+ Days";
                }

                return new Dictionary<string, object>
            {
                { "LastAccessedDate", lastAccessedDate },
                { "DateCreated", DateTime.Parse(m["dateCreated"].ToString()) },
                { "LicenseDisplayName", m["accessLevel"]["licenseDisplayName"].ToString() },
                { "MailAddress", m["user"]["mailAddress"].ToString() },
                { "UserName", m["user"]["displayName"].ToString() },
                { "SubjectKind", m["user"]["subjectKind"].ToString() },
                { "PrincipalName", m["user"]["principalName"].ToString() },
                { "Range", label }
            };
            }).ToList();

            return userEntitlements;
        }

        public async Task<List<ReleasePipeline>> GetPipelineInfoAsync(GetProjectIn getProjectIn)
        {
            var pipelineInfoList = new List<ReleasePipeline>();
            var httpClient = new HttpClient();
            var base64AuthInfo = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{getProjectIn.PAT_Token}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64AuthInfo);

            var projectsUrl = $"https://dev.azure.com/{getProjectIn.OrganizationName}/_apis/projects?api-version=6.0";
            var projectsResponse = await httpClient.GetStringAsync(projectsUrl);
            var projects = JObject.Parse(projectsResponse)["value"];

            foreach (var project in projects)
            {
                var projName = project["name"].ToString();

                var cdPipelinesUrl = $"https://vsrm.dev.azure.com/{getProjectIn.OrganizationName}/{projName}/_apis/release/definitions?api-version=6.1-preview.4";
                var cdPipelinesResponse = await httpClient.GetStringAsync(cdPipelinesUrl);
                var cdPipelines = JArray.Parse(JObject.Parse(cdPipelinesResponse)["value"].ToString());

                foreach (var cdPipeline in cdPipelines)
                {
                    var cdId = cdPipeline["id"].ToString();
                    var cdBuildUrl = $"https://vsrm.dev.azure.com/{getProjectIn.OrganizationName}/{projName}/_apis/release/releases?definitionId={cdId}&api-version=7.0";

                    DateTime? lastBuildDate = null;
                    string lastReleaseName = "No release run found";
                    string releaseRange = "No release found";

                    try
                    {
                        var cdBuildResponse = await httpClient.GetStringAsync(cdBuildUrl);
                        var cdBuilds = JArray.Parse(JObject.Parse(cdBuildResponse)["value"].ToString());

                        if (cdBuilds.Count > 0)
                        {
                            lastBuildDate = DateTime.Parse(cdBuilds[0]["modifiedOn"].ToString());
                            lastReleaseName = cdBuilds[0]["name"].ToString();
                            releaseRange = GetReleaseRange(lastBuildDate.Value);
                        }
                    }
                    catch (Exception)
                    {
                        lastBuildDate = null;
                    }

                    var ciNameUrl = $"https://vsrm.dev.azure.com/{getProjectIn.OrganizationName}/{projName}/_apis/release/definitions/{cdId}?api-version=7.0";
                    var ciNameResponse = await httpClient.GetStringAsync(ciNameUrl);
                    var ciName = JObject.Parse(ciNameResponse);

                    var pipelineInfo = new ReleasePipeline
                    {
                        ProjectName = projName,
                        PipelineName = ciName["name"].ToString(),
                        LastReleaseName = lastReleaseName,
                        LastBuildDate = lastBuildDate,
                        CreatedOn = DateTime.Parse(ciName["createdOn"].ToString()),
                        CreatedBy = ciName["createdBy"]["uniqueName"].ToString(),
                        LastReleaseRange = releaseRange
                    };

                    pipelineInfoList.Add(pipelineInfo);
                }
            }

            return pipelineInfoList;
        }

        private string GetReleaseRange(DateTime lastCommitDate)
        {
            var currentDate = DateTime.Now;
            var monthsDifference = Math.Round((currentDate - lastCommitDate).TotalDays / 30);

            if (monthsDifference <= 6) return "1 - 6 Months";
            if (monthsDifference <= 12) return "6 - 12 Months";
            if (monthsDifference <= 18) return "12 - 18 Months";
            return "Older than 18 Months";
        }

        public async Task<List<BuildPipeline>> GetBuildPipelineInfoAsync(GetProjectIn getProjectIn)
        {
            var pipelineInfoList = new List<BuildPipeline>();

            // Set the PAT token for authorization
            var base64AuthInfo = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{getProjectIn.PAT_Token}"));
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64AuthInfo);

            var projectsUrl = $"https://dev.azure.com/{getProjectIn.OrganizationName}/_apis/projects?api-version=6.0";
            var projectsResponse = await _httpClient.GetStringAsync(projectsUrl);
            var projects = JObject.Parse(projectsResponse)["value"];

            foreach (var project in projects)
            {
                var projName = project["name"].ToString();

                var ciPipelinesUrl = $"https://dev.azure.com/{getProjectIn.OrganizationName}/{projName}/_apis/pipelines?api-version=7.1-preview.1";
                var ciPipelinesResponse = await _httpClient.GetStringAsync(ciPipelinesUrl);
                var ciPipelines = JArray.Parse(JObject.Parse(ciPipelinesResponse)["value"].ToString());

                foreach (var ciPipeline in ciPipelines)
                {
                    var ciId = ciPipeline["id"].ToString();
                    var ciBuildUrl = $"https://dev.azure.com/{getProjectIn.OrganizationName}/{projName}/_apis/pipelines/{ciId}/runs?api-version=6.0";

                    DateTime? lastBuildDate = null;
                    string lastBuildStatus = "No build run found";
                    string lastBuildResult = "No build run found";
                    string buildRange = "No build found";

                    try
                    {
                        var ciBuildResponse = await _httpClient.GetStringAsync(ciBuildUrl);
                        var ciBuilds = JArray.Parse(JObject.Parse(ciBuildResponse)["value"].ToString());

                        if (ciBuilds.Count > 0)
                        {
                            lastBuildDate = DateTime.Parse(ciBuilds[0]["createdDate"].ToString());
                            lastBuildStatus = ciBuilds[0]["state"].ToString();
                            lastBuildResult = ciBuilds[0]["result"].ToString();
                            buildRange = GetBuildRange(lastBuildDate.Value);
                        }
                    }
                    catch (Exception)
                    {
                        lastBuildDate = null;
                    }

                    var ciNameUrl = $"https://dev.azure.com/{getProjectIn.OrganizationName}/{projName}/_apis/pipelines/{ciId}?api-version=7.1-preview.1";
                    var ciNameResponse = await _httpClient.GetStringAsync(ciNameUrl);
                    var ciName = JObject.Parse(ciNameResponse);

                    var pipelineInfo = new BuildPipeline
                    {
                        ProjectName = projName,
                        PipelineName = ciName["name"]?.ToString() ?? "Unknown",
                        LastBuildOn = lastBuildDate,
                        LastBuildStatus = lastBuildStatus,
                        LastBuildResult = lastBuildResult,
                        LastBuildRange = buildRange,
                        LastEditDate = DateTime.Parse(ciName["configuration"]?["designerJson"]?["createdDate"]?.ToString() ?? DateTime.Now.ToString()),
                        IsHostedAgent = (bool?)ciName["configuration"]?["designerJson"]?["queue"]?["pool"]?["isHosted"] ?? false
                    };

                    pipelineInfoList.Add(pipelineInfo);
                }
            }

            return pipelineInfoList;
        }
        private string GetBuildRange(DateTime lastBuildDate)
        {
            var currentDate = DateTime.Now;
            var monthsDifference = Math.Round((currentDate - lastBuildDate).TotalDays / 30);

            if (monthsDifference <= 6) return "1 - 6 Months";
            if (monthsDifference <= 12) return "6 - 12 Months";
            if (monthsDifference <= 18) return "12 - 18 Months";
            return "Older than 18 Months";
        }

        public async Task<List<Repository>> GetRepoCommitInfoAsync(GetProjectIn getProjectIn)

        {
            string organization = getProjectIn.OrganizationName;
            string patToken = getProjectIn.PAT_Token;


            var repoCommitInfoList = new List<Repository>();
            var base64AuthInfo = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($":{patToken}"));

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64AuthInfo);

            // Fetch projects
            var projectsUrl = $"https://dev.azure.com/{organization}/_apis/projects?api-version=6.0";
            var projectsResponse = await _httpClient.GetStringAsync(projectsUrl);
            var projects = JObject.Parse(projectsResponse)["value"];

            foreach (var project in projects)
            {
                var projectName = project["name"]?.ToString();
                Console.WriteLine($"Project: {projectName}");

                // Fetch repositories
                var reposUrl = $"https://dev.azure.com/{organization}/{projectName}/_apis/git/repositories?api-version=6.1-preview.1";
                var reposResponse = await _httpClient.GetStringAsync(reposUrl);
                var repos = JObject.Parse(reposResponse)["value"];

                foreach (var repo in repos)
                {
                    var repoName = repo["name"]?.ToString();
                    var repoUrl = $"https://dev.azure.com/{organization}/{projectName}/_apis/git/repositories/{repoName}/commits?api-version=7.1-preview.1";

                    var repoResponse = await _httpClient.GetStringAsync(repoUrl);
                    var commits = JObject.Parse(repoResponse)["value"];

                    DateTime? lastCommitDate = null;
                    string commitRange = "No Commit";

                    if (commits.Count() > 0)
                    {
                        lastCommitDate = DateTime.Parse(commits[0]["committer"]["date"]?.ToString());
                        var currentDate = DateTime.UtcNow;

                        var monthsDifference = Math.Round((currentDate - lastCommitDate.Value).TotalDays / 30);
                        commitRange = monthsDifference switch
                        {
                            <= 6 => "1 - 6 Months",
                            <= 12 => "6 - 12 Months",
                            <= 18 => "12 - 18 Months",
                            _ => "Older than 18 Months"
                        };
                    }

                    var repoCommitInfo = new Repository
                    {
                        ProjectName = projectName,
                        RepoName = repoName,
                        CommitDate = lastCommitDate,
                        CommitRange = commitRange
                    };

                    repoCommitInfoList.Add(repoCommitInfo);
                }
            }

            return repoCommitInfoList;
        }



    }
}

