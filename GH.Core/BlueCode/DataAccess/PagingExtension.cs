using System.Linq;

namespace GH.Core.BlueCode.DataAccess
{
    public static class PagingExtensions
    {
        public static IQueryable<TSource> Page<TSource>(IQueryable<TSource> source, int pageIndex, int pageSize)
        {
            return source.Skip((pageIndex - 1) * pageSize).Take(pageSize);
        }
    }
}
