#!/bin/bash

BASEDIR="$(dirname "$BASH_SOURCE")"
pwsh -executionpolicy bypass -noexit -command "import-module '$BASEDIR\PrtgAPI.psd1'; cd ~"