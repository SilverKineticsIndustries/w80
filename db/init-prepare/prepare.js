disableTelemetry();

const dbname = process.env["MONGO_INITDB_DATABASE"];

db = db.getSiblingDB(dbname);
db.User.createIndex( { Email: 1, Role: 1 }, { unique: true } );
db.Application.createIndex( { UserId: 1, } );