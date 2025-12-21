-- Assign permissions to roles
MERGE INTO "RolePermissions" AS t
USING (
  VALUES
    (1,1), (1,2), (1,3), (1,4), (1,5),
    (2,1), (2,2), (2,5), (2,4)
) AS s("RoleId","PermissionId")
ON t."RoleId" = s."RoleId" AND t."PermissionId" = s."PermissionId"
WHEN NOT MATCHED THEN
  INSERT ("RoleId","PermissionId") VALUES (s."RoleId", s."PermissionId");
