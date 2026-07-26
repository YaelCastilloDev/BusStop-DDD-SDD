CREATE ROLE exporter WITH LOGIN PASSWORD 'exporter' IN ROLE pg_monitor;
GRANT pg_read_all_data TO exporter;
GRANT EXECUTE ON FUNCTION pg_catalog.pg_ls_waldir() TO exporter;
