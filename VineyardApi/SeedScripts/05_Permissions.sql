-- Seed permissions
MERGE INTO "Permissions" AS t
USING (
  VALUES
    (1, 'CanEditContent'),
    (2, 'CanEditTheme'),
    (3, 'CanManageUsers'),
    (4, 'CanViewAdminPanel'),
    (5, 'CanPublishContent')
) AS s("Id","Name")
ON t."Id" = s."Id"
WHEN MATCHED THEN
  UPDATE SET "Name" = s."Name"
WHEN NOT MATCHED THEN
  INSERT ("Id","Name") VALUES (s."Id", s."Name");
