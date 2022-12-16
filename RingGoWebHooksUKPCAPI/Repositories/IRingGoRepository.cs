using System.Threading.Tasks;
using ANPRTechOps.RingGoIntegration.Web.Models;

namespace ANPRTechOps.RingGoIntegration.Web.Repositories
{
    public interface IRingGoRepository
    {
        Task<int> InsertRingGoSession(SessionModel model);

        Task<SessionDto> FindRingGoSessionByReference(string reference);

        Task<bool> DeleteRingGoSession(int id);
    }
}