using PX.Data;
using PX.Objects.PM;

namespace TreeViewDemo.DAC
{

    public class TreeViewTask : IBqlTable
    {
        #region CategoryID
        public abstract class categoryID : PX.Data.IBqlField
        {
        }
        [PXDBIdentity]
        public virtual int? CategoryID { get; set; }
        #endregion

        #region CategoryCD
        public abstract class categoryCD : PX.Data.IBqlField
        {
        }
        [PXDBString(10, IsKey = true, InputMask = "<CCCCCCCCCC")]
        [PXUIField(DisplayName = "Category ID")]
        [PXDefault]
        public virtual string CategoryCD { get; set; }
        #endregion

        #region Description
        public abstract class description : PX.Data.IBqlField
        {
        }
        [PXDBString(50)]
        [PXDefault]
        [PXUIField(DisplayName = "Description")]
        public virtual string Description { get; set; }
        #endregion

        #region ParentCategoryID
        public abstract class parentCategoryID : PX.Data.IBqlField
        {
        }
        [PXDBInt]
        [PXDBLiteDefault(typeof(TreeViewTask.categoryID))]
        public virtual int? ParentCategoryID { get; set; }
        #endregion

        #region SortOrder
        public abstract class sortOrder : PX.Data.IBqlField
        {
        }
        [PXDBInt]
        [PXDefault(0)]
        public virtual int? SortOrder { get; set; }
        #endregion
    }
}