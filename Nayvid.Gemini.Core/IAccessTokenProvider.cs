using System.Threading;
using System.Threading.Tasks;

namespace Nayvid.Gemini.Core
{
    public interface IAccessTokenProvider
    {
        Task<string> GetAccessTokenAsync(CancellationToken cancellationToken = default);
    }
}
