PORT=5000
LOGLEVEL=ALL

all: build start

build:
	@nuget restore WebApplication2.sln -PackagesDirectory packages
	@msbuild WebApplication2.sln -p:TargetFramework=net472

start:
	@cd WebApplication2 && xsp4 --port $(PORT) --nonstop --loglevels $(LOGLEVEL)