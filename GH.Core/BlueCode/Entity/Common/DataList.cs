using System.Collections.Generic;

namespace GH.Core.BlueCode.Entity.Common
{
    public class DataList<T>
    {
        private int totalPages;
        private int currentItems;

        public DataList(){}

        public DataList(List<T> dataOfCurrentPage, int totalItems, int pageIndex, int pageSize)
        {
            this.DataOfCurrentPage = dataOfCurrentPage;
            this.TotalItems = totalItems;
            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
        }
        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalItems { get; set; }

        public int TotalPages
        {
            get { return (TotalItems / PageSize) + ((TotalItems % PageSize) != 0 ? 1 : 0); }
        }
        public int CurrentItems
        {
            get
            {
                return DataOfCurrentPage.Count;
            }
        }
        public List<T> DataOfCurrentPage { get; set; }
    }
}
