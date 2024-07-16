
                  x=~                 u+=~~~+u.        .n~~%x.
                 88x.   .e.   .e.   z8F      `8N.    x88X   888.
                '8888X.x888:.x888  d88L       98E   X888X   8888L
                 `8888  888X '888k 98888bu.. .@*   X8888X   88888
                  X888  888X  888X "88888888NNu.   88888X   88888X
                  X888  888X  888X  "*8888888888i  88888X   88888X
                  X888  888X  888X  .zf""*8888888L 88888X   88888f
                 .X888  888X. 888~ d8F      ^%888E 48888X   88888
                 `%88%Y"*888Y"     88>        `88~  ?888X   8888"
                   `~     `"       '%N.       d*"    "88X   88*`
                                      ^"====="`        ^"==="`
                                    [by Silver Kinetics Industries]

                    A tool to make job hunting slightly easier.


### Introduction
-----------------

  W80 is a tool for job seekers to organize their job hunting data, stay on top of all the job related
  appointments and compare previous and current employment opportunities to understand areas which can
  be improved.

### Technologies
-----------------

  W80 is built with C# on top of .NET8. It utilizes MongoDB for data storage and uses
  the MongoDB C# driver as the ORM mapper. The front end is built with ReactJS + Redux.

### Dependencies
-----------------

  On the back-end the system utilizes Serilog for logging.
  The development began by using the .NET Entity Framework provider for MongoDB, but since that
  was in an early preview release, it included a couple of bugs which blocked further progress.
  That is why the decision was made to switch to the MongoDB C# Driver as the ORM mapper.
  The system also utilizes Mapperly from Riok for mapping of objects. This tool was selected
  because of its performance based on the benchmarking results @: https://github.com/mjebrahimi/Benchmark.netCoreMappers

  On the front-end, in addition to React and Redux, the system also utilized react-big-caledar, dayjs,
  i18next and react-google-recaptcha as some of the bigger third party libraries.

### Start app
----------------

  To try out the app you need two things, an .env file and the docker compose file.
  If you just cloned the repo, run make init and this will go through a initialization script
  which will create a .env file. Almost all options have default value.

  Afterwards, you can start the app using:

    make start-app

  Login with: testuser@silverkinetics.dev / longpassword123

  When done you can run:

    make stop-app

### Development Setup
----------------------

  **Currently this project is not accepting any pull requests.**

  You need to have docker and docker compose installed.

  After cloning the repo for the first time, make sure you run "make init" which will
  create your default .env file. You can modify this as you choose afterwards.

  You should create a .gitconfig file in the repo root folder and fill in all your
  git settins for this project. The git/.config file has the include for the ./.gitconfig file.

  I like to use Makefiles to create simple recipies for various tasks you might do
  during development. You can see all the recipies and their explanations by running
  "make" or "make list"

  To begin development you can run:

    make start-devel-db

  This should start the development database. Then you can open the repo in VS Code
  and run the ./src/server/controllers/w80.Controllers.csproj project. That is the
  backend endpoints API project. Finally, go to ./src/server/webapp and run
  "npm install" followed by "npm start" to start the web app.

  For development, the following ports are utilized:

  - 15000 = Database
  - 15001 = Server
  - 15002 = Webapp

  For development, MongoDB mounts its data to the ./data/data folder. The backend
  logs are persisted to the ./logs folder.

### Testing
----------------------

  All the tests, including unit, integration and end-to-end are in ./tests.

  In order to run unit and integration tests from VSCode, just verify that the
  ./tests/server/.runsettings file looks fine. It should have been created during
  "make init" stage. Afterwards, you can run the tests from VS Code.

  You can also run unit and integration outside of VS Code by runnung:

     make run-server-tests

  Cypress framework is used for end-to-end testing. To run the tests without any visual UI, run:

     make run-end-to-end-tests

  If you would like to see the actual browser automation of the end-to-end testing,
  run npx cypress open, and the Cypress UI will open. From there you can run each tests
  and look at the automation in realtime.