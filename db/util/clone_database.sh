#!/bin/sh

# Parameter 1: Source database name
# Parameter 2: Target database name

sourceDatabase=$1
targetDatabase=$2

if [ ! -n $sourceDatabase ]; then
    echo "--> Please provide the source database name as first positional parameter."
    exit 1
fi
if [ ! -n $targetDatabase ]; then
    echo "--> Please provide the target database name as second positional parameter."
    exit 1
fi

if [ "$sourceDatabase" = "$targetDatabase" ]; then
    echo "--> Source database name cannot be equal to the target database name."
    exit 1
fi

w80DbContainerID=$(docker ps | grep w80-devel-database | cut -f1 -d" ")
if [ ! -n $w80DbContainerID ]; then
    echo "--> Running W80 database container (w80-devel-database) not found ..."
    exit 1
fi

docker exec $w80DbContainerID bash -c "mongosh --eval \"use $targetDatabase\" --quiet --eval  'db.dropDatabase()'   \
mongodb://$MONGO_INITDB_ROOT_USERNAME:$MONGO_INITDB_ROOT_PASSWORD@localhost > /dev/null"

docker exec $w80DbContainerID bash -c                                                                               \
"mongodump --quiet --archive --authenticationDatabase=admin --db=$sourceDatabase                                    \
mongodb://$MONGO_INITDB_ROOT_USERNAME:$MONGO_INITDB_ROOT_PASSWORD@localhost                                         \
| mongorestore --quiet --archive --authenticationDatabase=admin                                                     \
--nsFrom=$sourceDatabase.\* --nsTo=$targetDatabase.\*                                                               \
mongodb://$MONGO_INITDB_ROOT_USERNAME:$MONGO_INITDB_ROOT_PASSWORD@localhost > /dev/null"
