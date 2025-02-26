namespace cv_site.Models
{
    public class GithubRepo
    {
        public string Name { get; set; }
        public string Language { get; set; }
        public DateTime LastCommit { get; set; }
        public int Stars { get; set; }
        public int PullRequests { get; set; }
        public string Url { get; set; }
    }
}
