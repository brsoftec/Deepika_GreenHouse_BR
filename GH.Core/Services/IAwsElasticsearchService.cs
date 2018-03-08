using GH.Core.Models;
using GH.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace GH.Core.Services
{
    public interface IAwsElasticsearchService
    {
        Task DeleteIndex(string canonicalURI);
        Task Index(string canonicalURI, string settings = null);
        Task<dynamic> AddOrUpdateDocument(string canonicalURI, string id, dynamic document);
        Task<dynamic> AddDocuments(string index, string indexType, string idField, params dynamic[] documents);
        Task<dynamic> AddOrUpdateDocuments(string index, string indexType, string idField, params dynamic[] documents);
        Task<EsSearchResult> SearchDocuments(string canonicalURI, dynamic query, int? start, int? length);
    }
}