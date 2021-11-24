using System.Threading.Tasks;

namespace WallHaven.WallHavenClient
{
    public interface IWallHavenRequest
    {
        Task<WallHavenResponse> GetWallpaper(string id, string url = "", string token = "");
        Task<WallHavenResponse> Search(string searchParams, string url = "", string token = "");
    }
}