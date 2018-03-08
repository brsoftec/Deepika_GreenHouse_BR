using GH.Core.Models;
using System.Collections.Generic;

namespace GH.Core.Services
{
    public interface IResourceService
    {
        List<Resource> GetAllResources();
        Dictionary<string,Resource> GetResourcesPerPath();
        List<Resource> GetResources(string type = null);
        Resource GetResource(string id = null);
        ResourceService.Subscription GetSubscriptionByAccountId(string accountId);
    }
}