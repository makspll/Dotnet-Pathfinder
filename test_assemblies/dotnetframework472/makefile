PORT=5000

build:
	@nuget restore dotnetframework472.sln -PackagesDirectory packages
	@msbuild dotnetframework472.sln

start:
	@xsp4 --port $(PORT) --nonstop