CREATE OR REPLACE VIEW public.vw_admin_users AS
SELECT
    u.username,
    u.email,

    CASE u.role
        WHEN 1 THEN 'User'
        WHEN 2 THEN 'Administrator'
        ELSE 'Unknown'
    END AS role,

    u.status AS account_status,
    u.created_at,

    (
        SELECT COUNT(*)
        FROM public.quizzes AS q
        WHERE q.author_id = u.user_id
    ) AS authored_quiz_count,

    (
        SELECT COUNT(*)
        FROM public.quiz_attempts AS attempt
        WHERE attempt.user_id = u.user_id
    ) AS quiz_attempt_count,

    (
        SELECT COUNT(*)
        FROM public.image_assets AS image
        WHERE image.uploaded_by_id = u.user_id
    ) AS uploaded_image_count,

    u.user_id
FROM public.users AS u;