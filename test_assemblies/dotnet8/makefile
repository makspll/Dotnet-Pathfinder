PORT=5000
FRAMEWORK=net8.0

all: build start

build:
	@dotnet build -f $(FRAMEWORK)
start:
	@dotnet run --urls=http://localhost:$(PORT)