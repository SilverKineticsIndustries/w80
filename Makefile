# Load all env vars from .env so all recipe commands here can see them.
ifneq (,$(wildcard ./.env))
 	include .env
 	export
endif

.PHONY: list init start-app run-server-tests run-end-to-end-tests \
		start-devel-db del-devel-data clean show-todos

list:
	@fgrep -h "##" $(MAKEFILE_LIST) | fgrep -v fgrep | sed -e 's/\\$$//' | sed -e 's/##//'

init:                    ## Initialize development environment
	@echo ">> Initializing development environment for w80 ... "
	@if [ -f ./.env ] ; then echo "A .env file was found. Exiting initialization ..." ; false ; fi
	@read -p ">> Do you want to set /proc/sys/fs/inotify/max_user_instances to 256? (Required by cypress n2n tests) Y/N (default N):" setinotify; 		\
	if [ "$$setinotify" = "Y" ]; then 																										\
		sudo bash -c 'echo 256 > /proc/sys/fs/inotify/max_user_instances'; 																	\
    fi
	@echo W80_Database__Name=W80 >> .env
	@echo W80_CaptchaKey= >> .env
	@echo DBPORT=15000 >> .env
	@echo APIPORT=15001 >> .env
	@echo "# React port" >> .env
	@echo PORT=15002 >> .env
	@read -p ">> Enter database username or blank for root (default):" dbuser; 																\
	read -p ">> Enter database password or blank for root (default):" dbpassword; 															\
		test $$dbuser || dbuser=root; 										 																\
		test $$dbpassword || dbpassword=root; 																								\
		echo MONGO_INITDB_ROOT_USERNAME=$$dbuser >> .env;																					\
		echo MONGO_INITDB_ROOT_PASSWORD=$$dbpassword >> .env;																				\
		echo W80_Database__ConnectionString=mongodb://$$dbuser:$$dbpassword@localhost:"$$DBPORT" >> .env;									\
		if ! [ -f ./tests/server/.runsettings ] ; then																						\
			echo "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" 																			\
			"<RunSettings>\n" 																												\
			"<RunConfiguration>\n"																											\
				"<EnvironmentVariables>\n"																									\
					"<W80_Tests_User_Password>longpassword123</W80_Tests_User_Password>\n"															\
					"<W80_Tests_Admin_Password>longpassword123</W80_Tests_Admin_Password>\n"												 	\
					"<W80_Database__ConnectionString>mongodb://$$dbuser:$$dbpassword@localhost:15000</W80_Database__ConnectionString>\n"	\
				"</EnvironmentVariables>\n"																									\
			"</RunConfiguration>\n"																											\
			"</RunSettings>" > ./tests/server/.runsettings ;																				\
		fi
	@read -p ">> Enter JWT token key or blank for new guid (default):" W80_Jwt__Key;														\
		test $$W80_Jwt__Key || W80_Jwt__Key=$$(uuidgen); 																					\
		echo W80_Jwt__Key=$$W80_Jwt__Key>> .env
	@read -p ">> Enter invitation key or blank for new guid (default):" W80_InvitationKey;													\
		test $$W80_InvitationKey || W80_InvitationKey=$$(uuidgen);    																		\
		echo W80_InvitationKey=$$W80_InvitationKey >> .env
	@read -p ">> Enter email confirmation key or blank for new guid (default):" W80_EmailConfirmationKey; 									\
		test $$W80_EmailConfirmationKey || W80_EmailConfirmationKey=$$(uuidgen);  															\
		echo W80_EmailConfirmationKey=$$W80_EmailConfirmationKey >> .env
	@read -p ">> Enter email sender key or blank for empty value:" W80_Notifications__EmailSenderKey; 	    								\
		test $$W80_Notifications__EmailSenderKey || W80_Notifications__EmailSenderKey=123;      													\
		echo W80_Notifications__EmailSenderKey=$$W80_Notifications__EmailSenderKey >> .env
	@echo CYPRESS_USER_EMAIL=testuser@silverkinetics.dev >> .env
	@echo CYPRESS_USER_PASSWORD=longpassword123 >> .env
	@echo W80_Tests_User_Password=longpassword123 >> .env
	@echo W80_Tests_Admin_Password=longpassword123 >> .env
	@echo ASPNETCORE_ENVIRONMENT=Development >> .env
	@echo REACT_APP_BASE_API=http://localhost:"$$APIPORT" >> .env
	@echo REACT_APP_BASE_API_TIMEOUT_IN_MILLISECONDS=5000 >> .env
	@echo REACT_APP_NOTIFICATION_CHECK_IN_MILLISECONDS=30000 >> .env
	@echo REACT_APP_BROWSER_NOTIFICATIONS_THRESHOLD_IN_MINUTES=30 >> .env
	@echo "# REACT_APP_TIMEOUT_IN_MILLISECONDS=5000" >> .env
	@echo "# REACT_APP_CAPTCHA_SITE_KEY=" >> .env
	@echo ">> For running tests from VS Code, ./tests/server/.runsettings file was created."
	@echo ">> Initialization finished."
	@echo ">> You can now run 'make start-app' to run app."

start-app:               ## Start application
	mkdir -p ./data/data
	docker compose -f compose.devel.yaml rm -f
	docker compose -f compose.devel.yaml build
	docker compose -f compose.devel.yaml up -d --remove-orphans
	open "http://localhost:`$$WEBPORT`"

stop-app:                ## Stop application
	docker compose -f compose.devel.yaml stop

start-devel-db:          ## Start development database
	mkdir -p ./data/data
	docker compose -f compose.dbonly.yaml rm -f
	docker compose -f compose.dbonly.yaml build
	docker compose -f compose.dbonly.yaml up -d --remove-orphans

run-server-tests:        ## Run server (unit + integration) tests
	docker compose -f compose.devel.yaml rm -f
	docker compose -f compose.devel.yaml build
	docker compose -f compose.devel.yaml up -d --remove-orphans
	-dotnet test --logger:"console;verbosity=normal" --nologo
	docker compose -f compose.devel.yaml stop

run-end-to-end-tests:    ## Run end-to-end tests
	@echo "*************************************************"
	@echo "* ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! *"
	@echo "* ! Make sure you dont have reCaptcha enabled ! *"
	@echo "* !      and user culture is English          ! *"
	@echo "* ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! ! *"
	@echo "*************************************************"
	docker compose -f compose.devel.yaml rm -f
	docker compose -f compose.devel.yaml build
	docker compose -f compose.devel.yaml up -d --remove-orphans
	-cd ./src/webapp; npm install; npx cypress run -q;
	docker compose -f compose.devel.yaml stop

del-devel-data:          ## Delete development db data
	sudo rm -r ./data/

clean:                   ## Clean all build artifacts
	dotnet clean

test:                    ## Initialize development environment

show-todos:              ## Show TODOs
	grep -i -R --binary-files=without-match -A 10 --color=always "TODO" ./src/server/ ./src/webapp/src/ ./tests/n2n ./tests/server