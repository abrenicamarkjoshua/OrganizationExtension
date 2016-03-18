using PX.Data;
using PX.Objects.PM;
using System;
using System.Collections;
using System.Collections.Generic;
using PX.SM;

using PX.Objects.EP;
namespace TreeStructure
{
    public class ProjectTaskEntryExtension : PXGraphExtension<ProjectTaskEntry>
    {
        #region pmtaskmaster
        [Serializable]
        public class PMTaskMaster : PMTask
        {
            #region TaskID
            public new abstract class taskID : IBqlField
            {
            }

            [PXDBIdentity(IsKey = true), PXUIField(DisplayName = "Task ID", Enabled = false, Visibility = PXUIVisibility.Invisible)]
            public override int? TaskID
            {
                get
                {
                    return this._TaskID;
                }
                set
                {
                    this._TaskID = value;
                }
            }
            #endregion taskid

            #region Description

            public new abstract class description : IBqlField
            {
            }
            [PXDBString(50, InputMask = ""), PXDefault, PXUIField(DisplayName = "Description", Visibility = PXUIVisibility.SelectorVisible)]
            public override string Description
            {
                get
                {
                    return this._Description;
                }
                set
                {
                    this._Description = value;
                }
            }
            #endregion description

            #region parentID
            public String _parentID;
            public abstract class parentID : PX.Data.IBqlField
            {
            }
            [PXDBString(15, IsUnicode = true)]
            [PXSelector(typeof(Search<PMTask.taskCD>),
                typeof(PMTask.taskID),
                typeof(PMTask.description),
                typeof(PMTask.status)
                )]
            [PXUIField(DisplayName = "Parent Task ID", Visibility = PXUIVisibility.SelectorVisible)]
            public virtual String ParentID
            {
                get
                {
                    return _parentID;
                }
                set
                {
                    _parentID = value;
                }
            }
            #endregion parentid
            
        }
        #endregion pmtaskmaster

        public PXSelect<PMTaskMaster, Where<PMTaskMaster.parentID,
             Equal<Required<PMTask.taskCD>>>> Folders;

        protected virtual IEnumerable folders(
        [PXInt]
        int? TaskID
    )
        {
            if (TaskID == null)
            {
                yield return new PMTaskMaster()
                {
                    TaskID = 0,
                    Description = PXSiteMap.RootNode.Title
                };

            }

            foreach (PMTaskMaster item in PXSelect<PMTaskMaster, Where<PMTaskMaster.parentID,
            Equal<Required<PMTask.taskCD>>>>.Select(Base, TaskID))
            {
                if (!string.IsNullOrEmpty(item.Description))
                    yield return item;
            }
        }
       

        public PXSelect<PMTaskMaster, Where<PMTaskMaster.parentID, Equal<Argument<string>>>> Items;
    }
}
