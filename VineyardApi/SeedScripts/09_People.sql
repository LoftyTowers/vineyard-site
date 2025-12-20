-- Seed about page people (table-backed)
WITH about_page AS (
  SELECT "Id" AS "PageId"
  FROM "Pages"
  WHERE "Route" = 'about'
  LIMIT 1
)
INSERT INTO "People" ("Id","PageId","Name","Blurb","ImageUrl","SortOrder")
SELECT
  '1dab4df9-fed6-4182-95f7-6c17350595e8'::uuid,
  about_page."PageId",
  'Charles',
  'After decades running the farm, Charles planted the first vines the year he retired. He''s the reason any of this exists, and he still walks the rows most mornings, making sure everything''s looking right.',
  'assets/temp-images/ReadyForHarvest.jpg',
  1
FROM about_page
WHERE NOT EXISTS (
  SELECT 1
  FROM "People" p
  WHERE p."PageId" = about_page."PageId"
);
