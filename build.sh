#!/bin/bash

BASEDIR="$(dirname "$BASH_SOURCE")"
pwsh -executionpolicy bypass -noexit -noninteractive -command "ipmo psreadline; ipmo '$BASEDIR\Tools\PrtgAPI.Build' -Args \$true"