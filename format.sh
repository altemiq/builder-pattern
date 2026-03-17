#!/bin/sh

dotnet format whitespace $(dirname "$0") --report $(dirname "$0")/whitespace.report.json --verbosity detailed --no-restore
dotnet format style $(dirname "$0")  --report $(dirname "$0")/style.report.json --severity info --verbosity detailed --no-restore
dotnet format analyzers $(dirname "$0") --report $(dirname "$0")/analyzers.report.json --severity info --verbosity detailed --exclude-diagnostics S1133 --no-restore
