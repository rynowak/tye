using System.Threading.Tasks;

namespace OpenArm.Repositories
{
    public interface IRepository
    {
        Task InitializeAsync();
    }
}