using PX.Data;
using PX.Objects.PM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TreeStructure
{
    [PXTable(typeof(PMTask.taskID),
        IsOptional = true)]
    public class PMTaskTableExtension : PXCacheExtension<PMTask>
    {
        #region ParentID
        public abstract class parentID : PX.Data.IBqlField
        {
        }
        [PXDBInt]
        [PXDefault(0)]
        [PXUIField(DisplayName = "Parent Task ID")]
        public string ParentID { get; set; }
        #endregion ParentID
    }
}
