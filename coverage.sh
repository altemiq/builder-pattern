#!/bin/bash

current=$(realpath $(dirname "$0"))

# remove any current values
results=$current/coverage/results
if [ -d "$results" ]; then
  rm -rf $results
fi

# Run dotnet test
dotnet test --solution $current --results-directory $results --coverage --coverage-output-format cobertura

# install the report generator
dotnet tool install -g dotnet-reportgenerator-globaltool

reports=$current/coverage/reports
if [ -d "$reports" ]; then
  rm -rf $reports
fi

# run the report generator
reportgenerator -reports:$results/*.cobertura.xml -targetdir:$reports -reporttypes:'Html_Dark;Cobertura;MarkdownSummary' -verbosity:Verbose