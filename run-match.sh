#!/bin/bash
echo ENV:${ASPNETCORE_ENVIRONMENT}
APP_FILE=server-dash.dll
OPTION="--MatchServer:Active true"

cd bin
IP=$(curl -s --connect-timeout 1 http://169.254.169.254/latest/meta-data/local-hostname)
Endpoint=$(curl -s --connect-timeout 1 http://169.254.169.254/latest/meta-data/public-hostname)
InstanceId=$(curl -s --connect-timeout 1 http://169.254.169.254/latest/meta-data/instance-id)
export IP=${IP%%.*}
echo "{\"IP\": \"${IP}\", \"Endpoint\": \"${Endpoint}\", \"InstanceId\": \"${InstanceId}\"}">Endpoint.json

mkdir -p logs/out/$(date +%Y-%m)
dotnet ${APP_FILE} ${OPTION} &>logs/out/$(date +%Y-%m)/$(date +%F_%H%M%S).${IP}.out
