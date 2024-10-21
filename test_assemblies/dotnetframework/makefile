MAKEFILE_DIR := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
PORT=5000
LOGLEVEL=ALL

all: build start

build:
	unset MSBUILDUSESERVER && unset MSBuildExtensionsPath && unset MSBuildSDKsPath && unset MSBuildLoadMicrosoftTargetsReadOnly && cd ${MAKEFILE_DIR} && nuget restore dotnetframework.sln -PackagesDirectory packages
	unset MSBUILDUSESERVER && unset MSBuildExtensionsPath && unset MSBuildSDKsPath && unset MSBuildLoadMicrosoftTargetsReadOnly && cd ${MAKEFILE_DIR} && msbuild dotnetframework.sln -p:TargetFramework=net472

start:
	@cd ${MAKEFILE_DIR}/dotnetframework && xsp4 --port $(PORT) --nonstop --loglevels $(LOGLEVEL)