using System.Threading.Tasks;

namespace WallHaven.WallHavenClient
{
    public interface IWallHavenRequest
    {
        Task<WallHavenResponse> GetWallpaper(string id);
        Task<WallHavenResponse> Search(string searchParams);
    }
}