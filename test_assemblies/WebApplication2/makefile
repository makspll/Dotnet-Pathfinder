MAKEFILE_DIR := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
PORT=5000
LOGLEVEL=ALL

all: build start

build:
	nuget restore ${MAKEFILE_DIR}WebApplication2.sln -PackagesDirectory ${MAKEFILE_DIR}packages
	msbuild ${MAKEFILE_DIR}WebApplication2.sln -p:TargetFramework=net472

start:
	cd ${MAKEFILE_DIR}WebApplication2 && xsp4 --port $(PORT) --nonstop --loglevels $(LOGLEVEL)