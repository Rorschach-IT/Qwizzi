DO $$
DECLARE
    seq RECORD;
BEGIN
    FOR seq IN
        SELECT sequence_schema, sequence_name
        FROM information_schema.sequences
        WHERE sequence_schema = 'public'
    LOOP
        EXECUTE format('ALTER SEQUENCE %I.%I RESTART WITH 1;', seq.sequence_schema, seq.sequence_name);
    END LOOP;
END $$;
