#!/bin/bash

f=$1
if [ "$f" == "" ]; then
  read -r -p "Input file: " f
fi
#echo ${f}

cd=`pwd`
rootpath="$(echo ${cd} | sed -e 's/~.*\/.*/~/g')"
#echo ${rootpath}
targetpath=$(echo ${cd} | sed -e 's/Table~/Common/g')
#echo ${targetpath}

exepath=${rootpath}/ExcelConsole.exe

if [ -z "${f}" ]; then
  ${exepath}
else
  ${exepath} "${f}"
fi

if [[ "$2"!="nowait" ]]; then
  read -t 5 -n1 -r -p "Press any key to continue..."
fi
exit
