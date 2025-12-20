-- Verify pages and current version pointers.
SELECT p."Id",
       p."Route",
       p."CurrentVersionId",
       pv."VersionNo",
       pv."CreatedUtc"
FROM "Pages" p
LEFT JOIN "PageVersions" pv ON pv."Id" = p."CurrentVersionId"
ORDER BY p."Route";

-- Count versions per page.
SELECT p."Id",
       p."Route",
       COUNT(pv."Id") AS version_count
FROM "Pages" p
LEFT JOIN "PageVersions" pv ON pv."PageId" = p."Id"
GROUP BY p."Id", p."Route"
ORDER BY p."Route";

-- Check for missing current version pointers.
SELECT COUNT(*) AS pages_missing_current
FROM "Pages"
WHERE "CurrentVersionId" IS NULL;

-- Check for duplicate version numbers.
SELECT "PageId",
       "VersionNo",
       COUNT(*) AS duplicates
FROM "PageVersions"
GROUP BY "PageId", "VersionNo"
HAVING COUNT(*) > 1;
