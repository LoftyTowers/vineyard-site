-- Seed pages + versions
WITH seed_pages AS (
  INSERT INTO "Pages" ("Id","Route","DefaultContent","CreatedAt","UpdatedAt","CurrentVersionId","DraftVersionId")
  VALUES
    (
      '75f1dc70-6120-42c0-9c5e-8138fb755bbe'::uuid,
      '',
      '{
        "blocks": [
          { "type": "h1", "content": "Welcome to Hollywood Farm Vineyard" },
          { "type": "h2", "content": "Experience the beauty of our vines" },
          { "type": "image", "content": { "imageId": "9b7f9c1a-6f41-4dd1-9f5b-2db279b9d2a1", "alt": "Sunrise at the vineyard", "caption": "Morning light over the first row", "variant": "hero" } },
          { "type": "richText", "contentHtml": "<p>Tucked away in the quiet countryside of North Essex, Hollywood Farm Vineyard is a small family project rooted in passion, tradition, and legacy.</p><p>Our family has farmed this land for over a century. Through five generations, it has been passed down, worked on, and cared for - each adding something new to the story. When Charles retired, he decided it was time for a different kind of planting: a vineyard.</p><p>That one decision brought the whole family together. Aunts, uncles, cousins, and kids all got involved. Today, three generations pitch in with planting, pruning, and picking.</p><p>We have had one hand-picked harvest so far and while we are just getting started, the roots run deep.</p>" }
        ]
      }'::jsonb,
      '2025-07-23 00:00:00+00'::timestamptz,
      '2025-07-23 00:00:00+00'::timestamptz,
      '1e576b6d-3417-42cd-86fe-4c1f0e0ea54e'::uuid,
      NULL
    ),
    (
      'd1c84029-8121-47cc-9d84-a94c30b163b3'::uuid,
      'about',
      '{
        "blocks": [
          { "type": "h1", "content": "Our Story" },
          { "type": "image", "content": { "imageId": "3c4f2e3a-7e1d-4c22-9e2c-5a0d4b2b1c8f", "alt": "Ready for Harvest", "caption": "Our first harvest, picked by hand in 2024" } },
          { "type": "h2", "content": "The People Behind the Vines" },
          { "type": "people", "content": [
            { "imageUrl": "assets/temp-images/ReadyForHarvest.jpg", "name": "Charles", "text": "After decades running the farm, Charles planted the first vines the year he retired. He''s the reason any of this exists, and he still walks the rows most mornings, making sure everything''s looking right." }
          ]},
          { "type": "p", "content": "Together, we are building something small, meaningful, and completely our own." }
        ]
      }'::jsonb,
      '2025-07-23 00:00:00+00'::timestamptz,
      '2025-07-23 00:00:00+00'::timestamptz,
      '6e9989f4-19fa-4869-97c9-ae99e0f8e739'::uuid,
      NULL
    ),
    (
      'a0412df9-703d-420a-a1af-2bec3380769f'::uuid,
      'gallery',
      '{
        "blocks": [
          { "type": "h1", "content": "Gallery" },
          { "type": "h2", "content": "A few moments from our journey so far." },
          { "type": "p", "content": "Here are a few snapshots of the family, the vineyard, and the land we''re growing into." },
          { "type": "image", "content": { "imageId": "9b7f9c1a-6f41-4dd1-9f5b-2db279b9d2a1", "alt": "Sunrise at the vineyard", "caption": "Morning light over the first row" } },
          { "type": "image", "content": { "imageId": "6f2d7a0e-0e6a-4a6d-9f1b-48f2a3d7f9b2", "alt": "Harvest complete", "caption": "The last crate picked before sunset" } },
          { "type": "image", "content": { "imageId": "1a6f8e2b-4d5d-4c1e-8a7b-8e2f1d4c5a6b", "alt": "The family dog", "caption": "One of the friendliest members of the crew" } },
          { "type": "image", "content": { "imageId": "7c2c2d5b-6a3d-4d33-9b1f-2d7e9f5b1c01", "alt": "Just planted", "caption": "The first baby vines going into the soil" } },
          { "type": "image", "content": { "imageId": "8a1b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d", "alt": "Picked grapes in sunset", "caption": "Fresh grapes and golden light - a perfect pairing" } },
          { "type": "image", "content": { "imageId": "3c4f2e3a-7e1d-4c22-9e2c-5a0d4b2b1c8f", "alt": "Ready for harvest", "caption": "The week before we picked our first crop" } },
          { "type": "image", "content": { "imageId": "2b5d1c7e-3f6a-4b2d-8c9e-7a3f2d1c0b9e", "alt": "The misty vineyard", "caption": "Early autumn fog across the rows" } },
          { "type": "image", "content": { "imageId": "4d3c2b1a-0f9e-8d7c-6b5a-4c3d2e1f0a9b", "alt": "The moonlit mist", "caption": "Taken on a quiet evening just before harvest" } }
        ]
      }'::jsonb,
      '2025-07-23 00:00:00+00'::timestamptz,
      '2025-07-23 00:00:00+00'::timestamptz,
      '2d2a8ee5-5425-469d-a439-df0ce7318c2e'::uuid,
      NULL
    )
  RETURNING "Id","CurrentVersionId","DefaultContent","CreatedAt"
)
INSERT INTO "PageVersions" ("Id","PageId","VersionNo","ContentJson","CreatedUtc","Status","PublishedUtc")
SELECT "CurrentVersionId","Id",1,"DefaultContent","CreatedAt",'Published',"CreatedAt"
FROM seed_pages;
