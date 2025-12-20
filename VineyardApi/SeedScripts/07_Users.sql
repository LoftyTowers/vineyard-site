-- Seed default superuser
MERGE INTO "Users" AS t
USING (
  VALUES ('00000000-0000-0000-0000-000000000001'::uuid, 'admin@example.com', '$2b$12$f2GB4ciZjzhcqrEYgwh.IecJYxvd3uqS1RrQfj.V1kpTill.QmbMe', 'admin@example.com', NOW(), TRUE)
) AS s("Id","Username","PasswordHash","Email","CreatedAt","IsActive")
ON t."Id" = s."Id"
WHEN NOT MATCHED THEN
  INSERT ("Id","Username","PasswordHash","Email","CreatedAt","IsActive")
  VALUES (s."Id", s."Username", s."PasswordHash", s."Email", s."CreatedAt", s."IsActive");
