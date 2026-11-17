#!/bin/bash

# Waits for SQL Server to accept logins, then runs init-db.sql. Exits non-zero if either step fails,
# so `service_completed_successfully` only lets the API start once the schema exists.
SERVER="sqlserver"
USERNAME="sa"
PASSWORD="${MSSQL_SA_PASSWORD:?MSSQL_SA_PASSWORD is not set}"
DB_SCRIPT="/tmp/init-db.sql"
SQLCMD="/opt/mssql-tools/bin/sqlcmd"
MAX_ATTEMPTS=30
SLEEP_DURATION=2

attempt=0
while [ $attempt -lt $MAX_ATTEMPTS ]; do
    echo "[DB INIT] Attempting to connect to SQL Server... (Attempt: $((attempt+1)))"

    if $SQLCMD -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT 1" -h -1 > /dev/null 2>&1; then
        echo "[DB INIT] Connection successful. Executing script..."
        if $SQLCMD -b -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -i "$DB_SCRIPT"; then
            echo "[DB INIT] Done."
            exit 0
        fi

        echo "[DB INIT] Script failed."
        exit 1
    fi

    attempt=$((attempt+1))
    sleep $SLEEP_DURATION
done

echo "[DB INIT] Failed to connect to SQL Server after $MAX_ATTEMPTS attempts."
exit 1
