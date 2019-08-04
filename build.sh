#!/bin/bash

BASEDIR="$(dirname "$BASH_SOURCE")"
pwsh -executionpolicy bypass -noexit -noninteractive -command "ipmo psreadline; ipmo '$BASEDIR\build\PrtgAPI.Build' -Args \$true"