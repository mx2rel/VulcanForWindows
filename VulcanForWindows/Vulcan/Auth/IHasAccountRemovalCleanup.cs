using System.Threading.Tasks;

namespace Vulcanova.Features.Auth;

public interface IHasAccountRemovalCleanup
{
    Task DoPostRemovalCleanUpAsync(int accountId);
}