# Options:
#   -i, -input <String>                                Input path of analyze csproj or directory, if input multiple csproj split with ','. (Required)
#   -o, -output <String>                               Output file path(.cs) or directory(multiple generate file). (Required)
#   -c, -conditionalSymbol <String>                    Conditional compiler symbols, split with ','. (Default: null)
#   -r, -resolverName <String>                         Set resolver name. (Default: GeneratedResolver)
#   -n, -namespace <String>                            Set namespace root name. (Default: MessagePack)
#   -m, -useMapMode <Boolean>                          Force use map mode serialization. (Default: False)
#   -ms, -multipleIfDirectiveOutputSymbols <String>    Generate #if-- files by symbols, split with ','. (Default: null)

MessagePack.Generator/osx/mpc -i Dash/Dash.csproj -o Lib-Dash/Dash/DashMPackResolver.cs -n Dash
MessagePack.Generator/osx/mpc -i server-dash/server-dash.csproj -o server-dash/ServerDashMPackResolver.cs -n server_dash -checkInputName true

# rm Dash/__buildtempDash.AssemblyInfo.cs
# rm Dash/__buildtempDash.AssemblyInfoInputs.cache
# rm Dash/__buildtempDash.assets.cache
# rm server-dash/__buildtempserver-dash.AssemblyInfo.cs
# rm server-dash/__buildtempserver-dash.AssemblyInfoInputs.cache
# rm server-dash/__buildtempserver-dash.assets.cache



# MessagePack.Generator/osx/mpc -i ../../../../../../allm/dash/server-dash/server-dash/server-dash.csproj -o ../../../../../../allm/dash/server-dash/server-dash/ServerDashMPackResolver.cs -n server_dash

# MessagePack.Generator/osx/mpc -i ../../../../../../allm/dash/server-dash/Dash/Dash.csproj -o ../../../../../../allm/dash/server-dash/Dash/Dash/DashMPackResolver.cs -n Dash