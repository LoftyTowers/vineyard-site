-- Seed images
MERGE INTO "Images" AS t
USING (
  VALUES
    (
      '9b7f9c1a-6f41-4dd1-9f5b-2db279b9d2a1'::uuid,
      'vineyard/hero-sunrise.jpg',
      'assets/temp-images/ASunriseAtTheVineyard.jpg',
      'ASunriseAtTheVineyard.jpg',
      'image/jpeg',
      245678,
      1600,
      900,
      'Sunrise at the vineyard',
      'Morning light over the first row',
      '2025-07-23 00:00:00+00'::timestamptz,
      TRUE
    ),
    (
      '3c4f2e3a-7e1d-4c22-9e2c-5a0d4b2b1c8f'::uuid,
      'vineyard/ready-for-harvest.jpg',
      'assets/temp-images/ReadyForHarvest.jpg',
      'ReadyForHarvest.jpg',
      'image/jpeg',
      232104,
      1400,
      933,
      'Ready for harvest',
      'The week before we picked our first crop',
      '2025-07-23 00:00:00+00'::timestamptz,
      TRUE
    ),
    (
      '6f2d7a0e-0e6a-4a6d-9f1b-48f2a3d7f9b2'::uuid,
      'vineyard/harvest-complete.jpg',
      'assets/temp-images/HarvestComplete.jpg',
      'HarvestComplete.jpg',
      'image/jpeg',
      221890,
      1400,
      933,
      'Harvest complete',
      'The last crate picked before sunset',
      '2025-07-23 00:00:00+00'::timestamptz,
      TRUE
    ),
    (
      '1a6f8e2b-4d5d-4c1e-8a7b-8e2f1d4c5a6b'::uuid,
      'vineyard/hello-woof.jpg',
      'assets/temp-images/HelloWoof.jpg',
      'HelloWoof.jpg',
      'image/jpeg',
      198765,
      1200,
      1600,
      'The family dog',
      'One of the friendliest members of the crew',
      '2025-07-23 00:00:00+00'::timestamptz,
      TRUE
    ),
    (
      '2b5d1c7e-3f6a-4b2d-8c9e-7a3f2d1c0b9e'::uuid,
      'vineyard/the-misty-vineyard.jpg',
      'assets/temp-images/TheMistyVineyard.jpg',
      'TheMistyVineyard.jpg',
      'image/jpeg',
      210432,
      1600,
      1067,
      'The misty vineyard',
      'Early autumn fog across the rows',
      '2025-07-23 00:00:00+00'::timestamptz,
      TRUE
    ),
    (
      '7c2c2d5b-6a3d-4d33-9b1f-2d7e9f5b1c01'::uuid,
      'vineyard/just-planted.jpg',
      'assets/temp-images/JustPlanted.jpg',
      'JustPlanted.jpg',
      'image/jpeg',
      201245,
      1400,
      933,
      'Just planted',
      'The first baby vines going into the soil',
      '2025-07-23 00:00:00+00'::timestamptz,
      TRUE
    ),
    (
      '8a1b3c4d-5e6f-7a8b-9c0d-1e2f3a4b5c6d'::uuid,
      'vineyard/picked-grapes-in-sunset.jpg',
      'assets/temp-images/PickedGrapesInSunset.jpg',
      'PickedGrapesInSunset.jpg',
      'image/jpeg',
      205678,
      1400,
      933,
      'Picked grapes in sunset',
      'Fresh grapes and golden light - a perfect pairing',
      '2025-07-23 00:00:00+00'::timestamptz,
      TRUE
    ),
    (
      '4d3c2b1a-0f9e-8d7c-6b5a-4c3d2e1f0a9b'::uuid,
      'vineyard/the-moonlit-mist.jpg',
      'assets/temp-images/TheMoonlitMist.jpg',
      'TheMoonlitMist.jpg',
      'image/jpeg',
      208910,
      1400,
      933,
      'The moonlit mist',
      'Taken on a quiet evening just before harvest',
      '2025-07-23 00:00:00+00'::timestamptz,
      TRUE
    )
) AS s("Id","StorageKey","PublicUrl","OriginalFilename","ContentType","ByteSize","Width","Height","AltText","Caption","CreatedUtc","IsActive")
ON t."Id" = s."Id"
WHEN MATCHED THEN
  UPDATE SET
    "StorageKey" = s."StorageKey",
    "PublicUrl" = s."PublicUrl",
    "OriginalFilename" = s."OriginalFilename",
    "ContentType" = s."ContentType",
    "ByteSize" = s."ByteSize",
    "Width" = s."Width",
    "Height" = s."Height",
    "AltText" = s."AltText",
    "Caption" = s."Caption",
    "CreatedUtc" = s."CreatedUtc",
    "IsActive" = s."IsActive"
WHEN NOT MATCHED THEN
  INSERT ("Id","StorageKey","PublicUrl","OriginalFilename","ContentType","ByteSize","Width","Height","AltText","Caption","CreatedUtc","IsActive")
  VALUES (s."Id", s."StorageKey", s."PublicUrl", s."OriginalFilename", s."ContentType", s."ByteSize", s."Width", s."Height", s."AltText", s."Caption", s."CreatedUtc", s."IsActive");
