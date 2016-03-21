CREATE TABLE [dbo].[TaskTableExtension]
(
    [CompanyID] [int] NOT NULL,
    [TaskID] [int] IDENTITY(1,1) NOT NULL,
    [TaskCD] [nvarchar](10) NOT NULL,
    [ProjectID] [int] NOT NULL,
    [Description] [nvarchar](50) NOT NULL,
    [ParentCategoryID] [int] NULL,
    [SortOrder] [int] NOT NULL,

    CONSTRAINT [PK_TreeViewCategory] PRIMARY KEY CLUSTERED 
    (
        [CompanyID] ASC,
        [TaskID] ASC
    )
    WITH 
    (
        PAD_INDEX  = OFF, 
        STATISTICS_NORECOMPUTE  = OFF, 
        IGNORE_DUP_KEY = OFF, 
        ALLOW_ROW_LOCKS  = ON, 
        ALLOW_PAGE_LOCKS  = ON
    ) 
    ON [PRIMARY]
) 
ON [PRIMARY]

GO
