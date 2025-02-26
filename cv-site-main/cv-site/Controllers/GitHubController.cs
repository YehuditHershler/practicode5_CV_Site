using cv_site.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Octokit;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace cv_site.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GitHubController : ControllerBase
    {
        private readonly GitHubClient _client;
        private readonly IMemoryCache _cache;

        private const string cacheKey = "portfolio_key";

        public GitHubController(IMemoryCache cache, GitHubClient gitHubClient)
        {
            _client = gitHubClient;
            _cache = cache;
        }

        [HttpGet("GetPortfolio")]
        public async Task<ActionResult<IEnumerable<GithubRepo>>> GetPortfolio(string username)
        {
            if (_cache.TryGetValue(cacheKey, out List<GithubRepo> cachedPortfolio))
                return Ok(cachedPortfolio);

            var repositories = await _client.Repository.GetAllForUser(username);
            var result = repositories.Select(async repo => new GithubRepo
            {
                Name = repo.Name,
                Language = repo.Language,
                LastCommit = repo.UpdatedAt.DateTime,
                Stars = repo.StargazersCount,
                PullRequests = (await _client.PullRequest.GetAllForRepository(username, repo.Name)).Count(),
                Url = repo.HtmlUrl
            });
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromSeconds(30))
                .SetSlidingExpiration(TimeSpan.FromSeconds(10));

            _cache.Set(cacheKey, result, cacheOptions);
            return Ok(result);
        }

        [HttpGet("SearchRepositories")]
        public async Task<ActionResult<IEnumerable<Repository>>> SearchRepositories(string? repoName, Language? language, string? username)
        {
            var request = new SearchRepositoriesRequest(repoName)
            {
                Language = language,
                User = username
            };

            var result = await _client.Search.SearchRepo(request);
            return Ok(result.Items);
        }
    }
}

