USE [TimeClockApp]
GO
/****** Object:  Table [dbo].[Config]    Script Date: 12/22/2025 10:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Config]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Config](
	[ConfigId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[StringValue] [nvarchar](max) NULL,
	[IntValue] [int] NULL,
	[Hint] [nvarchar](max) NULL,
 CONSTRAINT [PK_Config] PRIMARY KEY CLUSTERED 
(
	[ConfigId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Employee]    Script Date: 12/22/2025 10:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Employee]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Employee](
	[EmployeeId] [int] IDENTITY(1,1) NOT NULL,
	[Employee_Name] [nvarchar](50) NOT NULL,
	[Employee_PayRate] [decimal](9, 2) NOT NULL,
	[Employee_Employed] [int] NOT NULL,
	[JobTitle] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED 
(
	[EmployeeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Expense]    Script Date: 12/22/2025 10:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Expense]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Expense](
	[ExpenseId] [int] IDENTITY(1,1) NOT NULL,
	[ProjectId] [int] NOT NULL,
	[PhaseId] [int] NOT NULL,
	[ExpenseTypeId] [int] NOT NULL,
	[ExpenseTypeCategoryName] [nvarchar](max) NULL,
	[ExpenseDate] [date] NOT NULL,
	[Memo] [nvarchar](max) NULL,
	[Amount] [decimal](9, 2) NOT NULL,
	[IsRecent] [bit] NOT NULL,
	[ExpenseProject] [nvarchar](max) NULL,
	[ExpensePhase] [nvarchar](max) NULL,
 CONSTRAINT [PK_Expense] PRIMARY KEY CLUSTERED 
(
	[ExpenseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[ExpenseType]    Script Date: 12/22/2025 10:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseType]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[ExpenseType](
	[ExpenseTypeId] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ExpenseType] PRIMARY KEY CLUSTERED 
(
	[ExpenseTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Phase]    Script Date: 12/22/2025 10:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Phase]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Phase](
	[PhaseId] [int] IDENTITY(1,1) NOT NULL,
	[PhaseTitle] [nvarchar](450) NOT NULL,
 CONSTRAINT [PK_Phase] PRIMARY KEY CLUSTERED 
(
	[PhaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[Project]    Script Date: 12/22/2025 10:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Project]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[Project](
	[ProjectId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](max) NOT NULL,
	[Status] [int] NOT NULL,
	[ProjectDate] [date] NOT NULL,
 CONSTRAINT [PK_Project] PRIMARY KEY CLUSTERED 
(
	[ProjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
/****** Object:  Table [dbo].[TimeCard]    Script Date: 12/22/2025 10:49:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TimeCard]') AND type in (N'U'))
BEGIN
CREATE TABLE [dbo].[TimeCard](
	[TimeCardId] [int] IDENTITY(1,1) NOT NULL,
	[EmployeeId] [int] NOT NULL,
	[ProjectId] [int] NOT NULL,
	[PhaseId] [int] NOT NULL,
	[TimeCard_EmployeeName] [nvarchar](max) NOT NULL,
	[TimeCard_Status] [int] NOT NULL,
	[TimeCard_DateTime] [datetime2](7) NOT NULL,
	[TimeCard_Date] [date] NOT NULL,
	[TimeCard_StartTime] [time](0) NOT NULL,
	[TimeCard_EndTime] [time](0) NOT NULL,
	[TimeCard_WorkHours]  AS (CONVERT([decimal](4,2),datediff(minute,[TimeCard_StartTime],[TimeCard_EndTime])/(60.0))) PERSISTED,
	[TimeCard_EmployeePayRate] [decimal](9, 2) NOT NULL,
	[TimeCard_bReadOnly] [bit] NOT NULL,
	[ProjectName] [nvarchar](max) NULL,
	[PhaseTitle] [nvarchar](max) NULL,
 CONSTRAINT [PK_TimeCard] PRIMARY KEY CLUSTERED 
(
	[TimeCardId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
END
GO
SET IDENTITY_INSERT [dbo].[Config] ON 
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (1, N'Company', N'Alexander Builder', NULL, N'The business entity name')
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (2, N'FirstRun', CAST(CONVERT(date, GETDATE()) AS NVARCHAR), NULL, N'Date this app was 1st ran. For internal use only')
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (3, N'CurrentProject', NULL, 1, N'Current Project to default to')
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (4, N'CurrentPhase', NULL, 1, N'Current Phase to default to')
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (5, N'WorkCompRate', N'0.171118', NULL, N'Worker Comp Rate per $100 remuneration')
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (6, N'EstimateEmployerTaxes', N'0.1019', NULL, NULL)
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (7, N'ProfitRate', N'0.1', NULL, NULL)
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (8, N'OverHeadRate', N'0.08', NULL, NULL)
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (9, N'IsAdmin', NULL, 0, N'Enables dangerous timecard edits')
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (10, N'DaysInPayPeriod', NULL, 7, N'Number of Days in a Pay Period (weekly=7,biweekly=14,etc) (Default 7)')
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (11, N'PayDayOfWeek', NULL, 5, N'Day of week that is the end of the pay period (0=Sunday...3=Wednesday...5=Friday,6=Saturday)(Default 5)')
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (12, N'Version', N'1.7', NULL, N'Application Database version')
GO
INSERT [dbo].[Config] ([ConfigId], [Name], [StringValue], [IntValue], [Hint]) VALUES (13, N'AppTheme', NULL, 0, N'Override App theme (0=Default-Unspecified, 1=Light, 2=Dark)')
GO
SET IDENTITY_INSERT [dbo].[Config] OFF
GO
SET IDENTITY_INSERT [dbo].[Employee] ON 
GO
INSERT [dbo].[Employee] ([EmployeeId], [Employee_Name], [Employee_PayRate], [Employee_Employed], [JobTitle]) VALUES (1, N'John Doe', CAST(25.00 AS Decimal(9, 2)), 0, N'Job Title')
GO
INSERT [dbo].[Employee] ([EmployeeId], [Employee_Name], [Employee_PayRate], [Employee_Employed], [JobTitle]) VALUES (2, N'Jane Doe', CAST(25.00 AS Decimal(9, 2)), 0, N'Job Title')
GO
SET IDENTITY_INSERT [dbo].[Employee] OFF
GO
SET IDENTITY_INSERT [dbo].[ExpenseType] ON 
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (1, N'Deleted')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (11, N'Dumps')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (2, N'Income')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (5, N'Materials')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (7, N'Misc')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (12, N'Overhead')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (3, N'Payroll')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (13, N'Permit')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (8, N'Refund')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (9, N'Subcontractor')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (10, N'Taxes')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (6, N'Toll.Gas')
GO
INSERT [dbo].[ExpenseType] ([ExpenseTypeId], [CategoryName]) VALUES (4, N'WorkersComp')
GO
SET IDENTITY_INSERT [dbo].[ExpenseType] OFF
GO
SET IDENTITY_INSERT [dbo].[Phase] ON 
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (1, N'.Misc')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (2, N'Administrative')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (3, N'Bathroom')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (4, N'Blueprints')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (5, N'Building Paper')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (6, N'Cabinets')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (7, N'Cement')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (8, N'Cement-Forms')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (9, N'Clean Up')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (10, N'Countertop')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (11, N'Crown Molding')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (12, N'Data/Comm/AV')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (13, N'Deck')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (14, N'Demo')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (15, N'Doors')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (16, N'Drywall')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (17, N'Drywall-Tape+Mud')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (18, N'Drywall-Texture')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (19, N'Dumps')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (21, N'Electrical')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (20, N'Electric-Finish')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (22, N'Excavation')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (23, N'Fence')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (24, N'Finish Hardware')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (25, N'Flooring')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (26, N'Framing')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (27, N'Gas Line')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (28, N'HVAC')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (29, N'Inspection')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (30, N'Insulation')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (31, N'Irrigation')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (32, N'Kitchen')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (33, N'Landscaping')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (34, N'Light Fixtures')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (35, N'Masonry/Brick')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (36, N'Moving')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (37, N'Painting')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (38, N'Plumbing')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (39, N'Plumbing-Rough')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (40, N'Prep-Painting')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (41, N'Roofing')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (42, N'Siding')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (43, N'Stairs')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (44, N'Stucco')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (45, N'Stucco-Lath')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (46, N'Subfloor')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (47, N'Tile')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (48, N'Trim/Baseboards')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (49, N'Water Heater')
GO
INSERT [dbo].[Phase] ([PhaseId], [PhaseTitle]) VALUES (50, N'Windows')
GO
SET IDENTITY_INSERT [dbo].[Phase] OFF
GO
SET IDENTITY_INSERT [dbo].[Project] ON 
GO
INSERT [dbo].[Project] ([ProjectId], [Name], [Status], [ProjectDate]) VALUES (1, N'.None', 1, CONVERT (date, GETDATE()))
GO
INSERT [dbo].[Project] ([ProjectId], [Name], [Status], [ProjectDate]) VALUES (2, N'Sample', 1, CONVERT (date, GETDATE()))
GO
SET IDENTITY_INSERT [dbo].[Project] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Employee_Employee_Name]    Script Date: 12/22/2025 10:49:07 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Employee]') AND name = N'IX_Employee_Employee_Name')
CREATE UNIQUE NONCLUSTERED INDEX [IX_Employee_Employee_Name] ON [dbo].[Employee]
(
	[Employee_Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Expense_ExpenseTypeId]    Script Date: 12/22/2025 10:49:07 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Expense]') AND name = N'IX_Expense_ExpenseTypeId')
CREATE NONCLUSTERED INDEX [IX_Expense_ExpenseTypeId] ON [dbo].[Expense]
(
	[ExpenseTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Expense_PhaseId]    Script Date: 12/22/2025 10:49:07 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Expense]') AND name = N'IX_Expense_PhaseId')
CREATE NONCLUSTERED INDEX [IX_Expense_PhaseId] ON [dbo].[Expense]
(
	[PhaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Expense_ProjectId]    Script Date: 12/22/2025 10:49:07 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Expense]') AND name = N'IX_Expense_ProjectId')
CREATE NONCLUSTERED INDEX [IX_Expense_ProjectId] ON [dbo].[Expense]
(
	[ProjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ExpenseType_CategoryName]    Script Date: 12/22/2025 10:49:07 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[ExpenseType]') AND name = N'IX_ExpenseType_CategoryName')
CREATE UNIQUE NONCLUSTERED INDEX [IX_ExpenseType_CategoryName] ON [dbo].[ExpenseType]
(
	[CategoryName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Phase_PhaseTitle]    Script Date: 12/22/2025 10:49:07 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Phase]') AND name = N'IX_Phase_PhaseTitle')
CREATE UNIQUE NONCLUSTERED INDEX [IX_Phase_PhaseTitle] ON [dbo].[Phase]
(
	[PhaseTitle] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TimeCard_PhaseId]    Script Date: 12/22/2025 10:49:07 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TimeCard]') AND name = N'IX_TimeCard_PhaseId')
CREATE NONCLUSTERED INDEX [IX_TimeCard_PhaseId] ON [dbo].[TimeCard]
(
	[PhaseId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_TimeCard_ProjectId]    Script Date: 12/22/2025 10:49:07 PM ******/
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[TimeCard]') AND name = N'IX_TimeCard_ProjectId')
CREATE NONCLUSTERED INDEX [IX_TimeCard_ProjectId] ON [dbo].[TimeCard]
(
	[ProjectId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Employee__Employ__4BAC3F29]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Employee] ADD  DEFAULT ((0)) FOR [Employee_Employed]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Expense__IsRecen__5535A963]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Expense] ADD  DEFAULT (CONVERT([bit],(1))) FOR [IsRecent]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__Project__Status__52593CB8]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[Project] ADD  DEFAULT ((0)) FOR [Status]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__TimeCard__Projec__5AEE82B9]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[TimeCard] ADD  CONSTRAINT [DF__TimeCard__Projec__5AEE82B9]  DEFAULT ((1)) FOR [ProjectId]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__TimeCard__PhaseI__5BE2A6F2]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[TimeCard] ADD  CONSTRAINT [DF__TimeCard__PhaseI__5BE2A6F2]  DEFAULT ((1)) FOR [PhaseId]
END
GO
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DF__TimeCard__TimeCa__5CD6CB2B]') AND type = 'D')
BEGIN
ALTER TABLE [dbo].[TimeCard] ADD  CONSTRAINT [DF__TimeCard__TimeCa__5CD6CB2B]  DEFAULT (getdate()) FOR [TimeCard_DateTime]
END
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Expense_ExpenseType_ExpenseTypeId]') AND parent_object_id = OBJECT_ID(N'[dbo].[Expense]'))
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [FK_Expense_ExpenseType_ExpenseTypeId] FOREIGN KEY([ExpenseTypeId])
REFERENCES [dbo].[ExpenseType] ([ExpenseTypeId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Expense_ExpenseType_ExpenseTypeId]') AND parent_object_id = OBJECT_ID(N'[dbo].[Expense]'))
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [FK_Expense_ExpenseType_ExpenseTypeId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Expense_Phase_PhaseId]') AND parent_object_id = OBJECT_ID(N'[dbo].[Expense]'))
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [FK_Expense_Phase_PhaseId] FOREIGN KEY([PhaseId])
REFERENCES [dbo].[Phase] ([PhaseId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Expense_Phase_PhaseId]') AND parent_object_id = OBJECT_ID(N'[dbo].[Expense]'))
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [FK_Expense_Phase_PhaseId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Expense_Project_ProjectId]') AND parent_object_id = OBJECT_ID(N'[dbo].[Expense]'))
ALTER TABLE [dbo].[Expense]  WITH CHECK ADD  CONSTRAINT [FK_Expense_Project_ProjectId] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Project] ([ProjectId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Expense_Project_ProjectId]') AND parent_object_id = OBJECT_ID(N'[dbo].[Expense]'))
ALTER TABLE [dbo].[Expense] CHECK CONSTRAINT [FK_Expense_Project_ProjectId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TimeCard_Employee_EmployeeId]') AND parent_object_id = OBJECT_ID(N'[dbo].[TimeCard]'))
ALTER TABLE [dbo].[TimeCard]  WITH CHECK ADD  CONSTRAINT [FK_TimeCard_Employee_EmployeeId] FOREIGN KEY([EmployeeId])
REFERENCES [dbo].[Employee] ([EmployeeId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TimeCard_Employee_EmployeeId]') AND parent_object_id = OBJECT_ID(N'[dbo].[TimeCard]'))
ALTER TABLE [dbo].[TimeCard] CHECK CONSTRAINT [FK_TimeCard_Employee_EmployeeId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TimeCard_Phase_PhaseId]') AND parent_object_id = OBJECT_ID(N'[dbo].[TimeCard]'))
ALTER TABLE [dbo].[TimeCard]  WITH CHECK ADD  CONSTRAINT [FK_TimeCard_Phase_PhaseId] FOREIGN KEY([PhaseId])
REFERENCES [dbo].[Phase] ([PhaseId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TimeCard_Phase_PhaseId]') AND parent_object_id = OBJECT_ID(N'[dbo].[TimeCard]'))
ALTER TABLE [dbo].[TimeCard] CHECK CONSTRAINT [FK_TimeCard_Phase_PhaseId]
GO
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TimeCard_Project_ProjectId]') AND parent_object_id = OBJECT_ID(N'[dbo].[TimeCard]'))
ALTER TABLE [dbo].[TimeCard]  WITH CHECK ADD  CONSTRAINT [FK_TimeCard_Project_ProjectId] FOREIGN KEY([ProjectId])
REFERENCES [dbo].[Project] ([ProjectId])
ON DELETE CASCADE
GO
IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_TimeCard_Project_ProjectId]') AND parent_object_id = OBJECT_ID(N'[dbo].[TimeCard]'))
ALTER TABLE [dbo].[TimeCard] CHECK CONSTRAINT [FK_TimeCard_Project_ProjectId]
GO
