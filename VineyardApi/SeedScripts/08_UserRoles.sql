MERGE INTO "UserRoles" AS ur
USING (
  VALUES ('00000000-0000-0000-0000-000000000001'::uuid, 1)
) AS s("UserId","RoleId")
ON ur."UserId" = s."UserId" AND ur."RoleId" = s."RoleId"
WHEN NOT MATCHED THEN
  INSERT ("UserId","RoleId") VALUES (s."UserId", s."RoleId");
