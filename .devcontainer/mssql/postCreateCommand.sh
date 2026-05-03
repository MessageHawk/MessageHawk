#!/bin/bash
set -euo pipefail
dacpac="false"
sqlfiles="false"
SApassword=$1
dacpath=$2
sqlpath=$3
sqlcmd_bin="/opt/mssql-tools/bin/sqlcmd"
ready="false"

echo "SELECT * FROM SYS.DATABASES" | dd of=testsqlconnection.sql
for i in {1..90};
do
    if "$sqlcmd_bin" -C -S localhost -U sa -P "$SApassword" -d master -i testsqlconnection.sql > /dev/null 2>&1
    then
        echo "SQL server ready"
        ready="true"
        break
    else
        echo "Not ready yet... ($i/90)"
        sleep 1
    fi
done
rm testsqlconnection.sql
if [ "$ready" != "true" ]
then
    echo "ERROR: SQL Server did not become ready in time. Check Docker memory (SQL Server needs ~2 GB) and db container logs." >&2
    exit 1
fi

for f in $dacpath/*
do
    if [ $f == $dacpath/*".dacpac" ]
    then
        dacpac="true"
        echo "Found dacpac $f"
    fi
done

for f in $sqlpath/*
do
    if [ $f == $sqlpath/*".sql" ]
    then
        sqlfiles="true"
        echo "Found SQL file $f"
    fi
done

if [ $sqlfiles == "true" ]
then
    for f in $sqlpath/*
    do
        if [ $f == $sqlpath/*".sql" ]
        then
            echo "Executing $f"
            "$sqlcmd_bin" -C -S localhost -U sa -P "$SApassword" -d master -i "$f"
        fi
    done
fi

if [ $dacpac == "true" ] 
then
    for f in $dacpath/*
    do
        if [ $f == $dacpath/*".dacpac" ]
        then
            dbname=$(basename $f ".dacpac")
            echo "Deploying dacpac $f"
            /opt/sqlpackage/sqlpackage /Action:Publish /SourceFile:$f /TargetTrustServerCertificate:True /TargetServerName:db /TargetDatabaseName:$dbname /TargetUser:sa /TargetPassword:$SApassword
        fi
    done
fi
