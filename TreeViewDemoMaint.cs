using System.Linq;
using PX.Data;
using System.Collections;
using TreeViewDemo.DAC;
using PX.Objects.PM;
using System.Collections.Generic;
using System;
using PX.Objects.GL;

namespace TreeViewDemo
{
    public class TreeViewDemoMaint : PXGraph<TreeViewDemoMaint>
    {
        
        #region SelectedNode
        public class SelectedNode : IBqlTable
        {
            #region TreeViewTaskID
            public abstract class treeViewTaskID : PX.Data.IBqlField
            {
            }
            [PXDBInt(IsKey = true)]
            public virtual int? TreeViewTaskID { get; set; }
            #endregion

            #region GridTaskID
            public abstract class gridTaskID : PX.Data.IBqlField
            {
            }
            [PXDBInt(IsKey = true)]
            public virtual int? GridTaskID { get; set; }
            #endregion
        }
        #endregion

        #region CurrentSelection
        protected SelectedNode CurrentSelected
        {
            get
            {
                PXCache cache = Caches[typeof(SelectedNode)];
                if (cache.Current == null)
                {
                    cache.Insert();
                    cache.IsDirty = false;
                }
                return (SelectedNode)cache.Current;
            }
        }

        private bool skipTaskIDVerification = false;
        protected bool SkipTaskIDVerification
        {
            get
            {
                return skipTaskIDVerification;
            }
        }
        #endregion

        #region Data Members
        public PXSelect<TaskTableExtension,
                   Where<TaskTableExtension.parentTaskID, Equal<Argument<int?>>>,
                   OrderBy<Asc<TaskTableExtension.sortOrder>>> Folders;

        protected virtual IEnumerable folders(
            [PXInt]
			int? taskID)
        {
            if (taskID == null)
            {
                yield return new TaskTableExtension()
                {
                    TaskID = 0,
                    Description = PXSiteMap.RootNode.Title
                };
            }
            foreach (TaskTableExtension item in PXSelect<TaskTableExtension,
                                                  Where<TaskTableExtension.parentTaskID, Equal<Required<TaskTableExtension.taskID>>>>
                                              .Select(this, taskID))
            {
                if (!string.IsNullOrEmpty(item.Description))
                    yield return item;
            }
        }

        public PXSelect<TaskTableExtension,
                   Where<TaskTableExtension.parentTaskID, Equal<Argument<int?>>>,
                   OrderBy<Asc<TaskTableExtension.parentTaskID, Asc<TaskTableExtension.sortOrder>>>> Items;

        protected virtual IEnumerable items(
            [PXInt]
			int? taskID)
        {



            if (taskID == null && Folders.Current != null)
                taskID = Folders.Current.TaskID;




            if (taskID == null)
            {
                Items.Cache.AllowInsert = false;
                return null;
            }

            if (!skipTaskIDVerification)
            {
                if (taskID != 0)
                    Items.Cache.AllowInsert = PXSelect<TaskTableExtension,
                                                  Where<TaskTableExtension.taskID, Equal<Required<TaskTableExtension.taskID>>>>
                                              .Select(this, taskID).Count > 0;
                CurrentSelected.TreeViewTaskID = taskID;
            }
            else
                skipTaskIDVerification = false;
            return PXSelect<TaskTableExtension,
                       Where<TaskTableExtension.parentTaskID, Equal<Required<TaskTableExtension.taskID>>>>
                   .Select(this, taskID);
        }

        public PXSelect<TaskTableExtension,
                   Where<TaskTableExtension.taskID, Equal<Required<TaskTableExtension.taskID>>>> Item;
        #endregion

        #region Actions
        public PXSave<TaskTableExtension> Save;
        public PXCancel<TaskTableExtension> Cancel;

        #endregion

        #region Event Handlers
        protected virtual void TaskTableExtension_RowSelected(PXCache sender, PXRowSelectedEventArgs e)
        {
            TaskTableExtension row = e.Row as TaskTableExtension;
            if (row != null)
                CurrentSelected.GridTaskID = row.TaskID;
        }

        protected virtual void TaskTableExtension_RowDeleted(PXCache sender, PXRowDeletedEventArgs e)
        {
            TaskTableExtension row = e.Row as TaskTableExtension;
            if (row == null) return;

            DeleteRecurring(row);
        }

        protected virtual void TaskTableExtension_ParentTaskID_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            TaskTableExtension row = e.Row as TaskTableExtension;
            if (row == null) return;

            e.NewValue = CurrentSelected.TreeViewTaskID ?? 0;
            e.Cancel = true;
        }

        protected virtual void TaskTableExtension_SortOrder_FieldDefaulting(PXCache sender, PXFieldDefaultingEventArgs e)
        {
            TaskTableExtension row = (TaskTableExtension)e.Row;
            if (row == null) return;

            e.NewValue = 1;
            e.Cancel = true;

            PXResultset<TaskTableExtension> list = Items.Select(row.ParentTaskID);
            if (list.Count > 0)
            {
                TaskTableExtension last = list[list.Count - 1];
                e.NewValue = last.SortOrder + 1;
            }
        }
        #endregion

        public PXAction<TaskTableExtension> CheckCurrentObject;
        [PXUIField(DisplayName = "Check Current Object")]
        [PXButton]
        public virtual IEnumerable checkCurrentObject(PXAdapter adapter)
        {
            if (Items.Current == null)
                throw new PXException("Current object is null");
            else
                throw new PXException(string.Format("Current object TaskID = {0}", Items.Current.TaskCD));
        }

        #region Moving Actions
        public PXAction<TaskTableExtension> Left;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable left(PXAdapter adapter)
        {
            TaskTableExtension current = Item.SelectWindowed(0, 1, CurrentSelected.TreeViewTaskID);
            if (current != null && current.ParentTaskID != 0)
            {
                TaskTableExtension parent = Item.SelectWindowed(0, 1, current.ParentTaskID);
                if (parent != null)
                {
                    int parentIndex;
                    PXResultset<TaskTableExtension> items = SelectSiblings(parent.ParentTaskID,
                                                                         parent.TaskID,
                                                                         out parentIndex);
                    if (parentIndex >= 0)
                    {
                        TaskTableExtension last = items[items.Count - 1];
                        current = (TaskTableExtension)Items.Cache.CreateCopy(current);
                        current.ParentTaskID = parent.ParentTaskID;
                        current.SortOrder = last.SortOrder + 1;
                        Items.Update(current);
                        PXSelect<TaskTableExtension, Where<TaskTableExtension.parentTaskID, Equal<Required<TaskTableExtension.taskID>>>>.Clear(this);
                    }
                }
            }
            return adapter.Get();
        }

        public PXAction<TaskTableExtension> Right;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable right(PXAdapter adapter)
        {
            TaskTableExtension current = Item.SelectWindowed(0, 1, CurrentSelected.TreeViewTaskID);
            if (current != null)
            {
                int currentItemIndex;
                PXResultset<TaskTableExtension> items = SelectSiblings(current.ParentTaskID,
                                                                     current.TaskID,
                                                                     out currentItemIndex);
                if (currentItemIndex > 0)
                {
                    TaskTableExtension prev = items[currentItemIndex - 1];
                    items = SelectSiblings(prev.TaskID);
                    int index = 1;
                    if (items.Count > 0)
                    {
                        TaskTableExtension last = items[items.Count - 1];
                        index = (last.SortOrder ?? 0) + 1;
                    }
                    current = (TaskTableExtension)Items.Cache.CreateCopy(current);
                    current.ParentTaskID = prev.TaskID;
                    current.SortOrder = index;
                    Items.Update(current);
                    PXSelect<TaskTableExtension, Where<TaskTableExtension.parentTaskID, Equal<Required<TaskTableExtension.taskID>>>>.Clear(this);
                }
            }
            return adapter.Get();
        }

        public PXAction<TaskTableExtension> Down;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable down(PXAdapter adapter)
        {
            int currentItemIndex;
            PXResultset<TaskTableExtension> items = SelectSiblings(CurrentSelected.TreeViewTaskID,
                                                                 CurrentSelected.GridTaskID,
                                                                 out currentItemIndex);
            if (currentItemIndex >= 0 && currentItemIndex < items.Count - 1)
            {
                TaskTableExtension current = items[currentItemIndex];
                TaskTableExtension next = items[currentItemIndex + 1];

                current.SortOrder += 1;
                next.SortOrder -= 1;

                Items.Update(current);
                Items.Update(next);
            }
            return adapter.Get();
        }

        public PXAction<TaskTableExtension> Up;
        [PXUIField(MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select)]
        [PXButton]
        public virtual IEnumerable up(PXAdapter adapter)
        {
            int currentItemIndex;
            PXResultset<TaskTableExtension> items = SelectSiblings(CurrentSelected.TreeViewTaskID,
                                                                 CurrentSelected.GridTaskID,
                                                                 out currentItemIndex);
            if (currentItemIndex > 0)
            {
                TaskTableExtension current = items[currentItemIndex];
                TaskTableExtension prev = items[currentItemIndex - 1];

                current.SortOrder -= 1;
                prev.SortOrder += 1;

                Items.Update(current);
                Items.Update(prev);
            }
            return adapter.Get();
        }
        #endregion

        #region Methods
        public override void Persist()
        {
            base.Persist();
            skipTaskIDVerification = true;
        }

        protected void DeleteRecurring(TaskTableExtension map, bool deleteRootNode = false)
        {
            if (map != null)
            {
                foreach (TaskTableExtension child in PXSelect<TaskTableExtension,
                                                       Where<TaskTableExtension.parentTaskID, Equal<Required<TaskTableExtension.taskID>>>>
                                                   .Select(this, map.TaskID))
                    DeleteRecurring(child, true);
                if (deleteRootNode)
                    Items.Cache.Delete(map);
            }
        }

        protected PXResultset<TaskTableExtension> SelectSiblings(int? patentID)
        {
            int currentIndex;
            return SelectSiblings(patentID, 0, out currentIndex);
        }

        protected PXResultset<TaskTableExtension> SelectSiblings(int? parentID, int? taskID, out int currentIndex)
        {
            currentIndex = -1;
            if (parentID == null) return null;
            PXResultset<TaskTableExtension> items = Items.Select(parentID);

            int i = 0;
            foreach (TaskTableExtension item in items)
            {
                if (item.TaskID == taskID)
                    currentIndex = i;
                item.SortOrder = i + 1;
                Items.Update(item);
                i += 1;
            }
            return items;
        }

        #endregion

        #region project task budget
        public ProjectStatusSelect<PMProjectStatusEx, Where<PMProjectStatusEx.accountGroupID, IsNotNull>, OrderBy<Asc<PMProjectStatusEx.sortOrder>>> ProjectStatus;
        public PXSelectJoin<PMTask, LeftJoin<PMProject, On<PMTask.projectID, Equal<PMProject.contractID>>>, Where<PMProject.nonProject, Equal<False>, And<PMProject.isTemplate, Equal<False>>>> Task;

        public virtual IEnumerable projectStatus()
        {
            if (Task.Current == null)
            {
                yield break;
            }

            Dictionary<string, PMProjectStatusEx> cachedItems = new Dictionary<string, PMProjectStatusEx>();

            bool isDirty = false;

            int cxMax = 0;
            foreach (PMProjectStatusEx item in ProjectStatus.Cache.Cached)
            {
                cxMax = Math.Max(cxMax, item.LineNbr.Value);
                string key = string.Format("{0}.{1}.{2}.{3}", item.AccountGroupID, item.ProjectID, item.ProjectTaskID, item.InventoryID.GetValueOrDefault());

                if (!cachedItems.ContainsKey(key))
                    cachedItems.Add(key, item);

                if (ProjectStatus.Cache.GetStatus(item) == PXEntryStatus.Inserted ||
                    ProjectStatus.Cache.GetStatus(item) == PXEntryStatus.Updated ||
                    ProjectStatus.Cache.GetStatus(item) == PXEntryStatus.Notchanged ||
                    ProjectStatus.Cache.GetStatus(item) == PXEntryStatus.Held)
                {

                    if (ProjectStatus.Cache.GetStatus(item) == PXEntryStatus.Inserted ||
                    ProjectStatus.Cache.GetStatus(item) == PXEntryStatus.Updated)
                    {
                        isDirty = true;
                    }

                    yield return item;
                }

            }

            PXSelectBase<PMProjectStatus> select = new PXSelectJoinGroupBy<PMProjectStatus,
                    InnerJoin<PMTask, On<PMTask.projectID, Equal<PMProjectStatus.projectID>, And<PMTask.taskID, Equal<PMProjectStatus.projectTaskID>>>,
                    InnerJoin<PMAccountGroup, On<PMProjectStatus.accountGroupID, Equal<PMAccountGroup.groupID>>>>,
                    Where<PMProjectStatus.projectID, Equal<Current<PMTask.projectID>>,
                    And<PMProjectStatus.projectTaskID, Equal<Current<PMTask.taskID>>>>,
                    Aggregate<GroupBy<PMProjectStatus.accountGroupID,
                    GroupBy<PMProjectStatus.projectID,
                    GroupBy<PMProjectStatus.projectTaskID,
                    GroupBy<PMProjectStatus.inventoryID,
                    Sum<PMProjectStatus.amount,
                    Sum<PMProjectStatus.qty,
                    Sum<PMProjectStatus.revisedAmount,
                    Sum<PMProjectStatus.revisedQty,
                    Sum<PMProjectStatus.actualAmount,
                    Sum<PMProjectStatus.actualQty>>>>>>>>>>>, OrderBy<Asc<PMAccountGroup.sortOrder>>>(this);

            int cx = cxMax + 1;
            foreach (PXResult<PMProjectStatus, PMTask, PMAccountGroup> res in select.Select())
            {
                PMProjectStatus row = (PMProjectStatus)res;
                PMTask task = (PMTask)res;
                PMAccountGroup ag = (PMAccountGroup)res;

                string key = string.Format("{0}.{1}.{2}.{3}", row.AccountGroupID, row.ProjectID, row.ProjectTaskID, row.InventoryID.GetValueOrDefault());

                if (!cachedItems.ContainsKey(key))
                {
                    PMProjectStatusEx item = new PMProjectStatusEx();
                    item.LineNbr = cx++;
                    item = (PMProjectStatusEx)ProjectStatus.Cache.Insert(item);
                    item.ProjectID = row.ProjectID;
                    item.ProjectTaskID = row.ProjectTaskID;
                    item.AccountGroupID = row.AccountGroupID;
                    item.InventoryID = row.InventoryID;
                    item.Description = row.Description;
                    item.UOM = row.UOM;
                    item.Rate = row.Rate;
                    item.Qty = row.Qty;
                    item.Amount = row.Amount;
                    item.RevisedQty = row.RevisedQty;
                    item.RevisedAmount = row.RevisedAmount;
                    item.ActualQty = row.ActualQty;
                    item.ActualAmount = row.ActualAmount;
                    PMProjectStatus rowDetail = (PMProjectStatus)PXSelect<
                        PMProjectStatus
                        , Where<
                            PMProjectStatus.isProduction, Equal<True>
                            , And<PMProjectStatus.projectID, Equal<Required<PMProjectStatus.projectID>>
                                , And<PMProjectStatus.projectTaskID, Equal<Required<PMProjectStatus.projectTaskID>>
                                    , And<PMProjectStatus.inventoryID, Equal<Required<PMProjectStatus.inventoryID>>
                                        , And<PMProjectStatus.accountGroupID, Equal<Required<PMProjectStatus.accountGroupID>>>
                                        >
                                    >
                                >
                            >
                        >.Select(this, row.ProjectID, row.ProjectTaskID, row.InventoryID, row.AccountGroupID);

                    if (rowDetail != null)
                        item.IsProduction = true;
                    item.TaskStatus = task.Status;
                    item.Type = ag.Type;
                    switch (ag.Type)
                    {
                        case AccountType.Asset:
                            item.SortOrder = 1;
                            break;
                        case AccountType.Liability:
                            item.SortOrder = 2;
                            break;
                        case AccountType.Income:
                            item.SortOrder = 3;
                            break;
                        case AccountType.Expense:
                            item.SortOrder = 4;
                            break;
                    }
                    ProjectStatus.Cache.SetStatus(item, PXEntryStatus.Held);

                    yield return item;
                }
            }

            ProjectStatus.Cache.IsDirty = isDirty;
        }

        #endregion

        #region TaskTableExtension
        public PXSelect<TaskTableExtension> TaskTableExtension;
        #endregion

    }
}
