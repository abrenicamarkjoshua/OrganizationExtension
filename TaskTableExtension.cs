using PX.Data;
using PX.Objects.PM;

namespace TreeViewDemo.DAC
{
    
    public class TaskTableExtension : IBqlTable
    {
        #region TaskID
        public abstract class taskID : PX.Data.IBqlField
        {
        }
        [PXDBIdentity]
         public virtual int? TaskID { get; set; }
        #endregion

        #region TaskCD
        
        public abstract class taskCD : PX.Data.IBqlField
        {
        }
        [PXDBString(10, IsKey = true, InputMask = "<CCCCCCCCCC")]
        [PXUIField(DisplayName = "Task ID", Visibility = PXUIVisibility.SelectorVisible)]
        [PXDefault]
        [PXSelector(typeof(Search<PMTask.taskCD>),
                    typeof(PMTask.taskID),
                    typeof(PMTask.taskCD),
                    typeof(PMTask.description)
            )]
        public virtual string TaskCD { get; set; }
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

        #region ParentTaskID
        public abstract class parentTaskID : PX.Data.IBqlField
        {
        }
        [PXDBInt]
        [PXDBLiteDefault(typeof(TaskTableExtension.taskID))]
        public virtual int? ParentTaskID { get; set; }
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
