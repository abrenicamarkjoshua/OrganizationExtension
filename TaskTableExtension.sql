CREATE TABLE [dbo].[TaskTableExtension]
(
    [CompanyID] [int] NOT NULL,
    [TaskTableExtensionID] [int] IDENTITY(1,1) NOT NULL,
    [TaskID] [int] NOT NULL,
   
    [Description] [nvarchar](50) NOT NULL,
    [ParentTaskTableExtensionID] [int] NULL,
    [SortOrder] [int] NOT NULL,

    CONSTRAINT [PK_TaskTableExtension] PRIMARY KEY CLUSTERED 
    (
        [CompanyID] ASC,
        [TaskTableExtensionID] ASC
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
