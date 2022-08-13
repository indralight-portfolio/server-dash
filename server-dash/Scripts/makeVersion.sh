pushd ./Scripts > /dev/null
Version=`cat version.txt`
echo "Version : ${Version}"
popd > /dev/null

pushd ../ > /dev/null
RevServer=`git rev-parse --short HEAD`
popd > /dev/null
pushd ../Lib-Dash > /dev/null
RevDash=`git rev-parse --short HEAD`
popd > /dev/null
pushd ../Data-Dash > /dev/null
RevData=`git rev-parse --short HEAD`
popd > /dev/null

sed 's/\$Version\$/'${Version}'/g;s/\$RevServer\$/'${RevServer}'/g;s/\$RevDash\$/'${RevDash}'/g;s/\$RevData\$/'${RevData}'/g' ./Scripts/BuildVersion.txt > ./BuildVersion.cs

echo "{\"Version\":\"${Version}\",\"RevServer\":\"${RevServer}\",\"RevDash\":\"${RevDash}\",\"RevData\":\"${RevData}\",\"DateTime\":\"$(date '+%Y-%m-%d %H:%M:%S')\"}">meta.json
