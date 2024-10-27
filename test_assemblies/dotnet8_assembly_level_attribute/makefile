MAKEFILE_DIR := $(dir $(abspath $(lastword $(MAKEFILE_LIST))))
REMOVE_CLASSES_LIST=InheritingControllerNoRoute \
	InheritingController2NoRoute \
	DefaultConventionalController \
	DefaultControllerConventional \
	ConventionalControllerOverriddenActionNames \
	ControllerWithForeignBase
COPY_FILES_LIST=$(shell cat .gitignore)
COPY_DIR="${MAKEFILE_DIR}../dotnet8"
COPY_TARGET="${MAKEFILE_DIR}"
TEST_UTILS_DIR="${MAKEFILE_DIR}../TestUtils"
PORT=5000

all: build start

build: copy_routes
	@dotnet build -f net8.0
start:
	@dotnet run --urls=http://localhost:$(PORT)

copy_routes:
	@echo "Copying files from $(COPY_DIR) to $(COPY_TARGET)"
	@for file in $(COPY_FILES_LIST); do \
		echo "Copying '$$file'"; \
		cp $(COPY_DIR)/$$file ${COPY_TARGET}/$$file; \
		for className in $(REMOVE_CLASSES_LIST); do \
			echo "Removing class '$$className' from '$$file'"; \
			python ${TEST_UTILS_DIR}/remove_class.py ${COPY_TARGET}/$$file $$className; \
		done; \
	done