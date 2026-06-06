IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [AuditLogs] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NULL,
    [UserName] nvarchar(max) NOT NULL,
    [Action] nvarchar(max) NOT NULL,
    [Target] nvarchar(max) NULL,
    [Detail] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);

CREATE TABLE [Cars] (
    [Id] uniqueidentifier NOT NULL,
    [Slug] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [Brand] nvarchar(max) NOT NULL,
    [Type] nvarchar(20) NOT NULL,
    [Fuel] nvarchar(10) NOT NULL,
    [Price] bigint NOT NULL,
    [PriceDisplay] nvarchar(max) NOT NULL,
    [Year] int NOT NULL,
    [Image] nvarchar(max) NOT NULL,
    [ImagesJson] nvarchar(max) NOT NULL,
    [Badge] nvarchar(max) NULL,
    [SpecsJson] nvarchar(max) NOT NULL,
    [FeaturesJson] nvarchar(max) NOT NULL,
    [RentalPricePerDay] bigint NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [CreatedById] uniqueidentifier NULL,
    [ApprovedById] uniqueidentifier NULL,
    [ApprovedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Cars] PRIMARY KEY ([Id])
);

CREATE TABLE [DiscountRequests] (
    [Id] uniqueidentifier NOT NULL,
    [OrderId] uniqueidentifier NULL,
    [CarName] nvarchar(max) NOT NULL,
    [CarPrice] bigint NOT NULL,
    [Discount] bigint NOT NULL,
    [Reason] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [StaffId] uniqueidentifier NULL,
    [StaffName] nvarchar(max) NOT NULL,
    [ReviewedById] uniqueidentifier NULL,
    [ReviewNote] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_DiscountRequests] PRIMARY KEY ([Id])
);

CREATE TABLE [News] (
    [Id] uniqueidentifier NOT NULL,
    [Slug] nvarchar(max) NOT NULL,
    [Title] nvarchar(max) NOT NULL,
    [Excerpt] nvarchar(max) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Image] nvarchar(max) NOT NULL,
    [Category] nvarchar(max) NOT NULL,
    [ReadTime] nvarchar(max) NOT NULL,
    [PublishedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_News] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [Name] nvarchar(max) NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [Role] nvarchar(20) NOT NULL,
    [Branch] nvarchar(max) NOT NULL,
    [Avatar] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Orders] (
    [Id] uniqueidentifier NOT NULL,
    [OrderCode] nvarchar(450) NOT NULL,
    [CustomerId] uniqueidentifier NULL,
    [CarId] uniqueidentifier NULL,
    [CarName] nvarchar(max) NOT NULL,
    [CustomerName] nvarchar(max) NOT NULL,
    [Amount] bigint NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    [StaffId] uniqueidentifier NULL,
    [Notes] nvarchar(max) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Orders] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Orders_Users_CustomerId] FOREIGN KEY ([CustomerId]) REFERENCES [Users] ([Id]) ON DELETE SET NULL
);

CREATE UNIQUE INDEX [IX_Cars_Slug] ON [Cars] ([Slug]);

CREATE INDEX [IX_Orders_CustomerId] ON [Orders] ([CustomerId]);

CREATE UNIQUE INDEX [IX_Orders_OrderCode] ON [Orders] ([OrderCode]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260422153726_InitialCreate', N'9.0.4');

COMMIT;
GO

