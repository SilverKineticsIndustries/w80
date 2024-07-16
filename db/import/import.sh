#!/bin/sh

# First we seed the database with required initialization data
for f in /tmp/db/init-data/*.json
do
    name=$(basename $f | cut -d'.' -f1)
    mongoimport --host $MONGOIMPORT_HOST_STRING --username $MONGOIMPORT_USERNAME --password $MONGOIMPORT_PASSWORD --authenticationDatabase admin --db $MONGO_INITDB_DATABASE --collection $name --jsonArray --file $f
done

# The reason we dont put the seed data in .js files is that we also need
# to load seed data for tests where we create a new database for each test.
# There is no way from c# driver to run a .js script, but there is a way to
# load .json file, deserialize into BsonDocument and load.
# TODO: There is an issue here that tests run on database without preparation scripts ..

# Then we call any preparation scripts (adding indexes, etc)
for f in /tmp/db/init-prepare/*.js
do
    mongosh "$MONGOSH_CONNECTIONSTRING" --file $f
done

# Finally, if we are non-production, insert dummy users
if [ -d /tmp/db/test-data ]
then
    for f in /tmp/db/test-data/*.json
    do
        name=$(basename $f | cut -d'.' -f1)
        mongoimport --host $MONGOIMPORT_HOST_STRING --username $MONGOIMPORT_USERNAME --password $MONGOIMPORT_PASSWORD --authenticationDatabase admin --db $MONGO_INITDB_DATABASE --collection $name --jsonArray --file $f
    done
fi

