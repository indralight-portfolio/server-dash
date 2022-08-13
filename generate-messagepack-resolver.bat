@echo off
echo mpc arguments help:
echo  -i, --input=VALUE          [required]Input path of analyze csproj
echo  -o, --output=VALUE         [required]Output file path
echo  -c, --conditionalsymbol=VALUE
echo                             [optional, default=empty]conditional compiler
echo                               symbol
echo  -r, --resolvername=VALUE   [optional, default=GeneratedResolver]Set resolver
echo                               name
echo  -n, --namespace=VALUE      [optional, default=MessagePack]Set namespace root
echo                               name
echo  -m, --usemapmode           [optional, default=false]Force use map mode
echo                               serialization

@echo on
MessagePack.Generator\win\mpc.exe -i Dash\Dash.csproj -o Lib-Dash\Dash\DashMPackResolver.cs -n Dash
MessagePack.Generator\win\mpc.exe -i server-dash\server-dash.csproj -o server-dash\ServerDashMPackResolver.cs -n server_dash -checkInputName true
@echo off

rem del Dash\__buildtempDash.AssemblyInfo.cs
rem del Dash\__buildtempDash.AssemblyInfoInputs.cache
rem del Dash\__buildtempDash.assets.cache
rem del server-dash\__buildtempserver-dash.AssemblyInfo.cs
rem del server-dash\__buildtempserver-dash.AssemblyInfoInputs.cache
rem del server-dash\__buildtempserver-dash.assets.cache

timeout /t 5
