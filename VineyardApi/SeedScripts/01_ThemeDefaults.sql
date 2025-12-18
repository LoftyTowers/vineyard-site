-- Seed theme defaults
MERGE INTO "ThemeDefaults" AS t
USING (
  VALUES
    (1,'primary','#3B5F3B'),
    (2,'secondary','#A97449'),
    (3,'accent','#D5B57A'),
    (4,'background','#F9F6F1'),
    (5,'navbar','#EFE9DC'),
    (6,'navbar-border','#DDD3C2'),
    (7,'contrast','#2E2E2E'),
    (8,'heading font','"Playfair Display", serif'),
    (9,'body font','Lora, serif')
) AS s("Id","Key","Value")
ON t."Id" = s."Id"
WHEN MATCHED THEN
  UPDATE SET "Key" = s."Key", "Value" = s."Value"
WHEN NOT MATCHED THEN
  INSERT ("Id","Key","Value")
  VALUES (s."Id", s."Key", s."Value");
