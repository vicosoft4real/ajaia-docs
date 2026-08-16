INSERT INTO app_users (id, email, display_name, avatar_color, is_seeded, created_at)
VALUES
    ('00000000-0000-0000-0000-000000000001', 'amina@example.test', 'Amina Okafor', '#365CF5', true, now()),
    ('00000000-0000-0000-0000-000000000002', 'chidi@example.test', 'Chidi Okeke', '#25A77A', true, now()),
    ('00000000-0000-0000-0000-000000000003', 'tayo@example.test', 'Tayo Bello', '#C77A15', true, now())
ON CONFLICT (id) DO NOTHING;
