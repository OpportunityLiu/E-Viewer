using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    internal static class MathHelper
    {
        public static int GetPageCount(int recordCount, int pageSize)
        {
            return recordCount / pageSize + (recordCount % pageSize == 0 ? 0 : 1);
        }

        public static int GetSizeOfPage(int recordCount, int pageSize, int pageIndex)
        {
            var remainRecordCount = recordCount - pageIndex * pageSize;
            if(remainRecordCount < pageSize)
                return remainRecordCount;
            else
                return pageIndex;
        }

        public static int GetPageIndexOfRecord(int pageSize, int recordIndex)
        {
            return recordIndex / pageSize;
        }

        public static int GetStartIndexOfPage(int pageSize, int pageIndex)
        {
            return pageSize * pageIndex;
        }
    }
}
