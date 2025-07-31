-- Seed theme defaults
INSERT INTO "ThemeDefaults" ("Id","Key","Value") VALUES
  (1,'primary','#3B5F3B'),
  (2,'secondary','#A97449'),
  (3,'accent','#D5B57A'),
  (4,'background','#F9F6F1'),
  (5,'navbar','#EFE9DC'),
  (6,'navbar-border','#DDD3C2'),
  (7,'contrast','#2E2E2E'),
  (8,'heading font','"Playfair Display", serif'),
  (9,'body font','Lora, serif')
ON CONFLICT ("Id") DO UPDATE SET
  "Key"=EXCLUDED."Key","Value"=EXCLUDED."Value";

-- Seed pages
INSERT INTO "Pages" (
  "Id", 
  "Route", 
  "DefaultContent", 
  "CreatedAt", 
  "UpdatedAt"
)
VALUES
  (
    '75f1dc70-6120-42c0-9c5e-8138fb755bbe', 
    '', 
    '{
      "blocks": [
        { "type": "p", "content": "Tucked away in the quiet countryside of North Essex, Hollywood Farm Vineyard is a small family project rooted in passion, tradition, and legacy." },
        { "type": "p", "content": "Our family has farmed this land for over a century. Through five generations, it has been passed down, worked on, and cared for — each adding something new to the story. When Charles retired, he decided it was time for a different kind of planting: a vineyard." },
        { "type": "p", "content": "That one decision brought the whole family together. Aunts, uncles, cousins, and kids all got involved. Today, three generations pitch in with planting, pruning, and picking." },
        { "type": "p", "content": "We have had one hand-picked harvest so far and while we are just getting started, the roots run deep." }
      ]
    }'::jsonb,
    '2025-07-23 00:00:00+00',
    '2025-07-23 00:00:00+00'
  ),
  (
    'd1c84029-8121-47cc-9d84-a94c30b163b3', 
    'about', 
    '{
      "blocks": [
        { "type": "h1", "content": "Our Story" },
        { "type": "image", "content": { "src": "assets/temp-images/ReadyForHarvest.jpg", "alt": "Ready for Harvest", "caption": "Our first harvest, picked by hand in 2024" } },
        { "type": "h2", "content": "The People Behind the Vines" },
        { "type": "people", "content": [
          { "imageUrl": "assets/temp-images/ReadyForHarvest.jpg", "name": "Charles", "text": "After decades running the farm, Charles planted the first vines the year he retired. He’s the reason any of this exists – and he still walks the rows most mornings, making sure everything’s looking right." }
        ]},
        { "type": "p", "content": "Together, we are building something small, meaningful, and completely our own." }
      ]
    }'::jsonb,
    '2025-07-23 00:00:00+00',
    '2025-07-23 00:00:00+00'
  ),
  (
    'a0412df9-703d-420a-a1af-2bec3380769f',
    'gallery',
    '{
      "blocks": [
        { "type": "h1", "content": "Gallery" },
        { "type": "h2", "content": "A few moments from our journey so far." },
        { "type": "p", "content": "Here are a few snapshots of the family, the vineyard, and the land we’re growing into." },
        { "type": "image", "content": { "src": "assets/temp-images/ASunriseAtTheVineyard.jpg", "alt": "Sunrise at the vineyard", "caption": "Morning light over the first row" } },
        { "type": "image", "content": { "src": "assets/temp-images/HarvestComplete.jpg", "alt": "Harvest complete", "caption": "The last crate picked before sunset" } },
        { "type": "image", "content": { "src": "assets/temp-images/HelloWoof.jpg", "alt": "The family dog", "caption": "One of the friendliest members of the crew" } },
        { "type": "image", "content": { "src": "assets/temp-images/JustPlanted.jpg", "alt": "Just planted", "caption": "The first baby vines going into the soil" } },
        { "type": "image", "content": { "src": "assets/temp-images/PickedGrapesInSunset.jpg", "alt": "Picked grapes in sunset", "caption": "Fresh grapes and golden light — a perfect pairing" } },
        { "type": "image", "content": { "src": "assets/temp-images/ReadyForHarvest.jpg", "alt": "Ready for harvest", "caption": "The week before we picked our first crop" } },
        { "type": "image", "content": { "src": "assets/temp-images/TheMistyVineyard.jpg", "alt": "The misty vineyard", "caption": "Early autumn fog across the rows" } },
        { "type": "image", "content": { "src": "assets/temp-images/TheMoonlitMist.jpg", "alt": "The moonlit mist", "caption": "Taken on a quiet evening just before harvest" } }
      ]
    }'::jsonb,
    '2025-07-23 00:00:00+00',
    '2025-07-23 00:00:00+00'
  )
ON CONFLICT ("Id") DO UPDATE SET
  "Route" = EXCLUDED."Route",
  "DefaultContent" = EXCLUDED."DefaultContent",
  "CreatedAt" = EXCLUDED."CreatedAt",
  "UpdatedAt" = EXCLUDED."UpdatedAt";

-- Seed roles
INSERT INTO "Roles" ("Id", "Name") VALUES
  (1, 'Admin'),
  (2, 'Editor')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

-- Seed permissions
INSERT INTO "Permissions" ("Id", "Name") VALUES
  (1, 'CanEditContent'),
  (2, 'CanEditTheme'),
  (3, 'CanManageUsers'),
  (4, 'CanViewAdminPanel'),
  (5, 'CanPublishContent')
ON CONFLICT ("Id") DO UPDATE SET "Name" = EXCLUDED."Name";

-- Assign permissions to roles
INSERT INTO "RolePermissions" ("RoleId", "PermissionId") VALUES
  (1,1), (1,2), (1,3), (1,4), (1,5),
  (2,1), (2,2), (2,5), (2,4)
ON CONFLICT DO NOTHING;

-- Seed default superuser
INSERT INTO "Users" ("Id", "Username", "PasswordHash", "Email", "CreatedAt", "IsActive") VALUES
  ('00000000-0000-0000-0000-000000000001', 'admin@example.com', '$2b$12$f2GB4ciZjzhcqrEYgwh.IecJYxvd3uqS1RrQfj.V1kpTill.QmbMe', 'admin@example.com', NOW(), TRUE)
ON CONFLICT ("Id") DO NOTHING;
INSERT INTO "UserRoles" ("UserId", "RoleId")
  SELECT '00000000-0000-0000-0000-000000000001', 1
  WHERE NOT EXISTS (
    SELECT 1 FROM "UserRoles" WHERE "UserId"='00000000-0000-0000-0000-000000000001' AND "RoleId"=1
  );




