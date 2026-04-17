BEGIN TRANSACTION;
GO

ALTER TABLE [ShowcaseFeaturedProducts] DROP CONSTRAINT [PK_ShowcaseFeaturedProducts];
GO

ALTER TABLE [ShowcaseFeaturedProducts] ADD [Id] int NOT NULL IDENTITY;
GO

ALTER TABLE [ShowcaseFeaturedProducts] ADD CONSTRAINT [PK_ShowcaseFeaturedProducts] PRIMARY KEY ([Id]);
GO

CREATE UNIQUE INDEX [IX_ShowcaseFeaturedProducts_ShowcaseId_SortOrder] ON [ShowcaseFeaturedProducts] ([ShowcaseId], [SortOrder]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260417034659_AllowDuplicateShowcaseFeaturedProducts', N'8.0.11');
GO

COMMIT;
GO

