-- Seed roles
MERGE INTO "Roles" AS t
USING (
  VALUES
    (1, 'Admin'),
    (2, 'Editor')
) AS s("Id","Name")
ON t."Id" = s."Id"
WHEN MATCHED THEN
  UPDATE SET "Name" = s."Name"
WHEN NOT MATCHED THEN
  INSERT ("Id","Name") VALUES (s."Id", s."Name");
